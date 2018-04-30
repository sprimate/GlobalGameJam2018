using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnitySampleAssets.CrossPlatformInput;

namespace CompleteProject
{
    public class Player : Photon.MonoBehaviour
    {

#region PortedFromPlayerMovement

        public float bufferBetweenDamageTaken;
        float lastDamageTaken;
        int numTimesDied = 0;
        float soulValueMultiplier = 1;
        bool initialized;
        public GameObject targetVisualizationPoint;

        //If this bool is set to false, the soulDepreciationValue will be subtracted from soulValueMultiplier each death
        //If set to true, the soulValueMultiplier will be divided by the soulDepreciationValue each death;
        public bool soulDepreciationExponential = true; 
        float soulDepreciationValue = 2f; //After deaths, how do the souls depreciate in value?' Adjust the soulHealthRatio by this much each time
        public float minHealthPercentageForResurrection = 33;
		public float speed = defaultPlayerSpeed;            // The speed that the player will move at.
		const float defaultPlayerSpeed = 12f;
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
            floorMask = LayerMask.GetMask("Floor");
#endif
            // Set up references.
            anim = GetComponent<Animator>();
            playerRigidbody = GetComponent<Rigidbody>();

            // Setting up the references.
            playerAudio = GetComponent<AudioSource>();
            weapon = GetComponentInChildren<Weapon>();
        }

        void Resurrect()
        {
            soulValueMultiplier = soulDepreciationExponential ? soulValueMultiplier / soulDepreciationValue : soulValueMultiplier - soulDepreciationValue;
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            foreach (var p in GameJamGameManager.instance.players)
            {
                if (p != this)
                {
                    weapon.SetColor(p.weapon.GetColor() == 1 ? 2 : 1);
                    break;
                }
            }
            GameJamGameManager.instance.ClearSouls();
            GetComponent<PhotonView>().RPC("SetIsAlive", PhotonTargets.All);
        }

        [PunRPC]
        void SetIsAlive()
        {
            isDead = false;
            weapon.SetEffectsEnabled(true);
            weapon.enabled = true;
            GameJamGameManager.instance.TriggerEnemyTargetRecalculation();
        }

        IEnumerator InitializeAfterFrame()
        {
            yield return new WaitForEndOfFrame();

            // Set the initial health of the player.
            currentHealth = startingHealth;
            if (GameObject.Find("DamageImage") != null)
            {
                damageImage = GameObject.Find("DamageImage").GetComponent<Image>();
                healthSlider = GameObject.Find("HealthSlider").GetComponent<Slider>();
                healthSlider.maxValue = startingHealth;
            }

            foreach (var ability in FindObjectsOfType<APaladinAbility>())
            {
                var id = GameJamGameManager.instance.maxNumPlayers == 1 ? 1 : GameJamGameManager.LocalPlayerId - 1;
                if (ability.paladinPlayerNum == id)
                {
                    foreach (var button in FindObjectsOfType<AbilityButton>())
                    {
                        if (button.gameObject.name.Contains(ability.abilityNum.ToString()))
                        {
                            ability.SetInterface(button);
                            break;
                        }
                    }
                }
                else
                {
                    ability.gameObject.SetActive(false);
                }
            }
            initialized = true;

        }

        void FixedUpdate ()
        {
            Rigidbody rigidbody = GetComponent<Rigidbody>();

            if (rigidbody == null)
            {}
           // Debug.Log("Fixed");
			if (GameJamGameManager.LocalPlayerId != id || CommanderModeManager.instance.commanderMode)
			{
				return;
			}

            if (!initialized)
            {
                StartCoroutine(InitializeAfterFrame());
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
            if (Input.GetJoystickNames().Length ==  0)
            {
                Vector3 target = GameJamGameManager.instance.shipCamera.ScreenToWorldPoint(Input.mousePosition);
                target.y = playerRigidbody.transform.position.y;
                playerRigidbody.MoveRotation(Quaternion.LookRotation(target - playerRigidbody.transform.position, playerRigidbody.transform.up));
//                playerRigidbody.transform.LookAt(target);
                return;
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
            }
            else
            {
                Vector3 turnDir = new Vector3(Input.GetAxis("Right Stick Horizontal") , 0f , Input.GetAxis("Right Stick Vertical"));
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
            }
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

        public float startingHealth = 100;                            // The amount of health the player starts the game with.
        [SerializeField] float _currentHealth;
        public float currentHealth {
            get 
            { 
                return _currentHealth;
            } 
            set 
            {
                _currentHealth = value <= startingHealth ? value : startingHealth;
                if (healthSlider != null && id == GameJamGameManager.LocalPlayerId)
                {
                    healthSlider.value = _currentHealth;
                }
            }
        }                                   // The current health the player has.
        public Slider healthSlider;                                 // Reference to the UI's health bar.
        public Image damageImage;                                   // Reference to an image to flash on the screen on being hurt.
        public AudioClip deathClip;                                 // The audio clip to play when the player dies.
        public float flashSpeed = 5f;                               // The speed the damageImage will fade at.
        public Color flashColour = new Color(1f, 0f, 0f, 0.1f);     // The colour the damageImage is set to, to flash.

                                          // Reference to the Animator component.
        AudioSource playerAudio;                                    // Reference to the AudioSource component.
     //   Player playerMovement;                              // Reference to the player's movement.
        Weapon weapon;                              // Reference to the PlayerShooting script.
        public bool isDead;                                                // Whether the player is dead.
        bool damaged;                                               // True when the player gets damaged.

        void Update ()
        {
         //   Debug.Log("Update");
            // If the player has just been damaged...
            if (damageImage != null)
            {
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

            if (CommanderModeManager.instance.commanderMode)
            {
                return;
            }
            
            if (isDead)
            {
                Debug.Log("CommanderMode: " + CommanderModeManager.instance.commanderMode);
                if (id == GameJamGameManager.LocalPlayerId && Input.GetButtonUp("Swap"))
                {
                    if (currentHealth >= minHealthPercentageForResurrection/100 * startingHealth)
                    {
                        Resurrect();
                    }
                    else
                    {
                        Debug.Log("Unable to resurrect - Not enough health yet.");
                    }
                }
                return;
            }
            #region PlayerShooting
             // Add the time since Update was last called to the timer.
            if (id == GameJamGameManager.LocalPlayerId && Input.GetButtonUp("Swap"))
            {                  
            //  object[] parameters = new object[] {id == 1 ? 2 : 1, transform.position};
                Swap(id == 1 ? 2 : 1);
                //GetComponent<PhotonView>().RPC("Swap", PhotonTargets.All, parameters);
            }

			//only process input here if its from this player...
			if(GameJamGameManager.LocalPlayerId == id)
            {
	            if (Input.GetJoystickNames().Length == 0)
	            {
	                // If the Fire1 button is being press and it's time to fire...
                    if (Input.GetButton ("Fire1") && Time.timeScale != 0 && !EventSystem.current.IsPointerOverGameObject())
                    {
                        // ... shoot the gun.
                        object[] parameters = new object[] {id};
                        weapon.GetComponent<PhotonView>().RPC("Shoot", PhotonTargets.All, parameters );
                        //weapon.Shoot (id);
                    }

    //				if(Input.GetButtonUp ("Fire2"))
    //				{
    //					weapon.GetComponent<PhotonView>().RPC("SwapColor", PhotonTargets.All );
    //				}                    
	            }
	            else
	            {
	                // If there is input on the shoot direction stick and it's time to fire...
	                if ((Input.GetAxisRaw("Right Stick Horizontal") != 0 || Input.GetAxisRaw("Right Stick Vertical") != 0))
	                {
	                    // ... shoot the gun
						object[] parameters = new object[] {id};
						weapon.GetComponent<PhotonView>().RPC("Shoot", PhotonTargets.All, parameters );
	                }
				}
            }
            #endregion
        }

        void SmoothLag()
        {
            if (!GetComponent<PhotonView>().isMine)
            {
                /*if (teleportPosition.HasValue)
                {
                    transform.position = teleportPosition.Value;
                }
                else*/
                {
                    timeToReachGoal = currentPacketTime - lastPacketTime;
                    currentTime += Time.deltaTime;
                    if (float.IsNaN(positionAtLastPacket.x) || float.IsInfinity(positionAtLastPacket.x))
                    {
                        positionAtLastPacket = transform.position;
                    }
                    
                    if (timeToReachGoal != 0f)
                    {
                        transform.position = Vector3.Lerp(positionAtLastPacket, realPosition, (float)(currentTime / timeToReachGoal));
                        try
                        {
                            transform.rotation = Quaternion.Lerp(rotationAtLastPacket, realRotation, (float)(currentTime / timeToReachGoal));
                        } 
                        catch (Exception) { }
                    }
                }
            }
        }


        public void CollectSoul(float amount)
        {
            currentHealth += amount * soulValueMultiplier;
            if (currentHealth >= startingHealth)
            {
                currentHealth = startingHealth;
                Resurrect();
                return;
            }
        } 

        public bool TakeDamage (int amount)
        {
            if (lastDamageTaken + bufferBetweenDamageTaken > Time.time)
            {
                return false;
            }
            lastDamageTaken = Time.time;
            // Set the damaged flag so the screen will flash.
            damaged = true;

            // Reduce the current health by the damage amount.
            currentHealth -= amount;

            // Play the hurt sound effect.
            playerAudio.Play ();

            // If the player has lost all it's health and the death flag hasn't been set yet...
            if(currentHealth <= 0 && !isDead)
            {
                // ... it should die.
                Death ();
            }
            return true;
        }

        public void Swap(int playerTarget)
        {
            var allPlayers = FindObjectsOfType<Player>();

//            List<Vector3> players = new List<Vector3>();
            Player otherPlayer = null;
            foreach(Player x in allPlayers)//TODO - this is so shitty. Do better, priyal. 
            {
                if (x != this)
                {
                    otherPlayer = x;
                }
            }   

            if (allPlayers.Length == 1 || otherPlayer.isDead)
            {
			    weapon.GetComponent<PhotonView>().RPC("SwapColor", PhotonTargets.All );
                return;
            }        
            Vector3 tempPos = transform.position;
//            Debug.Log("Send " + id + " to " + otherPlayer.transform.position + " and " + otherPlayer.id + " to " + transform.position);
            object[] parameters = new object[4] {id, transform.position, otherPlayer.id, otherPlayer.transform.position};
            GetComponent<PhotonView>().RPC("SetPosition", PhotonTargets.All, parameters);
            otherPlayer.GetComponent<PhotonView>().RPC("SetPosition", PhotonTargets.All, parameters);
            //otherPlayer.GetComponent<PhotonView>().RPC("SetOtherTeleportPosition", PhotonTargets.All, parameters);

           //otherPlayer.GetComponent<PhotonView>().RPC("SetOtherTeleportPosition", PhotonTargets.All, parameters);
            if (PhotonNetwork.isMasterClient)//host jumps to client without telling client to go
            {
               // SetOtherTeleportPosition(tempPos);
                //otherPlayer.GetComponent<PhotonView>().RPC("SetPosition", PhotonTargets.All, parameters);
            }

            /*if (PhotonNetwork.player.IsMasterClient)
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
                    Debug.Log("Before position: " + transform.position);
                    var tempPos = p.transform.position;

                    p.transform.position = transform.position;
                    transform.position = tempPos;
                    Debug.Log("After position: " + transform.position);
                }
            }*/
        }

        [PunRPC]
        public void SetPosition(int player1, Vector3 pos1, int player2, Vector3 pos2)
        {
            Vector3 position = id == player1 ? pos2 : pos1; ;
            transform.position = position;

            if (id == GameJamGameManager.LocalPlayerId)
            {
                GameJamGameManager.instance.Swap();
            }
            else
            {
                teleportPosition = position;
            }
        }

        void LateUpdate() //NEed this in late update to override the Photon placing of the position
        {
            if (teleportPosition.HasValue)
            {
                transform.position = teleportPosition.Value; 
            }
        }

        /*Vector3 otherPosition;

        [PunRPC]
        public void SetOtherTeleportPosition(Vector3 pos)
        {
            if (id == GameJamGameManager.LocalPlayerId)
            {
                Debug.Log(id + ".) Setting Other Positionn: " + otherPosition);
                otherPosition = pos;
            }
        }*/

        void Death ()
        {
            // Set the death flag so this function won't be called again.
            isDead = true;
            // Turn off any remaining shooting effects.
            weapon.SetEffectsEnabled(false);
            weapon.enabled = false;

            // Tell the animator that the player is dead.
            anim.SetTrigger ("Die");

            // Set the audiosource to play the death clip and play it (this will stop the hurt sound from playing).
            playerAudio.clip = deathClip;
            playerAudio.Play ();

            // Turn off the movement and shooting scripts.
            //playerMovement.enabled = false;
            object[] parameters = new object[1] {id};
            GetComponent<PhotonView>().RPC("KillPlayer", PhotonTargets.All, parameters );            
        }

        [PunRPC]
        public void KillPlayer(int playerId)
        {
            bool allDead = true;
            foreach (var p in GameJamGameManager.instance.players)
            {
                if (p.id == playerId)
                {
                    p.isDead = true;
                }
                else if (!p.isDead)
                {
                    allDead = false;
                }
            }
            if (allDead || !GameJamGameManager.instance.allowGhostMode)
            {
                GameObject.FindGameObjectWithTag("HUD").GetComponent<Animator>().SetTrigger("GameOver");
                Destroy();
            }
            else if (id == GameJamGameManager.LocalPlayerId)
            {
                GameJamGameManager.instance.TriggerEnemyTargetRecalculation();
                EnterGhostMode();           
            }
        }

        void EnterGhostMode()
        {
            transform.position = new Vector3(transform.position.x, GameJamGameManager.instance.underworldFloor.transform.position.y, transform.position.z);
        }

        protected void Destroy()
        {
            PhotonView pv = GetComponent<PhotonView>();
            if (pv.isMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
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
			int playerColor = GameJamGameManager.LocalPlayerId - 1; ;
            playerColor = playerColor == 0 ? 1 : playerColor;
			/*if (id == 1) 
			{
				playerColor = PlayerColor.ColorID1;
			} 
			else 
			{
				playerColor = PlayerColor.ColorID2;
			}
            */

			object[] parameters = new object[] {playerColor};
			weapon.GetComponent<PhotonView>().RPC("SetColor", PhotonTargets.All, parameters );
		}

		public void TemporaryFiringRateIncrease()
		{
			weapon.TemporaryFiringRateIncrease ();
		}

		public void TemporarySpeedIncrease()
		{
			speed = defaultPlayerSpeed * 1.5f;
			Invoke ("ResetPlayerSpeed", 10f);
		}

		private void ResetPlayerSpeed()
		{
			speed = defaultPlayerSpeed;
		}

        public Quaternion realRotation = Quaternion.identity;
        public Quaternion rotationAtLastPacket = Quaternion.identity;
        public Vector3 realPosition = Vector3.zero;
        public Vector3 positionAtLastPacket = Vector3.zero;
        public double currentTime = 0.0;
        public double currentPacketTime = 0.0;
        public double lastPacketTime = 0.0;
        public double timeToReachGoal = 0.0;
        public Vector3? teleportPosition;
        bool shouldReset = false;
        void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {              
            if (stream.isWriting)
            {
                stream.SendNext((Vector3)transform.position);
                stream.SendNext((Quaternion)transform.rotation);
            }
            else
            {
                if (teleportPosition.HasValue)
                {
                    transform.position = teleportPosition.Value;
                    if (shouldReset) 
                    {
                        teleportPosition = null;
                        Debug.Log("resetting");
                    }  
                    else
                    {
                        Debug.Log("Received before resetting");
                    }
                    shouldReset = !shouldReset; 
                }
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