using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMain : MonoBehaviour
{
    PlayerController playerCtrl;
    private bool jumpLock = false;
    private bool attackLock = false;
    private void Awake()
    {
        playerCtrl = GetComponent<PlayerController>();
    }
    
    void FixedUpdate()
    {
        int horizontal;
        int vertical;

        if (!playerCtrl.active)
            return;

        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");
        playerCtrl.ActionMove(horizontal);

        if (Input.GetButton("Jump") && !jumpLock)
        {
            playerCtrl.ActionJump(vertical);
            jumpLock = true;
        }

        if (Input.GetKeyDown("f") && !attackLock)
        {
            playerCtrl.ActionAttack();
            attackLock = true;
        }
    }

    private void Update()
    {
        if (Input.GetButtonUp("Jump"))
            jumpLock = false;
        if (Input.GetKeyUp("f"))
            attackLock = false;
    }
}
