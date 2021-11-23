using UnityEngine;

public class MovimientoDinamicoTarget : MonoBehaviour
{
    private Transform[] planes;
    public Transform fatherPlanes;
    public GameObject prefab;
    public float velocidadRotacion;

    private int currPlane = 0;
    private int currQuadrante = 1;
    private float t;

    private Vector3 axisTarget;
    private Vector3 currentAxis;

    void Start() {
        planes = new Transform[24];
        for (int grados = 0, i = 0; grados < 360; grados+=15,i++)
        {
            Quaternion q = Quaternion.Euler(grados, 0, 0);
            GameObject plane = Instantiate(prefab, Vector3.zero, q);
            plane.transform.SetParent(fatherPlanes);
            planes[i] = plane.transform;
        }
        //currPlane = Random.Range(0, planes.Length);


        currentAxis = planes[currPlane].right * 2;
        axisTarget = -planes[currPlane].forward * 2;
        currQuadrante = 1;
    }

    // Update is called once per frame
    void Update() {
        t += Time.deltaTime;
        transform.position = Vector3.Slerp(currentAxis, axisTarget, t * velocidadRotacion);
        if(t * velocidadRotacion > 1) {
            if(currQuadrante == 1) {
                currentAxis = axisTarget;
                axisTarget = -planes[currPlane].right * 2;
                currQuadrante = 2;
                t = 0;
            } else if(currQuadrante == 2) {
                currentAxis = axisTarget;
                axisTarget = planes[currPlane].forward * 2;
                currQuadrante = 3;
                t = 0;
            } else if(currQuadrante == 3) {
                currentAxis = axisTarget;
                axisTarget = planes[currPlane].right * 2;
                currQuadrante = 4;
                t = 0;
            } else if(currQuadrante == 4) {
                currPlane++;
                currPlane = currPlane < planes.Length ? currPlane : 0;
                currQuadrante = 1;
                t = 0;
                currentAxis = planes[currPlane].right * 2;
                axisTarget = -planes[currPlane].forward * 2;
            }
        }
    }
}
