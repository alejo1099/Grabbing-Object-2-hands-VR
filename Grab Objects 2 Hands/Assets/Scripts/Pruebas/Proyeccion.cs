using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Proyeccion : MonoBehaviour
{
    public Transform right;
    public Transform left;
    Vector3 up;

    private void OnDrawGizmos() {
        // up = Vector3.Cross(left.position, right.position);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(Vector3.zero, Vector3.up);

        Vector3 proj = Vector3.Project(transform.forward, Vector3.up);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(Vector3.zero, proj);
        // Debug.Log(proj.magnitude);
        Gizmos.color = Color.blue;
        Vector3 newUp = proj + (Vector3.forward * (Mathf.Sqrt(3) * 0.5f));
        Gizmos.DrawRay(Vector3.zero, newUp);
        Debug.Log(newUp.magnitude);
        // Gizmos.DrawRay(Vector3.zero, right.position);

        // Gizmos.color = Color.blue;
        // Gizmos.DrawRay(Vector3.zero, left.position);
    }
}
