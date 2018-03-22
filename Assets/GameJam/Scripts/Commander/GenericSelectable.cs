using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class GenericSelectable : MonoBehaviour, ISelectHandler, IPointerClickHandler, IPointerEnterHandler, IDeselectHandler
{

    public static HashSet<GenericSelectable> allMySelectables = new HashSet<GenericSelectable>();
    public static HashSet<GenericSelectable> currentlySelected = new HashSet<GenericSelectable>();

    Renderer myRenderer;

    Material unselectedMaterial;
    [SerializeField]
    Material selectedMaterial;
    public bool selected { get; set; }
    protected virtual void Awake()
    {
        allMySelectables.Add(this);
        myRenderer = transform.GetComponentsInChildren<Renderer>()[0];
        unselectedMaterial = myRenderer.material;

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        bool shifting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (!shifting)
        {
            DeselectAll(eventData);
        }

        bool justDeselected = selected;
        if (selected)
        {
            OnDeselect(eventData);
        }

        if (eventData.clickCount == 2)
        {
            foreach (var selectable in allMySelectables)
            {
                Type t = GetType();
                if (GetType() == selectable.GetType())
                {
                    selectable.OnSelect(eventData);
                }
            }
        }
        else if (!justDeselected)
        {
            OnSelect(eventData);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        selected = true;
        currentlySelected.Add(this);
        myRenderer.material = selectedMaterial;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selected = false;
        myRenderer.material = unselectedMaterial;
    }

    public static void DeselectAll(BaseEventData eventData, GameObject except = null)
    {
        foreach (GenericSelectable selectable in currentlySelected)
        {
            if (selectable.gameObject != except)
            {
                selectable.OnDeselect(eventData);
            }
        }
        currentlySelected.Clear();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer Entered");
    }
}