using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class HandMapping : MonoBehaviour{
    protected int size = 22;

    /// <summary>
    ///     Maps the joints of a hand according to 
    ///     https://software.intel.com/sites/landingpage/realsense/camera-sdk/v1.1/documentation/html/index.html?doc_hand_hand_joints.html
    /// </summary>
    /// <returns>
    ///     Array with the transforms of the joints
    ///     Unmapped joints are null
    /// </returns>
    public abstract Joint[] GetJoints();

    /// <summary>
    ///     Helper function for inheriting classes 
    ///     Sets all the joint neighbours according to the bone specification
    /// </summary>
    protected void SetNeighbours(Joint[] joints) {
        if(joints[1] == null) {
            //Ignore middle of hand
            ApplyBone(joints, 0, 10);
        } else {
            ApplyBone(joints, 0, 1);
            ApplyBone(joints, 1, 10);
        }

        ApplyBone(joints, 0, 2);
        ApplyBone(joints, 0, 6);
        ApplyBone(joints, 0, 14);
        ApplyBone(joints, 0, 18);      

        ApplyFinger(joints, 2);
        ApplyFinger(joints, 6);
        ApplyFinger(joints, 10);
        ApplyFinger(joints, 14);
        ApplyFinger(joints, 18);
    }

    /// <summary>
    ///     Helper function for SetNeighbours
    /// </summary>
    private void ApplyBone(Joint[] joints, int index_a, int index_b) {
        Joint a = joints[index_a];
        Joint b = joints[index_b];
        if(a == null || b == null) {
            return;
        }
        a.neighbours.Add(new Joint.Neighbour(index_b, b));
        b.neighbours.Add(new Joint.Neighbour(index_a, a));
    }

    /// <summary>
    ///     Helper function for SetNeighbours
    /// </summary>
    private void ApplyFinger(Joint[] joints, int start, int length = 4) {
        for (int j = 0; j < length - 1; j++) {
            if (start + j + 1 >= size) {
                return;
            }
            ApplyBone(joints, start + j, start + j + 1);
        }
    }
}