using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class RGBSubscriber : MonoBehaviour
{
    public Renderer targetRenderer;
    private Texture2D _tex;
    private ROSConnection _ros;

    void Start()
    {
        _ros = ROSConnection.GetOrCreateInstance();
        _ros.Subscribe<CompressedImageMsg>("useRGB", OnImage);
    }

    void OnImage(CompressedImageMsg msg)
    {
        if (_tex == null) _tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        _tex.LoadImage(msg.data);
        if (targetRenderer != null) targetRenderer.material.mainTexture = _tex;
    }
}
