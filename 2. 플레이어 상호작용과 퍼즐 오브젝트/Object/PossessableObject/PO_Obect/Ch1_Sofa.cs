using UnityEngine;

public class Ch1_Sofa : HideArea
{
    protected override void Start()
    {
        isPossessed = false;
        hasActivated = false;
    }

    protected override void Update()
    {
        if (!isPossessed)
            return;

        InteractTutorial();

        if (Input.GetKeyDown(KeyCode.E))
        {
            isHiding = false;
            Unpossess();
        }
    }
    private void InteractTutorial()
    {
        TutorialManager.Instance.Show(TutorialStep.HideArea_Interact);
    }
    
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if(collision.CompareTag("Player") && !SaveManager.IsPuzzleSolved("시계"))
        {
            hasActivated = false;

        }
        if(collision.CompareTag("Player")  && SaveManager.IsPuzzleSolved("시계"))
        {
            hasActivated = true;
            MarkActivatedChanged();
        }

    }
}
