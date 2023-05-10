using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartAnimationController : MonoBehaviour
{
    // Start is called before the first frame update
    public Animator partAnimator;

    public bool isFlyer = false;
    public bool flying = false;
    public Coroutine floatCoroutine;
    void Start()
    {
        partAnimator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Fly()
    {
        partAnimator.SetBool("Flying", true);
        floatCoroutine = StartCoroutine(AnimationManager.FloatUpAndDown(transform.parent.gameObject, 1, 2));
        flying = true;
    }

    public void StopFlying()
    {
        partAnimator.SetBool("Flying", false);
        AnimationManager.abortFloatUpAndDown = true;
        flying = false;
    }

}
