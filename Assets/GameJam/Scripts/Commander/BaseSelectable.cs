using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseSelectable : GenericSelectable, IBeginDragHandler, IDragHandler, IEndDragHandler {
    
    [SerializeField] Camera commanderCamera;
    public int basePowerRegenerationRate;
    public GenericSelectable turret;
    public GenericSelectable unit;
    public CircleCollider2D radiusCalculation;
    bool currentlyDraggingPower;
    BorderSpawner borderSpawner;

    public GenericSelectable unitToSpawnOnTimer;
    public float spawnRate;
    float lastSpawn;

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
            var objectPos = commanderCamera.ScreenToWorldPoint(mousePos);
            float t = 0f;
            if (objectPos.y != commanderCamera.transform.position.y) 
            {
                t = (transform.position.y - commanderCamera.transform.position.y) / (objectPos.y - commanderCamera.transform.position.y); 
            }
            Vector3 newObjectPos = new Vector3();
            newObjectPos.x = commanderCamera.transform.position.x + (objectPos.x - commanderCamera.transform.position.x) * t;
            newObjectPos.y = commanderCamera.transform.position.y + (objectPos.y - commanderCamera.transform.position.y) * t;
            newObjectPos.z = commanderCamera.transform.position.z + (objectPos.z - commanderCamera.transform.position.z) * t;

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

            buildableBeingPlaced.transform.position = newObjectPos;////commanderCameratransform.position - (direction.normalized * 2f);//objectPos;
            buildableBeingPlaced.transform.LookAt(2 * buildableBeingPlaced.transform.position - transform.position);// look away from base.
           
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
        HandleSpawns();
    }

void HandleSpawns()
	{
		if (!PhotonNetwork.player.IsMasterClient)
		{
			return;
		}
		
		if (lastSpawn + spawnRate < Time.time)
		{
            if (power < unitToSpawnOnTimer.PowerCost)
            {
                return;
            }
            float maxDistance = radiusCalculation.radius * radiusCalculation.transform.lossyScale.x;
            float minDistance = GetObjectDepth() + unitToSpawnOnTimer.GetObjectDepth();

            //Debug.Log("Radius: " + radius);
			var spawnDistance = Random.Range(minDistance, maxDistance);
			var randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f,1f));
			Vector3 spawnPosition = transform.position + (randomDirection * spawnDistance);		
			//toSpawn.enemyColorId = Random.Range(1, 3); //3 is exclusive
			PhotonNetwork.InstantiateSceneObject(unitToSpawnOnTimer.name, spawnPosition, Quaternion.LookRotation(randomDirection), 0, null);
            power -= unitToSpawnOnTimer.PowerCost;	
            lastSpawn = Time.time;
		}
	}
}
