using UnityEngine;

public class Ch2_Bag : BasePossessable
{
    [SerializeField] private GameObject q_Key;
    [SerializeField] private GameObject bagClosed;
    [SerializeField] private GameObject bagOpen;
    [SerializeField] private GameObject drawingClue1;
    private Animator clue1Anim;

    protected override void Start()
    {
        base.Start();
        if(drawingClue1 != null)
            clue1Anim = drawingClue1.GetComponent<Animator>();
    }

    protected override void Update()
    {
        base.Update();
        
        if (!isPossessed || !hasActivated)
        {
            q_Key.SetActive(false);
            return;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            q_Key.SetActive(false);
            ShowClue1();
            Unpossess();
        }
        
        q_Key.SetActive(true);
    }

    private void ShowClue1()
    {
        bagClosed.SetActive(false);
        bagOpen.SetActive(true);
        drawingClue1.SetActive(true);
        clue1Anim.SetTrigger("Drop");

        hasActivated = false;
        MarkActivatedChanged();
    }
}
