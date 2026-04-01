using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 对象池管理器
/// </summary>
public class ObjectPoolsManager : SingletonBase<ObjectPoolsManager>
{
    //所有对象池的父对象
    private GameObject PoolParent;
    private string PoolParentName="ObjectPools";
    //存放所有对象池的列表
    private List<ObjectPool> ObjectPoolsList = new List<ObjectPool>();
    //存放游戏对象和所属对象池
    private Dictionary<GameObject, ObjectPool> ObjectPoolsDic = new Dictionary<GameObject, ObjectPool>();
    public GameObject Spawn(GameObject prefab,Vector3 position,Quaternion rotation,Transform Parent=null)
    {
        
        if (prefab==null) return null;
        CreatPoolParentIfNull();
        //查找预制体属于哪个对象池
        ObjectPool objectPool=FindPoolByPrefabOrCreatPool(prefab);
        
        //从对应的对象池获取对象
        GameObject go = objectPool.Spawn(position,rotation,Parent);
       
        //把对象和对象池添加进入字典
        ObjectPoolsDic.Add(go, objectPool);
        return go;
    }
    /// <summary>
    /// 如果没有父亲就生成一个
    /// </summary>
    private void CreatPoolParentIfNull()
    {
        if (PoolParent==null)
        {
            //清空场景残留数据并创建父对象
            //如果为空说明是游戏开始或者切换场景父对象被销毁了
            ObjectPoolsList.Clear();
            ObjectPoolsDic.Clear();
            PoolParent = new GameObject("ObjectPools");
        }

    }
    /// <summary>
    /// 通过预制体查找所属对象池
    /// </summary>
    /// <returns></returns>
    private ObjectPool FindPoolByPrefab(GameObject prefab)
    {
        
        if (prefab == null) return null;
        for (int i=0;i<ObjectPoolsList.Count;i++)
        {
            if (ObjectPoolsList[i].prefab == prefab)
            {
                return ObjectPoolsList[i];
            }
        }
        return null;
    }
    /// <summary>
    /// 通过预制体查找对象池若没有就创建
    /// </summary>
    /// <returns></returns>
    private ObjectPool FindPoolByPrefabOrCreatPool(GameObject prefab)
    {
        CreatPoolParentIfNull();
        //判断对象池是否为空
        ObjectPool objectPool = FindPoolByPrefab(prefab);
        if (objectPool==null)
        {
            objectPool = new GameObject($"ObjectPool-{prefab.name}").AddComponent<ObjectPool>();
            objectPool.prefab = prefab;
            objectPool.transform.SetParent(PoolParent.transform);
            ObjectPoolsList.Add(objectPool);
        }
        return objectPool;

    }
    /// <summary>
    /// 在正在使用的对象中查找对象池
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    private ObjectPool FindPoolByUsedGameObj(GameObject go)
    {
        if (go == null) return null;
        foreach (var pool in ObjectPoolsList)
        {
            foreach (var obj in pool.usedGameObjectList)
            {
                if (go == obj) return pool;
            }
        }
        return null;
    }
    /// <summary>
    /// 预先加载一定数量的游戏对象
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="amount"></param>
    public void PreLoadPrefab(GameObject prefab,int amount)
    {
        if (prefab == null) return;
        if (amount <= 0) return;
        ObjectPool pool = FindPoolByPrefabOrCreatPool(prefab);
        
        pool.PreLoad(amount);
    }
    /// <summary>
    /// 回收游戏对象
    /// </summary>
    /// <param name="go"></param>
    /// <param name="Delaytime"></param>
    public void Despawn(GameObject go,float Delaytime)
    {
        if (go == null) return;
        MonoManager.Instance.Start_Coroutine(DespawnCoroutine(go, Delaytime));

    }
    /// <summary>
    /// 回收游戏对象的协程
    /// </summary>
    /// <param name="go"></param>
    /// <param name="Delaytime"></param>
    /// <returns></returns>
    IEnumerator DespawnCoroutine(GameObject go,float Delaytime)
    {
        if (Delaytime > 0) yield return new WaitForSeconds(Delaytime);
        if (ObjectPoolsDic.TryGetValue(go,out ObjectPool pool))
        {
            ObjectPoolsDic.Remove(go);
            pool.Despawn(go);
        }
        else
        {
            //在字典中如果没有找到就去正在使用的对象中查找
            pool = FindPoolByUsedGameObj(go);
            if(pool!=null)
            pool.Despawn(go);
            Debug.Log("字典中找不到");
        }
    }
}
