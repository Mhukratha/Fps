using UnityEngine;

public class FloatingRotating : MonoBehaviour
{
    public float rotationSpeed = 50f;      // ความเร็วในการหมุน
    public float floatAmplitude = 0.5f;    // ระยะทางการลอยขึ้นลง
    public float floatFrequency = 1f;      // ความเร็วในการลอยขึ้นลง

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // ลอยขึ้นลง
        float newY = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = startPos + new Vector3(0f, newY, 0f);

        // หมุนรอบแกน Y
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
