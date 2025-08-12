using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_SwitchboardPuzzleManager : MonoBehaviour
{
    public Ch2_SwitchboardButton[] pieces; // 0 ~ 5

    private Ch2_Switchboard switchboard;

    public bool CanControl { get; private set; } = false;

    private void Start()
    {
        switchboard = GetComponentInParent<Ch2_Switchboard>();
    }
    public void EnablePuzzleControl()
    {
        CanControl = true;
    }

    public void DisablePuzzleControl()
    {
        CanControl = false;
    }

    public void CheckSolution()
    {
        if (Ch2_SwichboardPuzzleSolver.IsPathConnected(pieces))
        {
            DisablePuzzleControl();

            switchboard.SolvedPuzzle();
        }
    }
}
