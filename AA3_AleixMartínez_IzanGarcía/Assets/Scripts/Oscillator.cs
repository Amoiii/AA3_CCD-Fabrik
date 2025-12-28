using UnityEngine;

public class Oscillator : MonoBehaviour
{
    public float speed = 2.0f;     // Velocidad de movimiento
    public float distance = 1.5f;  // Cuánto se aleja del centro

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Mueve el objeto en el eje X usando una onda Seno
        float offset = Mathf.Sin(Time.time * speed) * distance;
        transform.position = startPos + new Vector3(offset, 0, 0);
    }
}