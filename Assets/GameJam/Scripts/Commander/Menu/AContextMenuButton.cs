using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public abstract class AContextMenuButton : MonoBehaviour {

    public void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Action);

    }

	public abstract void Action();
}
