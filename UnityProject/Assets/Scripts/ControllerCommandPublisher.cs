using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class ControllerCommandPublisher : MonoBehaviour
{
    public InputActionProperty leftGrip, leftTrigger, leftStick;
    public InputActionProperty rightGrip, rightTrigger, rightStick;
    private ROSConnection _ros;

    void Start()
    {
        _ros = ROSConnection.GetOrCreateInstance();
    }

    void Update()
    {
        var msg = new JoyMsg();
        msg.axes = new[]
        {
            ReadVec2(leftStick).x, ReadVec2(leftStick).y,
            ReadVec2(rightStick).x, ReadVec2(rightStick).y,
            ReadFloat(leftTrigger), ReadFloat(rightTrigger),
            ReadFloat(leftGrip), ReadFloat(rightGrip)
        };
        msg.buttons = new int[0];
        _ros.Publish("useJoy", msg);
    }

    static Vector2 ReadVec2(InputActionProperty a) => a.action != null ? a.action.ReadValue<Vector2>() : Vector2.zero;
    static float ReadFloat(InputActionProperty a) => a.action != null ? a.action.ReadValue<float>() : 0f;
}
