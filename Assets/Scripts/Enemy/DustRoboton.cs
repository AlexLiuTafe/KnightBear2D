using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustRoboton : MonoBehaviour
{
    public Animator anim;

    private Vector2 direction;
    private Transform target;
    private SpriteRenderer rend;
    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        anim = GetComponent<Animator>();
        rend = GetComponent<SpriteRenderer>();
        
    }
    // Update is called once per frame
    void Update()
    {
        if (target)
        {
            direction = (target.transform.position - transform.position).normalized;
 
            if (direction != Vector2.zero)
            {

                anim.SetFloat("Horizontal", direction.x);
                anim.SetFloat("Vertical", direction.y);
               
            }
            else
            {
                anim.SetBool("Idle", true);
            }
        }
    }
}

