using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;
    //负数表容纳无数个
    public int capacity = -1;
    //未被使用的对象
    public List<GameObject> unUsedGameObjectList = new List<GameObject>();
    //已被使用的对象
    public List<GameObject> usedGameObjectList = new List<GameObject>();
    //所有对象的总量
    public int TotalGameObjectCount => unUsedGameObjectList.Count + usedGameObjectList.Count;
    /// <summary>
    /// 从对象池获取对象
    /// </summary>
    /// <param name="postion"></param>
    /// <param name="rotation"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public GameObject Spawn(Vector3 postion,Quaternion rotation,Transform parent = null)
    {
        GameObject go;
        
        if (unUsedGameObjectList.Count>0)
        {
            go = unUsedGameObjectList[0];
          
            unUsedGameObjectList.RemoveAt(0);
            usedGameObjectList.Add(go);
            go.transform.position = postion;
            go.transform.rotation = rotation;
            go.transform.SetParent(parent,false);
            go.SetActive(true);
        }
        else
        {
            go = Instantiate(prefab, postion,rotation, parent);
            usedGameObjectList.Add(go);
        }
        return go;
    }
    /// <summary>
    /// 从对象池回收对象
    /// </summary>
    /// <param name="go"></param>
    public void Despawn(GameObject go)
    {
        if (go == null) return;
        foreach (GameObject obj in usedGameObjectList)
        {
            if (obj==go)
            {
                if (capacity>=0&&usedGameObjectList.Count>=capacity)
                {
                    //因为容量满了，所以需要先销毁一个才能回收
                    if (unUsedGameObjectList.Count>0)
                    {
                        Destroy(unUsedGameObjectList[0]);
                        unUsedGameObjectList.RemoveAt(0);
                    }
                }
                
            }
            unUsedGameObjectList.Add(go);
            usedGameObjectList.RemoveAt(0);
            go.SetActive(false);
            go.transform.SetParent(transform, false);
            return;
        }
    }
    /// <summary>
    /// 回收所有对象
    /// </summary>
    public void DespawnAll()
    {
        //从后往前遍历性能更好，从前往后每removeAt一个后面的都需要向前移动一位
        for (int i=usedGameObjectList.Count-1;i>=0;i--)
        {
            Despawn(usedGameObjectList[i]);
        }
        usedGameObjectList.Clear();
    }
    /// <summary>
    /// 预先加载一定数量的对象
    /// </summary>
    /// <param name="amount"></param>
    public void PreLoad(int amount=1)
    {
        if (prefab == null) return;
        if (amount < 0) return;
        for (int i=0;i<amount;i++)
        {
            GameObject go = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            go.SetActive(false);
            go.transform.SetParent(transform, false);
            unUsedGameObjectList.Add(go);
            go.name = prefab.name;
        }
       
    }
}
