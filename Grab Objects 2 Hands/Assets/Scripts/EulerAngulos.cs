using UnityEngine;

public class EulerAngulos : MonoBehaviour
{
    private void Update()
    {
        Quaternion inverso = transform.localRotation * Quaternion.Euler(0f, 180f, 0f);
        Vector3 euler = inverso.eulerAngles;
        float numeroY = euler.y;
        numeroY -= 180f;

        print(numeroY + " Modificado " + euler.y + " Normal");
    }
}