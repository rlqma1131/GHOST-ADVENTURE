using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_MemoryNegative_01_CarToy : MemoryFragment
{
    private Animator anim;

    void Start()
    {
        isScannable = true;
        anim = GetComponentInChildren<Animator>();
    }

    public void ActivateCake()
    {
        isScannable = true;
        if (TryGetComponent(out UniqueId uid))
            SaveManager.SetMemoryFragmentScannable(uid.Id, isScannable);
    }
}
