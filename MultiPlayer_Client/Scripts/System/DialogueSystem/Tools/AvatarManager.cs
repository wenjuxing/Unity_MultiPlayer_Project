using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using System;

public class AvatarManager : SingletonBase<AvatarManager>,IAvatarService
{
    //本地头像路径前缀
    private string localAvatarPrefix;
    //存储对话头像字典
    private Dictionary<int, Sprite> avatarCache = new Dictionary<int, Sprite>();
    //缓存正在加载的异步操作
    private Dictionary<int, AsyncOperationHandle<Sprite>> loadingHandles = new Dictionary<int, AsyncOperationHandle<Sprite>>();
    private void Awake()
    {
        localAvatarPrefix = Resources.Load<AvatarConfig>("DialogueConfigs/AvatarConfig").avatarPrefix;
    }
    /// <summary>
    /// 异步加载角色头像
    /// </summary>
    /// <param name="speakerId"></param>
    /// <returns></returns>
    public async Task<Sprite> GetAvatarByIdAsync(int speakerId)
    {
        //先尝试获取
        if (avatarCache.TryGetValue(speakerId,out Sprite cacheSprite))
        {
            return cacheSprite;
        }

        //判断是否在加载，避免重复加载
        if (loadingHandles.TryGetValue(speakerId,out AsyncOperationHandle<Sprite> existingHandle))
        {
            //等待已有的加载完成
            await existingHandle.Task;
            return avatarCache.TryGetValue(speakerId, out var sprite) ? sprite : await GetDefaultAvatarAsync();
        }

        //构造资源地址
        string avatarAddress = $"{localAvatarPrefix}{speakerId}";

        //异步加载资源
        AsyncOperationHandle<Sprite> handle= Addressables.LoadAssetAsync<Sprite>(avatarAddress);

        //加入字典标记为正在加载
        loadingHandles.Add(speakerId, handle);

        try
        {
            //等待资源加载
            Sprite sprite = await handle.Task;
            if (sprite != null)
            {
                //加入缓存
                avatarCache.Add(speakerId, sprite);
                return sprite;
            }
            else
            {
                Debug.LogError($"Addressables 未找到 ID={speakerId} 的头像，地址：{avatarAddress}");
                return await GetDefaultAvatarAsync();
            }
        }
        catch (Exception e)
        {
            // 捕获加载异常（如网络错误、资源损坏）
            Debug.LogError($"加载 ID={speakerId} 头像失败：{e.Message}");
            return await GetDefaultAvatarAsync();
        }
        finally 
        {
            //异步操作字典中移除
            loadingHandles.Remove(speakerId);
            //释放内存，避免内存泄漏
            Addressables.Release(handle);
        }
    }
    /// <summary>
    /// 获取默认头像
    /// </summary>
    /// <returns></returns>
    private async Task<Sprite> GetDefaultAvatarAsync()
    {
        //默认头像地址
        string defaultAddress = $"{localAvatarPrefix}Default";

        //尝试获取默认头像
        //使用-1作为默认头像的key
        if (avatarCache.TryGetValue(-1, out Sprite sprite))
        {
            return sprite;
        }
        AsyncOperationHandle<Sprite> defaultHandle = Addressables.LoadAssetAsync<Sprite>(defaultAddress);
        try
        {
          Sprite defaultSprite= await defaultHandle.Task;
            if (defaultSprite==null)
            {
                Debug.LogError($"默认头像加载失败！地址：{defaultAddress}");
                //避免空指针异常
                return Sprite.Create(new Texture2D(1,1),new Rect(0,0,1,1),new Vector2(0.5f,0.5f));
            }
            avatarCache.Add(-1, defaultSprite);
            return defaultSprite;
        }
        catch (Exception e)
        {
            Debug.LogError($"加载默认头像异常：{e.Message}");
            return Sprite.Create(new Texture2D(1, 1), new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }
        finally
        {
            //避免内存泄漏
            Addressables.Release(defaultHandle);
        }
    }
    /// <summary>
    /// 切换场景时清除缓存
    /// </summary>
    public void ClearAvatarCache()
    {
        avatarCache.Clear();
        Debug.Log("头像缓存已清理");
    }
}
