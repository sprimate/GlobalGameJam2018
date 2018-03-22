using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class HideLayerOnToggle : MonoBehaviour {

    public LayerMask layerMask;
    public Camera mainCamera;
	// Use this for initialization
	void Awake () {
        GetComponent<Toggle>().onValueChanged.AddListener(AdjustDisplayedObjects);
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
	}

    void AdjustDisplayedObjects(bool val)
    {

        if (val)
        {
            mainCamera.cullingMask = layerMask.AddToLayer(mainCamera.cullingMask);
           // mainCamera.cullingMask |= (1 << layer);// associatedLayer.value);
        }
        else
        {
            mainCamera.cullingMask = layerMask.RemoveFromLayer(mainCamera.cullingMask);
//            mainCamera.cullingMask &= ~(1 << layer);// associatedLayer.value);
        }
    }
}
