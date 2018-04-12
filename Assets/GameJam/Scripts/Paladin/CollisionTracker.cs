using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionTracker : MonoBehaviour {

    public HashSet<GameObject> collidingObjects = new HashSet<GameObject>();

    public void OnTriggerEnter(Collision other)
    {
        collidingObjects.Add(other.gameObject);
    }

    public void OnTriggerExit(Collider other)
    {
        collidingObjects.Remove(other.gameObject);
    }
}
