using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRotation : MonoBehaviour {
	// Update is called once per frame
	public float rotationSpeed;
	Vector3 rotation;
	void Start()
	{
		rotation = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * rotationSpeed;
	}
	void Update ()
	{
		transform.Rotate (rotation); //rotates 50 degrees per second around z axis
	}
}
