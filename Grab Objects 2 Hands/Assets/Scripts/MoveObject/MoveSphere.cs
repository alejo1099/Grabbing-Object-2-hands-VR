using UnityEngine;

public class MoveSphere : MonoBehaviour
{
    [SerializeField]
    private Transform anotherHand;
    public float speedMovement;

    private float movementX;
    private float movementZ;
    private float movementY;

    private float currX;
    private float currY;
    private float currZ;

    private float currXAnother;
    private float currYAnother;
    private float currZAnother;

    private bool getValues;
    Vector3 prevPosition;

    void Update()
    {
        Vector3 velocity = (transform.position - prevPosition) / Time.deltaTime;

        prevPosition = transform.position;
        if(!Input.anyKey) {
            NaturalMovement();
            return;
        }
        movementX = Input.GetAxis("Horizontal");
        movementZ = Input.GetAxis("Vertical");
        movementY = Input.GetAxis("Up") * 2f;
        Vector3 newPosition = new Vector3(movementX, movementY, movementZ) * speedMovement;
        transform.position += newPosition;
        anotherHand.position -= newPosition;
        getValues = false;
    }

    private void NaturalMovement() {
        if(!getValues) {
            currX = transform.position.x;
            currY = transform.position.y;
            currZ = transform.position.z;
            currXAnother = anotherHand.position.x;
            currYAnother = anotherHand.position.y;
            currZAnother = anotherHand.position.z;
            getValues = true;
        }
        movementX = movementY = movementZ = Mathf.PingPong(Time.time * 0.05f, 0.1f) - 0.05f;
        Vector3 smoothMovementTransform = new Vector3(currX - movementX, currY - movementY, currZ - movementZ);
        transform.position = smoothMovementTransform;
        smoothMovementTransform = new Vector3(currXAnother + movementX, currYAnother + movementY, currZAnother + movementZ);
        anotherHand.position = smoothMovementTransform;
    }
}
