using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Movement variables
    [SerializeField] private float maxSpeed = 4f;
    [SerializeField] private float rotationSpeed = 2.5f;
    [SerializeField] private float acceleration = 0.05f;
    private bool rowFast;
    private float speed;
    private Vector3 inputDirection;
    private Vector3 lastDirection;

    // Anim variables
    private enum animState { idle = 0, rowing = 1, fastRowing = 2}
    private animState aState;
    [SerializeField] private GameObject boatLight;
    private Animator lightAnim;

    // Components
    private Collider2D col;
    private Rigidbody2D rb;
    private Animator anim;

    // Wall variables
    [SerializeField] private GameObject background;   
    private Vector3 velocityOffset;
    private bool isTouchingWall;

    // Health variables
    public List<GameObject> hearts = new List<GameObject>();
    private int health;
    private bool isInvulnerable;
    private float invulnerableTimer;
    [SerializeField] private float invulnerableTime = 2f;


    // Start is called before the first frame update
    void Start()
    {
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        lightAnim = boatLight.GetComponent<Animator>();

        rowFast = false;

        aState = animState.idle;
        
        health = 3;
    }

    private void FixedUpdate()
    {
        MoveCharacter();

        // FixedUpdate happens before OnTriggerStay so this defaults to false each frame
        isTouchingWall = false;
    }

    // Update is called once per frame
    void Update()
    {
        inputDirection.x = Input.GetAxisRaw("Horizontal");
        inputDirection.y = Input.GetAxisRaw("Vertical");

        if (invulnerableTimer > 0)
        {
            invulnerableTimer -= Time.deltaTime;
            if (invulnerableTimer < 0)
            {
                isInvulnerable = false;
                col.isTrigger = false;
            }
        }

        if (Input.GetAxisRaw("Row Fast") != 0)
        {
            rowFast = true;
        }
        else
        {
            rowFast = false;
        }

        anim.SetInteger("state", (int) aState);
        lightAnim.SetInteger("state", (int) aState);
    }

    // Move function
    void MoveCharacter()
    {        
        if (inputDirection != Vector3.zero)
        {
            lastDirection = inputDirection;
            float angle = -90 + Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            rb.MoveRotation(Quaternion.RotateTowards(this.transform.rotation, rotation, rotationSpeed));

            if (rowFast == true)
            {
                speed = Mathf.Lerp(speed, maxSpeed * 1.6f, acceleration * 1.6f);
                aState = animState.fastRowing;
            }
            else
            {
                speed = Mathf.Lerp(speed, maxSpeed, acceleration);
                aState = animState.rowing;
            }

        } 
        else
        {
            speed = Mathf.Lerp(speed, 0, acceleration);

            if (Input.GetAxisRaw("Turn Left") != 0)
            {
                rb.MoveRotation(rb.rotation + (rotationSpeed / 2));
            }
            else if (Input.GetAxisRaw("Turn Right") != 0)
            {
                rb.MoveRotation(rb.rotation - (rotationSpeed / 2));
            }

            aState = animState.idle;
        }

        if (!isTouchingWall && velocityOffset != Vector3.zero)
        {
            velocityOffset += -velocityOffset * acceleration * 2;
            if (velocityOffset.sqrMagnitude < 0.05f)
            {
                velocityOffset = Vector3.zero;
            }
        }

        rb.velocity = transform.up * speed + velocityOffset;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject colObject = collision.collider.gameObject;

        if (colObject.tag == "Bottle")
        {
            colObject.GetComponent<BottleProperties>().Invoke("Collect", 0);
            Debug.Log("Collected a Bottle!");
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        GameObject colObject = collision.collider.gameObject;

        if (colObject.tag == "Enemy" && !isInvulnerable)
        {
            TakeDamage();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Wall-H")
        {
            isTouchingWall = true;
            Vector3 wallPosition = collision.gameObject.transform.position;
            if (wallPosition.x < transform.position.x)
            {
                velocityOffset += Vector3.right * acceleration * 2;
            } 
            else if (wallPosition.x > transform.position.x)
            {
                velocityOffset += Vector3.left * acceleration * 2;
            }
        }
        else if (collision.gameObject.tag == "Wall-V")
        {
            isTouchingWall = true;
            Vector3 wallPosition = collision.gameObject.transform.position;
            if (wallPosition.y < transform.position.y)
            {
                velocityOffset += Vector3.up * acceleration * 2;
            }
            else if (wallPosition.y > transform.position.y)
            {
                velocityOffset += Vector3.down * acceleration * 2;
            }
        }
    }

    void TakeDamage()
    {
        health--;
        hearts[health].SetActive(false);

        if (health < 1)
        {
            Destroy(gameObject, 2f);
        }

        invulnerableTimer = invulnerableTime;
        isInvulnerable = true;
        col.isTrigger = true;

        CinemachineShake.cSInstance.ShakeCamera(5f, 2f);
    }

    void Heal()
    {
        if (health < 3)
        {
            health++;
            hearts[health].SetActive(true);
        }
    }
}
