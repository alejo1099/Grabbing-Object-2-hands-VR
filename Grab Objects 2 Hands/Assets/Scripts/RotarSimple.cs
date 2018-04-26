using UnityEngine;

public class RotarSimple : MonoBehaviour
{
    public Transform derecha, izquierda;

    public Vector3 ejeRotacion;

    private void Update()
    {
        ActualizarPosicion();
        ActualizarRotacion();
        //ZLockedAim();

        Debug.DrawRay(transform.position, transform.forward, Color.blue);
        Debug.DrawRay(transform.position, transform.right, Color.red);
        Debug.DrawRay(transform.position, transform.up, Color.green);
    }

    private void ActualizarPosicion()
    {
        transform.position = Vector3.Lerp(derecha.position, izquierda.position, 0.5f);
    }

    private void ActualizarRotacion()
    {
        Vector3 direccion = (derecha.position - izquierda.position).normalized;
        Vector3 arriba = Quaternion.Euler(ejeRotacion) * direccion;
        transform.rotation = Quaternion.LookRotation(direccion, arriba);

        //transform.rotation = Quaternion.LookRotation(izquierda.transform.position - derecha.transform.position, izquierda.transform.TransformDirection(Vector3.forward));
    }

    protected virtual void ZLockedAim()
    {
        Vector3 forward = (izquierda.transform.position - derecha.transform.position).normalized;

        // calculate rightLocked rotation
        Quaternion rightLocked = Quaternion.LookRotation(forward, Vector3.Cross(-izquierda.transform.right, forward).normalized);

        // delta from current rotation to the rightLocked rotation
        Quaternion rightLockedDelta = Quaternion.Inverse(transform.rotation) * rightLocked;

        float rightLockedAngle;
        Vector3 rightLockedAxis;

        // forward direction and roll
        rightLockedDelta.ToAngleAxis(out rightLockedAngle, out rightLockedAxis);

        if (rightLockedAngle > 180f)
        {
            // remap ranges from 0-360 to -180 to 180
            rightLockedAngle -= 360f;
        }

        // make any negative values into positive values;
        rightLockedAngle = Mathf.Abs(rightLockedAngle);

        // calculate upLocked rotation
        Quaternion upLocked = Quaternion.LookRotation(forward, derecha.transform.forward);

        // delta from current rotation to the upLocked rotation
        Quaternion upLockedDelta = Quaternion.Inverse(transform.rotation) * upLocked;

        float upLockedAngle;
        Vector3 upLockedAxis;

        // forward direction and roll
        upLockedDelta.ToAngleAxis(out upLockedAngle, out upLockedAxis);

        // remap ranges from 0-360 to -180 to 180
        if (upLockedAngle > 180f)
        {
            upLockedAngle -= 360f;
        }

        // make any negative values into positive values;
        upLockedAngle = Mathf.Abs(upLockedAngle);

        // assign the one that involves less change to roll
        transform.rotation = (upLockedAngle < rightLockedAngle ? upLocked : rightLocked);
    }
}
