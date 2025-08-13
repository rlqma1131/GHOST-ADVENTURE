using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

    [System.Serializable]

public class InventorySlot_PossessableObject : MonoBehaviour
{
    public ItemData item;
    public Image iconImage;
    public int quantity;
    public TMP_Text keyText_PO;

        public void SetKeyVisible(bool on)
    {
        if (keyText_PO) keyText_PO.gameObject.SetActive(on);
    }

    public InventorySlot_PossessableObject(ItemData item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }

    public void SetSlot(InventorySlot_PossessableObject slot)
    {
        item = slot.item;
        iconImage.sprite = slot.item.Item_Icon;
    }  

    // 아이템 사용 - 수량감소
    public void UseItem(int amount)
    {
        quantity -= amount;
        if (quantity < 0) quantity = 0;

        if (IsEmpty())
        {
            SetEmpty(); // 슬롯을 비우는 처리
        }
    }

    // 수량이 0인지 확인
    public bool IsEmpty()
    {
        return quantity <= 0;
    }

    public void SetEmpty()
    {
        item = null;
        quantity = 0;
        
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }


}

