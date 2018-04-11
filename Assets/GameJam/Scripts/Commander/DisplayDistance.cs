using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayDistance : MonoBehaviour {

    public Transform target;
    Vector3 targetPos;
    public void OnGUI()
    {
        if (target != null)
        {
            GUI.Label(new Rect(10, 10, 100, 20),"Dist: " + Vector3.Distance(target.transform.position, transform.position).ToString());
        }
    }

    public void Update()
    {
        if (target != null)
        {
            transform.LookAt(target);
            var main = GetComponent<ParticleSystem>().main;
            main.startLifetime = Vector3.Distance(transform.position, target.position) / main.startSpeed.constant;
        }
    }
}
