using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class HideLayerOnToggle : MonoBehaviour {

    public GameObject layerObjectReference;
    public Camera mainCamera;
    int layer;
	// Use this for initialization
	void Awake () {
        GetComponent<Toggle>().onValueChanged.AddListener(AdjustDisplayedObjects);
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        layer = layerObjectReference.layer;
	}

    void AdjustDisplayedObjects(bool val)
    {
        if (val)
        {
            mainCamera.cullingMask |= (1 << layer);// associatedLayer.value);
        }
        else
        {
            mainCamera.cullingMask &= ~(1 << layer);// associatedLayer.value);
        }
    }
}
