﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animcontroller_lipsmack : MonoBehaviour
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
        if (Input.GetKeyDown("2"))
        {
            anim.CrossFade("lip-smack-2", 0.25f);
        }

    }
}
