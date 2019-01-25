using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : BaseCharacterController
{
    public float initHpMax = 20.0f;
    public float initSpeed = 30.0f;
    public int jumpCount = 0;
    public LayerMask mask;
    public bool blocking = false;

    protected override void Awake()
    {
        base.Awake();
        blockingLayer = mask;
        SetSpeed(initSpeed);
       
    }

    protected override void FixedUpdateCharacter()
    {
        base.FixedUpdateCharacter();

        if (jumped)
        {
            if (grounded)
            {
                jumped = false;
                jumpCount = 0;
            }
        }
        else if (!jumped)
            jumpCount = 0;

        transform.localScale = new Vector3(baseScaleX * dir, transform.localScale.y, transform.localScale.z);

        Camera.main.transform.position = transform.position - Vector3.forward;
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Road")
            blocking = true;
        else
            blocking = false;
    }

    public override void ActionMove(float n)
    {
        if (!active)
            return;

        if (n != 0f)
        {
            dir = (int)Mathf.Sign(n);

        }
        base.ActionMove(n);
    }

    public override void ActionJump(int vertical, int count = 0)
    {
        base.ActionJump(vertical, jumpCount);
        ++jumpCount;
    }

    public void ActionAttack()
    {
        if(grounded)
            animator.SetTrigger("Attack_A");
    }

}
