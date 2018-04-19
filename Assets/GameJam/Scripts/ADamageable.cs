using System;
using System.Collections;
using System.Collections.Generic;
using CompleteProject;
using UnityEngine;

[RequireComponent (typeof(PhotonView))]
public abstract class ADamageable : Photon.MonoBehaviour{

	protected virtual bool SendDamageAcrossNetwork {
		get {
			return false;
		}
	}

	public float soulValue;
    public Soul soul;
	public bool IsDead;                                // Whether the enemy is dead.
	public int enemyColorId;
	protected AudioSource enemyAudio;                     // Reference to the audio source.
	public int startingHealth = 100;            // The amount of health the enemy starts the game with.
	protected float currentHealth;                   // The current health the enemy has.
	ParticleSystem hitParticles;                // Reference to the particle system that plays when the enemy is damaged.
	PhotonView pv;

	public const int numDropTypes = 3;
	public GameObject healthDrop;
	public GameObject firingRateDrop;
	public GameObject speedDrop;

	protected virtual void Awake()
	{
		currentHealth = startingHealth;
		pv = GetComponent<PhotonView>();
		if (pv != null && !pv.ObservedComponents.Contains(this))
		{
			pv.ObservedComponents.Add(this);
		}
		enemyAudio = GetComponent <AudioSource> ();
		hitParticles = GetComponentInChildren <ParticleSystem> ();
	}
	public virtual void TakeDamage (int playerColor, int amount, Vector3? hitPoint = null)
	{
		// If the enemy is dead...
		if(amount == 0 || IsDead)
		{
			// ... no need to take damage so exit the function.
			return;
		}

		if (enemyColorId != 0 && playerColor != enemyColorId) 
		{
//			Debug.Log("Returning cause play colors match: " + playerColor + " vs " + enemyColorId);
			//dont take damage if the shooting player isnt your type
			return;
		}

		// Play the hurt sound effect.

		// Reduce the current health by the amount of damage sustained.
		if (SendDamageAcrossNetwork)
		{
			object[] parameters = new object[1] {amount};
			GetComponent<PhotonView>().RPC("RemoveDamage", PhotonTargets.All, parameters);
		}
		else
		{
			currentHealth -= amount;
		}

		// If the current health is less than or equal to zero...
		if(currentHealth <= 0)
		{
			GameJamGameManager.instance.RecordEnemyDestroyed();
			if (GetComponent<PhotonView>())
			{
			// ... the enemy is dead.
				GetComponent<PhotonView>().RPC("Death", PhotonTargets.All);
			}
			else
			{
				Debug.LogError("Can't find PhotonView on object '" + gameObject, gameObject);
				Death();
			}
		}
		try{
			enemyAudio.Play ();
            if (hitPoint.HasValue)
            {
                // Set the position of the particle system to where the hit was sustained.
                hitParticles.transform.position = hitPoint.Value;

                // And play the particles.
                hitParticles.Play();
            }
		}
		catch(Exception)
		{
			//Debug.Log("Exception caught: " + e);
		}
	}	

	[PunRPC]
	protected virtual void RemoveDamage(int amount)
	{
		currentHealth -= amount;
	}

	protected void Destroy()
	{
		PhotonView pv = GetComponent<PhotonView>();
		if (pv == null)
		{
			GameObject.Destroy(gameObject);
		}
		else if (pv.isMine)
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

    public virtual void OnDestroy()
    {
        if (soul == null)
            return;
        foreach (var player in GameJamGameManager.instance.players)
        {
            if (player.id == GameJamGameManager.LocalPlayerId)
            {
                if (player.isDead)
                {
                    var sGo = Instantiate(soul.gameObject);
                    Soul s = sGo.GetComponent<Soul>();
					s.value = soulValue;
                    s.transform.position = new Vector3(transform.position.x, GameJamGameManager.instance.underworldFloor.transform.position.y,transform.position.z);
                    GameJamGameManager.instance.souls.Add(s);
                }
            }
        }
    }

    [PunRPC]
    protected abstract void Death();
/*	{
		IsDead = true;
		Destroy();
	}*/
}
