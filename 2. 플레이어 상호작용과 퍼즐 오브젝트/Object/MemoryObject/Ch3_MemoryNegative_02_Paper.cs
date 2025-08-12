using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch3_MemoryNegative_02_Paper : MemoryFragment
{
    private bool colected = false;
    public bool Colected => colected;

    public void ActivatePaper()
    {
        isScannable = true;
        if (TryGetComponent(out UniqueId uid))
            SaveManager.SetMemoryFragmentScannable(uid.Id, isScannable);
    }

    public override void AfterScan()
    {
        colected = true;

        base.AfterScan();
    }
}
