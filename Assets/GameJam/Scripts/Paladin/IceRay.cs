using DigitalRuby.PyroParticles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceRay : AConfirmedProjectileAbility {

    public FireBaseScript iceProjectile;
    public float duration;

    protected override void Confirm()
    {
        FireBaseScript particleJawn = GameObject.Instantiate(iceProjectile);
        particleJawn.Duration = duration;
        particleJawn.StartTime = 0f;
        particleJawn.StopTime = duration;
        particleJawn.transform.position = rangeVisualizer.position;
        particleJawn.transform.rotation = rangeVisualizer.rotation;
        Cancel();
        base.Confirm();
    }
}
