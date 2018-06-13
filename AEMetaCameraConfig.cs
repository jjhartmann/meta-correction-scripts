using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AEMetaCameraConfig : MonoBehaviour
{

    /// <summary>
    /// Test cameras projection matrix
    /// </summary>
    public float rotate_angle = 0;
    public float scale = 1;
    public Vector2 shearing = Vector2.zero;
    public Vector2 translation = Vector2.zero;
    public Vector2 tiltshift = Vector2.zero;

    private Matrix4x4 MatRotate = Matrix4x4.identity;
    private Matrix4x4 MatScale = Matrix4x4.identity;
    private Matrix4x4 MatXShearing = Matrix4x4.identity;
    private Matrix4x4 MatYShearing = Matrix4x4.identity;
    private Matrix4x4 MatTranslation = Matrix4x4.identity;
    private Matrix4x4 MatTiltShift = Matrix4x4.identity;

    private Matrix4x4 OriginalMatProjection;


    /// <summary>
    /// Determines the layers used to render Meta2 glasses
    /// </summary>
    [SerializeField] private LayerMask mLayer;


    [SerializeField] private bool mEnableProjectionMatrixAdjustments = false;

    /// <summary>
    /// Camera used to render the scene view
    /// </summary>
    [SerializeField]
    private Camera mLeftCamera;
    [SerializeField]
    private Camera mRightCamera;
    [SerializeField]
    private Camera mWebCamera;

    // Use this for initialization
    void Start()
    { 
        if (mLeftCamera != null)
        {
            OriginalMatProjection = mLeftCamera.projectionMatrix;
            mLeftCamera.cullingMask = mLayer.value;
        }
        if (mRightCamera != null)
        {
            mRightCamera.cullingMask = mLayer.value;
        }
        if (mWebCamera != null)
        {
            mWebCamera.cullingMask = mLayer.value;
        }
    }


    void Update()
    {
        if (mLeftCamera == null || mRightCamera == null || !mEnableProjectionMatrixAdjustments) return;

        MatScale[0, 0] = scale;
        MatScale[1, 1] = scale;

        MatRotate[0, 0] = Mathf.Cos(rotate_angle * Mathf.Deg2Rad);
        MatRotate[0, 1] = -Mathf.Sin(rotate_angle * Mathf.Deg2Rad);
        MatRotate[1, 0] = Mathf.Sin(rotate_angle * Mathf.Deg2Rad);
        MatRotate[1, 1] = Mathf.Cos(rotate_angle * Mathf.Deg2Rad);

        MatXShearing[0, 1] = shearing.x;
        MatYShearing[1, 0] = shearing.y;

        MatTranslation[0, 2] = translation.x;
        MatTranslation[1, 2] = translation.y;

        var uMat = OriginalMatProjection *
                   MatScale *
                   MatRotate *
                   MatXShearing *
                   MatYShearing *
                   MatTranslation;

        uMat.m30 = tiltshift.x;
        uMat.m31 = tiltshift.y;

        mLeftCamera.projectionMatrix = uMat;
        mRightCamera.projectionMatrix = uMat;

    }
}
