using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PowerWeb : MonoBehaviour {
    public LineRenderer lineRenderer;
    public Dictionary<BaseSelectable, int> powerDict { get; private set; }
    Dictionary<BaseSelectable, ParticleSystem> powerUIDict = new Dictionary<BaseSelectable, ParticleSystem>();
    public int totalPower {get; private set;}
    BaseSelectable _target;
    public BaseSelectable target {
        get {
            return _target;
        } set {
            _target = value;
            if (target != null)
            {
                CancelTransfer(target);
                TransferPower2();
            }

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
        //selectable.power -= powerToTransfer;
        totalPower += powerToTransfer;
        ParticleSystem newParticleSystem = Instantiate(PowerWebManager.instance.powerTransferParticleSystemPrefab, transform);
        newParticleSystem.transform.name = "PowerLine (" + powerUIDict.Count + ")";
        newParticleSystem.transform.position = selectable.transform.position;
        powerUIDict[selectable] = newParticleSystem;
        newParticleSystem.Stop();
    }

    public void AbsorbWeb(PowerWeb web)
    {
        foreach (var pair in web.powerDict)
        {
            AddPower(pair.Key, pair.Value);
        }
        Destroy(web.gameObject);
    }

    public void SetTargetPosition(Vector3 targetPos)
    {
        foreach (var key in powerUIDict.Keys.ToArray<BaseSelectable>())
        {
            var main = powerUIDict[key].main;
            main.startLifetime = Vector3.Distance(powerUIDict[key].transform.position, targetPos) / main.startSpeed.constant;
            powerUIDict[key].transform.LookAt(targetPos);
        }
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
        if (!sender.IsDead)
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
        //TransferPower();
    }

    void TransferPower2()
    {
        SetTargetPosition(target.transform.position);
        foreach (var sender in powerDict.Keys.ToArray<BaseSelectable>())
        {

            var powerToSend = powerDict[sender];
            float timeforParticlesToStayAlive = powerToSend / PowerWebManager.instance.powerTransferredPerSecond;
            Debug.Log(powerToSend + "/" + PowerWebManager.instance.powerTransferredPerSecond + " = " + timeforParticlesToStayAlive);
            Action particleColissionCallback = () => { }; //avoid a nullref
            particleColissionCallback += () =>
            {
                StartCoroutine(TransferPowerOverTime(timeforParticlesToStayAlive, powerDict[sender], target));
                target.OnParticleColissionCallback -= particleColissionCallback;
            };
            target.OnParticleColissionCallback += particleColissionCallback;
            StartCoroutine(SendPowerForTime(sender, timeforParticlesToStayAlive));
        }
    }

    IEnumerator SendPowerForTime(BaseSelectable sender, float time)
    {
        ParticleSystem particleSystem = powerUIDict[sender];
        particleSystem.transform.position = Vector3.MoveTowards(particleSystem.transform.position, target.transform.position, sender.GetObjectDepth() + 1f);
        particleSystem.Play();
        var startTime = Time.time;
        float lastUpdateTime = startTime;
        yield return StartCoroutine(TransferPowerOverTime(time, powerDict[sender], sender));
        particleSystem.Stop();
        powerUIDict.Remove(sender);
        powerDict.Remove(sender);
        while (particleSystem.particleCount > 0)
        {
            yield return null;
        }
        Debug.Log("Destroying system");
        Destroy(particleSystem);
        if (powerDict.Count <= 0)
        {
            DestroyWeb();
        }
    }

    IEnumerator TransferPowerOverTime(float time, float power, BaseSelectable toTransfer)
    {
        var startTime = Time.time;
        float totalPowerTransferred = 0;
        float lastUpdateTime = Time.time;
        while (Time.time < startTime + time)
        {
            float powerToMove = (Time.time - lastUpdateTime)/ time * power;
            Debug.Log("Moving " + powerToMove + " (" + ((Time.time - startTime) / time * 100f) + "%" + " - " + time);
            lastUpdateTime = Time.time;
            if (totalPowerTransferred + powerToMove > power)
            {
                powerToMove = power - totalPowerTransferred;
            }
            totalPowerTransferred += powerToMove;
            if (toTransfer == target)
            {
                target.power += powerToMove;
            }
            else
            {
                toTransfer.power -= powerToMove;
            }
            yield return new WaitForFixedUpdate();
        }

        if (toTransfer == target)
        {
            target.power += power - totalPowerTransferred;
        }
        else
        {
            toTransfer.power -= (power - totalPowerTransferred);
        }


    }

    public void OnParticleCollisionStarted()
    {
        
    }

    void TransferPower()
    {
        if (target != null)
        {
            int powerToMove = 0;
            foreach (var sender in powerDict.Keys.ToArray<BaseSelectable>())
            {
                powerUIDict[sender].transform.position = sender.transform.position;

                var remainingPower = powerDict[sender];
                float thisUnitsPowerTransfer = Time.deltaTime * Vector3.Distance(target.transform.position, sender.transform.position) * PowerWebManager.instance.powerTransferredPerSecond;
                bool lastTransfer = thisUnitsPowerTransfer > remainingPower;
                if (lastTransfer)
                {
                    thisUnitsPowerTransfer = remainingPower;
                }
                powerToMove += Mathf.CeilToInt(thisUnitsPowerTransfer);
                powerDict[sender] -= Mathf.CeilToInt(thisUnitsPowerTransfer);
                if (lastTransfer)
                {
                    var lineRendererToDestroy = powerUIDict[sender];
                    Destroy(lineRendererToDestroy);
                    powerUIDict.Remove(sender);
                    powerDict.Remove(sender);
                }
            }
            SetTargetPosition(target.transform.position);
            target.power += powerToMove;
            if (powerDict.Count <= 0)
            {
                DestroyWeb();
            }
        }
        else
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            // Casts the ray and get the first game object hit
            Physics.Raycast(ray, out hit);
            if (hit.collider != null)
            {
                SetTargetPosition(hit.point);
            }
        }
    }
}