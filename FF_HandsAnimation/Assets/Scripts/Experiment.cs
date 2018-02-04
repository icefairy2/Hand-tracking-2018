using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experiment : MonoBehaviour {
    public float duration = 5f;
    public float speed = 0.01f;

    private Transform root;
    private Transform bone;
	// Use this for initialization
	void Start () {
        root = transform;
        bone = transform.GetChild(1);
        Debug.Log("Child: " + root.GetChild(1).name);
	}
	
	// Update is called once per frame
	void Update () {
        if (Time.time % duration < duration/2) {
            Vector3 temp = bone.position;
            root.position += Vector3.one * speed;
            bone.position = temp + Vector3.one * speed;
        } else {
            
        }
	}
}
