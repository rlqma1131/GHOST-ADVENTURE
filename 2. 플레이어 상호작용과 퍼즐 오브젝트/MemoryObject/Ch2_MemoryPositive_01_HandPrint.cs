using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_MemoryPositive_01_HandPrint : MemoryFragment
{
    public Ch2_ClearDoor exit;

    public void ActivateHandPrint()
    {
        isScannable = true;
        if (TryGetComponent(out UniqueId uid))
            SaveManager.SetMemoryFragmentScannable(uid.Id, isScannable);
    }

    public override void AfterScan() 
    {
        exit.ActivateClearDoor();

        base.AfterScan();
    }
}
