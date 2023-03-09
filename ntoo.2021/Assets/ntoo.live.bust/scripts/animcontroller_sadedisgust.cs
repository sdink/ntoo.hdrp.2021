using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animcontroller_sadedisgust : MonoBehaviour
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
        if (Input.GetKeyDown("6"))
        {
            anim.CrossFade("sade-disgust-6", 0.25f);
        }

    }
}
