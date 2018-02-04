using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour {
    public GameObject c0;
    public GameObject c1;
    public GameObject c2;
    public GameObject p0;

    private Transform joint;
    private List<Neighbour> n;
    private Vector3 originalV;
    private Quaternion originalRotation;

    public class Neighbour {
        public Transform t;
        public bool child;

        public Neighbour(Transform t, bool child) {
            this.t = t;
            this.child = child;
        }
    }

	// Use this for initialization
	void Start () {
        joint = transform;
        n = new List<Neighbour>();
        if (c0 != null)
            n.Add(new Neighbour(c0.transform, true));
        if (c1 != null)
            n.Add(new Neighbour(c1.transform, true));
        if (c2 != null)
            n.Add(new Neighbour(c2.transform, true));
        if (p0 != null)
            n.Add(new Neighbour(p0.transform, false));

        Vector3 v = Vector3.zero;
        Vector3 parent = joint.position;
        int childcount = 0;
        foreach (Neighbour p in n) {
            if (!p.child) {
                parent = p.t.position;
            } else {
                v += p.t.position;
                childcount++;
            }
        }

        Vector3 direction = v - parent * childcount;
        originalV = direction + parent - joint.position;
        originalRotation = joint.rotation;
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 v = Vector3.zero;
        Vector3 parent = joint.position;
        int childcount = 0;
		foreach(Neighbour p in n) {
            if (!p.child) {
                parent = p.t.position;
            } else {
                v += p.t.position;
                childcount++;
            }
        }

        Vector3 direction = v - parent * childcount;

        Quaternion rotation = Quaternion.FromToRotation(originalV, direction + parent - joint.position);
        Debug.Log(originalV);
        Debug.Log(direction + parent - joint.position);

        Debug.DrawLine(parent, direction + parent, Color.red);
        Debug.DrawLine(joint.position, direction + parent, Color.blue);
        
        joint.rotation = rotation * originalRotation;


    }
}
