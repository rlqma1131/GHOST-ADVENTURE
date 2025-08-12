using UnityEngine;

public static class Ch2_SwichboardPuzzleSolver
{
    public static bool IsPathConnected(Ch2_SwitchboardButton[] buttons)
    {
        // 0번 버튼에서 시작
        foreach (var button in buttons)
        {
            if (IsConnectedToStart(button))
            {
                bool[] visited = new bool[buttons.Length];
                DFS(buttons, System.Array.IndexOf(buttons, button), visited);
                if (AllVisited(visited) && EndVisited(buttons, visited))
                    return true;
            }
        }
        return false;
    }

    // 모든 버튼이 방문되었는지 체크
    static bool AllVisited(bool[] visited)
    {
        foreach (var v in visited)
            if (!v) return false;
        return true;
    }

    // 끝(5번)이 방문됐는지 체크
    static bool EndVisited(Ch2_SwitchboardButton[] buttons, bool[] visited)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].id == 5 && visited[i])
            {
                // 5번 오른쪽이 열려 있는지 확인
                return buttons[i].connection.right;
            }
        }
        return false;
    }

    // 깊이우선탐색(연결된 버튼을 따라가면서 visited에 체크)
    static void DFS(Ch2_SwitchboardButton[] buttons, int index, bool[] visited)
    {
        if (visited[index]) return;
        visited[index] = true;
        for (int i = 0; i < buttons.Length; i++)
        {
            if (visited[i]) continue;
            if (IsConnected(buttons[index], buttons[i]))
                DFS(buttons, i, visited);
                Debug.Log($"Connected: {buttons[index].id}({buttons[index].gridPosition}) -> {buttons[i].id}({buttons[i].gridPosition})");
        }
    }

    // 인접 + 방향이 모두 맞으면 연결된 것으로 판정
    static bool IsConnected(Ch2_SwitchboardButton a, Ch2_SwitchboardButton b)
    {
        Vector2Int diff = b.gridPosition - a.gridPosition;
        if (diff == new Vector2Int(0, 1) && a.connection.right && b.connection.left) return true; // b가 a의 오른쪽
        if (diff == new Vector2Int(0, -1) && a.connection.left && b.connection.right) return true; // b가 a의 왼쪽
        if (diff == new Vector2Int(1, 0) && a.connection.bottom && b.connection.top) return true; // b가 a의 아래쪽
        if (diff == new Vector2Int(-1, 0) && a.connection.top && b.connection.bottom) return true; // b가 a의 위쪽
        return false;
    }


    // 시작 조건: id==0, left가 열려 있으면 시작
    static bool IsConnectedToStart(Ch2_SwitchboardButton button)
    {
        return button.id == 0 && button.connection.left;
    }
}
