using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AConfirmedProjectileAbility : AConfirmedPaladinAbility {
    public Transform rangeVisualizer;

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

    IEnumerator UpdateMeshRenderersAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        foreach (var renderer in rangeVisualizer.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = true;
        }
    }

    protected override void RevertSetup()
    {
        rangeVisualizer.transform.SetParent(transform);
        rangeVisualizer.transform.GetChild(0).gameObject.SetActive(false);
    }
}
