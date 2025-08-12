using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch3_MemoryPositive_Mirror : MemoryFragment
{
    private Ch3_Scanner scanner;

    void Start()
    {
        scanner = FindObjectOfType<Ch3_Scanner>();
    }

    public void ActivateObj()
    {
        isScannable = true;
        if (TryGetComponent(out UniqueId uid))
            SaveManager.SetMemoryFragmentScannable(uid.Id, isScannable);
    }

    public override void AfterScan()
    {
        scanner.ActiveScanner();

        base.AfterScan();
    }
}
