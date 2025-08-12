using UnityEngine;

public class Ch2_DoorLock : BaseInteractable
{   
    [SerializeField] private ItemData needItem; // 문을 여는데 필요한 아이템
    [SerializeField] private GameObject q_Key;
    [SerializeField] private CH2_SecurityGuard guard;
    [SerializeField] private GameObject closeDoorSprite;
    private bool doorOpenAble; // 문을 열 수 있는 영역에 있는지 확인
    private bool doorOpen; // 문을 열었는지 확인
    Inventory_PossessableObject inventory; // 빙의 인벤토리(needItem을 갖고 있는지 확인용)

    void Start()
    {
        doorOpenAble = false;
        doorOpen = false;
    }

    void OnEnable()
    {
        doorOpen = false;
        guard.isdoorLockOpen = doorOpen;
    }


    void Update()
    {   
        if (Input.GetKeyDown(KeyCode.Q))
        {   
            if(doorOpenAble && !doorOpen)
            {
                inventory = Inventory_PossessableObject.Instance;
                
                if(inventory == null || needItem != inventory.selectedItem())
                {
                    UIManager.Instance.PromptUI.ShowPrompt("문을 열 수 있는 카드키가 필요해");
                    return;
                }
                if(needItem == inventory.selectedItem() && needItem != null && doorOpenAble)
                {
                    // UIManager.Instance.PromptUI.ShowPrompt("문이 열렸습니다.", 1.5f);
                    q_Key.SetActive(false);
                    OpenDoorLock();
                    return;
                }
                return;
            }

        }
    }

    // 도어락 풀리고 문 열림 -> 도어락 오브젝트 숨김
    private void OpenDoorLock()
    {
        doorOpen = true;
        guard.isdoorLockOpen = doorOpen;
        q_Key.SetActive(false);
        inventory.TryUseSelectedItem();   
        closeDoorSprite.SetActive(false);
        this.gameObject.SetActive(false);
    }
    
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Person") || collision.CompareTag("Player"))
        {
            SetHighlight(true);
            q_Key.SetActive(true);
            doorOpenAble = true;
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Person") || collision.CompareTag("Player"))
        {
            SetHighlight(false);
            q_Key.SetActive(false);
            doorOpenAble = false;    
            PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
        }
    }


}
