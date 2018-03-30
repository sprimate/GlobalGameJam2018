using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 public class TestScript : MonoBehaviour
 {
     Transform circle;
     public float scaleSpeed, rotSpeed;
 
     void Awake()
     {
         circle = transform.Find("Circle");
     }
 
     void Update()
     {
 
         float dxz = Input.GetAxis("Vertical") * scaleSpeed * Time.deltaTime;
         circle.localScale += new Vector3(dxz, 0.0f, dxz);
         circle.Rotate(Vector3.up * rotSpeed * Time.deltaTime);
     }
 }