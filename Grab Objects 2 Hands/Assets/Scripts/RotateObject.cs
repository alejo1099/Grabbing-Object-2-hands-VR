using System.Collections.Generic;
using UnityEngine;

// With this code we can rotate a object with two points of reference, in this case right hand and left hand
// This code is intended for VR
//This code is almost exactly the same that Follow Target
public class RotateObject : MonoBehaviour
{
    // References to the two points
    [SerializeField]
    private Transform leftHand, rightHand;

    // Currentdirection between two points of above //PreviousDirection between two points of above 
    private Vector3 currentDirection, previousDirection;

    // Axis right, left, forward, up and down (we don´t need back axis)
    // Think of this like directions
    private Vector3 right, left, forward, up, down;

    // Save previous directions and currentAverageUp here
    private Queue<Vector3> trackingAxisUp, trackingDirections;

    // Mean vector(direction) of all elements of trackingAxisUp
    private Vector3 currentAverageUp;

    // Restart the trackers but with content updated, not initial content
    private bool restart;

    [SerializeField]
    private float speedRotation = 1f;

    //We start getting initial values
    private void Start()
    {
        //We get the direction of the points(positions), point to rightHand or leftHand
        // currentDirection = (leftHand.position - rightHand.position).normalized; // For left handed
        currentDirection = (rightHand.position - leftHand.position).normalized; // For right handed

        //We set the forward´s object with the direction of above
        transform.forward = previousDirection = currentDirection;

        //Fill the trackers with initial content
        trackingAxisUp = new Queue<Vector3>(5);
        for (int i = 0; i < 5; i++)
            trackingAxisUp.Enqueue(transform.up);
        trackingDirections = new Queue<Vector3>(4);
        for (int i = 0; i < 4; i++)
            trackingDirections.Enqueue(currentDirection);

        restart = true;
    }

    void Update()
    {
        if(Input.GetAxis("Horizontal") != 0)
            RotateAroundCurrentAxis();
        else
            UpdatePosition();
    }

    //With this we can rotate the object around itself and not for two reference points
    //Here we use rodrigues' rotation formula, this is explain below in "CalculateCurrentRotationAxis()" method
    private void RotateAroundCurrentAxis() {
        Vector3 localUp = transform.forward;
        Vector3 localForward = transform.up;
        Vector3 localRight = Vector3.Cross(localUp, localForward);
        float degreesOfRotation = Input.GetAxis("Horizontal") * speedRotation;
        degreesOfRotation *= Mathf.Deg2Rad;
        localForward = (localForward * Mathf.Cos(degreesOfRotation)) + (localRight * Mathf.Sin(degreesOfRotation));
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(localUp, localForward), 0.8f);
        restart = false;
    }

    // Update the position between the two points just with the "midPoint formula"    (x1 + x2, y1 + y2, z1 + z2) / 2
    private void UpdatePosition() {
        transform.position = (rightHand.position + leftHand.position) * 0.5f;
        CalculateDirection();
    }

    private void CalculateDirection() {
        // currentDirection = (leftHand.position - rightHand.position).normalized; // For left handed
        currentDirection = (rightHand.position - leftHand.position).normalized; // For right handed

        // if these are equal, it means that the object is idle
        if (currentDirection == previousDirection) {
            RestartQueues();
            return;
        }

        //with this we can restart the data if the object is idle again
        restart = false;
        UpdateDirections();
    }

    // We restart the trackers because when the object is idle the previous data own to another rotation
    // and if we remain with that data, we affect the new calculus, and we get rotations with "skips"
    private void RestartQueues() {
        // Like said above, we fill the trackers with update(current) data
        if(restart) return;
        for (int i = 0; i < 5; i++) {
            trackingAxisUp.Dequeue();
            trackingAxisUp.Enqueue(transform.up);
        }
        for (int i = 0; i < 4; i++) {
            trackingDirections.Dequeue();
            trackingDirections.Enqueue(currentDirection);
        }
        restart = true;
    }

    // We only tracking the last 4 directions
    private void UpdateDirections() {
        trackingDirections.Dequeue(); //Delete the last direction
        trackingDirections.Enqueue(currentDirection); // push the update direction
        CalculateCurrentRotationAxis();
    }

    // Each rotation depends of two things, to know where is forward(easy part, is just currentDirection),
    // and where is upward(hard part)
    // You can search "Rodrigues' rotation formula" to know the idea behind this
    private void CalculateCurrentRotationAxis() {
        Vector3[] arrDirections = trackingDirections.ToArray(); // We use the directions that we have saved

        //Here we will make something similar to 3d cartesian plane   (x, y, z) axis
        //but limited to a circle, this circle with radius 1, and origin in "this position"(transform.position)

        //With these two lines we get the y axis of our plane
        Vector3 midPointDir = (arrDirections[0] + arrDirections[3]) * 0.5f;
        forward = midPointDir.normalized;


        //Here we get the x axis of our plane, is the forward axis but rotate 90 degrees
        right = (arrDirections[3] - arrDirections[0]).normalized;

        //Now with this we get the z axis, this axis point to "up" 
        up = Vector3.Cross(forward, right);

        //Now we generate rotate axis, align with currentDirection
        forward = currentDirection;

        //We get right rotate 90 degrees with respect to currentDirection(the new forward)
        right = Vector3.Cross(forward, up);

        //We get the opposite directions
        down = -up; // negative z axis
        left = -right; // negative x axis
        SearchAxisNearToUp();
    }

    // Here we search an axis we have made that is points near to transform.up's direction(current up or upward of this object)
    // We actually don't know wich axis is near to transform.up, 
    //but get it is important because it set where is up at this moment of the rotation
     private void SearchAxisNearToUp()
    {
        float maxDot = Mathf.NegativeInfinity;
        Vector3 currentTransformUp = transform.up;
        int currentIndex = -1;

        float dot = Vector3.Dot(currentTransformUp, up);
        if (dot > maxDot && dot > 0.75f) {
            currentIndex = 0;
            maxDot = dot;
        }
        dot = Vector3.Dot(currentTransformUp, down);
        if (dot > maxDot && dot > 0.75f) {
            currentIndex = 1;
            maxDot = dot;
        }
        dot = Vector3.Dot(currentTransformUp, right);
        if (dot > maxDot && dot > 0.75f) {
            currentIndex = 2;
            maxDot = dot;
        }
        dot = Vector3.Dot(currentTransformUp, left);
        if (dot > maxDot && dot > 0.75f) {
            currentIndex = 3;
            maxDot = dot;
        }
        //If we found a suitable axis, we save it
        if(currentIndex != -1)
            UpdateTrackingAxisUp(currentIndex == 0 ? up : currentIndex == 1 ? down : currentIndex == 2 ? right : left);
        else
        //else we just save our current upward
            UpdateTrackingAxisUp(transform.up);
    }

    // We only tracking the last 5 Up axis
    private void UpdateTrackingAxisUp(Vector3 newElement) {
        trackingAxisUp.Dequeue();
        trackingAxisUp.Enqueue(newElement);
        CalculateAverageAxisUp(newElement);
    }


    // If we just choice the most near axis to transform.up, we would get an inestable axis that will cause skips bewtween each rotation
    //For prevent this we get the mean of our saved axis Up
    private void CalculateAverageAxisUp(Vector3 newAxis) {
        Vector3[] vectors = trackingAxisUp.ToArray();
        Vector3 averageUp = Vector3.zero;
        for (int i = 0; i < 5; i++)
            averageUp += vectors[i];
        averageUp = (averageUp * 0.2f).normalized;
        Rotate(averageUp);
    }

    // Finally we apply the rotation mantaining the orientation of the object(mantaining the up's object)
    private void Rotate(Vector3 currentUpward) {
        // We save this because if the object is stopped, we restart the data with this
        currentAverageUp = currentUpward;

        //We get the angular velocity for interpolate the rotation
        float angle = Vector3.Angle(currentDirection, previousDirection);
        float angularSpeed = angle / Time.deltaTime;

        //We generate the new rotation that we want apply, 
        //and we update our forward direction but maintaining our upward direction
        Quaternion rotationToApply = Quaternion.LookRotation(forward, currentAverageUp);

        //Here we interpolate smoothly between the two rotations, based on the angularSpeed
        //So, if we rotate fast the object, the interpolation will be instantly
        transform.rotation = Quaternion.Slerp(transform.rotation, rotationToApply, angularSpeed * 0.01f);

        //We save the currentDirection for check if we are idle or not
        previousDirection = currentDirection;
    }

    // Visual representation about what is happen
    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, forward * 3);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, up * 3);
        Gizmos.DrawRay(transform.position, down * 3);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, right * 3);
        Gizmos.DrawRay(transform.position, left * 3);
    }
}