using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using System;

public class AddressableManager : SingletonBase<AddressableManager>,IGoodsIconsService
{
    //本地图标路径前缀
    private string localGoodsIconsPrefix;
    //存储商品图标字典
    private Dictionary<int, Sprite> GoodsIconsCache = new Dictionary<int, Sprite>();
    //缓存正在执行的异步操作
    private Dictionary<int, AsyncOperationHandle<Sprite>> LoadingHandles = new Dictionary<int, AsyncOperationHandle<Sprite>>();

    private void Awake()
    {
        //加载ScriptableObject配置文件获取资源路径
        localGoodsIconsPrefix = Resources.Load<GoodsIconsConfig>("ShopConfigs/GoodsIconsConfig").goodsIconsPrefix;
        Debug.Log("资源路径前缀"+localGoodsIconsPrefix);
    }
    /// <summary>
    /// 异步加载图标
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    public async Task<Sprite> GetIconByIdAsync(int Id)
    {
        //先尝试从字典中取出来
        if (GoodsIconsCache.TryGetValue(Id,out Sprite sprite))
        {
            return sprite;
        }
        //判断是否正在加载，避免重复加载
        if (LoadingHandles.TryGetValue(Id,out AsyncOperationHandle<Sprite> existingHandle))
        {
            await existingHandle.Task;
            return GoodsIconsCache.TryGetValue(Id, out Sprite sp) ? sp : null;
        }
        //构造资源地址
        string GoodsIconsAddress = $"{localGoodsIconsPrefix}{Id}";
        //异步加载资源
        AsyncOperationHandle<Sprite> handle=Addressables.LoadAssetAsync<Sprite>(GoodsIconsAddress);
        //加载字典标记为正在加载
        LoadingHandles.Add(Id,handle);

        try 
        {
            Sprite goodIcon = await handle.Task;
            if (goodIcon != null)
            {
                GoodsIconsCache.Add(Id, goodIcon);
                return goodIcon;
            }
            else
            {
                Debug.LogError($"Addressables 未找到 ID={Id} 的资源，地址：{GoodsIconsAddress}");
                return null;
            }
        }
        catch(Exception e)
        {
            // 捕获加载异常（如网络错误、资源损坏）
            Debug.LogError($"加载 ID={Id} 资源失败：{e.Message}");
            return null;
        }
        finally
        {
            //把异步操作从字典中移除
            LoadingHandles.Remove(Id);
            //手动释放资源,避免内存泄漏
            Addressables.Release(handle);
        }
    }

}
