using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 继承MonoBehaviour的单例基类
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonBase<T> : MonoBehaviour where T:MonoBehaviour
{
    //构造函数私有化，防止外部new对象
    protected SingletonBase() { }
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance==null)
            {
                //在场景中查找是否有T类,有的话赋值给单例
                instance = FindObjectOfType<T>();
                if (instance==null)
                {
                    GameObject obj = new GameObject(typeof(T).Name);
                    instance=obj.AddComponent<T>();
                }
            }
            return instance;
        }
    }
}
