using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class GenericSelectable : MonoBehaviour, ISelectHandler, IPointerClickHandler, IPointerEnterHandler, IDeselectHandler
{

    public static HashSet<GenericSelectable> allMySelectables = new HashSet<GenericSelectable>();
    public static HashSet<GenericSelectable> currentlySelected = new HashSet<GenericSelectable>();
    public MenuButtonContainer contextMenu;
    Renderer myRenderer;

    public bool alive { get; protected set; }
    public int power;
    Material unselectedMaterial;
    [SerializeField]
    Material selectedMaterial;
    public bool selected { get; set; }
    protected virtual void Awake()
    {
        alive = true;
        allMySelectables.Add(this);
        myRenderer = transform.GetComponentsInChildren<Renderer>()[0];
        unselectedMaterial = myRenderer.material;
    }

    public static HashSet<Type> GetSelectableTypes()
    {
        HashSet<Type> ret = new HashSet<Type>();
        foreach (var s in currentlySelected)
        {
            ret.Add(s.GetType());
        }
        return ret;
    }

    public static bool IsAllSelectedThisType(GenericSelectable selectable)
    {
        HashSet<Type> types = GetSelectableTypes();
        if (types.Count != 1)
        {
            return false;
        }
        return types.Contains(selectable.GetType());
    }

    public bool IsAllSelectedThisType()
    {
        return IsAllSelectedThisType(this);
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

    public void OnDeselect(BaseEventData eventData = null)
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
        if (except)
        {
            var selectable = except.GetComponent<GenericSelectable>();
            if (selectable != null)
            {
                currentlySelected.Add(selectable);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer Entered");
    }
}