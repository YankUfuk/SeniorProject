using UnityEngine;

public class DemoCameraRotator : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed = 10f;
    public bool rotateAutomatically = true;

    private void Update()
    {
        if (!rotateAutomatically || target == null)
        {
            return;
        }

        transform.RotateAround(target.position, Vector3.up, rotationSpeed * Time.deltaTime);
        transform.LookAt(target);
    }
}
