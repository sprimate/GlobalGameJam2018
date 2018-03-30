using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBuildable {
	IBuildable CreateBuildableInstance();
	float GetObjectDepth();
	int PowerCost {get;}
	Transform transform {get;}
	GameObject gameObject {get;}
}