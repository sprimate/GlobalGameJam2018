using System.Collections;
using System.Collections.Generic;
using CompleteProject;
using UnityEngine;

public class Enemy : MonoBehaviour {
 	public float timeBetweenAttacks = 0.5f;     // The time in seconds between each attack.
	public int attackDamage = 10;               // The amount of health taken away per attack.


	Animator anim;                              // Reference to the animator component.
	Player target;                  // Reference to the player's health.
	bool playerInRange;                         // Whether player is within the trigger collider and can be attacked.
	float timer;                                // Timer for counting up to the next attack.

	UnityEngine.AI.NavMeshAgent nav;               // Reference to the nav mesh agent.
	
	public int startingHealth = 100;            // The amount of health the enemy starts the game with.
	public int currentHealth;                   // The current health the enemy has.
	public float sinkSpeed = 2.5f;              // The speed at which the enemy sinks through the floor when dead.
	public int scoreValue = 10;                 // The amount added to the player's score when the enemy dies.
	public AudioClip deathClip;                 // The sound to play when the enemy dies.


	AudioSource enemyAudio;                     // Reference to the audio source.
	ParticleSystem hitParticles;                // Reference to the particle system that plays when the enemy is damaged.
	CapsuleCollider capsuleCollider;            // Reference to the capsule collider.
	bool isDead;                                // Whether the enemy is dead.
	bool isSinking;                             // Whether the enemy has started sinking through the floor.

	public int enemyColorId;


	void Awake ()
	{
		// Setting up the references.
		
		anim = GetComponent <Animator> ();
		nav = GetComponent <UnityEngine.AI.NavMeshAgent> ();
		enemyAudio = GetComponent <AudioSource> ();
		hitParticles = GetComponentInChildren <ParticleSystem> ();
		capsuleCollider = GetComponent <CapsuleCollider> ();

		// Setting the current health when the enemy first spawns.
		currentHealth = startingHealth;

	}


	void OnTriggerEnter (Collider other)
	{
		// If the entering collider is the player...
		if(other.gameObject == target)
		{
			// ... the player is in range.
			playerInRange = true;
		}
	}


	void OnTriggerExit (Collider other)
	{
		// If the exiting collider is the player...
		if(other.gameObject == target)
		{
			// ... the player is no longer in range.
			playerInRange = false;
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
		if(timer >= timeBetweenAttacks && playerInRange && currentHealth > 0)
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

		// If the player has health to lose...
		if(target.currentHealth > 0)
		{
			// ... damage the player.
			target.TakeDamage (attackDamage);
		}
	}
			
	public void TakeDamage (int playerColor, int amount, Vector3 hitPoint)
	{
		// If the enemy is dead...
		if(isDead)
			// ... no need to take damage so exit the function.
			return;

		if (playerColor != enemyColorId) {
			//dont take damage if the shooting player isnt your type
			return;
		}

		// Play the hurt sound effect.
		enemyAudio.Play ();

		// Reduce the current health by the amount of damage sustained.
		currentHealth -= amount;
		
		// Set the position of the particle system to where the hit was sustained.
		hitParticles.transform.position = hitPoint;

		// And play the particles.
		hitParticles.Play();

		// If the current health is less than or equal to zero...
		if(currentHealth <= 0)
		{
			// ... the enemy is dead.
			GetComponent<PhotonView>().RPC("Death", PhotonTargets.All);
		}
	}

	[PunRPC]
	void Death ()
	{
		Debug.Log("Death");
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
		StartCoroutine(DestroyAfterSeconds(2));
	}

	[PunRPC]
	void Destroy()
	{		
		PhotonView pv = GetComponent<PhotonView>();
		if (pv.isMine)
		{
			Debug.Log("Destroingn from RPC");
			PhotonNetwork.Destroy (gameObject);
		}
	}

	IEnumerator DestroyAfterSeconds(float time)
	{
		yield return new WaitForSeconds(time);
		PhotonView pv = GetComponent<PhotonView>();
		if (pv.isMine)
		{
			PhotonNetwork.Destroy (gameObject);
		}
		else{
			pv.RPC("Destroy", PhotonTargets.All);
		}
	}
}