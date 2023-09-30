using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileNote : MonoBehaviour
{
    // Outlet
    Rigidbody2D _rb;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.velocity = transform.right * 10f * -1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
