using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRotation : MonoBehaviour {
	// Update is called once per frame
	public float rotationSpeed;
	public bool randomRotation = true;
	public Vector3 definedRotation;
	Vector3 rotation;
	void Start()
	{
		if (randomRotation)
		{
			rotation = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
		}
		else
		{
			rotation = definedRotation;
		}
		rotation *= rotationSpeed;
	}
	void Update ()
	{
		transform.Rotate (rotation); //rotates 50 degrees per second around z axis
	}
}
