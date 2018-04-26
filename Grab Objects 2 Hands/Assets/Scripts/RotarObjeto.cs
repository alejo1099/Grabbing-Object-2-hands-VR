using UnityEngine;

public enum DireccionUp
{
    Arriba, Abajo
}

public class RotarObjeto : MonoBehaviour
{
    //Si derecha esta a la derecha del objeto, y el objeto apunta hacia arriba, entonces el valor correcto es (0,0,90)
    //Si derecha esta al frente del objeto, y el objeto apunta hacia arriba, entonces el valor correcto es (-90,0,0)

    //Si derecha esta diagonal de izquierda, y se encuentra al frente y a la derecha, y el objeto apunta hacia arriba, entonces el valor correcto es (-45,0,45)
    //El angulo depende de la posicion, y se puede ir promediando

    //Si derecha esta a la izquierda del objeto, y el objeto apunta hacia arriba, entonces el valor correcto es (0,0,-90)
    //Si derecha esta atras del objeto, y el objeto apunta hacia arriba, entonces el valor correcto es (90,0,0)

    //Cuando se esta rotando e interpolando el angulo, si por ejemplo, el angulo que (-45, 0, 45), entonces queda una rotacion que hace voltear al objeto, alrededor de -16 en el eje Z
    //y si se suma con el valor que deberia estar, quedaria (-45, -29, 45), y -29 - 16 = -45, por lo que hay relacion en esa rotacion tambien

    //El eje Y con el Vector -29.99999, 0, 60,00001 necesita -22.37 en el valor Y del vector para acomodarse a su rotacion
    //El eje Y con el Vector -60.00001, 0, 29,99999 necesita -33.435 en el valor Y del vector para acomodarse a su rotacion

    //-22.37  -10.472

    //Si la z esta entre 1 y 179, se puede girar alrededor de este sin ningun problema

    public DireccionUp direccionUp;

    public Transform derecha, izquierda;

    public Vector3 ejeRotacion;

    public float valorY;

    public float anguloEjeY;
    public float anguloEjeZ;
    public float anguloEjeX;

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
        VerificarUp();

        if (direccionUp == DireccionUp.Arriba)
            VerificarAnguloCartesianoEjeY();
        else if (direccionUp == DireccionUp.Abajo)
            VerificarAnguloCartesianoEjeYInverso();

        Vector3 direccion = (derecha.position - izquierda.position).normalized;
        Vector3 arriba = Quaternion.Euler(ejeRotacion) * direccion;
        transform.rotation = Quaternion.LookRotation(direccion, arriba);
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
        // if (anguloEjeZ <= 0 && anguloEjeZ >= -180f)
        //     direccionUp = DireccionUp.Arriba;
        // else if (anguloEjeZ >= 0 && anguloEjeZ <= 180f)
        //     direccionUp = DireccionUp.Abajo;
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
}