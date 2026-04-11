using UnityEngine;

public class FloatMotion : MonoBehaviour
{
    public float amplitude = 0.5f;   // Altura máxima do movimento
    public float speed = 2f;         // Velocidade da flutuação

    private Vector3 startPosition;

    void Start()
    {
        // Guarda a posição inicial do objeto
        startPosition = transform.position;
    }

    void Update()
    {
        // Calcula o deslocamento vertical com base no tempo
        float newX = startPosition.x + Mathf.Sin(Time.time * speed) * amplitude;

        // Aplica o movimento no eixo Y
        transform.position = new Vector3(newX ,transform.position.y, transform.position.z);
    }
}
