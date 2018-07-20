using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnMouseEnter()
    {
        Debug.Log("mouse entered");
        //instantiatedObject = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
    }

    void OnMouseExit()
    {
        Debug.Log("mouse exit");
        //Destroy(instantiatedObject);
    }
}
