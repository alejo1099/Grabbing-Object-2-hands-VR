using UnityEngine;

public class RotarSimple : MonoBehaviour
{
    public Transform derecha, izquierda;

    public Vector3 ejeRotacion;

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
        Vector3 direccion = (derecha.position - izquierda.position).normalized;
        Vector3 arriba = Quaternion.Euler(ejeRotacion) * direccion;
        transform.rotation = Quaternion.LookRotation(direccion, arriba);
    }
}
