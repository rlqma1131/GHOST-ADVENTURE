using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InvSide { Player, Possess }

public static class InventoryInputFocus
{
    public static InvSide Current = InvSide.Player; // 기본: 플레이어
    public static void Toggle()
    {
        Current = (Current == InvSide.Player) ? InvSide.Possess : InvSide.Player;
        // TODO: 여기서 UI 하이라이트/디밍 갱신 호출해도 됨
    }
}
