using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform target;

    private Vector3 currentDirection, previousDirection;

    private Vector3 right, left, forward, up, down;

    private Queue<Vector3> savedAxisUp, savedDirections;

    private Vector3 currentAverageUp;

    private bool restart;

    
    private void Start()
    {
        currentDirection = (target.position - transform.position).normalized;
        transform.forward = previousDirection = currentDirection;
        savedAxisUp = new Queue<Vector3>(5);
        for (int i = 0; i < 5; i++)
            savedAxisUp.Enqueue(transform.up);
        savedDirections = new Queue<Vector3>(4);
        for (int i = 0; i < 4; i++)
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
                RestartQueues();
                restart = true;
            }
            return;
        }
        restart = false;
        UpdateDirections();
    }

    private void RestartQueues() {
        for (int i = 0; i < 5; i++) {
            savedAxisUp.Dequeue();
            savedAxisUp.Enqueue(currentAverageUp);
        }
        for (int i = 0; i < 4; i++) {
            savedDirections.Dequeue();
            savedDirections.Enqueue(currentDirection);
        }
    }

    private void UpdateDirections() {
        savedDirections.Dequeue();
        savedDirections.Enqueue(currentDirection);
        CalculateCurrentRotationAxis();
    }

    private void CalculateCurrentRotationAxis() {
        Vector3[] arrDirections = savedDirections.ToArray();
        Vector3 midPointDir = (arrDirections[0] + arrDirections[3]) * 0.5f;
        forward = midPointDir.normalized;
        right = (arrDirections[3] - arrDirections[0]).normalized;
        up = Vector3.Cross(forward, right);
        forward = currentDirection;
        right = Vector3.Cross(forward, up);
        down = -up;
        left = -right;
        SearchAxisNearToUp();
    }

     private void SearchAxisNearToUp()
    {
        float maxDot = Mathf.NegativeInfinity;
        Vector3 currentTransformUp = transform.up;
        int currentIndex = -1;

        float dot = Vector3.Dot(currentTransformUp, up);
        if (dot > maxDot && dot > 0.65f) {
            currentIndex = 0;
            maxDot = dot;
        }
        dot = Vector3.Dot(currentTransformUp, down);
        if (dot > maxDot && dot > 0.65f) {
            currentIndex = 1;
            maxDot = dot;
        }
        dot = Vector3.Dot(currentTransformUp, right);
        if (dot > maxDot && dot > 0.65f) {
            currentIndex = 2;
            maxDot = dot;
        }
        dot = Vector3.Dot(currentTransformUp, left);
        if (dot > maxDot && dot > 0.65f) {
            currentIndex = 3;
            maxDot = dot;
        }
        
        if(currentIndex != -1)
            UpdateTrackingAxisUp(currentIndex == 0 ? up : currentIndex == 1 ? down : currentIndex == 2 ? right : left);
        else
            UpdateTrackingAxisUp(transform.up);
    }

    private void UpdateTrackingAxisUp(Vector3 newElement) {
        savedAxisUp.Dequeue();
        savedAxisUp.Enqueue(newElement);
        CalculateAverageAxisUp(newElement);
    }

    private void CalculateAverageAxisUp(Vector3 newAxis) {
        Vector3[] vectors = savedAxisUp.ToArray();
        Vector3 averageUp = Vector3.zero;
        for (int i = 0; i < 5; i++)
            averageUp += vectors[i];
        averageUp = (averageUp * 0.2f).normalized;
        Rotate(averageUp);
    }   

    private void Rotate(Vector3 currentUpward) {
        currentAverageUp = currentUpward;
        float angle = Vector3.Angle(currentDirection, previousDirection);
        float angularSpeed = angle / Time.deltaTime;
        Quaternion q = Quaternion.LookRotation(forward, currentAverageUp);
        transform.rotation = Quaternion.Slerp(transform.rotation, q, angularSpeed * 0.01f);
        previousDirection = currentDirection;
    }

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
