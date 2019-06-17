//==================================
//Author :
//Title :
//Date :
//Details :
//URL (Optional) :
//==================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Snake : MonoBehaviour
{

    [Header("Enemy Stats")]
    public float enemyHealth;
    public float enemyMaxHealth;
    public float detectionRange;
    public Transform target;
    private Vector2 direction;

    [Header("Enemy Speed")]
    public float speed;
    public float patrolSpeed;
    public float attackRange;
    public float runSpeed;
    public float stopDistance;
    public bool canMove = true;

    [Header("Attack")]
    public float damage;
    public float attackDelay;
    private bool normalAttack = false;
    public bool canAttack = false;

    private float attackCooldown;
    public float attackTimer;
    [Header("Animation")]
    public float idleAnim;//idleAnim parameter

    public Animator anim;
    private SpriteRenderer rend;
    [Header("Waypoint and State")]
    private int waypointIndex;

    public SnakeState currentState;
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRange);//Showing Detecion Radius
        Gizmos.DrawWireSphere(transform.position, attackRange);//Showing Attack Range
    }
    void Start()
    {

        rend = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        //enemyHealthBar = gameObject.transform.GetComponentInChildren<Slider>();
        //target = GameObject.FindGameObjectWithTag("Player").transform;
        target = WaypointSnake.wayPoints[0];
        currentState = SnakeState.Patrol;
    }

    // Update is called once per frame
    void Update()
    {
        //EnemyHealth
        //enemyHealthBar.value = Mathf.Clamp01(enemyHealth / enemyMaxHealth);
        if (target && canMove == true)
        {
            switch (currentState)
            {
                case SnakeState.Patrol:
                    Patrol();
                    break;
                case SnakeState.Active:
                    Chase();
                    StopDistance();
                    break;


            }
            EnemyAnimator();
        }
        if (canMove == false)
        {
            speed = 0;
        }


        #region UPDATE Attack
        if (canAttack == true && target && Vector2.Distance(transform.position, target.position) < attackRange)//Checking if player is exist in game
        {
            if (attackCooldown <= 0)
            {
                StartCoroutine(AfterAttackDelay(attackDelay));//Stop the movement when attacking
                attackCooldown = attackTimer;
            }
            attackCooldown -= Time.deltaTime;//Normal bullet reset time
        }
        #endregion

    }

    void Patrol()
    {
        canAttack = false;
        speed = patrolSpeed;
        direction = (target.transform.position - transform.position).normalized;//Storing Direction for Anim facing direction
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        idleAnim = 1;
        DetectTarget();


        if (Vector2.Distance(transform.position, target.position) <= 0.2f)
        {
            waypointIndex++;
            target = WaypointSnake.wayPoints[waypointIndex];

        }
        else
        {
            if (waypointIndex >= WaypointSnake.wayPoints.Length - 1)
            {
                waypointIndex = 0;// Doesnt reset to 0 ??

            }
        }

    }

    void Chase()
    {
        canAttack = true;
        normalAttack = true;
        if (Vector2.Distance(transform.position, target.position) > stopDistance)
        {
            speed = runSpeed;
            direction = (target.transform.position - transform.position).normalized; //storing facing direction to target.
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            idleAnim = 1;
        }


    }

    void StopDistance()
    {
        if (Vector2.Distance(transform.position, target.position) < stopDistance)
        {
            transform.position = this.transform.position;
            idleAnim = 0;
        }
    }

    void DetectTarget()
    {
        Collider2D col = Physics2D.OverlapCircle(transform.position, detectionRange);
        PlayerMovement player = col.GetComponentInParent<PlayerMovement>();
        if (player)
        {
            target = player.transform;
            currentState = SnakeState.Active;
        }
    }
    void SnakeAttack()
    {
        Collider2D col = Physics2D.OverlapCircle(transform.position, attackRange);
        PlayerMovement player = col.GetComponentInParent<PlayerMovement>();
        if (player)
        {
            
            player.TakeDamage(damage);
           
        }

    }


    void EnemyAnimator()
    {
        if (direction != Vector2.zero && canMove == true)
        {

            anim.SetFloat("Horizontal", direction.x);
            anim.SetFloat("Vertical", direction.y);
            anim.SetFloat("Idle", idleAnim);

            if (direction.x < 0)
            {
                transform.eulerAngles = new Vector2(0, 0);
            }
            else
            {
                transform.eulerAngles = new Vector2(0, -180);//ROTATE WHOLE GAMEOBJECT WITH SPRITE AS WELL
            }
        }
    }
    public void TakeDamage(float damage)
    {
        enemyHealth -= damage;
        if (enemyHealth <= 0)
        {
            canMove = false;
            canAttack = false;//Disable attack while death
        }
    }
    #region IEnumarator
    IEnumerator AfterAttackDelay(float delay)//Prevent enemy from moving while shooting
    {
       
        anim.SetTrigger("Attack");
        canAttack = false;
        canMove = false;
        yield return new WaitForSeconds(delay);
        SnakeAttack();
        canMove = true;
        canAttack = true;
    }

    #endregion

}
public enum SnakeState
{
    Patrol,
    Active

}
