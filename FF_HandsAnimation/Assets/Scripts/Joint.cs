using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Joint {
    public Transform transform; //Transform of the Joint object in the scene

    public class Neighbour {
        public readonly int index;
        public Joint joint;
        public Neighbour(int index, Joint joint) {
            this.index = index;
            this.joint = joint;
        }
    }

    public List<Neighbour> neighbours; //All other joints that share a bone with this joint

    //Transforms of points that are not tracked, but should be moved statically with
    //this joint
    public List<Joint> extensions;

    //Position and rotation of the joint before tracking
    public Vector3 originalPosition;
    public Quaternion originalRotation;
    public Vector3 originalOrientation;

    public Joint(Transform transform) {
        this.transform = transform;
        this.neighbours = new List<Neighbour>();
        this.extensions = new List<Joint>();
        this.originalPosition = new Vector3();
        this.originalRotation = new Quaternion();
    }
}
