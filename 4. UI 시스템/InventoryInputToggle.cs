using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryInputToggle : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            InventoryInputFocus.Toggle();
    }
}
