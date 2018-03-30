using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretSelectable : GenericSelectable {

	public new float GetObjectDepth(){
		return GetComponent<CapsuleCollider>().radius * transform.lossyScale.z;
	}
}
