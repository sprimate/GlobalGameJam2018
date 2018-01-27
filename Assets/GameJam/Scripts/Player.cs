using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnitySampleAssets.CrossPlatformInput;

namespace CompleteProject
{
    public class Player : Photon.MonoBehaviour
    {

#region PortedFromPlayerMovement
        public float speed = 6f;            // The speed that the player will move at.
		public int id;
        Vector3 movement;                   // The vector to store the direction of the player's movement.
        Animator anim;                      // Reference to the animator component.
        Rigidbody playerRigidbody;          // Reference to the player's rigidbody.
#if !MOBILE_INPUT
        int floorMask;                      // A layer mask so that a ray can be cast just at gameobjects on the floor layer.
        float camRayLength = 100f;          // The length of the ray from the camera into the scene.
#endif

    
        void Awake ()
        {
#if !MOBILE_INPUT
            // Create a layer mask for the floor layer.
            floorMask = LayerMask.GetMask ("Floor");
#endif

            // Set up references.
            anim = GetComponent <Animator> ();
            playerRigidbody = GetComponent <Rigidbody> ();

			 // Setting up the references.
            playerAudio = GetComponent <AudioSource> ();
            playerMovement = GetComponent <Player> ();
            weapon = GetComponentInChildren <Weapon> ();

            // Set the initial health of the player.
            currentHealth = startingHealth;
            damageImage = GameObject.Find("DamageImage").GetComponent<Image>();
			healthSlider = GameObject.Find("HealthSlider").GetComponent<Slider>();
        }


        void FixedUpdate ()
        {
           // Debug.Log("Fixed");
			if (GameJamGameManager.LocalPlayerId != id)
			{
				return;
			}
            // Store the input axes.
            float h = CrossPlatformInputManager.GetAxisRaw("Horizontal");
            float v = CrossPlatformInputManager.GetAxisRaw("Vertical");

            // Move the player around the scene.
            Move (h, v);

            // Turn the player to face the mouse cursor.
            Turning ();

            // Animate the player.
            Animating (h, v);
        }


        void Move (float h, float v)
        {
            // Set the movement vector based on the axis input.
            movement.Set (h, 0f, v);
            
            // Normalise the movement vector and make it proportional to the speed per second.
            movement = movement.normalized * speed * Time.deltaTime;

            // Move the player to it's current position plus the movement.
            playerRigidbody.MovePosition (transform.position + movement);
        }


        void Turning ()
        {
#if !MOBILE_INPUT
            // Create a ray from the mouse cursor on screen in the direction of the camera.
            Ray camRay = Camera.main.ScreenPointToRay (Input.mousePosition);

            // Create a RaycastHit variable to store information about what was hit by the ray.
            RaycastHit floorHit;

            // Perform the raycast and if it hits something on the floor layer...
            if(Physics.Raycast (camRay, out floorHit, camRayLength, floorMask))
            {
                // Create a vector from the player to the point on the floor the raycast from the mouse hit.
                Vector3 playerToMouse = floorHit.point - transform.position;

                // Ensure the vector is entirely along the floor plane.
                playerToMouse.y = 0f;

                // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
                Quaternion newRotatation = Quaternion.LookRotation (playerToMouse);

                // Set the player's rotation to this new rotation.
                playerRigidbody.MoveRotation (newRotatation);
            }
#else

            Vector3 turnDir = new Vector3(CrossPlatformInputManager.GetAxisRaw("Mouse X") , 0f , CrossPlatformInputManager.GetAxisRaw("Mouse Y"));

            if (turnDir != Vector3.zero)
            {
                // Create a vector from the player to the point on the floor the raycast from the mouse hit.
                Vector3 playerToMouse = (transform.position + turnDir) - transform.position;

                // Ensure the vector is entirely along the floor plane.
                playerToMouse.y = 0f;

                // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
                Quaternion newRotatation = Quaternion.LookRotation(playerToMouse);

                // Set the player's rotation to this new rotation.
                playerRigidbody.MoveRotation(newRotatation);
            }
#endif
        }


        void Animating (float h, float v)
        {
            // Create a boolean that is true if either of the input axes is non-zero.
            bool walking = h != 0f || v != 0f;

            // Tell the animator whether or not the player is walking.
            anim.SetBool ("IsWalking", walking);
        }
#endregion
		#region PortedFromPlayerHealth

        public int startingHealth = 100;                            // The amount of health the player starts the game with.
        public int currentHealth;                                   // The current health the player has.
        public Slider healthSlider;                                 // Reference to the UI's health bar.
        public Image damageImage;                                   // Reference to an image to flash on the screen on being hurt.
        public AudioClip deathClip;                                 // The audio clip to play when the player dies.
        public float flashSpeed = 5f;                               // The speed the damageImage will fade at.
        public Color flashColour = new Color(1f, 0f, 0f, 0.1f);     // The colour the damageImage is set to, to flash.

                                          // Reference to the Animator component.
        AudioSource playerAudio;                                    // Reference to the AudioSource component.
        Player playerMovement;                              // Reference to the player's movement.
        Weapon weapon;                              // Reference to the PlayerShooting script.
        bool isDead;                                                // Whether the player is dead.
        bool damaged;                                               // True when the player gets damaged.

        void Update ()
        {
         //   Debug.Log("Update");
            // If the player has just been damaged...
            if(damaged)
            {
                // ... set the colour of the damageImage to the flash colour.
                damageImage.color = flashColour;
            }
            // Otherwise...
            else
            {
                // ... transition the colour back to clear.
                damageImage.color = Color.Lerp (damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
            }


            // Reset the damaged flag.
            damaged = false;
            
            try
            {
                SmoothLag();
            }
            catch(Exception)
            {
                
            }

            #region PlayerShooting
             // Add the time since Update was last called to the timer.

#if !MOBILE_INPUT
            // If the Fire1 button is being press and it's time to fire...
			if(GameJamGameManager.LocalPlayerId == id)
            {
                if (Input.GetButton ("Fire1") && Time.timeScale != 0)
                {
                    // ... shoot the gun.
                    object[] parameters = new object[] {id};
                    weapon.GetComponent<PhotonView>().RPC("Shoot", PhotonTargets.All, parameters );
                    //weapon.Shoot (id);
                }
                if (Input.GetButtonUp("Swap"))
                {                  
                    object[] parameters = new object[] {id == 1 ? 2 : 1};
                    GetComponent<PhotonView>().RPC("Swap", PhotonTargets.All, parameters);
                }
            }

			if(GameJamGameManager.LocalPlayerId == id && Input.GetButtonUp ("Fire2"))
			{
				weapon.GetComponent<PhotonView>().RPC("SwapColor", PhotonTargets.All );
			}
#else
            // If there is input on the shoot direction stick and it's time to fire...
            if ((CrossPlatformInputManager.GetAxisRaw("Mouse X") != 0 || CrossPlatformInputManager.GetAxisRaw("Mouse Y") != 0) && timer >= timeBetweenBullets)
            {
                // ... shoot the gun
                weapon.Shoot(id);
            }
#endif
            #endregion
        
        }

        void SmoothLag()
        {
            if (!GetComponent<PhotonView>().isMine)
            {
                timeToReachGoal = currentPacketTime - lastPacketTime;
                currentTime += Time.deltaTime;
                transform.position = Vector3.Lerp(positionAtLastPacket, realPosition, (float)(currentTime / timeToReachGoal));
                transform.rotation = Quaternion.Lerp(rotationAtLastPacket, realRotation, (float)(currentTime/timeToReachGoal));
            }
        }

        public void TakeDamage (int amount)
        {
            // Set the damaged flag so the screen will flash.
            damaged = true;

            // Reduce the current health by the damage amount.
            currentHealth -= amount;

            // Set the health bar's value to the current health.
            healthSlider.value = currentHealth;

            // Play the hurt sound effect.
            playerAudio.Play ();

            // If the player has lost all it's health and the death flag hasn't been set yet...
            if(currentHealth <= 0 && !isDead)
            {
                // ... it should die.
                Death ();
            }
        }

        [PunRPC]
        public void Swap(int playerTarget)
        {
            if (PhotonNetwork.player.IsMasterClient)
            {
                GameObject p = null;
                foreach(Player x in FindObjectsOfType<Player>())
                {
                    if (x.id == playerTarget)
                    {
                        p = x.gameObject;
                    }
                }
                if (p != null)
                {
               // Player p = GameJamGameManager.instance.players[playerTarget-1];
                var tempPos = p.transform.position;
                p.transform.position = transform.position;
                transform.position = tempPos;
                }
            }
        }


        void Death ()
        {
            Debug.Log("Death played");

            // Set the death flag so this function won't be called again.
            isDead = true;

            // Turn off any remaining shooting effects.
            weapon.DisableEffects ();

            // Tell the animator that the player is dead.
            anim.SetTrigger ("Die");

            // Set the audiosource to play the death clip and play it (this will stop the hurt sound from playing).
            playerAudio.clip = deathClip;
            playerAudio.Play ();

            // Turn off the movement and shooting scripts.
            playerMovement.enabled = false;
            weapon.enabled = false;
        }


        public void RestartLevel ()
        {
            // Reload the level that is currently loaded.
            SceneManager.LoadScene (0);
        }
		#endregion

        #region PhotonStuff
        void OnPhotonInstantiate(PhotonMessageInfo info) 
        {
            GameJamGameManager gm = GameJamGameManager.instance;
            // e.g. store this gameobject as this player's charater in PhotonPlayer.TagObject
            PhotonView pv = GetComponent<PhotonView>();
            id = (int) pv.instantiationData[0];
            Debug.Log("ID: " + id);
           // gameObject.transform.SetParent(playersParent.transform);
            gm.players.Add(this);
            if (id == GameJamGameManager.LocalPlayerId)
            {           
                gameObject.name = "Active Player";
                CameraFollow.instance.target = transform;
            }
            else
            {
		        if (PhotonNetwork.player.IsMasterClient)
                {
                    pv.TransferOwnership(id);
                }
                gameObject.name = "Other Player";
            }

			initialColorAssignment ();
        }

		public void initialColorAssignment()
		{
			//maybe a hack? just use the id assigned from multiplayer
			int playerColor;
			if (id == 1) 
			{
				playerColor = PlayerColor.ColorID1;
			} 
			else 
			{
				playerColor = PlayerColor.ColorID2;
			}

			object[] parameters = new object[] {playerColor};
			weapon.GetComponent<PhotonView>().RPC("SetColor", PhotonTargets.All, parameters );
		}

        public Quaternion realRotation = Quaternion.identity;
        public Quaternion rotationAtLastPacket = Quaternion.identity;
        public Vector3 realPosition = Vector3.zero;
        public Vector3 positionAtLastPacket = Vector3.zero;
        public double currentTime = 0.0;
        public double currentPacketTime = 0.0;
        public double lastPacketTime = 0.0;
        public double timeToReachGoal = 0.0;
        void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {              
            if (stream.isWriting)
            {
                stream.SendNext((Vector3)transform.position);
                stream.SendNext((Quaternion)transform.rotation);
            }
            else
            {
                currentTime = 0.0;
                positionAtLastPacket = transform.position;
                rotationAtLastPacket = transform.rotation;
                realPosition = (Vector3) stream.ReceiveNext();
                if (float.IsNaN(realPosition.x)) //if not a number, ignore this update
                {
                    realPosition = positionAtLastPacket;
                }
                realRotation = (Quaternion) stream.ReceiveNext();             
                lastPacketTime = currentPacketTime;
                currentPacketTime = info.timestamp;
            }
        }
        #endregion

    }
}