using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TestLoad : MonoBehaviour
{
    private string path = "prefab/Mycube";
    private void OnGUI()
    {
        if (GUI.Button(new Rect(300,150,220,60),"同步加载文件夹资源"))
        {
           GameObject Mycube=ResourcesManager.Instance.Load(path)as GameObject;
            if (Mycube != null)
            {
                Instantiate(Mycube, Vector3.one, Quaternion.identity);
                return;
            }
            Debug.Log("路径不存在");
        }
        if (GUI.Button(new Rect(300, 250, 220, 60), "同步加载文件夹指定资源"))
        {
            Sprite MySprite = ResourcesManager.Instance.Load("Sprite/图片无法显示", typeof(Sprite))as Sprite;
            if (MySprite != null)
            {
                GameObject.Find("TestImage").GetComponent<Image>().sprite = MySprite;
                return;
            }
            Debug.Log("路径不存在");
        }
        if (GUI.Button(new Rect(300, 350, 220, 60), "同步加载文件夹指定资源"))
        {
            Sprite MySprite = ResourcesManager.Instance.Load<Sprite>("Sprite/图片无法显示");
            if (MySprite != null)
            {
                GameObject.Find("TestImage").GetComponent<Image>().sprite = MySprite;
                return;
            }
            Debug.Log("路径不存在");
        }
        if (GUI.Button(new Rect(300, 450, 220, 60), "同步加载指定文件夹所有资源"))
        {
            Object[] objects = ResourcesManager.Instance.LoadAll("prefab");
            if (objects != null)
            {
                foreach (var obj in objects)
                {
                    Instantiate(obj, Vector3.one, Quaternion.identity);
                }
                return;
            }
            Debug.Log("路径不存在");
        }
        if (GUI.Button(new Rect(300, 550, 220, 60), "同步加载指定文件夹所有指定资源"))
        {
            Object[] objects = ResourcesManager.Instance.LoadAll("prefab",typeof(GameObject));
            if (objects != null)
            {
                foreach (var obj in objects)
                {
                    Instantiate(obj, Vector3.zero, Quaternion.identity);
                }
                return;
            }
            Debug.Log("路径不存在");
        }
        if (GUI.Button(new Rect(300, 650, 220, 60), "异步加载单个资源"))
        {
            ResourcesManager.Instance.LoadAsync("prefab/MyCube", obj =>
             {
                 var Prefab = obj as GameObject;
                 if (Prefab == null) return;
                 Instantiate(Prefab, Vector3.one, Quaternion.identity);
                 Debug.Log("成功加载");
             });
        }
        if (GUI.Button(new Rect(300, 750, 220, 60), "异步加载指定类型资源"))
        {
            ResourcesManager.Instance.LoadAsync<GameObject>("prefab/MyCube", obj =>
            {
                if (obj == null) return;
                Instantiate(obj, Vector3.one, Quaternion.identity);
                Debug.Log("成功加载");
            });
            
        }
    }
}
