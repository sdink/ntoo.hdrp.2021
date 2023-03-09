using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animcontroller_sadesad : MonoBehaviour
{
    public Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("e"))
        {
            anim.CrossFade("sade-sad-E", 0.25f);
        }

    }
}
