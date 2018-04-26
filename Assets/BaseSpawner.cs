using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSpawner : MonoBehaviour {

    public Camera commanderCamera;
    public GameObject basePrefab;
    public AContextMenuButton contextMenuButton;
    public int numberOfBases = 10;
    public Transform centeredTransform;
    public float circleRadius = 30f;
	// Use this for initialization
	IEnumerator Start () {

        while (!GameJamGameManager.instance.gameStarted)
        {
            yield return null;
        }
        float vertexAngleFrequency = 360f / numberOfBases;
        float startAngle = numberOfBases % 2 == 0 ? vertexAngleFrequency / 2 : 90f;

        for (int i = 0; i < numberOfBases; i++)
        {
            var currentAngle = startAngle + i * vertexAngleFrequency;
            float xOffset = circleRadius * Mathf.Cos(currentAngle * Mathf.Deg2Rad);
            float zOffset = circleRadius * Mathf.Sin(currentAngle * Mathf.Deg2Rad);
            Vector3 position = new Vector3(centeredTransform.position.x + xOffset, centeredTransform.position.y, centeredTransform.position.z + zOffset);
            Quaternion rotation = Quaternion.LookRotation(position - centeredTransform.position);
            GameObject baseInstance = PhotonNetwork.Instantiate(basePrefab.name, position, rotation, 0, null);
            BaseSelectable b = baseInstance.GetComponent<BaseSelectable>();
            b.commanderCamera = commanderCamera;
            b.menu = new List<AContextMenuButton>();
            b.menu.Add(contextMenuButton);
            baseInstance.transform.SetParent(transform);
        }
    }
}
