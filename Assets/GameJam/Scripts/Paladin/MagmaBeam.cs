using CompleteProject;
using DigitalRuby.PyroParticles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagmaBeam : AConfirmedPaladinAbility {

    public Transform rangeVisualizer;
    public GameObject fireProjectile;
    public int numProjectiles = 3;
    public float untargetedProjectileAngle = 40f;

    protected override bool Setup()
    {
        var player = GameJamGameManager.instance.GetPlayer(GameJamGameManager.LocalPlayerId).targetVisualizationPoint;
        rangeVisualizer.transform.SetParent(GameJamGameManager.instance.GetPlayer(GameJamGameManager.LocalPlayerId).targetVisualizationPoint.transform, true);
        rangeVisualizer.transform.localPosition = Vector3.zero;
        rangeVisualizer.transform.localRotation = Quaternion.Euler(Vector3.zero);
        rangeVisualizer.transform.GetChild(0).gameObject.SetActive(true);
        StartCoroutine(UpdateMeshRenderersAfterFrame());
        return true;
    }

    protected override void RevertSetup()
    {
        rangeVisualizer.transform.SetParent(transform);
        rangeVisualizer.transform.GetChild(0).gameObject.SetActive(false);
    }

    protected override void Confirm()
    {
        var targets = new List<ADamageable>();
        foreach (var t in rangeVisualizer.gameObject.GetComponentInChildren<CollisionTracker>(true).collidingObjects)
        {
            ADamageable e = t.GetComponentInChildren<ADamageable>();
            if (e != null)
            {
                targets.Add(e);
            }
        }
        Player player = GameJamGameManager.instance.GetPlayer(GameJamGameManager.LocalPlayerId);

        foreach (var target in targets)
        {
            BeginEffect(target.transform.position);
        }


        if (targets.Count == 0)
        {
            var anglePer = untargetedProjectileAngle * 2 / (numProjectiles - 1);
            for (var i = 0; i < numProjectiles; i++)
            {
                var angle = -untargetedProjectileAngle + (i * anglePer);
                transform.position = rangeVisualizer.position;
                Vector3 targetRotationEuler = rangeVisualizer.rotation.eulerAngles;
                targetRotationEuler.y -= angle;
                transform.rotation = Quaternion.Euler(targetRotationEuler);
                transform.Translate(Vector3.forward * 100f);
                BeginEffect(transform.position);
            }
        }

        Cancel();
        base.Confirm();
    }

    IEnumerator UpdateMeshRenderersAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        rangeVisualizer.gameObject.GetComponentInChildren<MeshRenderer>().enabled = true;
    }
    private GameObject currentPrefabObject;
    private FireBaseScript currentPrefabScript;

    private void BeginEffect(Vector3 targetPosition)
    {
        Vector3 pos;
        float yRot = transform.rotation.eulerAngles.y;
        Vector3 forwardY = Quaternion.Euler(0.0f, yRot, 0.0f) * Vector3.forward;
        Vector3 forward = rangeVisualizer.forward;
        Vector3 right = rangeVisualizer.right;
        Vector3 up = rangeVisualizer.up;
        Quaternion rotation = Quaternion.identity;
        currentPrefabObject = GameObject.Instantiate(fireProjectile.gameObject);
        currentPrefabScript = currentPrefabObject.GetComponent<FireConstantBaseScript>();
        if (currentPrefabScript == null)
        {
            // temporary effect, like a fireball
            currentPrefabScript = currentPrefabObject.GetComponent<FireBaseScript>();
            if (currentPrefabScript.IsProjectile)
            {
                // set the start point near the player
                rotation = rangeVisualizer.rotation;
                pos = rangeVisualizer.position + forward + right + up;
            }
            else
            {
                // set the start point in front of the player a ways
                pos = rangeVisualizer.position + (forwardY * 10.0f);
            }
        }
        else
        {
            // set the start point in front of the player a ways, rotated the same way as the player
            pos = rangeVisualizer.position + (forwardY * 5.0f);
            rotation = rangeVisualizer.rotation;
            pos.y = 0.0f;
        }

        FireProjectileScript projectileScript = currentPrefabObject.GetComponentInChildren<FireProjectileScript>();
        if (projectileScript != null)
        {
            // make sure we don't collide with other fire layers
            projectileScript.ProjectileCollisionLayers &= (~UnityEngine.LayerMask.NameToLayer("FireLayer"));
        }

        currentPrefabObject.transform.position = pos;
        currentPrefabObject.transform.LookAt(targetPosition);
        //currentPrefabObject.transform.rotation = rotation;
    }
}
