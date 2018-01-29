using System.Collections;
using System.Collections.Generic;
using CompleteProject;
using UnityEngine;


public class BouncingEnemy : MonoBehaviour {
    [SerializeField]
    [Tooltip("Just for debugging, adds some velocity during OnEnable")]
private Vector3 initialVelocity;
    [SerializeField]
private float minVelocity = 10f;
private Vector3 lastFrameVelocity;
private Rigidbody rb;
private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
    }
private void Update()
    {
		if (Input.GetKeyDown(KeyCode.T))
		{
			Debug.Log("um...");
			rb.velocity = Vector3.MoveTowards(transform.position, GameObject.FindObjectOfType<Player>().transform.position, float.MaxValue) * minVelocity;
			return;
		}
		lastFrameVelocity = rb.velocity;

		if (lastFrameVelocity.magnitude > minVelocity)
		{
			lastFrameVelocity.Normalize();
			rb.velocity = lastFrameVelocity * minVelocity;
		}
		lastFrameVelocity = rb.velocity;

    }
private void OnCollisionEnter(Collision collision)
    {
		if (collision.gameObject.layer != LayerMask.NameToLayer("shootable"))
		{
       		Bounce(collision.contacts[0].normal);
		}
    }
private void Bounce(Vector3 collisionNormal)
    {
	var speed = lastFrameVelocity.magnitude;
	var direction = Vector3.Reflect(lastFrameVelocity.normalized, collisionNormal);
        rb.velocity = direction * Mathf.Max(minVelocity);
    }
}
