using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public Slider slider;
    public Transform faceCamera;

    public void SetMaxHealth(int health)
    {
        if (slider != null) slider.maxValue = health;
    }

    public void SetHealth(int health)
    {
        if (slider != null) slider.value = health;
    }

    void LateUpdate()
    {
        if (faceCamera != null)
        {
            transform.LookAt(transform.position + faceCamera.forward);
        }
    }
}
