using UnityEngine;
using System.Collections;

public class RotarSimple : MonoBehaviour
{
    // public Transform derecha, izquierda;

    // public Vector3 ejeRotacion;

    // private Vector3 direccion;

    public DireccionUp direccionUp;

    public Transform derecha, izquierda;

    public Vector3 ejeRotacion;

    public float valorY;

    public float anguloEjeY;
    public float anguloEjeZ;
    public float anguloEjeX;

    public bool actualizarRotacion = true;
    private bool interpolar;

    public float punto;

    private void FixedUpdate()
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
        VerificarAnguloCartesianoEjeZ();
        VerificarAnguloCartesianoEjeX();
        VerificarUp();
        PoderVerificar();

        if (actualizarRotacion)
        {
            if (direccionUp == DireccionUp.Arriba)
                VerificarAnguloCartesianoEjeY();
            else if (direccionUp == DireccionUp.Abajo)
                VerificarAnguloCartesianoEjeYInverso();

            Vector3 direccion = (derecha.position - izquierda.position).normalized;
            Vector3 arriba = Quaternion.Euler(ejeRotacion) * direccion;
            Quaternion rotacionRelativa = Quaternion.LookRotation(direccion, arriba);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionRelativa, Time.deltaTime * 4f);
        }
        else
        {
            Vector3 direccion = (derecha.position - izquierda.position).normalized;
            Quaternion rotacionRelativa = Quaternion.LookRotation(direccion, transform.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionRelativa, Time.deltaTime * 10f);
        }
    }

    private void PoderVerificar()
    {
        Vector3 resta = (derecha.position - izquierda.position).normalized;
        punto = Vector3.Dot(resta, Vector3.up);
        // if ((anguloEjeZ <= -15f && anguloEjeZ >= -165f) || (anguloEjeZ >= 15f && anguloEjeZ <= 165f))
        // {
        //     if ((anguloEjeX <= -15f && anguloEjeX >= -165f) || (anguloEjeX >= 15f && anguloEjeX <= 165f))
        //         actualizarRotacion = true;
        // }
        // else
        //     actualizarRotacion = false;
        if (punto >= 0.9f || punto <= -0.9f)
            actualizarRotacion = false;
        else
            actualizarRotacion = true;
    }

    private void VerificarAnguloCartesianoEjeY()
    {
        Vector3 direccion = (derecha.position - izquierda.position).normalized;
        direccion.y = 0;
        anguloEjeY = Vector3.SignedAngle(direccion, Vector3.forward, Vector3.up);

        if (anguloEjeY <= 0f && anguloEjeY >= -90f)
            ActualizarEjeCuadranteI(anguloEjeY);
        else if (anguloEjeY >= 0f && anguloEjeY <= 90f)
            ActualizarEjeCuadranteII(anguloEjeY);
        else if (anguloEjeY >= 90f && anguloEjeY <= 180f)
            ActualizarEjeCuadranteIII(anguloEjeY);
        else if (anguloEjeY <= -90f && anguloEjeY >= -180f)
            ActualizarEjeCuadranteIV(anguloEjeY);
    }

    private void ActualizarEjeCuadranteI(float angulo)
    {
        float interpolacion = Mathf.Abs(angulo) / 90f;
        Vector3 vectorEjeX = new Vector3(-90f, valorY, 0f);
        Vector3 vectorEjeZ = new Vector3(0f, valorY, 90f);

        Vector3 vectorFinal = Vector3.Lerp(vectorEjeX, vectorEjeZ, interpolacion);

        ejeRotacion = vectorFinal;
    }

    private void ActualizarEjeCuadranteII(float angulo)
    {
        float interpolacion = Mathf.Abs(angulo) / 90f;
        Vector3 vectorEjeX = new Vector3(-90f, valorY, 0f);
        Vector3 vectorEjeZ = new Vector3(0f, valorY, -90f);

        Vector3 vectorFinal = Vector3.Lerp(vectorEjeX, vectorEjeZ, interpolacion);

        ejeRotacion = vectorFinal;
    }

    private void ActualizarEjeCuadranteIII(float angulo)
    {
        float interpolacion = (Mathf.Abs(angulo) - 90f) / 90f;
        Vector3 vectorEjeX = new Vector3(90f, valorY, 0f);
        Vector3 vectorEjeZ = new Vector3(0f, valorY, -90f);

        Vector3 vectorFinal = Vector3.Lerp(vectorEjeZ, vectorEjeX, interpolacion);

        ejeRotacion = vectorFinal;
    }

    private void ActualizarEjeCuadranteIV(float angulo)
    {
        float interpolacion = (Mathf.Abs(angulo) - 90f) / 90f;
        Vector3 vectorEjeX = new Vector3(90f, valorY, 0f);
        Vector3 vectorEjeZ = new Vector3(0f, valorY, 90f);

        Vector3 vectorFinal = Vector3.Lerp(vectorEjeZ, vectorEjeX, interpolacion);

        ejeRotacion = vectorFinal;
    }

    private void VerificarAnguloCartesianoEjeZ()
    {
        Vector3 direccion = (derecha.position - izquierda.position).normalized;
        direccion.z = 0;
        anguloEjeZ = Vector3.SignedAngle(direccion, Vector3.up, -Vector3.forward);
    }

    private void VerificarAnguloCartesianoEjeX()
    {
        Vector3 direccion = (derecha.position - izquierda.position).normalized;
        direccion.x = 0;
        anguloEjeX = Vector3.SignedAngle(direccion, Vector3.up, Vector3.right);
    }

    private void VerificarUp()
    {
        float punto = Vector3.Dot(transform.up, Vector3.up);
        if (punto >= 0)
            direccionUp = DireccionUp.Arriba;
        else if (punto <= 0)
            direccionUp = DireccionUp.Abajo;
    }

    private void VerificarAnguloCartesianoEjeYInverso()
    {
        Vector3 direccion = (derecha.position - izquierda.position).normalized;
        // anguloVertical = Vector3.Dot(direccion, Vector3.up);
        direccion.y = 0;
        anguloEjeY = Vector3.SignedAngle(direccion, Vector3.forward, Vector3.up);

        if (anguloEjeY <= 0f && anguloEjeY >= -90f)
            ActualizarEjeCuadranteIDown(anguloEjeY);
        else if (anguloEjeY >= 0f && anguloEjeY <= 90f)
            ActualizarEjeCuadranteIIDown(anguloEjeY);
        else if (anguloEjeY >= 90f && anguloEjeY <= 180f)
            ActualizarEjeCuadranteIIIDown(anguloEjeY);
        else if (anguloEjeY <= -90f && anguloEjeY >= -180f)
            ActualizarEjeCuadranteIVDown(anguloEjeY);
    }

    private void ActualizarEjeCuadranteIDown(float angulo)
    {
        float interpolacion = Mathf.Abs(angulo) / 90f;
        Vector3 vectorEjeX = new Vector3(90f, valorY, 0f);
        Vector3 vectorEjeZ = new Vector3(0f, valorY, -90f);

        Vector3 vectorFinal = Vector3.Lerp(vectorEjeX, vectorEjeZ, interpolacion);

        ejeRotacion = vectorFinal;
    }

    private void ActualizarEjeCuadranteIIDown(float angulo)
    {
        float interpolacion = Mathf.Abs(angulo) / 90f;
        Vector3 vectorEjeX = new Vector3(90f, valorY, 0f);
        Vector3 vectorEjeZ = new Vector3(0f, valorY, 90f);

        Vector3 vectorFinal = Vector3.Lerp(vectorEjeX, vectorEjeZ, interpolacion);

        ejeRotacion = vectorFinal;
    }

    private void ActualizarEjeCuadranteIIIDown(float angulo)
    {
        float interpolacion = (Mathf.Abs(angulo) - 90f) / 90f;
        Vector3 vectorEjeX = new Vector3(-90f, valorY, 0f);
        Vector3 vectorEjeZ = new Vector3(0f, valorY, 90f);

        Vector3 vectorFinal = Vector3.Lerp(vectorEjeZ, vectorEjeX, interpolacion);

        ejeRotacion = vectorFinal;
    }

    private void ActualizarEjeCuadranteIVDown(float angulo)
    {
        float interpolacion = (Mathf.Abs(angulo) - 90f) / 90f;
        Vector3 vectorEjeX = new Vector3(-90f, valorY, 0f);
        Vector3 vectorEjeZ = new Vector3(0f, valorY, -90f);

        Vector3 vectorFinal = Vector3.Lerp(vectorEjeZ, vectorEjeX, interpolacion);

        ejeRotacion = vectorFinal;
    }

    // private void ActualizarRotacion()
    // {
    //     direccion = (derecha.position - izquierda.position).normalized;
    //     Vector3 arriba = Quaternion.Euler(ejeRotacion) * direccion;
    //     transform.rotation = Quaternion.LookRotation(direccion);

    //     //transform.rotation = Quaternion.LookRotation(izquierda.transform.position - derecha.transform.position, izquierda.transform.TransformDirection(Vector3.forward));
    // }

    // protected virtual void ZLockedAim()
    // {
    //     Vector3 forward = (izquierda.transform.position - derecha.transform.position).normalized;

    //     // calculate rightLocked rotation
    //     Quaternion rightLocked = Quaternion.LookRotation(forward, Vector3.Cross(-izquierda.transform.right, forward).normalized);

    //     // delta from current rotation to the rightLocked rotation
    //     Quaternion rightLockedDelta = Quaternion.Inverse(transform.rotation) * rightLocked;

    //     float rightLockedAngle;
    //     Vector3 rightLockedAxis;

    //     // forward direction and roll
    //     rightLockedDelta.ToAngleAxis(out rightLockedAngle, out rightLockedAxis);

    //     if (rightLockedAngle > 180f)
    //     {
    //         // remap ranges from 0-360 to -180 to 180
    //         rightLockedAngle -= 360f;
    //     }

    //     // make any negative values into positive values;
    //     rightLockedAngle = Mathf.Abs(rightLockedAngle);

    //     // calculate upLocked rotation
    //     Quaternion upLocked = Quaternion.LookRotation(forward, derecha.transform.forward);

    //     // delta from current rotation to the upLocked rotation
    //     Quaternion upLockedDelta = Quaternion.Inverse(transform.rotation) * upLocked;

    //     float upLockedAngle;
    //     Vector3 upLockedAxis;

    //     // forward direction and roll
    //     upLockedDelta.ToAngleAxis(out upLockedAngle, out upLockedAxis);

    //     // remap ranges from 0-360 to -180 to 180
    //     if (upLockedAngle > 180f)
    //     {
    //         upLockedAngle -= 360f;
    //     }

    //     // make any negative values into positive values;
    //     upLockedAngle = Mathf.Abs(upLockedAngle);

    //     // assign the one that involves less change to roll
    //     transform.rotation = (upLockedAngle < rightLockedAngle ? upLocked : rightLocked);
    // }
}
