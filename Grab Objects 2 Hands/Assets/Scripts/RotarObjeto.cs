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


    public DireccionUp direccionUp;

    public Transform derecha, izquierda;

    public Vector3 ejeRotacion;

    public float anguloHorizontal;
    public float anguloVertical;
    public bool actualizarAngulo;

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
        ActualizarEjeRotacion();
        //verificarProductroPunto();
        // Vector3 direccion = (derecha.position - izquierda.position).normalized;
        // if (actualizarAngulo)
        // {
        //     Vector3 arriba = Quaternion.Euler(ejeRotacion) * direccion;
        //     transform.rotation = Quaternion.LookRotation(direccion, arriba);
        // }
        // else
        //     transform.rotation = Quaternion.LookRotation(direccion);
    }

    // private void verificarProductroPunto()
    // {
    //     Vector3 direccion = (derecha.position - izquierda.position).normalized;
    //     anguloVertical = Vector3.Dot(direccion, Vector3.up);
    //     direccion.y = 0;
    //     anguloHorizontal = Vector3.SignedAngle(direccion, Vector3.forward, Vector3.up);

    //     if (anguloHorizontal < -85f && anguloHorizontal > -95f)
    //     {
    //         ejeRotacion = new Vector3(0, 0, 90f);
    //         actualizarAngulo = true;
    //     }
    //     else if (anguloHorizontal < 5f && anguloHorizontal > -5f)
    //     {
    //         ejeRotacion = new Vector3(-90f, 0, 0);
    //         actualizarAngulo = true;
    //     }
    //     else if (anguloVertical < 0.4f && anguloVertical > -0.4f)
    //     {
    //         actualizarAngulo = false;
    //     }
    //     else
    //     {
    //         actualizarAngulo = true;
    //     }
    // }

    private void ActualizarEjeRotacion()
    {
        Vector3 direccion = (derecha.position - izquierda.position).normalized;
        direccion.y = 0;
        anguloHorizontal = Vector3.SignedAngle(direccion, Vector3.forward, Vector3.up);




    }
}