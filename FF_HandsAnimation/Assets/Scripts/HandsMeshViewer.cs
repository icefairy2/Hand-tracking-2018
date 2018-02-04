/*******************************************************************************

Hands Mesh Viewer
Code structure adapted from Intel RealSense Unity SDK (HandsViewer.cs)

*******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HandsMeshViewer : MonoBehaviour
{
    public GameObject RightHand;
    public GameObject LeftHand;
    public float speed = 0.1f;
    public float rotateSpeedFactor = 200f;
    public float cameraSpeed = 60f;
    public bool applyRotation = true;
    public Vector3 nirwanha = Vector3.one * 1000f; //position where the hands are hidden from the camera

    private Joint[,] myJoints; //Array of joints
    //The tracking positions are offset for all joints so that the hand is in front of the camera
    private Vector3[] trackingOffset = { Vector3.zero, Vector3.zero };
    private bool[] handWasTracked = { false, false }; //Stores whether the hand was tracked in the previous frame

    private PXCMHandData.JointData[,] jointData; //non-smooth joint values
    private PXCMSmoother smoother = null; //Smoothing module instance
    private PXCMSmoother.Smoother3D[,] smoother3D = null; //smooth joint values
    public int smoothing = 4; //smoothing factor

    private PXCMSenseManager sm = null; //SenseManager Instance
    private pxcmStatus sts; //StatusType Instance
    private PXCMHandModule handAnalyzer; //Hand Module Instance
    private int MaxHands = 2; //Max Hands
    private int MaxJoints = PXCMHandData.NUMBER_OF_JOINTS; //Max Joints

    private Hashtable handList; //keep track of bodyside and hands for GUItext

    // Use this for initialization
    void Start()
    {
        handList = new Hashtable();

        /* Initialize a PXCMSenseManager instance */
        sm = PXCMSenseManager.CreateInstance();
        if (sm == null)
            Debug.LogError("SenseManager Initialization Failed");

        /* Enable hand tracking and retrieve an hand module instance to configure */
        sts = sm.EnableHand();
        handAnalyzer = sm.QueryHand();
        if (sts != pxcmStatus.PXCM_STATUS_NO_ERROR)
            Debug.LogError("PXCSenseManager.EnableHand: " + sts);

        /* Initialize the execution pipeline */
        sts = sm.Init();
        if (sts != pxcmStatus.PXCM_STATUS_NO_ERROR)
            Debug.LogError("PXCSenseManager.Init: " + sts);

        /* Retrieve the the DataSmoothing instance */
        sm.QuerySession().CreateImpl<PXCMSmoother>(out smoother);

        /* Create a 3D Weighted algorithm */
        smoother3D = new PXCMSmoother.Smoother3D[MaxHands,MaxJoints];

        /* Configure a hand - Enable Gestures and Alerts */
        PXCMHandConfiguration hcfg = handAnalyzer.CreateActiveConfiguration();
        if (hcfg != null) {
            hcfg.EnableAllGestures();
            hcfg.EnableAlert(PXCMHandData.AlertType.ALERT_HAND_NOT_DETECTED);
            hcfg.ApplyChanges();
            hcfg.Dispose();
        }
        
        myJoints = new Joint[MaxHands, MaxJoints];

        Joint[] joints;
        if (RightHand != null) {
            HandMapping RightMapping = RightHand.GetComponent<HandMapping>();
            joints = RightMapping.GetJoints();
            for (int j = 0; j < MaxJoints; j++) {
                myJoints[0, j] = joints[j];
            }
        }
        
        if (LeftHand != null) {
            HandMapping LeftMapping = LeftHand.GetComponent<HandMapping>();
            joints = LeftMapping.GetJoints();
            for (int j = 0; j < MaxJoints; j++) {
                myJoints[1, j] = joints[j];
            }
        }

        jointData = new PXCMHandData.JointData[MaxHands, MaxJoints];

        for (int i = 0; i < MaxHands; i++) {
            for (int j = 0; j < MaxJoints; j++) {
                Joint joint = myJoints[i, j];
                if (joint != null) {
                    joint.originalPosition = joint.transform.position;
                    joint.originalRotation = joint.transform.rotation;

                    //Calculate orientation
                    Vector3 v = Vector3.zero;
                    Vector3 parent = joint.transform.position;
                    int childcount = 0;

                    foreach (Joint.Neighbour n in joint.neighbours) {
                        if (n.index < j) {
                            parent = n.joint.transform.position;
                        } else {
                            v += n.joint.transform.position;
                            childcount++;
                        }
                    }
                    Vector3 direction = v - parent * childcount;
                    joint.originalOrientation = direction + parent - joint.transform.position;

                    //Joint extensions
                    foreach (Joint e in joint.extensions) {
                        e.originalPosition = e.transform.position;
                        e.originalRotation = e.transform.rotation;
                        e.originalOrientation = joint.transform.position - e.transform.position;
                    }
                    
                }

                smoother3D[i, j] = smoother.Create3DWeighted(smoothing);
                jointData[i, j] = new PXCMHandData.JointData();

                //Test output to check Hand Mapping
                if (myJoints[i, j] == null) {
                    Debug.Log("null");
                } else {
                    Debug.Log(myJoints[i, j].transform.name);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        /* Make sure SenseManager Instance is valid */
        if (sm == null)
            return;

        /* Wait until any frame data is available */
        if (sm.AcquireFrame(false) != pxcmStatus.PXCM_STATUS_NO_ERROR)
            return;

        /* Retrieve hand tracking Module Instance */
        handAnalyzer = sm.QueryHand();

        if (handAnalyzer != null)
        {
            /* Retrieve hand tracking Data */
            PXCMHandData _handData = handAnalyzer.CreateOutput();
            if (_handData != null)
            {
                _handData.Update();

                /* Retrieve all joint Data */
                bool[] someJointsDetected = { false, false };
                for (int i = 0; i < _handData.QueryNumberOfHands(); i++)
                {
                    PXCMHandData.IHand _iHand;
                    if (_handData.QueryHandData(PXCMHandData.AccessOrderType.ACCESS_ORDER_FIXED, i, out _iHand) == pxcmStatus.PXCM_STATUS_NO_ERROR)
                    {
                        for (int j = 0; j < MaxJoints; j++)
                        {
                            if (_iHand.QueryTrackedJoint((PXCMHandData.JointType)j, out jointData[i,j]) != pxcmStatus.PXCM_STATUS_NO_ERROR) {
                                jointData[i, j] = null;
                            } else if (jointData[i, j].confidence == 100) {
                                someJointsDetected[i] = true;
                            }
                        }
                        if (!handList.ContainsKey(_iHand.QueryUniqueId()))
                            handList.Add(_iHand.QueryUniqueId(), _iHand.QueryBodySide());
                    }
                }

                for (int i = 0; i < MaxHands; i++) {
                    if (!someJointsDetected[i] && handWasTracked[i]) {
                        for (int j = 0; j < MaxJoints; j++) {
                            Joint joint = myJoints[i, j];
                            if (joint == null) {
                                continue;
                            }
                            joint.transform.position = joint.originalPosition;
                            joint.transform.rotation = joint.originalRotation;

                            foreach (Joint e in joint.extensions) {
                                e.transform.position = e.originalPosition;
                                e.transform.rotation = e.originalRotation;
                            }
                        }
                    } else {
                        if (!handWasTracked[i]) {
                            if (myJoints[i,0] == null || jointData[i,0] == null || jointData[i,0].confidence < 100) {
                                break;
                            }
                            PXCMPoint3DF32 smoothedPoint = smoother3D[i, 0].SmoothValue(jointData[i,0].positionWorld);
                            Vector3 targetPosition = new Vector3(-1 * smoothedPoint.x, smoothedPoint.y, smoothedPoint.z);
                            trackingOffset[i] = targetPosition - myJoints[i, 0].originalPosition;
                        }
                        /* Smooth the data and move the joints*/
                        MoveJoints(i);
                    }
                    handWasTracked[i] = someJointsDetected[i];
                }
                
                
            }
            handAnalyzer.Dispose();
        }


        sm.ReleaseFrame();

        RotateCam();
    }

    void SwitchHands() {
        for(int j = 0; j < MaxJoints; j++) {
            Joint temp = myJoints[0, j];
            myJoints[0, j] = myJoints[1, j];
            myJoints[1, j] = temp;  
        }
        handWasTracked[0] = false;
        handWasTracked[1] = false;
    }

    //Close any ongoing Session
    void OnDisable()
    {
        if (smoother3D != null)
        {
            for (int i = 0; i < MaxHands; i++)
            {
                if (smoother3D[i,0] != null)
                {
                    for (int j = 0; j < MaxJoints; j++)
                    {
                        smoother3D[i,j].Dispose();
                        smoother3D[i,j] = null;
                    }
                }
            }
            smoother3D = null;
        }

        if (smoother != null)
        {
            smoother.Dispose();
            smoother = null;
        }

        if (sm != null)
        {
            sm.Close();
            sm.Dispose();
            sm = null;
        }
    }

    //Smoothen and Display the Joint Data by moving the Hand
    void MoveJoints(int i)
    {
        //Caclulate the target positions of the joints
        Vector3[] targetPositions = new Vector3[MaxJoints];
        for (int j = 0; j < MaxJoints; j++) {
            Joint joint = myJoints[i, j];
            PXCMHandData.JointData data = jointData[i, j];
            jointData[i, j] = null;
            if (joint == null || data == null || data.confidence != 100) {
                continue;
            }

            PXCMPoint3DF32 smoothedPoint = smoother3D[i, j].SmoothValue(data.positionWorld);
            Vector3 targetPosition = new Vector3(-1 * smoothedPoint.x, smoothedPoint.y, smoothedPoint.z);

            targetPosition -= trackingOffset[i];

            //Clamp the offset
            Vector3 offset = targetPosition - joint.transform.position;
            if (offset.magnitude > speed) {
                offset = offset.normalized * speed;
            }
            targetPosition = joint.transform.position + offset;

            //Ensure that the bone length to the parent joint is kept
            //Should only have one valid iteration, because joints only have one parent
            //The joints are iterated hierarchically, so that the position of the joint's parent is already updated here
            foreach (Joint.Neighbour neighbour in joint.neighbours) {
                if (neighbour.index >= j) { // = Is neighbour a child?
                    continue;
                }
                float bone_length = (neighbour.joint.originalPosition - joint.originalPosition).magnitude;
                //Set target position so that the bone length is kept
                targetPosition = neighbour.joint.transform.position + (targetPosition - neighbour.joint.transform.position).normalized * bone_length;
            }

            //Store the target position
            targetPositions[j] = targetPosition;
        }

        //Apply joint positions and rotate
        for (int j = 0; j < MaxJoints; j++) {
            Joint joint = myJoints[i, j];
            if (joint == null) {
                continue;
            }

            //Apply the position
            joint.transform.position = targetPositions[j];

            if (!applyRotation || joint.neighbours.Count == 0)
                continue;

            //Calculate orientation
            Vector3 v = Vector3.zero;
            Vector3 parent = joint.transform.position;
            int childcount = 0;

            foreach (Joint.Neighbour n in joint.neighbours) {
                if (n.index < j) {
                    parent = n.joint.transform.position;
                } else {
                    v += n.joint.transform.position;
                    childcount++;
                }
            }
                
            Vector3 direction = v - parent * childcount;
            Debug.DrawLine(parent, direction + parent, Color.red);
            Debug.DrawLine(joint.transform.position, direction + parent, Color.blue);

            Vector3 orientation = direction + parent - joint.transform.position;
            Quaternion targetRotation = Quaternion.FromToRotation(joint.originalOrientation, orientation);
            //Factor in the original rotation, if the joint transform didnt align with the target rotation 
            targetRotation =  targetRotation * joint.originalRotation;
            //Clamp rotation
            targetRotation = Quaternion.RotateTowards(joint.transform.rotation, targetRotation, rotateSpeedFactor * speed);

            //Move and Rotate Joint Extensions (skeleton joints that are not tracked)
            foreach (Joint e in joint.extensions) {
                Vector3 difference = e.originalPosition - joint.originalPosition;
                e.transform.position = targetPositions[j] + targetRotation * difference;
                e.transform.rotation = targetRotation * e.originalRotation;
            }

            //Joint extension could be a parent in the skeleton, reset position
            joint.transform.position = targetPositions[j];

            //Set own rotation
            joint.transform.rotation = targetRotation;
        }

        
    }

    //Key inputs to rotate camera and restart
    void RotateCam()
    {
        if (Input.GetKey(KeyCode.RightArrow))
            transform.RotateAround(myJoints[0,0].originalPosition, Vector3.up, cameraSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.LeftArrow))
            transform.RotateAround(myJoints[0, 0].originalPosition, Vector3.up, -cameraSpeed* Time.deltaTime);

        /* Restart the Level/Refresh Scene */
        //if (Input.GetKeyDown(KeyCode.R))
        //    Application.LoadLevel(0);

        /* Quit the Application */
        if (Input.GetKeyDown(KeyCode.Q))
            Application.Quit();

    }
}
