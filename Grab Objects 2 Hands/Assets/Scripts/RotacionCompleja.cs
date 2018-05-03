using UnityEngine;

public class RotacionCompleja : MonoBehaviour
{
    public DireccionUp direccionUp;

    public Transform derecha, izquierda;
    private Transform hijo;

    private Vector3 ejeRotacion;
    private Vector3 direccion;
    private Vector3 upWard;
    private Vector3 rightWard;

    private float ultimoAnguloZ;
    private float ultimoAnguloX;

    [Range(0f, 0.95f)]
    public float puntoLimite = 0.8f;

    private float anguloEjeY;
    private float anguloEjeZ;
    private float anguloEjeX;

    public bool actualizarRotacion = true;
    private bool anguloObtenido;
    private bool interpolar = true;
    private bool rightWardObtenido;
    private bool hijoRestaurado;

    public float punto;

    private void Start()
    {
        hijo = transform.GetChild(0);
        MantenerRelacionHijo();
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

        if (actualizarRotacion)
        {
            if (!hijoRestaurado)
                MantenerRelacionHijo();

            if (direccionUp == DireccionUp.Arriba)
                VerificarAnguloCartesianoGlobalEjeY();
            else if (direccionUp == DireccionUp.Abajo)
                VerificarAnguloCartesianoEjeYInverso();

            upWard = Quaternion.Euler(ejeRotacion.normalized) * direccion;
            Quaternion rotacionRelativa = Quaternion.LookRotation(direccion, upWard);
            if (interpolar)
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacionRelativa, Time.deltaTime * 6f);
            else
                transform.rotation = rotacionRelativa;

            if (rightWardObtenido)
                rightWardObtenido = false;
        }
        else
        {
            if (!rightWardObtenido)
            {
                rightWardObtenido = true;
                rightWard = transform.right;
                hijoRestaurado = false;
            }
            Vector3 direcionArriba = Vector3.Cross(direccion, rightWard);
            Quaternion rotacionRelativa = Quaternion.LookRotation(direccion, direcionArriba);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionRelativa, Time.deltaTime * 10f);
        }
    }

    private void PoderVerificar()
    {
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
        Vector3 direccionY = direccion;
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
        Vector3 vectorEjeX = new Vector3(-90f, 0f, 0f);
        Vector3 vectorEjeZ = new Vector3(0f, 0f, 90f);

        Vector3 vectorFinal = Vector3.Lerp(vectorEjeX, vectorEjeZ, interpolacion);
        ejeRotacion = vectorFinal;
    }

    private void ActualizarEjeCuadranteII(float angulo)
    {
        float interpolacion = Mathf.Abs(angulo) / 90f;
        Vector3 vectorEjeX = new Vector3(-90f, 0f, 0f);
        Vector3 vectorEjeZ = new Vector3(0f, 0f, -90f);

        Vector3 vectorFinal = Vector3.Lerp(vectorEjeX, vectorEjeZ, interpolacion);
        ejeRotacion = vectorFinal;
    }

    private void ActualizarEjeCuadranteIII(float angulo)
    {
        float interpolacion = (Mathf.Abs(angulo) - 90f) / 90f;
        Vector3 vectorEjeX = new Vector3(90f, 0f, 0f);
        Vector3 vectorEjeZ = new Vector3(0f, 0f, -90f);

        Vector3 vectorFinal = Vector3.Lerp(vectorEjeZ, vectorEjeX, interpolacion);
        ejeRotacion = vectorFinal;
    }

    private void ActualizarEjeCuadranteIV(float angulo)
    {
        float interpolacion = (Mathf.Abs(angulo) - 90f) / 90f;
        Vector3 vectorEjeX = new Vector3(90f, 0f, 0f);
        Vector3 vectorEjeZ = new Vector3(0f, 0f, 90f);

        Vector3 vectorFinal = Vector3.Lerp(vectorEjeZ, vectorEjeX, interpolacion);
        ejeRotacion = vectorFinal;
    }

    private void VerificarAnguloCartesianoEjeZ()
    {
        Vector3 direccionZ = direccion;
        direccionZ.z = 0;
        anguloEjeZ = Vector3.SignedAngle(direccionZ, Vector3.up, -Vector3.forward);
    }

    private void VerificarAnguloCartesianoEjeX()
    {
        Vector3 direccionX = direccion;
        direccionX.x = 0;
        anguloEjeX = Vector3.SignedAngle(direccionX, Vector3.up, Vector3.right);
    }

    private void VerificarAnguloCartesianoEjeYInverso()
    {
        Vector3 direccionY = direccion;
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
        Vector3 vectorEjeX = new Vector3(90f, 0f, 0f);
        Vector3 vectorEjeZ = new Vector3(0f, 0f, -90f);

        Vector3 vectorFinal = Vector3.Lerp(vectorEjeX, vectorEjeZ, interpolacion);
        ejeRotacion = vectorFinal;
    }

    private void ActualizarEjeCuadranteIIDown(float angulo)
    {
        float interpolacion = Mathf.Abs(angulo) / 90f;
        Vector3 vectorEjeX = new Vector3(90f, 0f, 0f);
        Vector3 vectorEjeZ = new Vector3(0f, 0f, 90f);

        Vector3 vectorFinal = Vector3.Lerp(vectorEjeX, vectorEjeZ, interpolacion);
        ejeRotacion = vectorFinal;
    }

    private void ActualizarEjeCuadranteIIIDown(float angulo)
    {
        float interpolacion = (Mathf.Abs(angulo) - 90f) / 90f;
        Vector3 vectorEjeX = new Vector3(-90f, 0f, 0f);
        Vector3 vectorEjeZ = new Vector3(0f, 0f, 90f);

        Vector3 vectorFinal = Vector3.Lerp(vectorEjeZ, vectorEjeX, interpolacion);
        ejeRotacion = vectorFinal;
    }

    private void ActualizarEjeCuadranteIVDown(float angulo)
    {
        float interpolacion = (Mathf.Abs(angulo) - 90f) / 90f;
        Vector3 vectorEjeX = new Vector3(-90f, 0f, 0f);
        Vector3 vectorEjeZ = new Vector3(0f, 0f, -90f);

        Vector3 vectorFinal = Vector3.Lerp(vectorEjeZ, vectorEjeX, interpolacion);
        ejeRotacion = vectorFinal;
    }

    private void MantenerRelacionHijo()
    {
        hijoRestaurado = true;
        Quaternion rotacionHijo = hijo.rotation;
        Vector3 posicionHijo = hijo.position;
        interpolar = false;
        ActualizarPosicion();
        ActualizarRotacion();
        interpolar = true;
        hijo.position = posicionHijo;
        hijo.rotation = rotacionHijo;
        Quaternion rotLocal = hijo.localRotation;
        hijo.localRotation = Quaternion.Euler(rotLocal.eulerAngles.x, rotLocal.eulerAngles.y, 0f);
    }
}
