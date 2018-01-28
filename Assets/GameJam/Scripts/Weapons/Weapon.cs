using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (PhotonView))]
public class Weapon : Photon.MonoBehaviour {
	ParticleSystem gunParticles;                    // Reference to the particle system.
	LineRenderer gunLine;                           // Reference to the line renderer.
	AudioSource gunAudio;                           // Reference to the audio source.
	Light gunLight;                                 // Reference to the light component.
	public Light faceLight;								// Duh
	public float effectsDisplayTime = 0.2f;                // The proportion of the timeBetweenBullets that the effects will display for.

	public float range = 100f;                      // The distance the gun can fire.

	float timer;                                    // A timer to determine when to fire.
	Ray shootRay = new Ray();                       // A ray from the gun end forwards.
	RaycastHit shootHit;                            // A raycast hit to get information about what was hit.
	int shootableMask;                              // A layer mask so the raycast only hits things on the shootable layer.
	 public int damagePerShot = 20;                  // The damage inflicted by each bullet.
	public float timeBetweenBullets = 0.15f;        // The time between each shot.

	int playerColorId;



	// Use this for initialization
	void Awake()
	{
		PhotonView pv = GetComponent<PhotonView>();
		if (!pv.ObservedComponents.Contains(this))
		{
			pv.ObservedComponents.Add(this);
		}
		            // Set up the references.
		gunParticles = GetComponent<ParticleSystem> ();
		gunLine = GetComponent <LineRenderer> ();
		gunAudio = GetComponent<AudioSource> ();
		gunLight = GetComponent<Light> ();
		shootableMask = LayerMask.GetMask ("Shootable");

		//faceLight = GetComponentInChildren<Light> ();
	}

	void Update()
	{
		            // If the timer has exceeded the proportion of timeBetweenBullets that the effects should be displayed for...
		if(timer >= timeBetweenBullets * effectsDisplayTime)
		{
			// ... disable the effects.
			DisableEffects ();
		}
		timer += Time.deltaTime;
	}

	[PunRPC]
	public void Shoot (int playerId)
	{

		if (timer < timeBetweenBullets)
		{
			return;
		}
		// Reset the timer.
		timer = 0f;

		// Play the gun shot audioclip.
		if(!gunAudio.isPlaying)
			gunAudio.Play();

		// Enable the lights.
		gunLight.enabled = true;
		faceLight.enabled = true;

		// Stop the particles from playing if they were, then start the particles.
		gunParticles.Stop ();
		gunParticles.Play ();

		// Enable the line renderer and set it's first position to be the end of the gun.
		gunLine.enabled = true;
		gunLine.SetPosition (0, transform.position);

		// Set the shootRay so that it starts at the end of the gun and points forward from the barrel.
		shootRay.origin = transform.position;
		shootRay.direction = transform.forward;
		// Perform the raycast against gameobjects on the shootable layer and if it hits something...
		if(Physics.Raycast (shootRay, out shootHit, range, shootableMask))
		{
			if (GameJamGameManager.LocalPlayerId == playerId )
			{
				// Try and find an EnemyHealth script on the gameobject hit.
				Enemy enemy = shootHit.collider.GetComponent <Enemy> ();

				// If the EnemyHealth component exist...
				if(enemy != null)
				{
					// ... the enemy should take damage.
					enemy.TakeDamage (playerColorId, damagePerShot, shootHit.point);
				}
			}
			else
			{
				Debug.Log("Not doing damage!");
			}

			// Set the second position of the line renderer to the point the raycast hit.
			gunLine.SetPosition (1, shootHit.point);
		}
		// If the raycast didn't hit anything on the shootable layer...
		else
		{
			// ... set the second position of the line renderer to the fullest extent of the gun's range.
			gunLine.SetPosition (1, shootRay.origin + shootRay.direction * range);
		}
	}

	public void DisableEffects ()
	{
		// Disable the line renderer and the light.
		gunLine.enabled = false;
		faceLight.enabled = false;
		gunLight.enabled = false;
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{      

    }

	[PunRPC]
	public void SetColor(int colorId){

		playerColorId = colorId;

		Color color = PlayerColor.getColorForId (colorId);

		gunLine.material.color = color;
		gunLight.color = color;

		var mainGunParticles = gunParticles.main;
		mainGunParticles.startColor = color;
	}

	[PunRPC]
	public void SwapColor(){
		//super crap hack for play testing
		int nextColor = playerColorId == 1? 2: 1;
		object[] parameters = new object[] {nextColor};
		GetComponent<PhotonView>().RPC("SetColor", PhotonTargets.All, parameters );
	}
}
