using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Ch2_SwitchboardSlotConnectionData
{
    public struct SlotConnection
    {
        public bool top, bottom, left, right;
    }


    private static Dictionary<int, SlotConnection[]> map = new Dictionary<int, SlotConnection[]>
    {
        { 0, new SlotConnection[]
            {
                new SlotConnection { left = true, right = true },                          // 0도: 없음
                new SlotConnection { right = true, bottom = true },                          // 90도: 없음
                new SlotConnection { left = true, bottom = true, right = true },// 180도
                new SlotConnection { left = true, bottom = true } // 270도
            }
        },
        { 1, new SlotConnection[]
            {
                new SlotConnection { left = true },                          // 0도
                new SlotConnection { right = true },                          // 90도
                new SlotConnection { bottom = true, right = true },// 180도
                new SlotConnection { left = true, bottom = true }                           // 270도
            }
        },
        { 2, new SlotConnection[]
            {
                new SlotConnection { left = true },                          // 0도
                new SlotConnection { },                          // 90도
                new SlotConnection { bottom = true },                          // 180도
                new SlotConnection { left = true, bottom = true } // 270도
            }
        },
        { 3, new SlotConnection[]
            {
                new SlotConnection { right = true },                          // 0도
                new SlotConnection { top = true },                          // 90도
                new SlotConnection { top = true, right = true },// 180도
                new SlotConnection { top = true, right = true  } // 270도
            }
        },
        { 4, new SlotConnection[]
            {
                new SlotConnection { left = true, top = true },// 0도
                new SlotConnection { left = true, top = true , right = true },// 90도
                new SlotConnection { top = true, right = true },                          // 180도
                new SlotConnection { left = true, right = true }                           // 270도
            }
        },
        { 5, new SlotConnection[]
            {
                new SlotConnection { left = true },                          // 0도
                new SlotConnection { left = true, top = true },                          // 90도
                new SlotConnection { top = true, right = true },// 180도
                new SlotConnection { right = true }                           // 270도
            }
        },
    };

    public static SlotConnection GetConnectionFor(int id, int rotation)
    {
        int index = rotation / 90;
        return map[id][index];
    }
}



