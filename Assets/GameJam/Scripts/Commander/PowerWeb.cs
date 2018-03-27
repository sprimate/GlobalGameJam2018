using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PowerWeb : MonoBehaviour {
    public Dictionary<BaseSelectable, int> powerDict { get; private set; }
    public int totalPower {get; private set;}
    BaseSelectable _target;
    public BaseSelectable target {
        get {
            return _target;
        } set {
            _target = value;
            CancelTransfer(target);
        }
    }
    struct FloatingPower
    {
        public Vector3 position;
        public int powerAmount;
    }
    List<FloatingPower> floatingPower = new List<FloatingPower>();
    public Action<PowerWeb> onWebDestroyed;

    public void Awake()
    {
        powerDict = new Dictionary<BaseSelectable, int>();
    }
    public void AddPower(BaseSelectable selectable, int powerToTransfer)
    {
        if (powerDict.ContainsKey(selectable))
        {
            powerDict[selectable] += powerToTransfer;
        }
        else
        {
            powerDict[selectable] = powerToTransfer;
        }
        selectable.power -= powerToTransfer;
        totalPower += powerToTransfer;
    }

    public void AbsorbWeb(PowerWeb web)
    {
        foreach (var pair in web.powerDict)
        {
            AddPower(pair.Key, pair.Value);
        }
        Destroy(web.gameObject);
    }

    public void CancelAllTransfers()
    {
        foreach (var key in powerDict.Keys.ToArray<BaseSelectable>())
        {
            CancelTransfer(key);
        }
        DestroyWeb();
    }

    public void CancelTransfer(BaseSelectable sender)
    {
        if (sender == null || powerDict == null || !powerDict.ContainsKey(sender))
        {
            return;
        }
        if (sender.alive)
        {
            sender.power += powerDict[sender];
        }
        else
        {
            FloatingPower floater = new FloatingPower();
            floater.powerAmount = powerDict[sender];
            floater.position = (target.transform.position - sender.transform.position) * 0.5f;
            floatingPower.Add(floater);
        }
        totalPower -= powerDict[sender];
        powerDict.Remove(sender);
        if (powerDict.Count <= 0)
        {
            DestroyWeb();
        }
    }

    public void TargetDestroyed()
    {
        foreach (var pair in powerDict)
        {
            CancelTransfer(pair.Key);
        }
    }

    public bool IsBaseSendingPower(BaseSelectable check)
    {
        return powerDict.ContainsKey(check) && powerDict[check] > 0;
    }

    public bool ContainsSender(BaseSelectable sender)
    {
        return powerDict.ContainsKey(sender);
    }

    void DestroyWeb()
    {
        if (onWebDestroyed != null)
        {
            onWebDestroyed.Invoke(this);
        }
        PowerWebManager.instance.OnWebDestroyed(this);
        Destroy(gameObject);
        target = null;
    }

    public void CenterWeb()
    {
        Vector3 center = Vector3.zero;
       
        foreach (var pair in powerDict)
        {
            center += pair.Key.transform.position;
        }
        transform.position = center / powerDict.Count;
    }

    public void Update()
    {
        TransferPower();
    }

    void TransferPower()
    {
        if (target != null)
        {
            int powerToMove = 0;
            List<BaseSelectable> toRemoveFromDict = new List<BaseSelectable>();
            foreach (var sender in powerDict.Keys.ToArray<BaseSelectable>())
            {
                var remainingPower = powerDict[sender];
                float thisUnitsPowerTransfer = Time.deltaTime * Vector3.Distance(target.transform.position, sender.transform.position) * PowerWebManager.instance.percentagePowerTransferredPerUnitPerSecond;
                bool lastTransfer = thisUnitsPowerTransfer > remainingPower;
                if (lastTransfer)
                {
                    thisUnitsPowerTransfer = remainingPower;
                }
                powerToMove += Mathf.CeilToInt(thisUnitsPowerTransfer);
                powerDict[sender] -= Mathf.CeilToInt(thisUnitsPowerTransfer);
                if (lastTransfer)
                {
                    powerDict.Remove(sender);
                }
            }
            target.power += powerToMove;
            if (powerDict.Count <= 0)
            {
                DestroyWeb();
            }
        }
    }
}