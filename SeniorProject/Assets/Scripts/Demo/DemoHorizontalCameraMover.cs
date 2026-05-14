using UnityEngine;

public class DemoHorizontalCameraMover : MonoBehaviour
{
    public Transform targetToMove;
    public float moveSpeed = 8f;
    public float minX = -12f;
    public float maxX = 12f;
    public bool useUnscaledTime = false;

    private void Start()
    {
        if (targetToMove == null)
        {
            targetToMove = transform;
        }
    }

    private void Update()
    {
        if (targetToMove == null)
        {
            return;
        }

        float direction = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            direction -= 1f;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            direction += 1f;
        }

        if (Mathf.Approximately(direction, 0f))
        {
            return;
        }

        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        Vector3 position = targetToMove.position;
        position.x = Mathf.Clamp(position.x + direction * moveSpeed * deltaTime, minX, maxX);
        targetToMove.position = position;
    }
}
