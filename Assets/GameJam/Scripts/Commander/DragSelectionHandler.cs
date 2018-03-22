using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class DragSelectionHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerDownHandler
{

    [SerializeField]
    Image selectionBoxImage;

    Vector2 startPosition;
    Rect selectionRect;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!Input.GetKey(KeyCode.Mouse0))
        {
            return;
        }

        selectionBoxImage.gameObject.SetActive(true);
        startPosition = eventData.position;
        selectionRect = new Rect();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!Input.GetKey(KeyCode.Mouse0))
        {
            return;
        }
        if (eventData.position.x < startPosition.x)
        {
            selectionRect.xMin = eventData.position.x;
            selectionRect.xMax = startPosition.x;
        }
        else
        {
            selectionRect.xMin = startPosition.x;
            selectionRect.xMax = eventData.position.x;
        }

        if (eventData.position.y < startPosition.y)
        {
            selectionRect.yMin = eventData.position.y;
            selectionRect.yMax = startPosition.y;
        }
        else
        {
            selectionRect.yMin = startPosition.y;
            selectionRect.yMax = eventData.position.y;
        }

        selectionBoxImage.rectTransform.offsetMin = selectionRect.min;
        selectionBoxImage.rectTransform.offsetMax = selectionRect.max;
        CheckSelections(eventData);
       
    }

    void CheckSelections(PointerEventData eventData)
    {
        foreach (GenericSelectable selectable in GenericSelectable.allMySelectables)
        {
            if (Camera.main.cullingMask != (Camera.main.cullingMask | (1 << selectable.gameObject.layer))) //if the main camera is filtering out this selectable
            {
                continue;
            }
            if (selectionRect.Contains(Camera.main.WorldToScreenPoint(selectable.transform.position)))
            {
                selectable.OnSelect(eventData);
            }
            else if (!Shifting() && selectable.selected)
            {
                selectable.OnDeselect(eventData);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!Input.GetKeyUp(KeyCode.Mouse0))
        {
            return;
        }

        selectionBoxImage.gameObject.SetActive(false);

        CheckSelections(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        float myDistance = 0;

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == gameObject)
            {
                myDistance = result.distance;
                break;
            }
        }

        GameObject nextObject = null;
        float maxDistance = Mathf.Infinity;

        foreach (RaycastResult result in results)
        {
            if (result.distance > myDistance && result.distance < maxDistance)
            {
                nextObject = result.gameObject;
                maxDistance = result.distance;
            }
        }

        if (nextObject)
        {
            ExecuteEvents.Execute<IPointerClickHandler>(nextObject, eventData, (x, y) => { x.OnPointerClick((PointerEventData)y); });
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!Shifting())
        {
            GenericSelectable.DeselectAll(new BaseEventData(EventSystem.current));
        }
    }

    bool Shifting()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }
}