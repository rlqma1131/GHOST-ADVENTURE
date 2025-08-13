// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// public enum InvSide { Player, Possess }
// public class InventoryInputManager : MonoBehaviour
// {
//     private Inventory_Player playerInventory;
//     private Inventory_PossessableObject possessableInventory;

//     private bool isPossessableInventoryActive = false;

//     void Start()
//     {
//         playerInventory = UIManager.Instance.Inventory_PlayerUI;
//         possessableInventory = UIManager.Instance.Inventory_PossessableObjectUI;
//     }
//     void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.Tab))
//         {
//             if(possessableInventory == null) return;

//             isPossessableInventoryActive = !isPossessableInventoryActive;
//             Debug.Log("빙의 인벤토리 제어 상태: " + isPossessableInventoryActive);
//             // if (!isPossessableInventoryActive)
//             // possessableInventory.ClearSelectedSlot(); // 선택 해제
//             // possessableInventory.ClearAllSlotHighlights();
//         }

//         // 입력 처리는 빙의 인벤토리 활성 상태일 때만
//         if (!isPossessableInventoryActive) return;

//         if (Input.GetKeyDown(KeyCode.Alpha1)) possessableInventory.SelectSlot(0);
//         if (Input.GetKeyDown(KeyCode.Alpha2)) possessableInventory.SelectSlot(1);
//         if (Input.GetKeyDown(KeyCode.Alpha3)) possessableInventory.SelectSlot(2);
//         if (Input.GetKeyDown(KeyCode.Alpha4)) possessableInventory.SelectSlot(3);
//         if (Input.GetKeyDown(KeyCode.Q)) possessableInventory.TryUseSelectedItem();
//     }
// }
