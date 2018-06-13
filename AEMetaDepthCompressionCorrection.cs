using System.Collections;
using System.Collections.Generic;
using AndroidEverywhere.MarkerTracking;
using UnityEngine;
using UnityEngine.Events;

public class AEMetaDepthCompressionCorrection : MonoBehaviour, IMarkerPoseDelgate
{
    // TODO: Implement logging for experiment

    // Enables Correction on object
    public bool EnableDepthCorrection = true;

    // The current feild of view from the camera
    public float FoV = 64f;

    // Multiplier to adjust size
    public Vector3 multipler = Vector3.one;

    // Custom layer
    public LayerMask customLayer = 0;

    // Custom Event after Initalixation
    public UnityEvent OnEndInitialization;

    public float DistanceScalar = 0.3f;
    public float LogAdjustment = 4.0f;

    // Linear Interpolcation
    public float MinThreshold = 1f;
    public float MaxThreshold = -0.1f;
    public float AngleNormalizationFactor = 90f;

    // reference to game object for manipulation
    private GameObject mReference;
    private Vector3 mCurrentHeadPos;
    private Quaternion mCurrentHeadRotation;

    [HideInInspector]
    public bool InitializeCopyConstructor = true;

    void Awake()
    {
        // Create reference object
        if (InitializeCopyConstructor)
        {
            gameObject.GetComponent<AEMetaDepthCompressionCorrection>().InitializeCopyConstructor = false;
            mReference = Instantiate(gameObject);
            mReference.GetComponent<AEMetaDepthCompressionCorrection>().InitializeCopyConstructor = false;
            mReference.GetComponent<AEMetaDepthCompressionCorrection>().enabled = false;
        }


    }

    // Use this for initialization
    void Start ()
    {
        if (mReference == null || MarkerGlobalManager.Instance == null)
            return;

        // Setup marker manager
        MarkerGlobalManager.Instance.AddListener(GetTrackingCategory(), this);

        // Assign multipler
        if (multipler == Vector3.one)
            multipler = transform.localScale;

        // Apply Layer changes
        AndroidEverywhere.AEUtlity.DeepCopyLayers(mReference, gameObject);

        // Set layers to default
        AndroidEverywhere.AEUtlity.ApplyFunctionRecursively(gameObject, go =>
        {
            go.layer = customLayer.value;
        });

        // Call delegates
        OnEndInitialization.Invoke();
    }

    // Update is called once per frame
    void Update ()
    {
        if (!EnableDepthCorrection || mReference == null) return;
        var currentRealPos = gameObject.transform.position;

        // Set Size
        var size = CalculateSize(mReference.transform.position);
        mReference.transform.localScale = new Vector3(size * multipler.x + multipler.x, size * multipler.y + multipler.y, size * multipler.z + multipler.z);

        // Heading
        var delta = gameObject.transform.position - mCurrentHeadPos;
        // Get angle
        var forward = mCurrentHeadRotation * Vector3.forward;
        var angle = Vector3.Angle(forward.normalized, delta.normalized);
        var anglenorm = angle / AngleNormalizationFactor;
        var inclidentAngleAdjustment = Mathf.Clamp01(AndroidEverywhere.AEUtlity.ReMap(anglenorm, 0f, 1f, MinThreshold, MaxThreshold));//Mathf.Lerp(MinThreshold, MaxThreshold, anglenorm);

        //if (mReference.name.StartsWith("Phone"))
        //{
        //    Debug.DrawRay(mCurrentHeadPos, forward * 5, new Color(0f, 0.98f, 1f));
        //    Debug.DrawRay(mCurrentHeadPos, delta.normalized * 5, new Color(0f, 1f, 0.23f));
        //    Debug.Log("Angle:  " + angle);
        //    Debug.Log("Anorm:  " + anglenorm);
        //    Debug.Log("Adjust: " + inclidentAngleAdjustment);
        //}

        // Set postion
        var newDist = Mathf.Log(delta.magnitude * LogAdjustment + 0.5f / LogAdjustment) * delta.magnitude * DistanceScalar * inclidentAngleAdjustment;
        mReference.transform.position = gameObject.transform.position + (delta.normalized * newDist);

        // Adjust Rotation
        mReference.transform.rotation = transform.rotation;
    }

    public void Clean()
    {
        Destroy(mReference);
    }

    public void SetSizeMultipler(Vector3 vec)
    {
        multipler = vec;
    }

    public GameObject GetReferenceObject()
    {
        return mReference;
    }

    private float CalculateSize(Vector3 pos)
    {
        var distance = Vector3.Distance(pos, transform.position);
        var size = Mathf.Tan(FoV/2) * distance;
        
        //Debug.Log(size);
        return size;
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    // POSE DELEGATE METHOD IMPLS
    /////////////////////////////////////////////////////////////////////////////////////////////////////////

    public TrackingCategory GetTrackingCategory()
    {
        return TrackingCategory.TRACKER_HEAD;
    }

    public void UpdatePose(Vector3 position, Quaternion rotation, PoseOptionsBase options = null)
    {
        mCurrentHeadPos = position;
        mCurrentHeadRotation = rotation;
    }
}
