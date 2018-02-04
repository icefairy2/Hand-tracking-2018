using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
///     Apply Mapping to Wrist Node
/// </summary>
public class RealMapping : HandMapping {
    public GameObject thumb;
    public GameObject index;
    public GameObject middle;
    public GameObject ring;
    public GameObject pinky;

    private int counter;
    private Joint[] joints;

    public override Joint[] GetJoints() {
        joints = new Joint[size];
        
        if (thumb == null || index == null || middle == null || ring == null || pinky == null){
            Debug.LogError("Fingers not assigned to hand mapping");
            return joints;
        }

        counter = 0;
        //Root
        joints[counter++] = new Joint(this.transform);
        //Middle of hand (missing)
        counter++;
        //Thumb
        IterateFinger(thumb, 4);
        //Index
        IterateFinger(index, 4);
        //Middle
        IterateFinger(middle, 4);
        //Ring
        IterateFinger(ring, 4);
        //Pinky
        IterateFinger(pinky, 4);

        SetNeighbours(joints);
        
        return joints;
    }

    protected void IterateFinger(GameObject finger, int length = 4)
    {
        Transform t = finger.transform;
        for(int i = 0; i < length; i++)
        {
            joints[counter++] = new Joint(t);
            if (i + 1 < length && t.childCount > 0) {
                t = t.GetChild(0);
            }
        }
    }
}
