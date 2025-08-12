// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class Ch2_Bench : MonoBehaviour
// {
//     Person targetPerson;
//     void OnTriggerEnter2D(Collider2D collision)
//     {   
//         if(collision.CompareTag("Person"))
//         {
//             Person.currentCondition = PersonCondition.Vital;
//             collision.GetComponent<CH2_SecurityGuard>().SetCondition(PersonCondition.Vital);
//             Debug.Log("현재컨디션" + Person.currentCondition);
//         }
//     }

//     void OnTriggerExit2D(Collider2D collision)
//     {
//         if(collision.CompareTag("Person"))
//         {
//             Person.currentCondition = PersonCondition.Tired;
//             collision.GetComponent<CH2_SecurityGuard>().SetCondition(PersonCondition.Tired);
//             Debug.Log("현재컨디션" + Person.currentCondition);
//         }

//     }
// }
