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


public class Snake : EnemyAI
{

   
    [Header("Attack")]
    public float damage;
    public float attackDelay;
	public float attackRange;

    private float attackCooldown;
    public float attackTimer;

    [Header("Waypoint and State")]
    private int waypointIndex;

	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		Gizmos.DrawWireSphere(transform.position, attackRange);
	}
	protected override void Start()
    {
        base.Start();
        //enemyHealthBar = gameObject.transform.GetComponentInChildren<Slider>();
        //target = GameObject.FindGameObjectWithTag("Player").transform;
        target = WaypointSnake.wayPoints[0];
    }

    // Update is called once per frame
    public override void Update()
    {
		//EnemyHealth
		//enemyHealthBar.value = Mathf.Clamp01(enemyHealth / enemyMaxHealth);
		base.Update();
       
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

    public override void Patrol()
    {
		base.Patrol();
		if (Vector2.Distance(transform.position, target.position) <= 0.2f)
        {
            waypointIndex++;
            if (waypointIndex >= WaypointSnake.wayPoints.Length)
            {
                waypointIndex = 0;

            }
            target = WaypointSnake.wayPoints[waypointIndex];
            

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

	protected override void EnemyAnimator()
	{
		base.EnemyAnimator();
		if (direction.x < 0)
		{
			transform.eulerAngles = new Vector2(0, 0);
		}
		else
		{
			transform.eulerAngles = new Vector2(0, -180);//ROTATE WHOLE GAMEOBJECT WITH SPRITE AS WELL
		}
	}
	public override void TakeDamage(float damage)
	{
		base.TakeDamage(damage);
		if (enemyHealth <= 0)
		{
			canMove = false;
			canAttack = false;//Disable attack while death
			Destroy(gameObject);
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

