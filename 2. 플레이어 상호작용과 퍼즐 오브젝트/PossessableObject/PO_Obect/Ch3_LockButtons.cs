using UnityEngine;
using UnityEngine.UI;

public class Ch3_LockButtons : MonoBehaviour
{
    [SerializeField] private GameObject highlight;
    [SerializeField] private Ch3_Lock parent;
    [SerializeField] private Ch3_Lock.ButtonType type;
    [SerializeField] private int numIndex = -1;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
        if (highlight != null) highlight.SetActive(false);
    }

    void OnClick()
    {
        parent.ClearAllHighlights();

        if (highlight != null)
            highlight.SetActive(true);

        parent.OnButtonClicked(type, numIndex);
    }

    public void DisableHighlight()
    {
        if (highlight != null)
            highlight.SetActive(false);
    }
}
