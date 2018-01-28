using System;
using System.Collections;
using System.Collections.Generic;
using CompleteProject;
using UnityEngine;

public class Enemy : ADamageable {
 	public float timeBetweenAttacks = 0.5f;     // The time in seconds between each attack.
	public int attackDamage = 10;               // The amount of health taken away per attack.
	Animator anim;                              // Reference to the animator component.
	Player target;                  // Reference to the player's health.
	float timer;                                // Timer for counting up to the next attack.
	UnityEngine.AI.NavMeshAgent nav;               // Reference to the nav mesh agent.
	
	public float sinkSpeed = 2.5f;              // The speed at which the enemy sinks through the floor when dead.
	public int scoreValue = 10;                 // The amount added to the player's score when the enemy dies.
	public AudioClip deathClip;                 // The sound to play when the enemy dies.
	CapsuleCollider capsuleCollider;            // Reference to the capsule collider.
	bool isSinking;                             // Whether the enemy has started sinking through the floor.
	IList<Player> playersInRange = new List<Player>();
	public float destroyEnemyGameObjectAfterKilledTime = 1f;


	protected override void Awake()
	{
		// Setting up the references.
		
		anim = GetComponent <Animator> ();
		nav = GetComponent <UnityEngine.AI.NavMeshAgent> ();
		capsuleCollider = GetComponent <CapsuleCollider> ();
		// Setting the current health when the enemy first spawns.
		currentHealth = startingHealth;
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

	void Update ()
	{
		float closestDistance = float.MaxValue;
		foreach(Player p in GameJamGameManager.instance.players)
		{
			var dist = Vector3.Distance(p.transform.position, transform.position);
			if (dist < closestDistance)
			{
				closestDistance = dist;
				target = p;
			}
		}
		// Add the time since Update was last called to the timer.
		timer += Time.deltaTime;

		// If the timer exceeds the time between attacks, the player is in range and this enemy is alive...
		if(timer >= timeBetweenAttacks && playersInRange.Count > 0 && currentHealth > 0)
		{
			// ... attack.
			Attack ();
		}

		// If the player has zero or less health...
		if(target.currentHealth <= 0)
		{
			// ... tell the animator the player is dead.
			anim.SetTrigger ("PlayerDead");
		}
		
		// If the enemy and the player have health left...
		if(currentHealth > 0 && target.currentHealth > 0)
		{
			// ... set the destination of the nav mesh agent to the player.
			nav.SetDestination (target.transform.position);
		}
		// Otherwise...
		else
		{
			// ... disable the nav mesh agent.
			nav.enabled = false;
		}

			// If the enemy should be sinking...
		if(isSinking)
		{
			// ... move the enemy down by the sinkSpeed per second.
			transform.Translate (-Vector3.up * sinkSpeed * Time.deltaTime);
		}
	}

	void Attack ()
	{
		// Reset the timer.
		timer = 0f;

		foreach(var p in playersInRange)
		{
			if(p.currentHealth > 0)
			{
				// ... damage the player.
				p.TakeDamage (attackDamage);
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
			capsuleCollider.isTrigger = true;

			// Tell the animator that the enemy is dead.
			anim.SetTrigger ("Dead");

			// Change the audio clip of the audio source to the death clip and play it (this will stop the hurt clip playing).
			enemyAudio.clip = deathClip;
			enemyAudio.Play ();
		}
		catch(Exception)
		{
			
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

	void Destroy()
	{
		PhotonView pv = GetComponent<PhotonView>();
		if (pv.isMine)
		{
			PhotonNetwork.Destroy (gameObject);
		}
		else
		{
			pv.RPC("DestroyRemotely", PhotonTargets.All);
		}
	}

	IEnumerator DestroyAfterSeconds(float time)
	{
		yield return new WaitForSeconds(time);
		Destroy();
	}
}