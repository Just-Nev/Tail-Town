using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    public float rotationSpeed = 180f; // Degrees per second
    private bool isOpen = false;
    private bool isRotating = false;

    void OnMouseDown()
    {
        if (!isRotating)
        {
            float targetAngle = isOpen ? 0f : 90f;
            StartCoroutine(RotateToAngle(targetAngle));
            isOpen = !isOpen;
        }
    }

    IEnumerator RotateToAngle(float targetZ)
    {
        isRotating = true;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetZ);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.rotation = targetRotation;
        isRotating = false;
    }
}

