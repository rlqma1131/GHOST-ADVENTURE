using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_SwitchboardButton : MonoBehaviour
{
    private int currentRotation = 0;
    private Ch2_SwitchboardPuzzleManager puzzleManager;

    public int id; // 0 ~ 5
    public Vector2Int gridPosition; // (0,0) ~ (1,2)
    public Ch2_SwitchboardSlotConnectionData.SlotConnection connection;

    private void Start()
    {
        puzzleManager = GetComponentInParent<Ch2_SwitchboardPuzzleManager>();

        connection = Ch2_SwitchboardSlotConnectionData.GetConnectionFor(id, 0);
    }

    private void OnMouseDown()
    {
        if (!puzzleManager.CanControl) return;

        RotateButton();
        Debug.Log($"{name}: id={id}, grid={gridPosition}, rotation={currentRotation}, conn(L:{connection.left},R:{connection.right},T:{connection.top},B:{connection.bottom})");
    }

    private void RotateButton()
    {
        currentRotation = (currentRotation + 90) % 360;
        transform.rotation = Quaternion.Euler(0, 0, -currentRotation);

        // 현재 이 버튼의 회전 상태에 따른 연결 정보 갱신
        connection = Ch2_SwitchboardSlotConnectionData.GetConnectionFor(id, currentRotation);

        // 퍼즐 정답 체크
        puzzleManager.CheckSolution();

        Debug.Log($"[{id}] 회전 완료 → 회전: {currentRotation}, 연결 상태 R:{connection.right}");
    }
}
