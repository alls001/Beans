using UnityEngine;

public class SlowRotation : MonoBehaviour
{
    // Velocidade de rotação em graus por segundo
    public float rotationSpeed = 10f;

    void Update()
    {
        // Rotaciona o objeto ao redor do eixo Z
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}