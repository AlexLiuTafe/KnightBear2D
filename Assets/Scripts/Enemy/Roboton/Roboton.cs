﻿//==================================
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


public class Roboton : MonoBehaviour
{

    [Header("Enemy Stats")]
    public float enemyHealth;
    public float enemyMaxHealth;
    public float detectionRange;
    public Transform target;
    private Vector2 direction;


    [Header("Enemy Health Bar")]
    //public Slider enemyHealthBar;

    [Header("Enemy Speed")]
    public float speed;
    public float patrolSpeed;
    public float attackRange;
    public float runSpeed;
    public float retreatSpeed;
    public float stopDistance;
    public float retreatDistance;
    private bool canMove = true;

    [Header("Attack")]
    public GameObject[] projectiles;
    public float shootDelay;
    public float explosionRadius;
    public float explosionDamage;
    public float explodeTimer;
    private bool normalAttack = false; //preventing Double Shooting
    private bool canAttack = false;

    private float shootCooldown;
    public float shootStartTimer;
    private float seekerCooldown;
    public float seekerStartTimer;

    [Header("Animation")]
    public float idleAnim;//idleAnim parameter

    public Animator anim;
    private SpriteRenderer rend;
    [Header("Waypoint and State")]

    private int waypointIndex;
    public State currentState;
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, explosionRadius);//Showing Explosion Radius
        Gizmos.DrawWireSphere(transform.position, detectionRange);//Showing Deetecion Radius
    }
    void Start()
    {

        rend = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        //enemyHealthBar = gameObject.transform.GetComponentInChildren<Slider>();
        //target = GameObject.FindGameObjectWithTag("Player").transform;
        target = Waypoint.wayPoints[0];
        seekerCooldown = seekerStartTimer;
        currentState = State.Patrol;
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
                case State.Patrol:
                    Patrol();
                    break;
                case State.Active:
                    Chase();
                    StopDistance();
                    Retreat();
                    break;


            }
            EnemyAnimator();
        }
        else if (canMove == false)
        {
            speed = 0;
        }

        #region UPDATE Attack
        if (canAttack == true && target && Vector2.Distance(transform.position, target.position) < attackRange)//Checking if player is exist in game
        {

            if (seekerCooldown > 0)//Checking normal shoot
            {
                if (shootCooldown <= 0)
                {

                    StartCoroutine(AfterShootDelay(shootDelay));//Stop the movement when attacking
                    shootCooldown = shootStartTimer;
                    seekerCooldown += 0.4f;//Balancing the Seeker Bullet timer
                }
            }
            else if (seekerCooldown <= 0)
            {

                StartCoroutine(SeekerDelay(shootDelay));
                seekerCooldown = seekerStartTimer;
                shootCooldown = 2f;//Resetting Normal shoot so doesnt spawn 2 projectiles.
            }

            seekerCooldown -= Time.deltaTime;//Seeker bullet reset time
            shootCooldown -= Time.deltaTime;//Normal bullet reset time
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
            target = Waypoint.wayPoints[waypointIndex];

        }
        else
        {
            if (waypointIndex >= Waypoint.wayPoints.Length - 1)
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
    void Retreat()
    {
        if (Vector2.Distance(transform.position, target.position) < retreatDistance)
        {
            speed = retreatSpeed;
            transform.position = Vector2.MoveTowards(transform.position, target.position, -speed * Time.deltaTime);
            idleAnim = 1;
        }


    }
    void StopDistance()
    {
        if (Vector2.Distance(transform.position, target.position) < stopDistance && Vector2.Distance(transform.position, target.position) > retreatDistance)
        {
            transform.position = this.transform.position;
            idleAnim = 0;
        }
    }
    void Shoot()
    {
        
        Instantiate(projectiles[0], transform.position, Quaternion.identity);
        FindObjectOfType<AudioManager>().PlaySound("NormalAttack");

    }
    void SeekerProjectile()
    {
        Instantiate(projectiles[1], transform.position, Quaternion.identity);
        FindObjectOfType<AudioManager>().PlaySound("SeekerAttack");
    }
    void ExplodeOnDeath()
    {
        canMove = false;
        Collider2D col = Physics2D.OverlapCircle(transform.position, explosionRadius);
        PlayerMovement player = col.GetComponentInParent<PlayerMovement>();
        if (player)
        {
            player.TakeDamage(explosionDamage);
        }

    }
    void DetectTarget()
    {
        Collider2D col = Physics2D.OverlapCircle(transform.position, detectionRange);
        PlayerMovement player = col.GetComponentInParent<PlayerMovement>();
        if (player)
        {
            target = player.transform;
            currentState = State.Active;
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
                transform.eulerAngles = new Vector2(0, -180);//ROTATE WHOLE GAMEOBJECT WITH SPRITE AS WELL
            }
            else
            {
                transform.eulerAngles = new Vector2(0, 0);//ROTATE WHOLE GAMEOBJECT WITH SPRITE AS WELL
            }
        }
    }
   
    public void TakeDamage(float damage)
    {
        enemyHealth -= damage;
        StartCoroutine(ChangeColor(0.15f));
        FindObjectOfType<AudioManager>().PlaySound("EnemyTakeDamage");
        if (enemyHealth <= 0)
        {
            canMove = false;
            speed = 0f;
            canAttack = false;//Disable attack while explodeAnim       
            StartCoroutine(ExplosionDelay(explodeTimer));//Delay Death
            Destroy(gameObject, 1.5f);
        }
    }
    #region IEnumarator
    IEnumerator AfterShootDelay(float delay)//Prevent enemy from moving while shooting
    {
        canAttack = false;
        canMove = false;
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(delay);
        Shoot();
        canMove = true;
        canAttack = true;
    }
    IEnumerator SeekerDelay(float delay)
    {
        normalAttack = false;//Preventing double Shooting
        canAttack = false;
        canMove = false;
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(delay);
        SeekerProjectile();
        canMove = true;
        canAttack = true;
        normalAttack = true;
    }
    IEnumerator ExplosionDelay(float delay)
    {
        anim.SetTrigger("Explode");
        yield return new WaitForSeconds(delay);
        FindObjectOfType<AudioManager>().PlaySound("EnemyDeath");
        ExplodeOnDeath();
        
    }
    IEnumerator ChangeColor(float delay)
    {
        rend.color = Color.red;
        yield return new WaitForSeconds(delay);
        rend.color = Color.white;
    }
    #endregion

}
public enum State
{
    Patrol,
    Active

}
