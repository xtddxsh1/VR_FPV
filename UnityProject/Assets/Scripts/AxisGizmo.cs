using UnityEngine;

[ExecuteAlways]
public class AxisGizmo : MonoBehaviour
{
    public float axisLength = 0.1f;
    void OnDrawGizmos(){
        Vector3 p = transform.position;
        Gizmos.color = Color.red;   Gizmos.DrawLine(p, p + transform.right * axisLength);
        Gizmos.color = Color.green; Gizmos.DrawLine(p, p + transform.up * axisLength);
        Gizmos.color = Color.blue;  Gizmos.DrawLine(p, p + transform.forward * axisLength);
    }
}
