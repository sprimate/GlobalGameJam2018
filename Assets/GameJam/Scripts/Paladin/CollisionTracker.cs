using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionTracker : MonoBehaviour {

    public HashSet<GameObject> collidingObjects;

    public void OnTriggerEnter(Collider col)
    {
        collidingObjects.Add(col.gameObject);
    }

    public void OnTriggerExit(Collider other)
    {
        collidingObjects.Remove(other.gameObject);
    }

    void OnEnable()
    {
        collidingObjects = new HashSet<GameObject>();
    }
}
