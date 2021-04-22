﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Static var
    public static PlayerController pC;
    
    // Movement variables
    [SerializeField] private float maxSpeed = 2.3f;
    [SerializeField] private float rotationSpeed = 2.6f;
    [SerializeField] private float acceleration = 0.017f;
    [SerializeField] private float fastRowMultiplier = 1.6f;
    public bool canSteer;
    private bool rowFast;
    private float speed;
    private Vector3 inputDirection;
    private Vector3 lastDirection;

    // Physics variables
    private Vector2 bounceDir;
    [SerializeField] private float bounceMagMax = 3f;
    private float bounceMag = 3f;

    // Anim variables
    private enum animState { idle = 0, rowing = 1, fastRowing = 2}
    private animState aState;
    [SerializeField] private GameObject boatLight;
    private Animator lightAnim;

    // Components
    // private Collider2D col;
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
    private float invulnerableTimerMax = 1.5f;
    private float invulnerableTimer;

    // Audio variables
    private float creakTimer;
    [SerializeField] private float avgCreakTime = 4f;
    [SerializeField] private float creakVolume = 0.4f;

    // Start is called before the first frame update
    void OnEnable()
    {
        // col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        lightAnim = boatLight.GetComponent<Animator>();

        rowFast = false;

        aState = animState.idle;
        
        health = 3;

        bounceDir = Vector2.zero;

        creakTimer = avgCreakTime;
        
        // Prevent player from steering for first 3 seconds of scene.
        StartCoroutine(allowSteerTimer(3f));
    }

    private void FixedUpdate()
    {
        MoveCharacter();

        // FixedUpdate happens before OnTriggerStay so this defaults to false each frame.
        isTouchingWall = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (canSteer)
        {
            inputDirection.x = Input.GetAxisRaw("Horizontal");
            inputDirection.y = Input.GetAxisRaw("Vertical");
        }

        if (invulnerableTimer > 0)
        {
            invulnerableTimer -= Time.deltaTime;
            if (invulnerableTimer < 0)
            {
                isInvulnerable = false;
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

        // Creak sound
        if (rb.velocity.magnitude > 1)
        {
            creakTimer -= Time.deltaTime;
            if (creakTimer < 0)
            {
                AudioController.aC.PlayRandomSFXAtPoint(AudioController.aC.boatCreak, transform.position, creakVolume);
                creakTimer = Random.Range(avgCreakTime - 1f, avgCreakTime + 1f);
            }
        }
    }

    // Move function
    void MoveCharacter()
    {
        // Disable movement if character recently hit enemy, bounce character away from collision point
        if (isInvulnerable)
        {
            rb.velocity = bounceDir * bounceMag;
            bounceMag *= 0.95f;
            aState = animState.idle;
            return;
        }

        if (inputDirection != Vector3.zero)
        {            
            lastDirection = inputDirection;
            float angle = -90 + Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            rb.MoveRotation(Quaternion.RotateTowards(this.transform.rotation, rotation, rotationSpeed));

            if (rowFast)
            {
                speed = Mathf.Lerp(speed, maxSpeed * fastRowMultiplier, acceleration * fastRowMultiplier);
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
            Heal();
            colObject.GetComponent<BottleProperties>().Collect();
            AudioController.aC.PlaySFXAtPoint(AudioController.aC.bottlePickUp, collision.contacts[0].point, 0.25f);
            UIManager.uIM.SetHelperMessageText("To Read Notes: Press 'i' or the ▲|Y Button", 4f);
        }
        else if (colObject.tag == "Enemy" && !isInvulnerable)
        {
            Vector2 dir = collision.contacts[0].point - new Vector2(transform.position.x, transform.position.y);
            AudioController.aC.PlayRandomSFXAtPoint(AudioController.aC.hitEnemy, collision.contacts[0].point, 0.4f);
            bounceDir = -dir.normalized;
            bounceMag = bounceMagMax;
            TakeDamage();
        }
        else if (colObject.tag == "HardDebris")
        {
            float impactVolume = collision.relativeVelocity.sqrMagnitude / Mathf.Pow(maxSpeed * fastRowMultiplier, 2) * 0.6f;
            AudioController.aC.PlayRandomSFXAtPoint(AudioController.aC.hitHardDebris, collision.contacts[0].point, impactVolume);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "RainUp")
        {
            EffectsController.eC.IncreaseRainState();
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.tag == "RainDown")
        {
            EffectsController.eC.DecreaseRainState();
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.tag == "LevelEnd")
        {
            StartCoroutine(GameController.gC.LoadNextSceneAsync(3f));
            StartCoroutine(EffectsController.eC.Fade(1f, 3f));
        }
        else if (collision.gameObject.tag == "Dialogue")
        {
            Dialogue d = collision.gameObject.GetComponent<Dialogue>();
            UIManager.uIM.showDialogue(d.duration, d.content, d.pauseGame, d.characterId, d.barkId, d.triggerId);
            Destroy(collision.gameObject);
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
            invulnerableTimerMax = 2f;
            EffectsController.eC.StartCoroutine(EffectsController.eC.PlayerDeath(2f, transform.position));
            Destroy(gameObject, 2f);
        }

        invulnerableTimer = invulnerableTimerMax;
        isInvulnerable = true;
        speed = 0f;

        CinemachineShake.cSInstance.ShakeCamera(5f, invulnerableTimerMax);
        invulnerableTimerMax = 1.5f;
    }

    void Heal()
    {
        if (health < 3)
        {
            hearts[health].SetActive(true);
            health++;
        }
    }

    public IEnumerator allowSteerTimer(float timer)
    {
        canSteer = false;
        
        yield return new WaitForSeconds(timer);

        canSteer = true;
    }
}
