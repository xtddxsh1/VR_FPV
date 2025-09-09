using System.Text.Json.Serialization;

namespace RosOnlyClient.Messages
{
    public class Header
    {
        [JsonPropertyName("frame_id")] public string frame_id { get; set; } = "unity_vr";
    }
    public class Point { public double x {get;set;} public double y{get;set;} public double z{get;set;} }
    public class Quaternion { public double x{get;set;} public double y{get;set;} public double z{get;set;} public double w{get;set;} }
    public class Pose { public Point position {get;set;} = new(); public Quaternion orientation {get;set;} = new(); }
    public class PoseStamped { public Header header {get;set;} = new(); public Pose pose {get;set;} = new(); }
}
