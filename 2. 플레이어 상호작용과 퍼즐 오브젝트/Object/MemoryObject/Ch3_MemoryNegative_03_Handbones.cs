using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch3_MemoryNegative_03_Handbones : MemoryFragment
{
    private bool colected = false;
    public bool Colected => colected;

    void Start()
    {
        isScannable = true;
    }

    public override void AfterScan()
    {
        colected = true;

        base.AfterScan();
    }
}
