using UnityEngine;

public class BillboardFlipByVelocity : MonoBehaviour
{
    [Header("Referências")]
    public Rigidbody projectileRb;
    public Camera targetCamera;

    [Header("Flip")]
    public bool invertFlip = false;
    public float minVelocityToFlip = 0.01f;

    private Vector3 baseScale;

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (projectileRb == null)
            projectileRb = GetComponentInParent<Rigidbody>();

        baseScale = transform.localScale;
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        // 1. Sempre olhar para a câmera
        transform.forward = targetCamera.transform.forward;

        // 2. Flipar conforme a direção horizontal do projétil
        if (projectileRb == null) return;

        float horizontal = projectileRb.linearVelocity.x;

        if (Mathf.Abs(horizontal) < minVelocityToFlip)
            return;

        bool goingLeft = horizontal < 0f;

        float scaleX = Mathf.Abs(baseScale.x);

        if (goingLeft)
            scaleX = invertFlip ? scaleX : -scaleX;
        else
            scaleX = invertFlip ? -scaleX : scaleX;

        transform.localScale = new Vector3(scaleX, baseScale.y, baseScale.z);
    }
}