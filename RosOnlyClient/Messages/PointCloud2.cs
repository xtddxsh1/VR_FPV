using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RosOnlyClient.Messages
{
    // sensor_msgs/PointField
    public class PointField
    {
        public string name { get; set; } = "";
        public uint offset { get; set; }
        public byte datatype { get; set; }  // FLOAT32 = 7
        public uint count { get; set; }
    }

    // sensor_msgs/PointCloud2 (subset) as rosbridge JSON
    public class PointCloud2
    {
        public uint height { get; set; }
        public uint width { get; set; }
        public List<PointField> fields { get; set; } = new();
        public bool is_bigendian { get; set; }
        public uint point_step { get; set; }
        public uint row_step { get; set; }
        // rosbridge encodes 'data' as base64 string
        [JsonPropertyName("data")] public string data_base64 { get; set; } = "";
        public bool is_dense { get; set; }
    }

    public static class PointCloud2Parser
    {
        public struct XYZRGB { public float x, y, z; public byte r, g, b, a; }

        public static List<XYZRGB> ParseXYZRGB(PointCloud2 pc, int maxPoints = 100000)
        {
            var pts = new List<XYZRGB>();
            if (pc.width == 0 || pc.height == 0) return pts;
            var total = checked((int)(pc.width * pc.height));
            var step = checked((int)pc.point_step);
            if (string.IsNullOrEmpty(pc.data_base64)) return pts;
            var bytes = Convert.FromBase64String(pc.data_base64);

            int offX = FindField(pc.fields, "x");
            int offY = FindField(pc.fields, "y");
            int offZ = FindField(pc.fields, "z");
            int offRGB = Math.Max(FindField(pc.fields, "rgb"), FindField(pc.fields, "rgba")); // either

            for (int i = 0; i < total && i < maxPoints; i++)
            {
                int p = i * step;
                float x = BitConverter.ToSingle(bytes, p + offX);
                float y = BitConverter.ToSingle(bytes, p + offY);
                float z = BitConverter.ToSingle(bytes, p + offZ);
                byte r=255,g=255,b=255,a=255;
                if (offRGB >= 0)
                {
                    // 'rgb' is stored as float containing packed 24-bit color
                    var rgbFloat = BitConverter.ToSingle(bytes, p + offRGB);
                    var packed = BitConverter.ToUInt32(BitConverter.GetBytes(rgbFloat), 0);
                    r = (byte)((packed >> 16) & 0xFF);
                    g = (byte)((packed >> 8) & 0xFF);
                    b = (byte)(packed & 0xFF);
                    a = 255;
                }
                pts.Add(new XYZRGB{ x=x, y=y, z=z, r=r, g=g, b=b, a=a });
            }
            return pts;
        }

        static int FindField(List<PointField> fields, string name)
        {
            for (int i=0;i<fields.Count;i++)
                if (fields[i].name == name) return (int)fields[i].offset;
            return -1;
        }
    }
}
