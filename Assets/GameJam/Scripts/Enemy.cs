using System;
using System.Collections;
using System.Collections.Generic;
using CompleteProject;
using UnityEngine;

public class Enemy : ADamageable {
 	public float timeBetweenAttacks = 0.5f;     // The time in seconds between each attack.
	public int attackDamage = 10;               // The amount of health taken away per attack.
	Animator anim;                              // Reference to the animator component.
	float timer;                                // Timer for counting up to the next attack.
	UnityEngine.AI.NavMeshAgent nav;               // Reference to the nav mesh agent.
	
	public float sinkSpeed = 2.5f;              // The speed at which the enemy sinks through the floor when dead.
	public int scoreValue = 10;                 // The amount added to the player's score when the enemy dies.
	public AudioClip deathClip;                 // The sound to play when the enemy dies.
	Collider physicsCollider;            // Reference to the capsule collider.
	bool isSinking;                             // Whether the enemy has started sinking through the floor.
	IList<Player> playersInRange = new List<Player>();
	public float destroyEnemyGameObjectAfterKilledTime = 1f;

	public float minSpeed;
	public float maxSpeed;
	public float timeAliveUntilMaxSpeed;

	public float angularSpeedMin;
	public float angularSpeedMax;
	public float timeAliveUntilMaxAngularSpeed;
	float spawnTime;
    int targetId;


	protected override void Awake()
	{
		base.Awake ();
		// Setting up the references.
		anim = GetComponent <Animator> ();
		nav = GetComponent <UnityEngine.AI.NavMeshAgent> ();
        foreach (var c in GetComponents<Collider>())
        {
            if (!c.isTrigger)
            {
                physicsCollider = c;
                break;
            }
        }
		// Setting the current health when the enemy first spawns.
		currentHealth = startingHealth;
		nav.speed = minSpeed;
		nav.angularSpeed = minSpeed;
        targetId = GameJamGameManager.instance.GetClosestTargetId(transform.position);
	}

	void OnTriggerEnter (Collider other)
	{
		//Destroy(other.gameObject);

	//	Debug.Log("Umm: " + target, other.gameObject);
		// If the entering collider is the player...
		var player = other.gameObject.GetComponent<Player>();
		if(player != null)
		{
			// ... the player is in range.
			playersInRange.Add(player);
		}
	}

	void OnTriggerExit (Collider other)
	{
		var player = other.gameObject.GetComponent<Player>();
		if(player != null)
		{
			// ... the player is no longer in range.
			playersInRange.Remove(player);
		}
	}

	void FixedUpdate()
	{
		UpdateSpeedValues();
        Player target = GameJamGameManager.instance.GetTarget(targetId);
		// If the enemy and the player have health left...
		if(currentHealth > 0 && target != null && !target.isDead)
		{
			// ... set the destination of the nav mesh agent to the player.
			nav.SetDestination (target.transform.position);
		}
		else //Otherwise, try to switch targets. If you can't, just be dead.
		{
			targetId = GameJamGameManager.instance.GetClosestTargetId(transform.position);
			if (targetId == 0)
			{
				nav.enabled = false;
			}
		}
	}

	void UpdateSpeedValues()
	{
		if (minSpeed == 0)
		{
			return;
		}
//		float maxSpeedTimePercentage = 1 - (Time.time-spawnTime)/(timeAliveUntilMaxSpeed);
		//Debug.Log("MaxSpeedTimePercentage: " + maxSpeedTimePercentage);
		nav.speed = minSpeed + ((maxSpeed - minSpeed) * ((Time.time-spawnTime)/(timeAliveUntilMaxSpeed)));
		nav.angularSpeed = angularSpeedMin + ((angularSpeedMax - angularSpeedMin) * ((Time.time-spawnTime)/(timeAliveUntilMaxAngularSpeed)));
	}
	void Update ()
	{
        if (GameJamGameManager.instance.retargetEnemies)
        {
            targetId = GameJamGameManager.instance.GetClosestTargetId(transform.position);
        }
        Player target = GameJamGameManager.instance.GetTarget(targetId);
        if (target == null)
        {
            return;
        }
        if (target.id != GameJamGameManager.LocalPlayerId)
        {
            return;
        }
		// Add the time since Update was last called to the timer.
		timer += Time.deltaTime;

		// If the timer exceeds the time between attacks, the player is in range and this enemy is alive...
		if(timer >= timeBetweenAttacks && playersInRange.Count > 0 && currentHealth > 0)
		{
			// ... attack.
			Attack ();
		}

		if (target == null)
		{

		}

		// If the player has zero or less health...
		if(target != null && target.currentHealth <= 0)
		{
			// ... tell the animator the player is dead.
			anim.SetTrigger ("PlayerDead");
		}

			// If the enemy should be sinking...
		if(isSinking)
		{
			// ... move the enemy down by the sinkSpeed per second.
			transform.Translate (-Vector3.up * sinkSpeed * Time.deltaTime);
		}
	}

	public override void TakeDamage(int playerColor, int amount, Vector3 hitPoint)
	{
		base.TakeDamage(playerColor, amount, hitPoint);

	}

	void Attack ()
	{
		foreach(var p in playersInRange)
		{
			if(p.currentHealth > 0)
			{
				// ... damage the player.
				if (p.TakeDamage (attackDamage))
				{
					// Reset the timer if the attack was successful.
					timer = 0f;
				}

			}
		}
		
		// If the player has health to lose...
	}

	[PunRPC]
	protected override void Death ()
	{
		try{
			// The enemy is dead.
			isDead = true;

			// Turn the collider into a trigger so shots can pass through it.
			physicsCollider.isTrigger = true;

			// Tell the animator that the enemy is dead.
			anim.SetTrigger ("Dead");

			// Change the audio clip of the audio source to the death clip and play it (this will stop the hurt clip playing).
			enemyAudio.clip = deathClip;
			enemyAudio.Play ();
		}
		catch(Exception)
		{
			
		}

		//low chance of dropping powerup
		MaybeDropSomething();
	}

	public float dropProb = .05f;
	private void MaybeDropSomething(){
		float roll = UnityEngine.Random.Range (0f, 1f);
		if (roll < dropProb) {
			
			GameObject item = healthDrop;
			int whichItem = (int)UnityEngine.Random.Range (0f, numDropTypes);
			switch (whichItem) {
			case 0:
				item = healthDrop;
				break;
			case 1:
				item = firingRateDrop;
				break;
			case 2:
				item = speedDrop;
				break;
			}


			Vector3 dropPos = gameObject.transform.position;
			//hack to prevent powerup models from clipping under floor
			dropPos.y += 1.0f;
			Instantiate(item, dropPos, Quaternion.identity);
		}

	}

	public void StartSinking ()
	{
		// Find and disable the Nav Mesh Agent.
		GetComponent <UnityEngine.AI.NavMeshAgent> ().enabled = false;

		// Find the rigidbody component and make it kinematic (since we use Translate to sink the enemy).
		GetComponent <Rigidbody> ().isKinematic = true;

		// The enemy should no sink.
		isSinking = true;

		// Increase the score by the enemy's score value.
		ScoreManager.score += scoreValue;

		// After 2 seconds destory the enemy.
		StartCoroutine(DestroyAfterSeconds(destroyEnemyGameObjectAfterKilledTime));
	}

	[PunRPC]
	void DestroyRemotely()
	{		
		PhotonView pv = GetComponent<PhotonView>();
		if (pv.isMine)
		{
			Debug.Log("Destroying from RPC");
			PhotonNetwork.Destroy (gameObject);
		}
	}

	IEnumerator DestroyAfterSeconds(float time)
	{
		yield return new WaitForSeconds(time);
		Destroy();
	}
}