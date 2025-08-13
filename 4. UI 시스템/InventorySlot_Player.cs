using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

public class InventorySlot_Player : MonoBehaviour
{
    public Image icon;
    public Image clearSlotIcon;
    public int clueIndex;
    public TMP_Text keyText;
[Header("Dim Settings")]
    [Range(0f,1f)] public float dimAlpha = 0.6f; // 뒷면 투명도
    private float normalAlpha = 1f;
    // public TextMeshProUGUI clueName;

    public void Setup(ClueData clue)
    {
        icon.sprite = clue.clue_Icon;
        icon.enabled = true; // 아이콘 표시
        // clueName.text = clue.clue_Name;
    }

    internal void Clear()
    {
        icon.sprite = null;
        icon.enabled = false; // 아이콘 숨기기
    }


    private void Start()
    {
        // UpdateKeyText();
        icon.enabled = false;
        SetKeyVisible(true);
        SetDim(false);
    }

    public void SetKeyVisible(bool visible)
    {
        if (keyText != null) keyText.gameObject.SetActive(visible);
    }

    public void SetDim(bool dim)
    {
        if (icon == null) return;
        var c = icon.color;
        c.a = dim ? dimAlpha : normalAlpha;
        icon.color = c;

        // 빈 슬롯 배경도 같이 처리하고 싶으면
        if (clearSlotIcon != null)
        {
            var bc = clearSlotIcon.color;
            bc.a = dim ? dimAlpha : normalAlpha;
            clearSlotIcon.color = bc;
        }
    }

    public void UpdateKeyText()
    {
        if (keyText == null || UIManager.Instance.ESCMenuUI == null) return;
        KeyCode key = UIManager.Instance.ESCMenuUI.GetKey(clueIndex);
        keyText.text = ESCMenu.KeyNameHelper.GetDisplayName(key);
    }
}
