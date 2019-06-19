using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyAI : MonoBehaviour

{

	[Header("Enemy Stats")]
	public float enemyHealth;
	public float enemyMaxHealth;
	public float detectionRange;
	public Transform target;
	protected Vector2 direction;

	[Header("Enemy Speed")]
	public float speed;
	public float patrolSpeed;
	public float runSpeed;
	public float stopDistance;
	public bool canMove = true;

	[Header("Attack")]
	protected bool normalAttack = false;
	public bool canAttack = false;

	[Header("Animation")]
	public float idleAnim;//idleAnim parameter

	public Animator anim;
	protected SpriteRenderer rend;
	

	public State currentState;
	protected virtual void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(transform.position, detectionRange);//Showing Detecion Radius
	}
	protected virtual void Start()
	{

		rend = GetComponent<SpriteRenderer>();
		anim = GetComponent<Animator>();
		//enemyHealthBar = gameObject.transform.GetComponentInChildren<Slider>();
		//target = GameObject.FindGameObjectWithTag("Player").transform;
		currentState = State.Patrol;
	}

	// Update is called once per frame
	public virtual void Update()
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

					StopDistance();
					break;


			}
			EnemyAnimator();
		}
		if (canMove == false)
		{
			speed = 0;
		}
       
}
    
	public virtual void Patrol()
	{
		canAttack = false;
		speed = patrolSpeed;
		direction = (target.transform.position - transform.position).normalized;//Storing Direction for Anim facing direction
		transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
		idleAnim = 1;
		DetectTarget();
        
	}

	public virtual void Chase()
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

	public virtual void StopDistance()
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
			currentState = State.Active;
		}
	}


	protected virtual void EnemyAnimator()
	{
		if (direction != Vector2.zero && canMove == true)
		{

			anim.SetFloat("Horizontal", direction.x);
			anim.SetFloat("Vertical", direction.y);
			anim.SetFloat("Idle", idleAnim);
            
        }
	}
	public  virtual void TakeDamage(float damage)
	{
		enemyHealth -= damage;
		StartCoroutine(ChangeColor(0.15f));
		
	}
	IEnumerator  ChangeColor(float delay)
	{
		rend.color = Color.red;
		yield return new WaitForSeconds(delay);
		rend.color = Color.white;
	}
}
public enum State
{
	Patrol,
	Active
}
