using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_MemoryFake_02_CCTV : MemoryFragment
{
    [SerializeField] private Ch2_CCTVMonitor cctvMonitor;

    void Start()
    {
        isScannable = true;
    }

    public override void AfterScan()
    {
        cctvMonitor.ActivateCCTVMonitor();

        base.AfterScan();
    }
}
