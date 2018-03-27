using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseSelectable : GenericSelectable, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public static float powerPercentageOnDrag = 50;
    Dictionary<BaseSelectable, int> movingPower = new Dictionary<BaseSelectable, int>();
    bool currentlyDraggingPower;
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsAllSelectedThisType())
        {
            currentlyDraggingPower = true;
            PowerWeb web = PowerWebManager.instance.GetNewPowerWeb();
            foreach (var selectable in currentlySelected)
            {
                web.AddPower(selectable as BaseSelectable, Mathf.RoundToInt(selectable.power * powerPercentageOnDrag / 100f));
            }
        }
    }

    public void CancelPowerTransfer()
    {
        PowerWebManager.instance.CancelAllTargetedPowerTransfers(this);
    }

    public void OnDrag(PointerEventData eventData)
    {

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentlyDraggingPower)
        {
            var nextObject = DragSelectionHandler.instance.GetNextRaycastedObject(eventData);
            BaseSelectable baseTarget = nextObject != null ? nextObject.GetComponent<BaseSelectable>() : null;
            if (baseTarget != null)
            {
                PowerWebManager.instance.SetWebTarget(baseTarget);
                baseTarget.OnSelect(eventData);
            }
            else
            {
                PowerWebManager.instance.CancelUntargetedPowerTransfer();
            }
        }
        movingPower = new Dictionary<BaseSelectable, int>();
    }

    bool sendPowerCancelled;
    public IEnumerator SendPower(BaseSelectable target, int powerToSend)
    {
        while (!sendPowerCancelled && alive)
        {

            yield return null;
        }
    }
}
