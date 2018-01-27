using UnityEngine;

namespace CompleteProject
{
    public class GameOverManager : MonoSingleton<GameOverManager>
    {

        Animator anim;                          // Reference to the animator component.


        void Awake ()
        {
            // Set up the reference.
            anim = GetComponent <Animator> ();
        }


        void Update ()
        {
            // If the player has run out of health...
            bool gameOver = false;
            foreach(var player in GameJamGameManager.instance.players)
            {
                if (player.currentHealth > 0)
                {
                    break;
                }
            }
            if(gameOver)
            {
                // ... tell the animator the game is over.
                anim.SetTrigger ("GameOver");
            }
        }
    }
}