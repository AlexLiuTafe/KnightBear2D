//==================================
//Author :ALEX LIU
//Title :ENEMY AI
//Date :4 June 2019
//Details : Referencing from (BLACKTHORNPROD) for Chase,Retreat and Stopping Distance
//URL (Optional) :
//==================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Roboton : EnemyAI
{

    [Header("Enemy Speed")]
    public float retreatSpeed;
    public float retreatDistance;


    [Header("Attack")]
    public GameObject[] projectiles;
	public float attackRange;
	public float shootDelay;
    public float explosionRadius;
    public float explosionDamage;
    public float explodeTimer;

    private float shootCooldown;
    public float shootStartTimer;
    private float seekerCooldown;
    public float seekerStartTimer;
    [Header("Waypoint and State")]

    private int waypointIndex;

	protected  override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		Gizmos.DrawWireSphere(transform.position, attackRange);//Showing Attack Range
	}
	protected override void Start()
    {
        base.Start();
		//enemyHealthBar = gameObject.transform.GetComponentInChildren<Slider>();
		//target = GameObject.FindGameObjectWithTag("Player").transform;
		seekerCooldown = seekerStartTimer;
        target = Waypoint.wayPoints[0];
      
    }

    // Update is called once per frame
   public override void Update()
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
                case State.Active: //HOW TO INSERT RETREAT FUNCTION
                    Chase();
                    Retreat();
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

    public override void Patrol()
    {
		base.Patrol();
        if (Vector2.Distance(transform.position, target.position) <= 0.2f)
        {
            waypointIndex++; 
            if (waypointIndex >= Waypoint.wayPoints.Length)
            {
                waypointIndex = 0;

            }
            
            target = Waypoint.wayPoints[waypointIndex];

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
   

    protected override void EnemyAnimator()
    {
		base.EnemyAnimator();
            if (direction.x < 0)
            {
                transform.eulerAngles = new Vector2(0, -180);//ROTATE WHOLE GAMEOBJECT WITH SPRITE AS WELL
            }
            else
            {
                transform.eulerAngles = new Vector2(0, 0);//ROTATE WHOLE GAMEOBJECT WITH SPRITE AS WELL
            }
        
    }
   
    public override void TakeDamage(float damage)
    {
		base.TakeDamage(damage);
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
	

	#endregion

}

