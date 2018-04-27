using UnityEngine;

public enum DireccionUp
{
    Arriba, Abajo
}

public enum OrientacionObjeto
{
    Cabeza, DeCabeza
}

public class RotarObjeto : MonoBehaviour
{
    public DireccionUp direccionUp;
    public Cuadrantes cuadranteDerecha;
    public OrientacionObjeto orientacionObjeto;

    public Transform derecha, izquierda, cabeza;

    public Vector3 ejeRotacion;

    private float valorY;

    [Range(0f, 0.95f)]
    public float puntoLimite = 0.8f;

    private float anguloEjeY;
    private float anguloEjeZ;
    private float anguloEjeX;

    private bool actualizarRotacion = true;

    public float punto;

    private void Update()
    {
        ActualizarPosicion();
        ActualizarRotacion();

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
            transform.rotation = rotacionRelativa;//Quaternion.Slerp(transform.rotation, rotacionRelativa, Time.deltaTime * 6f);
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
        }
        else
        {
            actualizarRotacion = true;
        }
    }

    private void VerificarUp()
    {
        if (!actualizarRotacion)
        {
            CalcularPlanoCartesianoLocal();

            // if (orientacionObjeto == OrientacionObjeto.Cabeza)
            // {
            if (cuadranteDerecha == Cuadrantes.II || cuadranteDerecha == Cuadrantes.III)
                direccionUp = DireccionUp.Abajo;
            else
                direccionUp = DireccionUp.Arriba;
            // }
            // else if (orientacionObjeto == OrientacionObjeto.DeCabeza)
            // {
            // if (cuadranteDerecha == Cuadrantes.II || cuadranteDerecha == Cuadrantes.III)
            //     direccionUp = DireccionUp.Arriba;
            // else
            //     direccionUp = DireccionUp.Abajo;
            // }
        }
    }

    private void CalcularUp()
    {
        float dot = Vector3.Dot(transform.up, Vector3.up);
        if (dot > 0f)
            orientacionObjeto = OrientacionObjeto.Cabeza;
        else if (dot < 0f)
            orientacionObjeto = OrientacionObjeto.DeCabeza;
    }

    private void CalcularPlanoCartesianoLocal()
    {
        Vector3 objeto = transform.position;
        Vector3 posicionCabeza = cabeza.position;
        Vector3 posicionDerecha = derecha.position;
        Vector3 posicionIzquierda = izquierda.position;
        posicionDerecha.y = posicionIzquierda.y = posicionCabeza.y = objeto.y;

        Vector3 planoY = (objeto - posicionCabeza);
        Vector3 planoX = Quaternion.Euler(0f, 90f, 0f) * planoY;
        Vector3 planoZ = Vector3.Cross(planoY, planoX);

        Vector3 posicionDerechaSobreElPlano = (posicionDerecha - objeto).normalized;
        float anguloDerecha = Vector3.SignedAngle(posicionDerechaSobreElPlano, planoY, planoZ);

        VerificarCuadrante(anguloDerecha);
    }

    private void VerificarCuadrante(float angulo)
    {
        if (angulo > -90f && angulo < 0f)
            cuadranteDerecha = Cuadrantes.I;
        else if (angulo > 0f && angulo < 90f)
            cuadranteDerecha = Cuadrantes.II;
        else if (angulo > 90f && angulo < 180f)
            cuadranteDerecha = Cuadrantes.III;
        else if (angulo > -180f && angulo < -90f)
            cuadranteDerecha = Cuadrantes.IV;
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