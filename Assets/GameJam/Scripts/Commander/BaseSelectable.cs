using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseSelectable : GenericSelectable, IBeginDragHandler, IDragHandler, IEndDragHandler {
    
    public int basePowerRegenerationRate;
    public GenericSelectable turret;
    public GenericSelectable unit;
    public CircleCollider2D radiusCalculation;
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
                web.AddPower(selectable as BaseSelectable, Mathf.RoundToInt(selectable.power * PowerPercentageSlider.instance.powerPercentageOnDrag / 100f));
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

    int frameOfBuildableStart;
    IBuildable buildableBeingPlaced;
    public void PlaceBuildable(IBuildable buildable)
    {
        if (buildableBeingPlaced != null)
        {
            Debug.Log("Already Placing GameObject " +  buildableBeingPlaced);
            return;
        }
        if (buildable.PowerCost > power)
        {
            Debug.Log("Not enough power to build this object");
            return;
        }
        buildableBeingPlaced = buildable.CreateBuildableInstance();
        power -= buildableBeingPlaced.PowerCost;
        radiusCalculation.gameObject.SetActive(true);
        frameOfBuildableStart = Time.frameCount;
    }

    public new float GetObjectDepth()
    {
        return GetComponent<SphereCollider>().radius * transform.lossyScale.z;
    }
    float lastRegenTime = 0f;
    void Update()
    {
        if (buildableBeingPlaced != null)
        {
            float maxDistance = radiusCalculation.radius * radiusCalculation.transform.lossyScale.x;
            float minDistance = GetObjectDepth() + buildableBeingPlaced.GetObjectDepth();
            var mousePos = Input.mousePosition;
            mousePos.z = 1000f;//Camera.current.transform.position.z - transform.position.z;    // we want 2m away from the camera position
            var objectPos = Camera.main.ScreenToWorldPoint(mousePos);

            float t = 0f;
            if (objectPos.y != Camera.main.transform.position.y) 
            {
                t = (transform.position.y - Camera.main.transform.position.y) / (objectPos.y - Camera.main.transform.position.y); 
            }
            Vector3 newObjectPos = new Vector3();
            newObjectPos.x = Camera.main.transform.position.x + (objectPos.x - Camera.main.transform.position.x) * t;
            newObjectPos.y = Camera.main.transform.position.y + (objectPos.y - Camera.main.transform.position.y) * t;
            newObjectPos.z = Camera.main.transform.position.z + (objectPos.z - Camera.main.transform.position.z) * t;

            var distance = Vector3.Distance(newObjectPos, transform.position);
            Vector3 direction = (newObjectPos - transform.position).normalized; 

            if (Vector3.Distance(newObjectPos, transform.position) > maxDistance)
            {
                newObjectPos = transform.position + direction * maxDistance;
            }
            else if (distance < minDistance)
            {
                newObjectPos = transform.position + direction * minDistance;
            }

            buildableBeingPlaced.transform.position = newObjectPos;////Camera.main.transform.position - (direction.normalized * 2f);//objectPos;
            buildableBeingPlaced.transform.LookAt(2 * buildableBeingPlaced.transform.position - transform.position);// look away from base.
            /*var direction =  Camera.main.transform.position - objectPos;

            var yDifference = transform.position.y - objectPos.y;
            var angle = Mathf.Deg2Rad * Vector3.Angle(Vector3.up, direction);
            var distance = yDifference/(1/Mathf.Cos(angle));
            var transformed = (direction.normalized * distance);
            var newObjectPos = objectPos + (direction.normalized * distance);
            //Debug.Log(objectPos + " + " + transformed + " = " + newObjectPos);

            //placingGameObject.transform.position = newObjectPos;
           // Debug.Log(objectPos.y + " - " + transform.position.y);
           // Vector3 direction = placingGameObject.transform.position - transform.position;*/
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                power += buildableBeingPlaced.PowerCost;
                Destroy(buildableBeingPlaced.gameObject);
                buildableBeingPlaced = null;
                radiusCalculation.gameObject.SetActive(false);
            }

            if (Input.GetMouseButtonUp(0) && Time.frameCount > frameOfBuildableStart + 1)
            {
                buildableBeingPlaced = null;
                radiusCalculation.gameObject.SetActive(false);
            }
        }
        else if (selected && Input.GetKeyUp(KeyCode.T))
        {
            PlaceBuildable(turret);
        }
        else if (selected && Input.GetKeyUp(KeyCode.U))
        {
            PlaceBuildable(unit);
        }

        if (Time.time > lastRegenTime + basePowerRegenerationRate)
        {
            lastRegenTime = Time.time;
            power++;
        }
        BasePowerDisplayer.instance.DisplayPower(this);
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
