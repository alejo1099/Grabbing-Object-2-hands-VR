using UnityEngine;
using System.Collections;

//Cuadrantes definidos como en un plano cartesiano, si la poscion en X y de Y de un objeto es positiva,entonces esta en el cuadrante I o 1
//Los cuadrantes aqui se definen horizontalmente, por lo que se reemplaza Y por Z
public enum Cuadrantes
{
    I, II, III, IV,
}

public class Pivote : MonoBehaviour
{
    public Cuadrantes cuadranteDerecha; //Indicador manejable, del cuadrante del control derecho(no puse el izquierdo, porque me di cuenta que no lo habia necesitado, y lo borre)
    public Transform padreCubo, cubo; //Transform de padreCubo, y cubo como se muestran en el inspector
    public Transform derecha, izquierda; //Referencias Controles
    public Transform cabeza; //Referencia a la cabeza o al casco del SteamVr

    private Transform miTransform; //Referencia a este Transform, conocido en el inspector como Padre

    private IEnumerator corutina; //Referencia a una variable IEnumerator para poder tener la capacidad, de iniciar y detener corutinas cuando se desee
    private WaitUntil waitUntilCompleteRotation; //Variable Waituntil, que servira para ejecutar la corutina deseada, basicamente acepta un metodo que devuelve bool
    //y detiene la corutina hasta que el metodo retorne un true

    private float interpolacionCubo; //Variable que servira para calcular la interpolacion en la corutina
    public float dot; //variable que referencia un producto punto Vector3.Dot
    public float puntoLimite; //Variable que determina, en que valor "dot" cambiara su eje de rotacion

    private Quaternion rotacionCubo; //Variable que almacena la rotacion del cubo, para hacer la interpolccion. Esto se hace para evitar los saltos bruscos que genera el cambio de eje

    //variables que determinan a que eje se debe apuntar
    public bool apuntarDown;
    public bool apuntarUp;
    public bool apuntarForward = true;
    public bool apuntarBack;

    private bool interpolar = false; //Variable que determina si cubo debe interpolar


    private Vector3 vectorForward; //Variable que determina hacia donde apunta el eje actual p/e miTransform.forward = vectorForward
    //o miTransform.up = vectorForward si si apuntarUp es positivo 

    private void Start()
    {
        waitUntilCompleteRotation = new WaitUntil(InterRotacionCubo); //Se crea la instancia de la variable WaitUntil, que ejecutara el metodo InterRotacionCubo,
        // cada vez que inicie la corutina, hasta que devuelva true
        miTransform = transform;
    }

    private void Update()
    {
        CalcularPlanoCartesiano(); //Se calcula el plano cartesiano del control derecho, para determinar su posicion con respecto al objeto que tiene
        //"en la mano", es decir este transform

        Vector3 posRight = derecha.position;
        Vector3 posLeft = izquierda.position;

        Vector3 vectorX = (posRight - posLeft).normalized; //Obtenemos la direccion que hay entre los 2 controles, con direccion a la derecha

        dot = Vector3.Dot(vectorX, Vector3.up); //Obtenemos el producto punto con respecto al vector3.Up. El producto punto basicamente devuelve un numero entre 1 y -1
        //donde valores cercanos a 1, quieren decir que el primer vector apunta en la misma direccion que el segundo vector, 0 significa que son perpendiculares, es decir que forman algo asi
        //como una L, y -1, significa que apuntan en direccion opuesta

        vectorForward = (posRight - posLeft).normalized; //Calcule el mismo vector porque no he "optimizado" el codigo, y solo importa que funcione primero

        Vector3 halfPos = Vector3.Lerp(derecha.position, izquierda.position, 0.5f); //Aqui obtenemos la posicion que hay entre los 2 controles
        miTransform.position = halfPos; //Y la posicion de este transform siempre sera en la mitad de los 2 controles

        Debug.DrawLine(posRight, posLeft, Color.red);

        Vector3 directionForward = Vector3.zero;

        Vector3 posRelRight = posRight;
        Vector3 posRelLeft = posLeft;
        posRelRight.y = posRelLeft.y = miTransform.position.y; //Definimos los mismo vectores, pero cambiamos su valor de y, para que esten alineados con el y de esta posicion

        DefinirEjeRotacion(dot);

        CalcularEjesCubo(posRight, posLeft, ref directionForward);

        Vector3 direccionDerecha = (izquierda.position - derecha.position).normalized; //Aqui determinamos la "derecha" del padreCubo
        Vector3 direccionArriba = Vector3.Cross(directionForward, direccionDerecha); //Aqui con la "derecha", y el "frente" definidos, sacamos el "arriba" del padreCubo

        Debug.DrawRay(halfPos, directionForward, Color.blue); //Con estos 2 se puede visualizar lo hecho anteriormente
        Debug.DrawRay(halfPos, direccionArriba, Color.green);

        padreCubo.rotation = Quaternion.LookRotation(directionForward, direccionArriba); //Aqui le decimos a padre cubo se rotacion, tambien dandole, su direccion de arriba
        //que sacamos anteriormente

        //Si en el metodo DefinirEjeRotacion, se establece cambiar el eje, entonces interpolamos, la rotacion de cubo(cubo visible), para que no genere un salto brusco de eje
        //como en el metodo DefinirEjeRotacion se guardo la rotacion del cubo, se agrega para que no se note que paso el salto
        if (interpolar)
        {
            interpolar = false; //No queremos que se ejecute cada frame
            cubo.rotation = rotacionCubo;
            if (corutina != null) StopCoroutine(corutina); //Aqui decimos que si el cubo esta haciendo la interpolacion actualmente, entonces que la detenga
            corutina = InterpolarRotacion();
            interpolacionCubo = 0;
            StartCoroutine(corutina);//Iniciampos la interpolacion
        }
    }

    private void CalcularPlanoCartesiano()
    {
        Vector3 posicionPadre = miTransform.position;
        Vector3 posicionCabeza = cabeza.position;
        Vector3 posicionDerecha = derecha.position;
        Vector3 posicionIzquierda = izquierda.position;
        posicionDerecha.y = posicionIzquierda.y = posicionCabeza.y = posicionPadre.y;

        //Se pone las posiciones de y de izquierda, derecha y cabeza a la de posicionpadre, para que el plano
        //cartesiano quede a la altura de este transform, que es la posicionPadre

        Vector3 planoY = (posicionPadre - posicionCabeza); //Sacamos la direccion que hay desde la posicion de la cabeza, hasta este transform, y ese seria el planoY, o eje Y de nuestro plano cartesiano

        Vector3 planoX = Quaternion.Euler(0f, 90f, 0f) * planoY; //Aqui se multiplica un Quaternion por un vector, porque se quiere rotar la direccion de planoY, 90 grados en el eje Y, 
        //esto solo funciona con direcciones y no con posiciones. Ahora el planoX es el ejeX de nuestro plano cartesiano

        Vector3 planoZ = Vector3.Cross(planoY, planoX);//Aqui se quiere sacar un vector perpendicular a los otros, basicamente planoY es como el transform.forward, el planoX es el right, y 
        //con las direcicones de esos 2 vectores se sace el planoZ que seria como el transform.up

        Vector3 posicionDerechaSobreElPlano = (posicionDerecha - posicionPadre).normalized; //Aqui calculamos la direccion que hay desde este transform, hasta el control derecho. Y se normaliza dado que no
                                                                                            //nos interesa su tamaño, solo su direccion

        float anguloDerecha = Vector3.SignedAngle(posicionDerechaSobreElPlano, planoY, planoZ); //Calculamos los grados, que hay entre la direccion del planoY, y la posicion del contorl derecho

        VerificarCuadrante(anguloDerecha, ref cuadranteDerecha); //Aqui mandamos ese angulo, y se verifica en que cuadrante esta el control derecho
        //El SignedAngle calcula los grados de la forma en que, si un objeto esta a la derecha, entonces sera un angulo negativo, y lo calcula de 0 a 180, en la izquierda, y 0 a -180 a la derecha

        //Dibujamos las direccion del plano cartesiano para visualizar mejor, esto se puede comentar si estorba visualmente
        // Debug.DrawRay(posicionPadre, planoY * 2f, Color.magenta);
        // Debug.DrawRay(posicionPadre, planoX * 2f, Color.yellow);
        // Debug.DrawRay(posicionPadre, planoZ * 2f, Color.white);
    }

    private void VerificarCuadrante(float angulo, ref Cuadrantes cuadrante)
    {
        if (angulo > -90f && angulo < 0f)
            cuadrante = Cuadrantes.I;
        else if (angulo > 0f && angulo < 90f)
            cuadrante = Cuadrantes.II;
        else if (angulo > 90f && angulo < 180f)
            cuadrante = Cuadrantes.III;
        else if (angulo > -180f && angulo < -90f)
            cuadrante = Cuadrantes.IV;
    }

    private void DefinirEjeRotacion(float punto)
    {
        //Aqui cogemos el producto punto que obtuvimos, y empezamos a definir encual eje se debe enfocar la direccion
        //tambien se guarda la rotacion del cubo, para que haga la transicion, suavemente, y no abrupta como sus padres

        //Aqui puede estar la base de la solucion

        //Si el punto es mayor al punto limite, (que se establecio como 0.8), y no se apunta al Up, entonces se apunta en esa direccion
        if (punto > puntoLimite && !apuntarUp)
        {
            interpolar = true;
            rotacionCubo = cubo.rotation;
            apuntarUp = true;
            apuntarForward = apuntarBack = apuntarDown = false;
        }
        //Aqui se hace lo mismo para apuntar hacia abajo, pero con valores negativos
        else if (punto < -puntoLimite && !apuntarDown)
        {
            interpolar = true;
            rotacionCubo = cubo.rotation;
            apuntarDown = true;
            apuntarUp = apuntarForward = apuntarBack = false;
        }

        //Aqui se define basicamente si al girar horizontalmente, se hace con la "cabeza" o lado arriba, o "abajo" , y se define dependiendo del cuadrante de derecha
        //Donde si el control derecha, esta terminando de hacer una transicion(pasar por encima del izquierdo). Se define en que cuadrante esta actualemnte
        //Y si cuando hace la transicion, esta al lado derecho del objeto, entonces rotara en el ejeY con la cabeza "arriba"
        else if ((punto < puntoLimite && punto > -puntoLimite) && (cuadranteDerecha == Cuadrantes.I || cuadranteDerecha == Cuadrantes.IV) && !apuntarBack && !apuntarForward)
        {
            interpolar = true;
            rotacionCubo = cubo.rotation;
            apuntarForward = true;
            apuntarUp = apuntarBack = apuntarDown = false;
        }
        //Aqui es lo mismo, que arriba, pero determina, si el control derecho queda a la izquierda del objeto, entonces girara en el Y de "cabeza"

        //Estos 2 solo se activan si se pasan los controles por encima del otro, es decir si el producto punto es cercano a 1, o -1
        else if ((punto < puntoLimite && punto > -puntoLimite) && (cuadranteDerecha == Cuadrantes.II || cuadranteDerecha == Cuadrantes.III) && !apuntarForward && !apuntarBack)
        {
            interpolar = true;
            rotacionCubo = cubo.rotation;
            apuntarBack = true;
            apuntarUp = apuntarForward = apuntarDown = false;
        }
    }

    private void CalcularEjesCubo(Vector3 posRight, Vector3 posLeft, ref Vector3 directionForward)
    {
        //Aqui se aplica lo ejecutado en el metodo DefinirEjeRotacion
        //Ademas cogemos directionForward, para devolver el vector, que esta al "frente" del vector que esta entre los 2 controles


        //El transform.forward apuntando hacia el vectorfoward (posright - posleft).normalized 
        if (apuntarForward)
        {
            miTransform.forward = vectorForward;

            posRight.y = posLeft.y = miTransform.position.y;
            directionForward = (posLeft - posRight).normalized;
            directionForward = Quaternion.Euler(0f, -90f, 0f) * directionForward;
        }
        else if (apuntarUp)
        {
            miTransform.up = vectorForward;

            posRight.x = posLeft.x = miTransform.position.x;
            directionForward = (posLeft - posRight).normalized;
            directionForward = Quaternion.Euler(90f, 0f, 0f) * directionForward;
        }
        else if (apuntarDown)
        {
            miTransform.up = -vectorForward;

            posRight.x = posLeft.x = miTransform.position.x;
            directionForward = (posLeft - posRight).normalized;
            directionForward = Quaternion.Euler(-90f, 0f, 0f) * directionForward;
        }
        else if (apuntarBack)
        {
            miTransform.forward = -vectorForward;

            posRight.y = posLeft.y = miTransform.position.y;
            directionForward = (posLeft - posRight).normalized;
            directionForward = Quaternion.Euler(0f, 90f, 0f) * directionForward;
        }
    }

    private IEnumerator InterpolarRotacion() //Cuando se inicia la corutina se detendra en el WaiUntiil
    {
        yield return waitUntilCompleteRotation;
    }

    private bool InterRotacionCubo()
    {
        //Aqui interpolamos la rotacion entre el cubo, y el padreCubo, dado que padreCubo, es el que realmente esta orientado
        //correctamente en sus ejes, y cubo simplemente como es hijo lo sigue, pero al cambiar los ejes de padre, el cambio es brusco, y se hace esto para que no se note nada
        Quaternion interpolacion = Quaternion.Slerp(cubo.rotation, padreCubo.rotation, interpolacionCubo);
        cubo.rotation = interpolacion;
        interpolacionCubo += 0.005f;
        if (interpolacionCubo >= 1)
        {
            interpolacionCubo = 1;
            interpolacion = Quaternion.Slerp(cubo.rotation, padreCubo.rotation, interpolacionCubo);
            cubo.rotation = interpolacion;
            return true;
        }
        return false;
    }
}