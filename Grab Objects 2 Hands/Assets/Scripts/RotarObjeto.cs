using UnityEngine;

public enum DireccionUp
{
    Arriba, Abajo
}

public class RotarObjeto : MonoBehaviour
{
    public DireccionUp direccionUp;

    public Transform derecha, izquierda;

    public Vector3 ejeRotacion;

    private float ultimoAnguloZ;
    private float ultimoAnguloX;
    private float valorY;

    [Range(0f, 0.95f)]
    public float puntoLimite = 0.8f;

    public float anguloEjeY;
    public float anguloEjeZ;
    public float anguloEjeX;

    public bool actualizarRotacion = true;
    private bool anguloObtenido;

    public float punto;

    private void FixedUpdate()
    {
        ActualizarPosicion();
        ActualizarRotacion();

        // Debug.DrawRay(transform.position, transform.forward, Color.blue);
        // Debug.DrawRay(transform.position, transform.right, Color.red);
        // Debug.DrawRay(transform.position, transform.up, Color.green);
    }

    private void ActualizarPosicion()
    {
        transform.position = Vector3.Lerp(derecha.position, izquierda.position, 0.5f);
    }

    private void ActualizarRotacion()
    {
        VerificarAnguloCartesianoEjeZ();
        VerificarAnguloCartesianoEjeX();
        PoderVerificar();
        VerificarUp();

        if (actualizarRotacion)
        {
            if (direccionUp == DireccionUp.Arriba)
                VerificarAnguloCartesianoGlobalEjeY();
            else if (direccionUp == DireccionUp.Abajo)
                VerificarAnguloCartesianoEjeYInverso();

            Vector3 direccion = (derecha.position - izquierda.position).normalized;
            Vector3 arriba = Quaternion.Euler(ejeRotacion.normalized) * direccion;
            Quaternion rotacionRelativa = Quaternion.LookRotation(direccion, arriba);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionRelativa, Time.deltaTime * 6f);
            Debug.DrawRay(transform.position, direccion, Color.blue);
            Debug.DrawRay(transform.position, arriba, Color.red);
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

        if (punto >= puntoLimite || punto <= -puntoLimite)
        {
            actualizarRotacion = false;
            if (!anguloObtenido)
            {
                ultimoAnguloZ = anguloEjeZ;
                ultimoAnguloX = anguloEjeX;
                anguloObtenido = true;
            }
        }
        else
        {
            actualizarRotacion = true;
            if (anguloObtenido)
            {
                if (VerificarCambioEje())
                {
                    CambiarOrientacionOpuesta();
                }
                anguloObtenido = false;
            }
        }
    }

    private void VerificarUp()
    {
        // if (!actualizarRotacion)
        // {
        //     if (VerificarCambioEje())
        //     {
        //         CambiarOrientacionOpuesta();
        //         ultimoAnguloX = anguloEjeX;
        //         ultimoAnguloZ = anguloEjeZ;
        //     }
        // }
    }

    private bool VerificarCambioEje()
    {
        int anteriorAnguloX = ultimoAnguloX >= 0f ? 1 : -1;
        int anteriorAnguloZ = ultimoAnguloZ >= 0f ? 1 : -1;

        int actualAnguloX = anguloEjeX >= 0f ? 1 : -1;
        int actualAnguloZ = anguloEjeZ >= 0f ? 1 : -1;

        if ((actualAnguloX != anteriorAnguloX) || (actualAnguloZ != anteriorAnguloZ))
        {
            return true;
        }
        return false;
    }

    private void CambiarOrientacionOpuesta()
    {
        if (direccionUp == DireccionUp.Arriba)
            direccionUp = DireccionUp.Abajo;
        else
            direccionUp = DireccionUp.Arriba;
    }

    private void VerificarAnguloCartesianoGlobalEjeY()
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

    private void VerificarAnguloCartesianoEjeYInverso()
    {
        Vector3 direccion = (derecha.position - izquierda.position).normalized;
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
}