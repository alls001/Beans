using System.Collections.Generic;
using UnityEngine;

public class QuadSpriteAnimator : MonoBehaviour
{
    [System.Serializable]
    public class AnimationClip2D
    {
        public string clipName;
        public List<Sprite> frames;
        public float frameRate = 12f;
        public bool loop = true;
    }

    [Header("Renderer")]
    public MeshRenderer targetRenderer;

    [Header("Visual Root")]
    public Transform visualRoot;

    [Header("Animations")]
    public List<AnimationClip2D> animations;

    [Header("Playback")]
    public bool useUnscaledTime = true;
    public bool smoothLocalOffset = true;
    public float offsetSmoothSpeed = 18f;

    private Dictionary<string, AnimationClip2D> animationDict;
    private AnimationClip2D currentClip;
    private int currentFrame;
    private float timer;
    private Material runtimeMaterial;

    private string currentClipName = "";
    private Sprite lastAppliedSprite;

    private Vector3 baseLocalPosition;
    private Vector3 targetLocalPosition;

    void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<MeshRenderer>();

        if (targetRenderer == null)
        {
            Debug.LogError("QuadSpriteAnimator: MeshRenderer não encontrado.");
            enabled = false;
            return;
        }

        if (visualRoot == null)
            visualRoot = targetRenderer.transform;

        runtimeMaterial = targetRenderer.material;

        animationDict = new Dictionary<string, AnimationClip2D>();

        foreach (var anim in animations)
        {
            if (anim == null) continue;
            if (string.IsNullOrEmpty(anim.clipName)) continue;
            if (anim.frames == null || anim.frames.Count == 0) continue;

            if (!animationDict.ContainsKey(anim.clipName))
                animationDict.Add(anim.clipName, anim);
            else
                Debug.LogWarning("QuadSpriteAnimator: nome de animação duplicado: " + anim.clipName);
        }

        baseLocalPosition = visualRoot.localPosition;
        targetLocalPosition = baseLocalPosition;
    }

    void Update()
    {
        UpdateAnimation();
        UpdatePositionSmoothing();
    }

    void UpdateAnimation()
    {
        if (currentClip == null || currentClip.frames == null || currentClip.frames.Count == 0)
            return;

        if (currentClip.frames.Count == 1)
        {
            if (lastAppliedSprite != currentClip.frames[0])
                ApplyFrame(currentClip.frames[0]);

            return;
        }

        float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (delta <= 0f) return;

        float safeFrameRate = Mathf.Max(0.01f, currentClip.frameRate);
        float frameTime = 1f / safeFrameRate;

        timer += delta;

        while (timer >= frameTime)
        {
            timer -= frameTime;

            int nextFrame = currentFrame + 1;

            if (nextFrame >= currentClip.frames.Count)
            {
                if (currentClip.loop)
                    nextFrame = 0;
                else
                    nextFrame = currentClip.frames.Count - 1;
            }

            if (nextFrame == currentFrame && !currentClip.loop)
                break;

            currentFrame = nextFrame;
            ApplyFrame(currentClip.frames[currentFrame]);
        }
    }

    void UpdatePositionSmoothing()
    {
        if (visualRoot == null) return;

        if (!smoothLocalOffset)
        {
            visualRoot.localPosition = targetLocalPosition;
            return;
        }

        float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float t = 1f - Mathf.Exp(-offsetSmoothSpeed * delta);
        visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, targetLocalPosition, t);
    }

    public void Play(string clipName, bool restartIfSame = false)
    {
        if (string.IsNullOrEmpty(clipName))
            return;

        if (!animationDict.TryGetValue(clipName, out AnimationClip2D nextClip))
        {
            Debug.LogWarning("QuadSpriteAnimator: animação não encontrada: " + clipName);
            return;
        }

        if (currentClip == nextClip && !restartIfSame)
            return;

        currentClip = nextClip;
        currentClipName = clipName;
        currentFrame = 0;
        timer = 0f;
        lastAppliedSprite = null;

        ApplyFrame(currentClip.frames[currentFrame]);
    }

    public string GetCurrentClipName()
    {
        return currentClipName;
    }

    void ApplyFrame(Sprite sprite)
    {
        if (sprite == null || runtimeMaterial == null || visualRoot == null)
            return;

        if (lastAppliedSprite == sprite)
            return;

        if (runtimeMaterial.HasProperty("_BaseMap"))
            runtimeMaterial.SetTexture("_BaseMap", sprite.texture);

        if (runtimeMaterial.HasProperty("_MainTex"))
            runtimeMaterial.SetTexture("_MainTex", sprite.texture);

        // NÃO altera escala
        // Só ajusta o visual local, nunca o objeto com Rigidbody

        Rect rect = sprite.rect;
        Vector2 spritePivot = sprite.pivot;
        Vector2 size = sprite.bounds.size;

        Vector2 pivotNormalized = new Vector2(spritePivot.x / rect.width, spritePivot.y / rect.height);

        float offsetX = (0.5f - pivotNormalized.x) * size.x;
        float offsetY = (0.5f - pivotNormalized.y) * size.y;

        targetLocalPosition = new Vector3(
            baseLocalPosition.x + offsetX,
            baseLocalPosition.y + offsetY,
            baseLocalPosition.z
        );

        lastAppliedSprite = sprite;
    }
}