using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Inventory_PossessableObject : MonoBehaviour
{
    // 싱글톤
    public static Inventory_PossessableObject Instance { get; private set; }

    public Transform slotParent;                                    // 슬롯이 생성될 위치
    public GameObject slotPrefab;                                   // 슬롯 프리팹
    private List<GameObject> spawnedSlots = new List<GameObject>(); // 생성된 슬롯들
    public InventorySlot_PossessableObject selectedSlot = null;     // 선택된 슬롯
    private int selectedSlotIndex = -1;                             // 선택된 슬롯 인덱스
    private Inventory_Player inventory_Player;                      // 인벤토리 - 플레이어
    private HaveItem haveItem;

    void Start()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        inventory_Player = FindObjectOfType<Inventory_Player>();
    }



     private void Update()
    {
        if (InventoryInputFocus.Current != InvSide.Possess) return;

        // 비활성/빈 인벤이면 무시
        if (!gameObject.activeSelf || spawnedSlots.Count == 0) return;

        for (int i = 0; i < 4; i++)
        {
            
            var alpha = KeyCode.Alpha1 + i;
            var keypad = KeyCode.Keypad1 + i;
            if (Input.GetKeyDown(alpha) || Input.GetKeyDown(keypad))
            {
                if (Inventory_Player.FocusIsPlayer) return;
                SelectSlot(i);
                break;
            }
        }
    }
    
    // slotPrefab을 slotParent에 생성하고 spawnedSlots에 추가함
    public void ShowInventory(List<InventorySlot_PossessableObject> slots)
    {
        Clear();

        for (int i = 0; i < slots.Count; i++)
        {
            GameObject obj = Instantiate(slotPrefab, slotParent);
            var slotComponent = obj.GetComponent<InventorySlot_PossessableObject>();
            slotComponent.SetSlot(slots[i]);

            int keyNumber = 1 + i;
            if (slotComponent.keyText_PO != null)
            {
                slotComponent.keyText_PO.text = keyNumber.ToString();
            }

            spawnedSlots.Add(obj);
            
        }
        SetKeyLabelsVisible(!Inventory_Player.FocusIsPlayer);
        gameObject.SetActive(true);   
    }

    public void OpenInventory(BasePossessable target)
    {
        haveItem = target.GetComponent<HaveItem>();
        if (haveItem != null)
        {
            ShowInventory(haveItem.inventorySlots);
            InventoryInputFocus.Current = InvSide.Possess;
            return;
        }
        HideInventory();
    }

    public void HideInventory()
    {
        Clear();
        gameObject.SetActive(false);
        InventoryInputFocus.Current = InvSide.Player; 
    }

    // 슬롯을 삭제. 슬롯리스트 안의 데이터도 삭제.
    public void Clear()
    {
        foreach (var slot in spawnedSlots)
        {
            Destroy(slot);
        }
        spawnedSlots.Clear();
    }

    public void UseOrPlaceItem(ItemData item)
    {
        if(item == null)
        {
            Debug.Log("item이 null임");
            return;
        }
        if (item.Item_Type == ItemType.Consumable)
        {
            // 아이템 사용 로직
            Debug.Log($"사용: {item.Item_Name}");
            UseItem(item, 1);
        }
        if(item.Item_Type == ItemType.Clue)
        {
            UIManager.Instance.InventoryExpandViewerUI.ShowClue(item.clue);
            inventory_Player.AddClue(item.clue);
            UseItem(item, 1);
        }
    }
    
    public void SelectSlot(int index)
    {
        if (selectedSlotIndex == index)
        {
            selectedSlotIndex = -1;
            selectedSlot = null;
            HighlightSlot(-1); // 전체 선택 해제
            Debug.Log("슬롯 선택 해제됨");
            return;
        }

        if (index >= 0 && index < spawnedSlots.Count)
        {
            selectedSlotIndex = index;
            selectedSlot = spawnedSlots[index].GetComponent<InventorySlot_PossessableObject>();

            // 선택된 슬롯 시각적 강조 (옵션)
            HighlightSlot(index);
        }
        else return;

        if(selectedSlot.item != null && selectedSlot.item.Item_Type == ItemType.Clue)
        {
            UseOrPlaceItem(selectedSlot.item);
        }
    }

    private void HighlightSlot(int index)
    {
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            GameObject slotObj = spawnedSlots[i];
            Outline outline = slotObj.GetComponent<Outline>();

            if (outline != null)
            {
                outline.enabled = (i == index);
            }
        }
    }
    public void TryUseSelectedItem()
    {
        if (selectedSlot == null || selectedSlot.item == null) return;

        if (CanUseItem(selectedSlot.item))
        {
            UseOrPlaceItem(selectedSlot.item);
        }
        else
        {
            Debug.Log("사용 조건이 충족되지 않음");
        }
    }

    private bool CanUseItem(ItemData item)
    {
        if (item == null) return false;

        if (item.Item_Type == ItemType.Consumable)
        {
            // return IsNearUsableTarget(); // 예: 문, 상자, 장치 등
        }
        return true;
    }

    public void UseItem(ItemData item, int amount)
    {
        InventorySlot_PossessableObject slot = spawnedSlots
            .ConvertAll(s => s.GetComponent<InventorySlot_PossessableObject>())
            .Find(s => s.item == item);

        if (slot != null)
        {
            slot.UseItem(amount);

            if (slot.IsEmpty())
            {
                haveItem.inventorySlots.RemoveAll(s => s.item == item);
                Debug.Log("해브아이템" + haveItem.inventorySlots.Count);
            }
        }

        // 아이템 사용 시 상태 저장 (기록)
    }

    public void SetKeyLabelsVisible(bool on)
    {
        foreach (var obj in spawnedSlots)
        {
            var slot = obj.GetComponent<InventorySlot_PossessableObject>();
            if (slot) slot.SetKeyVisible(on);
        }
    }
    public ItemData selectedItem() => selectedSlot != null ? selectedSlot.item : null;

}
