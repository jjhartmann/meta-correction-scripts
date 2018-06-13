using UnityEngine;
using System.Collections;
using AndroidEverywhere.Achiver;

namespace AndroidEverywhere.MarkerTracking
{
    public interface IMarkerPoseDelgate
    {
        TrackingCategory GetTrackingCategory();
        void UpdatePose(Vector3 position, Quaternion rotation, PoseOptionsBase options = null);
    }

    public class PoseOptionsBase
    {
        public string msg = "";
        public string id = "";
        public bool isValid = true;
        public LoggingMarkerTrackingTypes providerType;
        public TrackingCategory objectType;
    }



    public enum TrackingCategory
    {
        TRACKER_PHONE,
        TRACKER_HEAD,
        TRACKER_ALL,
        TRACKER_SHOULDER,
        TRACKER_ERROR,
    }

}
