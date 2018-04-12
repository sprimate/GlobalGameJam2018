using CompleteProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagmaBeam : APaladinAbility {

    public GameObject rangeVisualizer;
    public GameObject projectile;
    public GameObject projectileSpeed;
	bool aboutToFire;
    protected override void Use()
    {
        if (aboutToFire)
        {
            Fire();
        }
        else
        {
            var player = GameJamGameManager.instance.GetPlayer(GameJamGameManager.LocalPlayerId).targetVisualizationPoint;
            rangeVisualizer.transform.SetParent(GameJamGameManager.instance.GetPlayer(GameJamGameManager.LocalPlayerId).targetVisualizationPoint.transform, true);
            rangeVisualizer.transform.localPosition = Vector3.zero;
            rangeVisualizer.transform.localRotation = Quaternion.Euler(Vector3.zero);
            rangeVisualizer.transform.GetChild(0).gameObject.SetActive(true);
            StartCoroutine(UpdateMeshRenderersAfterFrame());
            aboutToFire = true;
        }
    }

    IEnumerator UpdateMeshRenderersAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        rangeVisualizer.gameObject.GetComponentInChildren<MeshRenderer>().enabled = true;
    }



    void Fire()
	{
        var targets = new List<Enemy>();
        foreach (var t in rangeVisualizer.gameObject.GetComponentInChildren<CollisionTracker>().collidingObjects)
        {
            Enemy e = t.GetComponentInChildren<Enemy>();
            if (e != null)
            {
                targets.Add(e);
            }
        }
        Player player = GameJamGameManager.instance.GetPlayer(GameJamGameManager.LocalPlayerId);

        foreach (var target in targets)
        {
            GameObject fireball = Instantiate(projectile);
            HardFollow follow = fireball.AddComponent<HardFollow>();
            follow.sticky = true;
            follow.target = target.transform;
        }

        Debug.Log("Firing Magma Beam (Confirmed) from " + player, player);
        Cancel();
	}

	void Cancel()
	{
        Debug.Log("Done With Magma Beam");
        rangeVisualizer.transform.SetParent(transform);
        rangeVisualizer.transform.GetChild(0).gameObject.SetActive(false);

        if (aboutToFire)
		{
			aboutToFire = false;
		}
	}
}
