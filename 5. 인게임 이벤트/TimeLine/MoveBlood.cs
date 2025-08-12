using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class MoveBlood : MonoBehaviour
{
  

    public void Move()
    {


        transform.DOMoveY(transform.position.y-0.7f, 0f);
        
        
    }
}
