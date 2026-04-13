using UnityEngine;

public class BossShadowTelegraph : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Vector3 minScale = new Vector3(0.2f, 0.2f, 0.2f);
    [SerializeField] private Vector3 maxScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private float yOffset = 0.18f;
    [SerializeField] private bool detachFromParentOnAwake = false;
    [SerializeField] private Color visibleColor = new Color(0f, 0f, 0f, 0.82f);
    [SerializeField] private Color hiddenColor = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private bool alignToGroundNormal = true;

    private Vector3 groundNormal = Vector3.up;

    public SpriteRenderer SpriteRenderer => spriteRenderer;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (detachFromParentOnAwake && transform.parent != null)
            transform.SetParent(null, true);

        Hide();
    }

    public void ShowAt(Vector3 worldPosition)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = visibleColor;
        }

        ApplyGroundAlignment();
        transform.position = GetOffsetPosition(worldPosition);
        transform.localScale = minScale;
    }

    public void SetProgress(float t)
    {
        float clamped = Mathf.Clamp01(t);
        transform.localScale = Vector3.Lerp(minScale, maxScale, clamped);
    }

    public void SetScale(Vector3 scale)
    {
        transform.localScale = scale;
    }

    public void SetPosition(Vector3 worldPosition)
    {
        ApplyGroundAlignment();
        transform.position = GetOffsetPosition(worldPosition);
    }

    public void SetYOffset(float newYOffset)
    {
        yOffset = newYOffset;
    }

    public void SetGroundNormal(Vector3 normal)
    {
        if (normal.sqrMagnitude < 0.001f)
            groundNormal = Vector3.up;
        else
            groundNormal = normal.normalized;

        ApplyGroundAlignment();
    }

    public void Hide()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hiddenColor;
            spriteRenderer.enabled = false;
        }
    }

    private void ApplyGroundAlignment()
    {
        if (!alignToGroundNormal)
            return;

        Vector3 facingNormal = Vector3.Dot(groundNormal, Vector3.up) < 0f ? -groundNormal : groundNormal;
        transform.rotation = Quaternion.FromToRotation(Vector3.forward, facingNormal);
    }

    private Vector3 GetOffsetPosition(Vector3 worldPosition)
    {
        Vector3 offsetNormal = groundNormal.sqrMagnitude < 0.001f ? Vector3.up : groundNormal.normalized;
        return worldPosition + (offsetNormal * yOffset);
    }
}
