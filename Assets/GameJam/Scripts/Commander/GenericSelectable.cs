using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

public class GenericSelectable : ADamageable, ISelectHandler, IPointerClickHandler, IPointerEnterHandler, IDeselectHandler, IBuildable
{

    public List<AContextMenuButton> menu;

    public static HashSet<GenericSelectable> allMySelectables = new HashSet<GenericSelectable>();
    public static HashSet<GenericSelectable> currentlySelected = new HashSet<GenericSelectable>();
    Renderer myRenderer;

    [SerializeField]int buildPowerCost;
    public int PowerCost {
        get{
            return buildPowerCost;
        }
    }
    public int power;
    Material unselectedMaterial;
    [SerializeField]
    Material selectedMaterial;
    public bool selected { get; set; }
    protected override void Awake()
    {
        base.Awake();
        allMySelectables.Add(this);
        myRenderer = transform.GetComponentsInChildren<Renderer>()[0];
        unselectedMaterial = myRenderer.material;
    }

    public override void TakeDamage(int playerColor, int amount, Vector3 hitPoint)
    {
        base.TakeDamage(playerColor, amount, hitPoint);
        power = currentHealth;
    }

    void Update()
    {
        currentHealth = power; //TODO - temporary way of combining power and health into one thing
    }

	public IBuildable CreateBuildableInstance()
    {
        return Instantiate(this);
    }
    public float GetObjectDepth()
    {
        BoxCollider bc = GetComponent<BoxCollider>();
        if (bc != null)
        {
           return bc.size.z * transform.lossyScale.z;
        }
        return 0f;//Assume boxCollider?
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
        OnSelect(eventData, true);
    }

    public virtual void OnSelect(BaseEventData eventData, bool selectionFinished)
    {
        selected = true;
        currentlySelected.Add(this);
        myRenderer.material = selectedMaterial;
        if (menu != null && menu.Count > 0 && selectionFinished)
        {
            ContextMenuManager.instance.AddMenu(menu);
        }
        else
        {
            ContextMenuManager.instance.CloseMenu();
        }
    }

    public void OnDeselect(BaseEventData eventData = null)
    {
        selected = false;
        myRenderer.material = unselectedMaterial;
        ContextMenuManager.instance.CloseMenu();
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