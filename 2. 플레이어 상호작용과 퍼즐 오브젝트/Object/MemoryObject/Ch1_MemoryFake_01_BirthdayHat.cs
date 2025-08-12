using UnityEngine;

public class Ch1_MemoryFake_01_BirthdayHat : MemoryFragment
{
    private Animator anim;

    void Start()
    {
        isScannable = false;
        anim = GetComponentInChildren<Animator>();
    }

    public void ActivateHat()
    {
        isScannable = true;
        if (TryGetComponent(out UniqueId uid))
            SaveManager.SetMemoryFragmentScannable(uid.Id, isScannable);
    }

    public override void AfterScan()
    {
        TutorialManager.Instance.Show(TutorialStep.MemoryStorageGuide);
        
        base.AfterScan();
    }
}
