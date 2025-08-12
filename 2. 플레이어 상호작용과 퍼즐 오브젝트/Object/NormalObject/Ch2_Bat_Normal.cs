using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_Bat_Normal : MonoBehaviour
{
    Animator ani;

    void Start()
    {
        ani = GetComponent<Animator>();
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        // ani.SetTrigger("Fly");
    }
}
