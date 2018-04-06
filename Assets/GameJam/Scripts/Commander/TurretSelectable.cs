using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretSelectable : GenericSelectable {

	public float fireRadius;
	float lastFireTime;
	public float fireRate;
	public int damagePerAttack; //eventually fire projectile

	public float effectsDisplayTime = 0.2f;

	LineRenderer gunLine; 

	void Start()
	{
		gunLine = GetComponent <LineRenderer> ();
	}

	public new float GetObjectDepth(){
		return GetComponent<CapsuleCollider>().radius * transform.lossyScale.z;
	}
	List<CompleteProject.Player> canHit = new List<CompleteProject.Player>();

	void FixedUpdate()
	{
		foreach(var player in GameJamGameManager.instance.players)
		{
			if (Vector3.Distance(player.transform.position, transform.position) < fireRadius)
			{
				canHit.Add(player);
			}
		}
		if (canHit.Count > 0)
		{
			var target = canHit[Random.Range(0, canHit.Count)];
			transform.LookAt(target.transform);
			canHit.Clear();
			if (CanFire())
			{
				AttackTarget(target);
			}
		}
	}
	float timer;
	void Update()
	{
		            // If the timer has exceeded the proportion of timeBetweenBullets that the effects should be displayed for...
		if(timer >= effectsDisplayTime)
		{
			// ... disable the effects.
			gunLine.enabled = false;
		}
		timer += Time.deltaTime;
	}

	void AttackTarget(CompleteProject.Player p)
	{
		Debug.Log("Attack " + p + " for " + damagePerAttack);
		timer = 0f;
		p.TakeDamage(damagePerAttack);
		gunLine.enabled = true;
		gunLine.SetPosition (0, transform.position);
		gunLine.SetPosition (1, p.transform.position);
		lastFireTime = Time.time;
	}

	bool CanFire()
	{
		return Time.time - lastFireTime >= fireRate;
	}
}
