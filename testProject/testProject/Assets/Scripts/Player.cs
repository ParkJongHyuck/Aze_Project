using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 1px = 0.01m
100px = 1m
70px = 1sec

x km/h = x * 1000 / 3600 * PIXELPERMETER * deltaTime

*/

public class Player : MonoBehaviour
{ 
    private float KPH = 20;
    private float MPP;        // 70pixel per 1 sec, 
    public LayerMask blockingLayer;
    private Rigidbody2D rb2D;
    private RaycastHit2D hit;
    private BoxCollider2D boxCollider;
    private Vector3 moveMent;

    private bool jumpLock;
    private bool platformActive;
    private int doubleJump;
    private float gravityForce = 1.3f;
    private float jumpForce = 20f;

    private void Awake()
    {
        jumpLock = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        MPP = KPH * 1000 / 3600;
        moveMent.y = 0f;
    }

    // Update is called once per frame
    private void Update()
    {
        bool jumpButton;
        jumpButton = Input.GetButtonUp("Jump");
        if (jumpButton)
        {
            jumpLock = true;
        }

    }
    void FixedUpdate()
    {
        int horizontal = 0;
        bool jumpButton;

        jumpButton = Input.GetButton("Jump");
        horizontal = (int)Input.GetAxisRaw("Horizontal");
        
        if (horizontal != 0)
            Run(horizontal,0);

        if (jumpButton && jumpLock && doubleJump < 2)
            Jump();
        

        Move();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "tileMap")
            platformActive = true;
        else
            platformActive = false;

        if (collision.tag == "Platform")
        {
            if (transform.position.y - collision.transform.position.y > 1)
                platformActive = true;
            else
                platformActive = false;
            
        }
    }

    private bool CollideCheck(Vector2 start, Vector2 end, out RaycastHit2D hit)
    {
        boxCollider.enabled = false;
        hit = Physics2D.Linecast(start, end, blockingLayer);
        boxCollider.enabled = true;

        if (hit.transform == null)
            return true;

        return false;

    }
    private void Jump()
    {
        jumpLock = false;
        moveMent.y = 0;
        moveMent += new Vector3(0f, jumpForce * Time.deltaTime, 0f);
        ++doubleJump;
    }

    private void Gravity()
    {
        if (moveMent.y < -0.2f)
        {
            moveMent.y = -0.3f;
        }
        else
            moveMent -= new Vector3(0f, gravityForce * Time.deltaTime, 0f);
    }

    private void Move()
    {
        Vector2 start = rb2D.position;
        Vector2 end = rb2D.position + new Vector2(0f, -0.9f + moveMent.y);

        transform.position += moveMent;

        if (platformActive)
        {
            if (CollideCheck(start, end, out hit))
                Gravity();
            else
            {
                moveMent.y = 0f;
                doubleJump = 0;
            }
        }
        else
            Gravity();

    }

    private void Run(int xDir,int yDir)
    {
        Vector2 start = rb2D.position;
        Vector2 end = rb2D.position + new Vector2(xDir * 0.75f, -0.5f);       // Collider Box

        if (CollideCheck(start, end, out hit))
            transform.position += new Vector3(xDir * MPP * Time.deltaTime, 0f);
    }
    
    
}
