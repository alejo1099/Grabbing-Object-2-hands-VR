using UnityEngine;

public enum DireccionUp
{
    Arriba, Abajo
}

public class RotarObjeto : MonoBehaviour
{
    public DireccionUp direccionUp;

    public Transform derecha, izquierda;

    private Vector3 ejeRotacion;
    private Vector3 direccion;

    private float ultimoAnguloZ;
    private float ultimoAnguloX;
    private float valorY;

    [Range(0f, 0.95f)]
    public float puntoLimite = 0.8f;

    private float anguloEjeY;
    private float anguloEjeZ;
    private float anguloEjeX;

    public bool actualizarRotacion = true;
    private bool anguloObtenido;
    private bool interpolar = true;

    public float punto;

    private void Start()
    {
        Quaternion rotacionHijo = transform.GetChild(0).rotation;
        Vector3 posicionHijo = transform.GetChild(0).position;
        interpolar = false;
        ActualizarPosicion();
        ActualizarRotacion();
        interpolar = true;
        transform.GetChild(0).position = posicionHijo;
        transform.GetChild(0).rotation = rotacionHijo;
    }

    private void FixedUpdate()
    {
        ActualizarPosicion();
        ActualizarRotacion();
    }

    private void ActualizarPosicion()
    {
        transform.position = Vector3.Lerp(derecha.position, izquierda.position, 0.5f);
    }

    private void ActualizarRotacion()
    {
        direccion = (derecha.position - izquierda.position).normalized;
        VerificarAnguloCartesianoEjeZ();
        VerificarAnguloCartesianoEjeX();
        PoderVerificar();
        //VerificarUp();

        if (actualizarRotacion)
        {
            if (direccionUp == DireccionUp.Arriba)
                VerificarAnguloCartesianoGlobalEjeY();
            else if (direccionUp == DireccionUp.Abajo)
                VerificarAnguloCartesianoEjeYInverso();

            //Vector3 direccion = (derecha.position - izquierda.position).normalized;
            Vector3 arriba = Quaternion.Euler(ejeRotacion.normalized) * direccion;
            Quaternion rotacionRelativa = Quaternion.LookRotation(direccion, arriba);
            if (interpolar)
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacionRelativa, Time.deltaTime * 6f);
            else
                transform.rotation = rotacionRelativa;
        }
        else
        {
            //Vector3 direccion = (derecha.position - izquierda.position).normalized;
            Quaternion rotacionRelativa = Quaternion.LookRotation(direccion, transform.up);
            transform.rotation = rotacionRelativa;//Quaternion.Slerp(transform.rotation, rotacionRelativa, Time.deltaTime * 10f);
        }
    }

    private void PoderVerificar()
    {
        //Vector3 resta = (derecha.position - izquierda.position).normalized;
        punto = Vector3.Dot(direccion, Vector3.up);

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

    // private void VerificarUp()
    // {
    //     // if (!actualizarRotacion)
    //     // {
    //     //     if (VerificarCambioEje())
    //     //     {
    //     //         CambiarOrientacionOpuesta();
    //     //         ultimoAnguloX = anguloEjeX;
    //     //         ultimoAnguloZ = anguloEjeZ;
    //     //     }
    //     // }
    // }

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
        Vector3 direccionY = direccion;//(derecha.position - izquierda.position).normalized;
        direccionY.y = 0;
        anguloEjeY = Vector3.SignedAngle(direccionY, Vector3.forward, Vector3.up);

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
        Vector3 direccionZ = direccion;//(derecha.position - izquierda.position).normalized;
        direccionZ.z = 0;
        anguloEjeZ = Vector3.SignedAngle(direccionZ, Vector3.up, -Vector3.forward);
    }

    private void VerificarAnguloCartesianoEjeX()
    {
        Vector3 direccionX = direccion;//(derecha.position - izquierda.position).normalized;
        direccionX.x = 0;
        anguloEjeX = Vector3.SignedAngle(direccionX, Vector3.up, Vector3.right);
    }

    private void VerificarAnguloCartesianoEjeYInverso()
    {
        Vector3 direccionY = direccion;//(derecha.position - izquierda.position).normalized;
        direccionY.y = 0;
        anguloEjeY = Vector3.SignedAngle(direccionY, Vector3.forward, Vector3.up);

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