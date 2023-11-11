using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Title : MonoBehaviour
{
    public void FinshComeIn()
    {
        Animator ani = GetComponent<Animator>();

        ani.SetBool("ComeIn", true);
    }
}
