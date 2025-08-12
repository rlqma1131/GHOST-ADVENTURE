using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch1_Mousehole : MonoBehaviour
{
    [SerializeField] Ch1_Mouse mouse;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Animal"))
            mouse.canEnter = true; 
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Animal"))
            mouse.canEnter = false;
    }
}
