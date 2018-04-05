using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagmaBeam : APaladinAbility {

	bool aboutToFire;
    protected override void Use()
    {
        aboutToFire = true;
    }

	void Fire()
	{
		aboutToFire = false;
	}

	void Cancel()
	{
		if (aboutToFire)
		{
			aboutToFire = false;
		}
	}

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	}
}
