using UnityEngine;

public static class CoordinateConverter
{
    // Unity (left-handed: X right, Y up, Z forward)
    // ROS REP-103 (right-handed: X forward, Y left, Z up)
    // Common mapping: pos_ros = (z, -x, y)
    public static Vector3 UnityToRosPosition(Vector3 u)
    {
        return new Vector3(u.z, -u.x, u.y);
    }

    public static Quaternion UnityToRosRotation(Quaternion q)
    {
        // Apply axis remap to quaternion: (x,y,z) -> (z,-x,y)
        // Equivalent to rotate basis: ex->ez, ey->-ex, ez->ey
        // Implement via matrix conversion to be robust
        var R = Matrix4x4.Rotate(q);
        // Extract basis vectors
        Vector3 ex = new Vector3(R.m00, R.m10, R.m20);
        Vector3 ey = new Vector3(R.m01, R.m11, R.m21);
        Vector3 ez = new Vector3(R.m02, R.m12, R.m22);
        // Remap to ROS basis
        Vector3 rx = ez;        // X_ros
        Vector3 ry = -ex;       // Y_ros
        Vector3 rz = ey;        // Z_ros
        // Build new rotation
        Matrix4x4 RR = new Matrix4x4();
        RR.SetColumn(0, new Vector4(rx.x, rx.y, rx.z, 0));
        RR.SetColumn(1, new Vector4(ry.x, ry.y, ry.z, 0));
        RR.SetColumn(2, new Vector4(rz.x, rz.y, rz.z, 0));
        RR.m33 = 1;
        return RR.rotation;
    }
}
