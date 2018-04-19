using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AOffensiveAbility : APaladinAbility {

    public int damage;
    public void DoDamage(ADamageable damageable)
    {
        damageable.TakeDamage(0, damage, null);
    }
}
