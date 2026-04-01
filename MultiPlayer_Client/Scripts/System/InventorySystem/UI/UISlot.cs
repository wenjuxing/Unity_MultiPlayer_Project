using GameClient;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISlot : MonoBehaviour,IItemBase<Item>
{
    private ItemUI _itemUI;
    public Button btn;
    //插槽的索引
    public int Index { get; set; }
    public int InitIndex { get; set; }
    public ItemUI ItemUI 
    {
        get
        {
           return transform.GetComponentInChildren<ItemUI>();
        }
        set
        {
            value.transform.SetParent(transform);
            value.transform.position = transform.position;

            RectTransform rt = value.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f); // 同时重置轴心点
                rt.anchoredPosition = Vector2.zero; // 相对父物体位置归零
            }
        }
    }

    public void InitInfo(Item item)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            Kaiyun.Event.FireIn("ShowItemDetails",item);
        });
        //清空插槽
        if (item == null)
        {
            if (_itemUI != null && _itemUI.gameObject != null)
            {
                Destroy(_itemUI.gameObject);
                _itemUI = null;
            }
            return;
        }
        //设置插槽
        if (transform.GetComponentInChildren<ItemUI>() == null)
        {
            var prefab = Resources.Load<GameObject>("Prefabs/UI/GoodsItem");
            _itemUI = Instantiate(prefab, transform).GetComponent<ItemUI>();
            _itemUI.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _itemUI.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _itemUI.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _itemUI.rectTransform.anchoredPosition = Vector2.zero;
            _itemUI.Item = item;        }
        else
        {
            _itemUI = transform.GetComponentInChildren<ItemUI>();
            _itemUI.Item = item;
        }
    }
}
