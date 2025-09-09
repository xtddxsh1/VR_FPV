using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using System.Runtime.InteropServices;

public class PointCloudSubscriber : MonoBehaviour
{
    public Material pointMaterial;
    public float pointSize = 4.0f;
    private ComputeBuffer _buf;
    private int _count;
    private ROSConnection _ros;

    struct PointXYZRGBA { public Vector3 pos; public uint rgba; }

    void Start()
    {
        _ros = ROSConnection.GetOrCreateInstance();
        _ros.Subscribe<PointCloud2Msg>("usePointCloud", OnCloud);
    }

    void OnDestroy(){ if (_buf != null) _buf.Dispose(); }

    void OnCloud(PointCloud2Msg msg)
    {
        int n = (int)(msg.width * msg.height);
        if (n <= 0) return;

        if (_buf == null || _count != n)
        {
            if (_buf != null) _buf.Dispose();
            _buf = new ComputeBuffer(n, Marshal.SizeOf<PointXYZRGBA>());
            _count = n;
        }

        int step = (int)msg.point_step;
        int offX = FindOffset(msg, "x");
        int offY = FindOffset(msg, "y");
        int offZ = FindOffset(msg, "z");
        int offRGB = FindOffset(msg, "rgb");

        unsafe{
            fixed (byte* src = msg.data){
                var arr = new PointXYZRGBA[n];
                for (int i = 0; i < n; i++){
                    byte* p = src + i * step;
                    float x = *(float*)(p + offX);
                    float y = *(float*)(p + offY);
                    float z = *(float*)(p + offZ);
                    uint rgbaPacked = 0xFFFFFFFF;
                    if (offRGB >= 0){
                        float rgbF = *(float*)(p + offRGB);
                        rgbaPacked = *(uint*)(&rgbF) | 0xFF000000;
                    }
                    arr[i].pos = new Vector3(x, y, z);
                    arr[i].rgba = rgbaPacked;
                }
                _buf.SetData(arr);
            }
        }
    }

    int FindOffset(PointCloud2Msg msg, string name){
        foreach (var f in msg.fields) if (f.name == name) return (int)f.offset;
        return -1;
    }

    void OnRenderObject(){
        if (_buf == null || _count == 0 || pointMaterial == null) return;
        pointMaterial.SetBuffer("_Points", _buf);
        pointMaterial.SetFloat("_PointSize", pointSize);
        pointMaterial.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Points, _count, 1);
    }
}
