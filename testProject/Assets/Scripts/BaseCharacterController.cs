using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCharacterController : MonoBehaviour
{
    [System.NonSerialized] public float hpMax = 100f;
    [System.NonSerialized] public float hp = 100f;
    [System.NonSerialized] public int dir = 1;
    [System.NonSerialized] public float baseScaleX = 1.0f;
    [System.NonSerialized] public float KPH = 20.0f;
    [System.NonSerialized] public float MPP;
    [System.NonSerialized] public float gravityForce = 0.03f;
    [System.NonSerialized] public float jumpForce = 0.5f;
    [System.NonSerialized] public bool jumped = false;
    [System.NonSerialized] public bool active = false;
    [System.NonSerialized] public bool grounded = false;
    [System.NonSerialized] public bool groundedPrev = false;

    [System.NonSerialized] public Animator animator;

    protected Transform groundCheck_L;
    protected Transform groundCheck_C;
    protected Transform groundCheck_R;
    protected Transform groundCheck_A;
    protected Collider2D boxCollider;
    protected Collider2D circleCollider;
    protected GameObject groundCheck_OnRoadObject;
    protected Rigidbody2D rb2D;
    protected LayerMask blockingLayer;
    protected RaycastHit2D hit;
    protected Vector3 movementV;

    private bool adjustment = false;
    private bool downJump = false;
    private bool running = false;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        groundCheck_L = transform.Find("GroundCheck_L");
        groundCheck_C = transform.Find("GroundCheck_C");
        groundCheck_R = transform.Find("GroundCheck_R");
        groundCheck_A = transform.Find("GroundCheck_A");
        
        dir = (transform.localScale.x > float.Epsilon) ? 1 : -1;
        baseScaleX = transform.localScale.x * dir;
        transform.localScale = new Vector3(baseScaleX, transform.localScale.y, transform.localScale.z);
        active = true;
        MPP = KPH * 1000 / 3600;  //1meter per 100pixel
    }

    protected virtual void FixedUpdate()
    {
        groundedPrev = grounded;
        grounded = false;

        ColliderCheck();
        Landing();

        FixedUpdateCharacter();
        animatorSetting();

    }

    private void Landing()        //수직 이동 + 중력 + 착지. 자연스러운 착지를 위한 임의의 조정.
    {
        Collider2D[] groundAdjust = new Collider2D[1];
        groundAdjust = Physics2D.OverlapPointAll(groundCheck_A.position + movementV);
        
        foreach (Collider2D groundAdjustCheck in groundAdjust)
        {
            if (!grounded && movementV.y < 0f && groundAdjustCheck != null)
            {
                movementV.y = -0.22f;
            }
        }
        
        if (!grounded)
        {
            movementV -= new Vector3(0f, gravityForce);
            if (movementV.y < -1f)
                movementV.y = -1f;
        }
        else if (!adjustment)
        {
            movementV.y = 0.02f;
            adjustment = true;
            jumped = false;
        }
        else
        {
            movementV.y = 0f;
        }

        transform.position += movementV;
    }

    private void ColliderCheck()        //지면을 밟고 있는지 충돌 체크.
    {
        Collider2D[][] groundCheckCollider = new Collider2D[3][];
        groundCheckCollider[0] = Physics2D.OverlapPointAll(groundCheck_L.position);
        groundCheckCollider[1] = Physics2D.OverlapPointAll(groundCheck_C.position);
        groundCheckCollider[2] = Physics2D.OverlapPointAll(groundCheck_R.position);

        foreach (Collider2D[] groundCheckList in groundCheckCollider)
        {
            foreach (Collider2D groundCheck in groundCheckList)
            {
                if (groundCheck != null)
                {
                    if (groundCheck.tag == "Platform" && movementV.y <= 0.2f && transform.position.y - groundCheck.transform.position.y > 1f)
                    {
                        if (!downJump)
                        {
                            groundCheck_OnRoadObject = groundCheck.gameObject;
                            grounded = true;
                        }
                    }
                    else
                        downJump = false;
                    
                    if (groundCheck.tag == "Road" && movementV.y <= 0.2f)
                    {
                        grounded = true;
                        groundCheck_OnRoadObject = groundCheck.gameObject;
                    }
                }
            }
        }
    }

    protected virtual void FixedUpdateCharacter() 
    {
        
    }

    private bool RaycastCheck(Vector2 start, Vector2 end, out RaycastHit2D hit)     //레이캐스트 충돌체크
    {
        boxCollider.enabled = false;
        circleCollider.enabled = false;
        hit = Physics2D.Linecast(start, end, blockingLayer);
        boxCollider.enabled = true;
        circleCollider.enabled = true;

        if (hit.transform == null)
            return true;

        return false;
    }
    public virtual void ActionMove(float n)     // 수평이동
    {
        if (n != 0.0f)
        {
            dir = (int)Mathf.Sign(n);
            Vector3 start = rb2D.position;
            Vector3 end = start + new Vector3(dir * 0.6f, 0f);
            running = true;
            if (RaycastCheck(start, end, out hit))
            {
                transform.position += new Vector3(dir * MPP * Time.deltaTime, 0f);
            }
        }
        else
            running = false;
        
    }
    private void animatorSetting()
    {
        if (grounded)
        {
            animator.ResetTrigger("Jump");
            if (running)
                animator.SetTrigger("Run");
            else
                animator.SetTrigger("Idle");
        }
        else
        {
            animator.ResetTrigger("Run");
            animator.ResetTrigger("Idle");
            animator.SetTrigger("Jump");
        }
    }

    public virtual void ActionJump(int vertical, int jumpCount)     // 점프액션
    {
        adjustment = false;

        if (vertical == -1 && grounded) //downJump
        {
            downJump = true;
            jumped = true;
        }
        else
        {
            switch (jumpCount)
            {
                case 0:
                    jumped = true;
                    movementV.y = jumpForce;
                    break;

                case 1:     // 2단 점프
                    if (!grounded)
                    {
                        jumped = true;
                        movementV.y = jumpForce;
                    }
                    break;

                default:
                    break;
            }
        }
    }
    
    public virtual void SetSpeed(float speed)       // 속도 조정
    {
        KPH = speed;
        MPP = KPH * 1000 / 3600;  //1meter per 100pixel
    }
}

