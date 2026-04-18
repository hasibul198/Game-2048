using UnityEngine;

public class LightSweeper : MonoBehaviour
{
    public float rotationRange = 20f; 
    public float speed = 2f;         
    private float startRotation;

    void Start()
    {
        
        startRotation = transform.localEulerAngles.z;
    }

    void Update()
    {
        
        float angle = Mathf.Sin(Time.time * speed) * rotationRange;
        transform.localRotation = Quaternion.Euler(0, 0, startRotation + angle);
    }
}