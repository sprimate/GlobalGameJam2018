using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class AConfirmedPaladinAbility : APaladinAbility {
    bool aboutToUse;
    protected override void Use()
    {
        if (aboutToUse)
        {
            Confirm();
        }
        else
        {
            aboutToUse = Setup();
        }
    }

    protected override void Update()
    {
        base.Update();
        if (Input.GetButtonUp("Cancel"))
        {
            Cancel();
        }
    }

    protected abstract void RevertSetup();
    /// <returns>Whether or not the setup was successful</returns>
    protected abstract bool Setup();
    protected void Cancel()
    {
        if (!aboutToUse)
        {
            return;
        }
        RevertSetup();
        if (aboutToUse)
        {
            aboutToUse = false;
        }
    }
}
