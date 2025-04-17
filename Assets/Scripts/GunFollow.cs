using UnityEngine;

public class GunFollow : MonoBehaviour
{
    public Transform followTarget;

    void LateUpdate()
    {
        if (followTarget != null)
        {
            transform.position = followTarget.position;
            transform.rotation = followTarget.rotation;
        }
    }
}
