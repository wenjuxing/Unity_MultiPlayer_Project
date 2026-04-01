using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolController : MonoBehaviour
{
    [SerializeField] private float Force = 20f;
    private GameObject prePrafab;
    private void Awake()
    {
        GameObject prePrafab = ResourcesManager.Instance.Load<GameObject>("prefab/Sphere");
        ObjectPoolsManager.Instance.PreLoadPrefab(prePrafab, 50);
    }
    private void OnGUI()
    {
        if (GUI.Button(new Rect(700,150,220,60),"卸载资源"))
        {
            //只有当函数正确执行才会回调
            ResourcesManager.Instance.UnLoadUnusedAssets(()=> { Debug.Log("资源卸载成功!"); });
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            GameObject prefab = ResourcesManager.Instance.Load<GameObject>("prefab/Sphere");
            GameObject go = ObjectPoolsManager.Instance.Spawn(prefab, Vector3.zero, Quaternion.identity);
            go.AddComponent<Rigidbody>().AddForce(go.transform.forward * Force);
            go.GetComponent<Rigidbody>().useGravity = false;
            ObjectPoolsManager.Instance.Despawn(go,2f);
        }
        
    }
}
