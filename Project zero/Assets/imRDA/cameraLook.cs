using UnityEngine;

public class CameraLook : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    [Header("Movement Settings")]
    public float smoothTime = 0.15f;
    private Vector3 currentVelocity = Vector3.zero;

    void Start()
    {
        if (offset == Vector3.zero && target != null)
        {
            offset = transform.position - target.position;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Вычисляем позицию
        Vector3 targetPosition = target.position + offset;

        // Если smoothTime очень мал, просто ставим позицию (оптимизация)
        if (smoothTime <= 0)
        {
            transform.position = targetPosition;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref currentVelocity,
                smoothTime
            );
        }
    }
}
