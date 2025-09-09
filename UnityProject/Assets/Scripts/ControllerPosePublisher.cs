using UnityEngine;
using UnityEngine.XR;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

public class ControllerPosePublisher : MonoBehaviour
{
    public Transform leftViz, rightViz;
    public bool convertToRosCoord = true; // 是否进行坐标系转换（Unity -> ROS REP-103）
    private ROSConnection _ros;

    void Start()
    {
        _ros = ROSConnection.GetOrCreateInstance();
    }

    void Update()
    {
        PublishNode(XRNode.LeftHand, "usePose", leftViz);
        PublishNode(XRNode.RightHand, "usePose", rightViz);
    }

    void PublishNode(XRNode node, string topic, Transform viz)
    {
        #if UNITY_2020_3_OR_NEWER
        var pos = InputTracking.GetLocalPosition(node);
        var rot = InputTracking.GetLocalRotation(node);
        #else
        var pos = Vector3.zero; var rot = Quaternion.identity;
        #endif

        if (viz != null) { viz.localPosition = pos; viz.localRotation = rot; }

        if (convertToRosCoord)
        {
            var rpos = CoordinateConverter.UnityToRosPosition(pos);
            var rrot = CoordinateConverter.UnityToRosRotation(rot);
            Publish(topic, rpos, rrot);
        }
        else
        {
            Publish(topic, pos, rot);
        }
    }

    void Publish(string topic, Vector3 p, Quaternion q)
    {
        var msg = new PoseStampedMsg
        {
            header = new RosMessageTypes.Std.HeaderMsg
            {
                frame_id = "unity_vr",
                stamp = ROSConnection.GetROSTimestamp()
            },
            pose = new PoseMsg
            {
                position = new PointMsg(p.x, p.y, p.z),
                orientation = new QuaternionMsg(q.x, q.y, q.z, q.w)
            }
        };
        _ros.Publish(topic, msg);
    }
}
