using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PowerWebManager : MonoSingleton<PowerWebManager> {

    public float percentagePowerTransferredPerUnitPerSecond = 1f;

    Dictionary<BaseSelectable, PowerWeb> webs = new Dictionary<BaseSelectable, PowerWeb>();
    PowerWeb untargetedWeb;
    // Use this for initialization
    public PowerWeb GetNewPowerWeb()
    {
        GameObject g = new GameObject("Power Web [" + (1 + webs.Count) + "]");
        g.transform.SetParent(transform);
        var ret = g.AddComponent<PowerWeb>();
        untargetedWeb = ret;
        return ret; 
    }

    public void SetWebTarget(BaseSelectable target, PowerWeb web = null)
    {
        bool useUntargetedWeb = web == null;
        if (useUntargetedWeb)
        {
            web = untargetedWeb;
        }

        if (webs.ContainsKey(target))
        {
            webs[target].AbsorbWeb(web);
        }
        else
        {
            webs[target] = web;
        }

        web.target = target;

        if (useUntargetedWeb)
        {
            untargetedWeb = null;
        }
        Debug.Log("Sending " + web.totalPower + " to " + target, target);
    }

    public List<PowerWeb> GetAllRelatedWebs(BaseSelectable selectable)
    {
        if (selectable == null)
        {
            return webs.Values.ToList<PowerWeb>();
        }

        List<PowerWeb> ret = new List<PowerWeb>();
        foreach (var pair in webs)
        {
            var web = pair.Value;
            if (web.ContainsSender(selectable))
            {
                ret.Add(web);
            }
        }
        return ret;
    }

    public void CancelAllTargetedPowerTransfers(BaseSelectable selectable = null)
    {
        foreach (PowerWeb web in GetAllRelatedWebs(selectable))
        {
            web.CancelTransfer(selectable);
        }
    }

    public void CancelUntargetedPowerTransfer()
    {
        if (untargetedWeb != null)
        {
            untargetedWeb.CancelAllTransfers();
        }
    }

    public void OnWebDestroyed(PowerWeb web)
    {
        if (web == untargetedWeb)
        {
            untargetedWeb = null;
        }
        else
        {
            webs.Remove(web.target);
        }
    }

    public void Update()
    {
        if (untargetedWeb != null)
        {
            Debug.Log("Total Untargeted Power: " + untargetedWeb.totalPower);
        }
    }
}