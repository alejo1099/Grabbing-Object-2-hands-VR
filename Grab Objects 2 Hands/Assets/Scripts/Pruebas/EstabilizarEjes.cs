using System;
using System.Collections.Generic;
using UnityEngine;

public class EstabilizarEjes : MonoBehaviour
{
    public Transform target;

    private Vector3 currentDirection, previousDirection;

    private Vector3 right, left, forward, up, down;
    private Vector3 minusRight, minusLeft;

    private Queue<Vector3> savedDirections;

    private bool restart;
    // public Transform plane;
    
    private void Start()
    {
        currentDirection = (target.position - transform.position).normalized;
        transform.forward = previousDirection = currentDirection;
        savedDirections = new Queue<Vector3>(20);
        for (int i = 0; i < 20; i++)
            savedDirections.Enqueue(currentDirection);
        restart = true;
    }

    void Update()
    {
        CalculateDirection();
    }

    private void CalculateDirection() {
        currentDirection = (target.position - transform.position).normalized;
        if (currentDirection == previousDirection) {
            if(!restart) {
                RestartDirections();
                restart = true;
            }
            return;
        }
        restart = false;
        UpdateDirections();
    }

    private void RestartDirections() {
        savedDirections.Clear();
        for (int i = 0; i < 20; i++)
            savedDirections.Enqueue(currentDirection);
    }

    private void UpdateDirections() {
        savedDirections.Dequeue();
        savedDirections.Enqueue(currentDirection);
        CalculateAngularVelocity();
    }

    private void CalculateAngularVelocity() {
        float angle = Vector3.Angle(currentDirection, previousDirection);
        float angularSpeed = angle / Time.deltaTime;
        if(angularSpeed > 50f)
            CalculateCurrentRotationAxis(16 ,19);
        else
            CalculateCurrentRotationAxis(0 ,19);
    }

    private void CalculateCurrentRotationAxis(int startIndex, int finalIndex) {
        Vector3[] arrDirections = savedDirections.ToArray();
        Vector3 midPointDir = (arrDirections[startIndex] + arrDirections[finalIndex]) * 0.5f;
        forward = midPointDir.normalized;
        right = (arrDirections[finalIndex] - arrDirections[startIndex]).normalized;
        up = Vector3.Cross(forward, right);
        forward = currentDirection;
        // Vector3.OrthoNormalize(ref forward, ref up);
        right = Vector3.Cross(forward, up);
        down = -up;
        left = -right;
        SearchNearestRotationAxis();
    }

    private void SearchNearestRotationAxis() {
        float dotUp = Vector3.Dot(transform.up, up);
        float dotDown = Vector3.Dot(transform.up, down);
        if (dotUp > dotDown)
            CalculateLenghtProjectedAxis(up);
        else
            CalculateLenghtProjectedAxis(down);
    }

    private void CalculateLenghtProjectedAxis(Vector3 axisUp) {
        Vector3 projected = Vector3.Project(transform.up, axisUp);
        float magnitude = (float)Math.Round(projected.magnitude, 1);
        Debug.Log(magnitude);
        if(magnitude >= 0.95f)
            Rotate(axisUp);
        else if(magnitude <= 0.3f) {
            float dotRight = Vector3.Dot(transform.up, right);
            float dotLeft = Vector3.Dot(transform.up, left);
            if(dotRight > dotLeft)
                Rotate(right);
            else
                Rotate(left);
        } else
            CalculateMinusCircle(axisUp, projected, magnitude);
    }


    //0.1 = 0.9949874371
    //0.2 = 0.9797958971
    //0.3 = 0.9539392014
    //0.4 = 0.916515139
    //0.5 = 0.8660254038
    //0.6 = 0.8
    //0.7 = 0.7141428429
    //0.8 = 0.6
    //0.9 = 0.4358898944

    private void CalculateMinusCircle(Vector3 axisUp, Vector3 minusOrigin, float lengthOrigin) {
        Vector3 minOrigin = axisUp * lengthOrigin;
        float magnitudeMinusOriginToTranformsUp = CalculateDistanceTransformUp(lengthOrigin);
        // float magnitudeMinusOriginToTranformsUp = (transform.up - minusOrigin).magnitude;
        // float magnitudeMinusUp = axisUp.magnitude - minusOrigin.magnitude;
        float magnitudeMinusUp = (float)Math.Round(1f - lengthOrigin, 1);
        Vector3 minusUp = axisUp - minOrigin;
        // Vector3 minusUp = axisUp - minusOrigin;
        Vector3 minusForward = currentDirection * magnitudeMinusUp;
        minusRight = Vector3.Cross(minusForward, minusUp).normalized;
        minusLeft = -(Vector3.Cross(minusForward, minusUp).normalized);

        // minusRight = (minusOrigin + (minusRight * magnitudeMinusOriginToTranformsUp)).normalized;
        // minusLeft = (minusOrigin + (minusLeft * magnitudeMinusOriginToTranformsUp)).normalized;
        minusRight = (minOrigin + (minusRight * magnitudeMinusOriginToTranformsUp)).normalized;
        minusLeft = (minOrigin + (minusLeft * magnitudeMinusOriginToTranformsUp)).normalized;
        SearchNearMinusAxis();
    }

    private float CalculateDistanceTransformUp(float length) {
        int intLength = (int)(length * 10);
        switch (intLength)
        {
            case 4:
                return 0.916515139f;
            case 5:
                return 0.8660254038f;
            case 6:
                return 0.8f;
            case 7:
                return 0.7141428429f;
            case 8:
                return 0.6f;
            case 9:
                return 0.4358898944f;
            default:
                return 1;
        }
    }

    private void SearchNearMinusAxis() {
        float dotLeft = Vector3.Dot(transform.up, minusLeft);
        float dotRight = Vector3.Dot(transform.up, minusRight);
        if (dotLeft > dotRight)
            Rotate(minusLeft);
        else
            Rotate(minusRight);
    }

    private void Rotate(Vector3 currentUpward) {
        Quaternion q = Quaternion.LookRotation(forward, currentUpward);
        transform.rotation = Quaternion.Slerp(transform.rotation, q, 1);
        previousDirection = currentDirection;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, forward * 3);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, up * 3);
        // Gizmos.DrawRay(transform.position, down * 3);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, right * 3);
        // Gizmos.DrawRay(transform.position, left * 3);

        // Transform currPlane = plane;

        // Vector3 forwDireccion = Vector3.ProjectOnPlane(target.position, currPlane.up);
        // Gizmos.color = Color.black;
        // Gizmos.DrawRay(transform.position, forwDireccion);

        // Gizmos.color = Color.green;
        // Gizmos.DrawRay(transform.position, currPlane.up * 2);

        // Vector3 righDireccion = -Vector3.Cross(forwDireccion, currPlane.up);
        // Gizmos.color = Color.red;
        // Gizmos.DrawRay(transform.position, righDireccion);

        // float dot = Vector3.Dot(diagonal.position, up);
        // Vector3 currentGizmosUp =  Vector3.zero;
        // if (dot > 0.2f)
        //     currentGizmosUp = up;
        // else
        //     currentGizmosUp = down;

        // Vector3 minusOrigin = Vector3.Project(transform.up, currPlane.up);
        // float magnitudeMinusUp = currPlane.up.magnitude - minusOrigin.magnitude;
        // float distanceOrigUp = (transform.up - minusOrigin).magnitude;
        // Vector3 minusUp = minusOrigin.normalized * magnitudeMinusUp;
        // Vector3 minusForward = forwDireccion.normalized * magnitudeMinusUp;

        // Vector3 gizmosMinusRight = Vector3.Cross(minusForward, minusUp).normalized;
        // Vector3 gizmosMinusLeft = -(Vector3.Cross(minusForward, minusUp).normalized);

        // Vector3 finalMinusRight = (minusOrigin + (gizmosMinusRight.normalized * distanceOrigUp)).normalized;
        // Vector3 finalMinusLeft = (minusOrigin + (gizmosMinusLeft.normalized * distanceOrigUp)).normalized;
        // transform.rotation = Quaternion.LookRotation(forwDireccion, finalMinusLeft);

        // Gizmos.color = Color.black;
        // Gizmos.DrawRay(transform.position, minusLeft);
        // Gizmos.color = Color.white;
        // Gizmos.DrawRay(transform.position, minusRight);
    }
}
