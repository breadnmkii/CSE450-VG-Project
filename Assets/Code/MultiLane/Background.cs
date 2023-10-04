using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    Rigidbody2D _rb;
    public GameObject anchor;
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.velocity = Vector2.left * 4.5f;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("BG"))
        {
           Util.Move(gameObject, anchor);
        }

    }
}
