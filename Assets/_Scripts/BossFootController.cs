using UnityEngine;

[ExecuteAlways]
public class BossFootController : MonoBehaviour
{
    private enum BossState
    {
        IdleGround,
        RiseOffscreen,
        ShadowChase,
        ShadowLock,
        Stomp,
        Vulnerable
    }

    [Header("References")]
    [SerializeField] private Transform footVisual;
    [SerializeField] private SpriteRenderer footRenderer;
    [SerializeField] private BossShadowTelegraph shadowTelegraph;
    [SerializeField] private HealthSystem bossHealth;
    [SerializeField] private PlantGrowthController victoryPlant;

    [Header("Timing")]
    [SerializeField] private float idleDuration = 3.4f;
    [SerializeField] private float shadowChaseDuration = 3f;
    [SerializeField] private float shadowLockDuration = 0.45f;
    [SerializeField] private float vulnerableDuration = 1.8f;

    [Header("Stomp")]
    [SerializeField] private float stompDamage = 3f;
    [SerializeField] private float stompRadius = 1.2f;
    [SerializeField] private bool autoStompRadius = true;
    [SerializeField] private float stompRadiusPadding = 0.1f;
    [SerializeField] private float groundY = 0f;
    [SerializeField] private float groundYOffset = 0f;
    [SerializeField] private float shadowYOffset = 0.12f;
    [SerializeField] private float riseSpeed = 34f;
    [SerializeField] private float dropSpeed = 120f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundReference;
    [SerializeField] private float groundRaycastHeight = 60f;
    [SerializeField] private float groundRaycastDistance = 200f;
    [SerializeField] private float riseDistance = 65f;
    [SerializeField] private float footGroundContactOffset = -8f;
    [SerializeField] private float shadowMoveSpeed = 80f;

    [Header("Visual Scale")]
    [SerializeField] private float footHeightMultiplier = 2.6f;
    [SerializeField] private float shadowWidthMultiplier = 1.0f;
    [SerializeField] private float shadowStartScaleMultiplier = 1.05f;
    [SerializeField] private float shadowMinScaleMultiplier = 0.75f;
    [SerializeField] private float shadowMaxScaleMultiplier = 1.2f;

    [Header("Sorting")]
    [SerializeField] private int playerSortingOrderOverride = 30;
    [SerializeField] private int footSortingOffset = 10;
    [SerializeField] private int shadowSortingOffset = -1;

    [Header("Spawn")]
    [SerializeField] private Transform spawnAnchor;
    [SerializeField] private Vector3 spawnDirectionFromPlayer = new Vector3(-1f, 0f, -0.18f);
    [SerializeField] private float spawnSeparationPadding = 4f;
    [SerializeField] private Vector3 spawnOffsetFromPlayer = Vector3.zero;

    [Header("Physical Foot")]
    [SerializeField] private bool enablePhysicalFoot = true;
    [SerializeField] private bool autoConfigureFootCollider = true;
    [SerializeField] private BoxCollider footCollider;
    [SerializeField] private Rigidbody footBody;
    [SerializeField] private float footColliderWidthMultiplier = 0.72f;
    [SerializeField] private float footColliderHeightMultiplier = 0.26f;
    [SerializeField] private float footColliderDepthMultiplier = 0.34f;
    [SerializeField] private float footColliderVerticalOffset = 0f;

    [Header("Vulnerable Hurtbox")]
    [SerializeField] private float vulnerableColliderWidthMultiplier = 1.02f;
    [SerializeField] private float vulnerableColliderHeightMultiplier = 0.34f;
    [SerializeField] private float vulnerableColliderDepthMultiplier = 0.82f;

    [Header("Player Push")]
    [SerializeField] private Vector3 playerPushDirection = new Vector3(0f, 0f, -1f);
    [SerializeField] private float collisionPushDistance = 0.55f;
    [SerializeField] private float collisionPushSpeed = 18f;
    [SerializeField] private float stompPushDistance = 1.15f;
    [SerializeField] private float stompPushSpeed = 24f;

    [Header("Boss Health")]
    [SerializeField] private float bossMaxHealth = 20f;
    [SerializeField] private float bossInvulnerabilityTime = 0.08f;
    [SerializeField] private float bossBlinkInterval = 0.04f;

    [Header("Boss Health UI")]
    [SerializeField] private bool showBossHealthBar = true;
    [SerializeField] private string bossHealthBarTitle = "PE GIGANTE";
    [SerializeField] private Vector2 bossHealthBarSize = new Vector2(380f, 28f);
    [SerializeField] private float bossHealthBarTopMargin = 24f;
    [SerializeField] private float bossHealthBarSideMargin = 16f;
    [SerializeField] private Color bossHealthBarBackColor = new Color(0.08f, 0.04f, 0.04f, 0.82f);
    [SerializeField] private Color bossHealthBarFillColor = new Color(0.9f, 0.08f, 0.08f, 0.95f);
    [SerializeField] private Color bossHealthBarBorderColor = new Color(1f, 0.78f, 0.28f, 0.95f);
    [SerializeField] private Color bossHealthBarTextColor = Color.white;

    [Header("Editor Preview")]
    [SerializeField] private bool previewIdlePoseInEditor = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private BossState state = BossState.IdleGround;
    private float stateTimer;
    private Vector3 spawnXZ;
    private Vector3 shadowLock;
    private bool stompApplied;
    private Transform playerTransform;
    private SpriteRenderer playerRenderer;
    private int originalPlayerSortingOrder;
    private bool storedPlayerSortingOrder;
    private float currentGroundY;
    private float currentOffscreenY;
    private Vector3 currentGroundNormal = Vector3.up;
    private float footBaseOffset;
    private float shadowScaleStart;
    private float shadowScaleMin;
    private float shadowScaleMax;
    private bool visualsConfigured;
    private Collider groundCollider;
    private float offscreenTargetY;
    private Vector3 shadowCurrentPos;
    private bool bossDefeated;
    private int originalFootLayer;
    private int enemyLayerIndex = -1;
    private GUIStyle bossHealthBarLabelStyle;

    private void Awake()
    {
        EnsureReferences();
        originalFootLayer = gameObject.layer;
        enemyLayerIndex = LayerMask.NameToLayer("Enemy");

        if (playerLayer.value == 0)
            playerLayer = LayerMask.GetMask("Player");

    }

    private void OnEnable()
    {
        EnsureReferences();
        if (!Application.isPlaying)
            RefreshEditorPreview();
    }

    private void OnValidate()
    {
        EnsureReferences();
        if (!Application.isPlaying)
            RefreshEditorPreview();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying && previewIdlePoseInEditor)
            RefreshEditorPreview();
    }

    private void Start()
    {
        EnsureReferences();
        EnsurePhysicsComponents();
        EnsureBossHealth();
        EnsureVictoryPlant();
        FindPlayer();
        ConfigureVisualsFromPlayer();
        ResolveGroundReference();
        SetSpawnPoint();
        PlaceFootAtGround(spawnXZ);
        SetState(BossState.IdleGround);
    }

    private void OnDisable()
    {
        if (Application.isPlaying && playerRenderer != null && storedPlayerSortingOrder)
            playerRenderer.sortingOrder = originalPlayerSortingOrder;

        gameObject.layer = originalFootLayer;
    }

    private void EnsureReferences()
    {
        if (footVisual == null)
            footVisual = transform;

        if (footRenderer == null && footVisual != null)
            footRenderer = footVisual.GetComponent<SpriteRenderer>();

        if (shadowTelegraph == null)
            shadowTelegraph = GetComponentInChildren<BossShadowTelegraph>(true);
        if (shadowTelegraph == null)
        {
            GameObject shadowObj = GameObject.Find("BossShadow");
            if (shadowObj != null)
                shadowTelegraph = shadowObj.GetComponent<BossShadowTelegraph>();
        }

        if (bossHealth == null)
            bossHealth = GetComponent<HealthSystem>();
    }

    private void EnsureBossHealth()
    {
        if (!Application.isPlaying)
        {
            if (bossHealth == null)
                bossHealth = GetComponent<HealthSystem>();
            return;
        }

        if (bossHealth == null)
            bossHealth = GetComponent<HealthSystem>();
        if (bossHealth == null)
            bossHealth = gameObject.AddComponent<HealthSystem>();

        bossHealth.maxHealth = bossMaxHealth;
        bossHealth.disappearDelayAfterDeath = 0f;
        bossHealth.knockbackForce = 0f;
        bossHealth.knockbackDuration = 0f;
        bossHealth.invulnerabilityTime = bossInvulnerabilityTime;
        bossHealth.blinkInterval = bossBlinkInterval;
        bossHealth.ResetHealthToMax();
    }

    private void EnsureVictoryPlant()
    {
        if (victoryPlant != null)
            return;

        GameObject plant = GameObject.Find("Plant");
        if (plant != null)
            victoryPlant = plant.GetComponent<PlantGrowthController>();
    }

    private void EnsurePhysicsComponents()
    {
        if (!enablePhysicalFoot || !Application.isPlaying)
            return;

        if (footCollider == null)
            footCollider = GetComponent<BoxCollider>();
        if (footCollider == null)
            footCollider = gameObject.AddComponent<BoxCollider>();

        if (footBody == null)
            footBody = GetComponent<Rigidbody>();
        if (footBody == null)
            footBody = gameObject.AddComponent<Rigidbody>();

        footBody.useGravity = false;
        footBody.isKinematic = true;
        footBody.constraints = RigidbodyConstraints.FreezeRotation;
        footBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        footBody.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void ConfigureFootCollider()
    {
        if (!enablePhysicalFoot || !autoConfigureFootCollider || footRenderer == null || footRenderer.sprite == null)
            return;

        if (footCollider == null)
            footCollider = GetComponent<BoxCollider>();
        if (footCollider == null)
            return;

        Vector3 spriteSize = footRenderer.sprite.bounds.size;
        bool useVulnerableHurtbox = Application.isPlaying && state == BossState.Vulnerable;

        float widthMultiplier = useVulnerableHurtbox ? vulnerableColliderWidthMultiplier : footColliderWidthMultiplier;
        float heightMultiplier = useVulnerableHurtbox ? vulnerableColliderHeightMultiplier : footColliderHeightMultiplier;
        float depthMultiplier = useVulnerableHurtbox ? vulnerableColliderDepthMultiplier : footColliderDepthMultiplier;

        float colliderWidth = Mathf.Max(0.1f, spriteSize.x * widthMultiplier);
        float colliderHeight = Mathf.Max(0.1f, spriteSize.y * heightMultiplier);
        float colliderDepth = Mathf.Max(0.1f, spriteSize.x * depthMultiplier);
        float spriteBottom = -footRenderer.sprite.bounds.extents.y;

        footCollider.center = new Vector3(
            0f,
            spriteBottom + (colliderHeight * 0.5f) + footColliderVerticalOffset,
            0f);
        footCollider.size = new Vector3(colliderWidth, colliderHeight, colliderDepth);
        footCollider.isTrigger = false;
    }

    private void SetFootColliderEnabled(bool enabled)
    {
        if (!enablePhysicalFoot || footCollider == null)
            return;

        footCollider.enabled = enabled;
    }

    private bool IsGroundedFootState()
    {
        return state == BossState.IdleGround ||
               state == BossState.Stomp ||
               state == BossState.Vulnerable;
    }

    private Vector3 GetPushDirection()
    {
        Vector3 direction = playerPushDirection;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector3.back;

        return direction.normalized;
    }

    private void PushPlayerBody(Rigidbody playerBody, float distance, float speed)
    {
        if (playerBody == null)
            return;

        Vector3 pushDirection = GetPushDirection();
        Vector3 pushedPosition = playerBody.position + (pushDirection * distance);
        playerBody.position = pushedPosition;

        Vector3 velocity = playerBody.linearVelocity;
        playerBody.linearVelocity = new Vector3(velocity.x * 0.25f, velocity.y, pushDirection.z * speed);
    }

    private bool TryGetPlayerBody(Collider collider, out Rigidbody playerBody)
    {
        playerBody = null;

        if (collider == null)
            return false;

        Transform root = collider.transform.root;
        if (root == null || !root.CompareTag("Player"))
            return false;

        playerBody = root.GetComponent<Rigidbody>();
        return playerBody != null;
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            if (previewIdlePoseInEditor)
                RefreshEditorPreview();
            return;
        }

        if (bossDefeated)
            return;

        stateTimer += Time.deltaTime;

        switch (state)
        {
            case BossState.IdleGround:
                if (stateTimer >= idleDuration)
                    BeginRise();
                break;

            case BossState.RiseOffscreen:
                UpdateRise();
                break;

            case BossState.ShadowChase:
                UpdateShadowChase();
                break;

            case BossState.ShadowLock:
                UpdateShadowLock();
                break;

            case BossState.Stomp:
                UpdateStompDrop();
                break;

            case BossState.Vulnerable:
                if (stateTimer >= vulnerableDuration)
                    SetState(BossState.IdleGround);
                break;
        }
    }

    private void OnGUI()
    {
        if (!Application.isPlaying || !showBossHealthBar || bossDefeated || bossHealth == null)
            return;

        float maxHealth = Mathf.Max(0.01f, bossHealth.maxHealth);
        float healthPercent = Mathf.Clamp01(bossHealth.CurrentHealth / maxHealth);
        float maxWidth = Mathf.Max(1f, Screen.width - (bossHealthBarSideMargin * 2f));
        float barWidth = Mathf.Min(bossHealthBarSize.x, maxWidth);
        float barHeight = Mathf.Max(8f, bossHealthBarSize.y);
        Rect barRect = new Rect(
            (Screen.width - barWidth) * 0.5f,
            bossHealthBarTopMargin,
            barWidth,
            barHeight);

        DrawSolidRect(new Rect(barRect.x - 3f, barRect.y - 3f, barRect.width + 6f, barRect.height + 6f), bossHealthBarBorderColor);
        DrawSolidRect(barRect, bossHealthBarBackColor);

        if (healthPercent > 0f)
        {
            Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * healthPercent, barRect.height);
            DrawSolidRect(fillRect, bossHealthBarFillColor);
        }

        DrawBorder(barRect, 2f, Color.black);

        string label = $"{bossHealthBarTitle}  {Mathf.CeilToInt(bossHealth.CurrentHealth)}/{Mathf.CeilToInt(maxHealth)}";
        GUI.Label(barRect, label, GetBossHealthBarLabelStyle());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!Application.isPlaying || !IsGroundedFootState())
            return;

        if (TryGetPlayerBody(collision.collider, out Rigidbody playerBody))
            PushPlayerBody(playerBody, collisionPushDistance, collisionPushSpeed);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!Application.isPlaying || !IsGroundedFootState())
            return;

        if (TryGetPlayerBody(collision.collider, out Rigidbody playerBody))
            PushPlayerBody(playerBody, collisionPushDistance * 0.18f, collisionPushSpeed);
    }

    private void BeginRise()
    {
        FindPlayer();
        ConfigureVisualsFromPlayer();
        UpdateGroundYFromGround();
        shadowCurrentPos = new Vector3(spawnXZ.x, currentGroundY, spawnXZ.z);
        offscreenTargetY = GetGroundedFootCenterY() + riseDistance;

        ShowFootAtGround(spawnXZ);
        ShowShadowAtGround(spawnXZ);
        ApplyShadowStartScale();
        if (debugLogs)
            Debug.Log($"BossFoot BeginRise: spawnXZ={spawnXZ} groundY={currentGroundY:F2} groundedFootY={GetGroundedFootCenterY():F2} offscreenTargetY={offscreenTargetY:F2}");
        SetState(BossState.RiseOffscreen);
    }

    private void UpdateRise()
    {
        Vector3 footPos = footVisual.position;
        footPos.y += riseSpeed * Time.deltaTime;
        ShowFootAtWorld(footPos);

        if (shadowTelegraph != null)
        {
            shadowTelegraph.SetGroundNormal(currentGroundNormal);
            shadowTelegraph.SetPosition(shadowCurrentPos);
            shadowTelegraph.SetScale(ScaleVector(shadowScaleMin > 0f ? shadowScaleMin : 0.08f));
        }

        if (footVisual.position.y >= offscreenTargetY)
        {
            currentOffscreenY = footVisual.position.y;
            if (debugLogs)
                Debug.Log($"BossFoot Offscreen at y={currentOffscreenY:F2}");
            SetState(BossState.ShadowChase);
        }
    }

    private void UpdateShadowChase()
    {
        FindPlayer();
        UpdateGroundYFromGround();

        Vector3 target = playerTransform != null ? playerTransform.position : spawnXZ;
        Vector3 desiredPos = new Vector3(target.x, currentGroundY, target.z);
        shadowCurrentPos = Vector3.MoveTowards(shadowCurrentPos, desiredPos, shadowMoveSpeed * Time.deltaTime);
        ShowShadowAtGround(shadowCurrentPos);
        ShowFootAtWorld(new Vector3(shadowCurrentPos.x, currentOffscreenY, shadowCurrentPos.z));

        if (shadowTelegraph != null)
        {
            float t = Mathf.Clamp01(stateTimer / Mathf.Max(0.01f, shadowChaseDuration));
            float minScale = shadowScaleMin > 0f ? shadowScaleMin : 0.08f;
            float maxScale = shadowScaleMax > 0f ? shadowScaleMax : 0.14f;
            shadowTelegraph.SetGroundNormal(currentGroundNormal);
            shadowTelegraph.SetScale(Vector3.Lerp(ScaleVector(minScale), ScaleVector(maxScale * 0.85f), t));
        }

        if (stateTimer >= shadowChaseDuration)
        {
            shadowLock = shadowCurrentPos;
            SetState(BossState.ShadowLock);
        }
    }

    private void UpdateShadowLock()
    {
        UpdateGroundYFromGround();
        shadowLock.y = currentGroundY;
        shadowCurrentPos = shadowLock;
        ShowFootAtWorld(new Vector3(shadowCurrentPos.x, currentOffscreenY, shadowCurrentPos.z));

        if (shadowTelegraph != null)
        {
            float t = Mathf.Clamp01(stateTimer / Mathf.Max(0.01f, shadowLockDuration));
            float minScale = shadowScaleMin > 0f ? shadowScaleMin : 0.08f;
            float maxScale = shadowScaleMax > 0f ? shadowScaleMax : 0.14f;
            shadowTelegraph.SetGroundNormal(currentGroundNormal);
            shadowTelegraph.SetPosition(shadowCurrentPos);
            shadowTelegraph.SetScale(Vector3.Lerp(ScaleVector(minScale), ScaleVector(maxScale), t));
        }

        if (stateTimer >= shadowLockDuration)
        {
            if (debugLogs)
                Debug.Log($"BossFoot Lock -> Drop: lock={shadowLock} offscreenY={currentOffscreenY:F2}");
            SetState(BossState.Stomp);
        }
    }

    private void UpdateStompDrop()
    {
        Vector3 targetGround = new Vector3(shadowLock.x, currentGroundY, shadowLock.z);
        Vector3 footPos = footVisual.position;
        footPos = Vector3.MoveTowards(footPos, new Vector3(targetGround.x, GetGroundedFootCenterY(), targetGround.z), dropSpeed * Time.deltaTime);
        ShowFootAtWorld(footPos);
        ShowShadowAtGround(targetGround);

        if (!stompApplied && Mathf.Abs(footPos.y - GetGroundedFootCenterY()) <= 0.01f)
        {
            stompApplied = true;
            spawnXZ = new Vector3(targetGround.x, 0f, targetGround.z);
            ApplyStompDamage(targetGround);
            SetState(BossState.Vulnerable);
        }
    }

    private void ApplyStompDamage(Vector3 targetGround)
    {
        float hitY = playerTransform != null ? playerTransform.position.y : targetGround.y;
        Vector3 hitCenter = new Vector3(targetGround.x, hitY, targetGround.z);
        Collider[] hits = Physics.OverlapSphere(hitCenter, stompRadius, playerLayer);
        if (debugLogs)
            Debug.Log($"BossFoot Stomp: target={hitCenter} radius={stompRadius:F2} hits={hits.Length}");
        foreach (Collider hit in hits)
        {
            HealthSystem health = hit.GetComponentInParent<HealthSystem>();
            if (health != null)
            {
                if (debugLogs)
                    Debug.Log($"BossFoot Hit: {hit.name}");
                health.TakeDamage(stompDamage);
            }

            Rigidbody playerBody = hit.GetComponentInParent<Rigidbody>();
            if (playerBody != null && hit.transform.root.CompareTag("Player"))
            {
                PushPlayerBody(playerBody, stompPushDistance, stompPushSpeed);
            }
        }
    }

    private void HideFoot()
    {
        if (footRenderer != null)
            footRenderer.enabled = false;
    }

    private void PlaceFootAtGround(Vector3 targetXZ)
    {
        ConfigureVisualsFromPlayer();
        UpdateGroundYFromGround();
        ShowFootAtWorld(new Vector3(targetXZ.x, GetGroundedFootCenterY(), targetXZ.z));
    }

    private void SetState(BossState newState)
    {
        state = newState;
        stateTimer = 0f;
        if (debugLogs)
            Debug.Log($"BossFoot state -> {state}");

        bool colliderActive =
            state == BossState.IdleGround ||
            state == BossState.Stomp ||
            state == BossState.Vulnerable;
        ConfigureFootCollider();
        SetFootColliderEnabled(colliderActive);
        SetDamageableLayer(!bossDefeated && state == BossState.Vulnerable);

        switch (state)
        {
            case BossState.IdleGround:
                stompApplied = false;
                PlaceFootAtGround(spawnXZ);
                if (shadowTelegraph != null)
                {
                    ShowShadowAtGround(new Vector3(spawnXZ.x, currentGroundY, spawnXZ.z));
                    ApplyShadowStartScale();
                }
                break;
            case BossState.RiseOffscreen:
                stompApplied = false;
                ShowShadowAtGround(spawnXZ);
                ApplyShadowStartScale();
                break;
            case BossState.ShadowChase:
                stompApplied = false;
                ShowShadowAtGround(spawnXZ);
                break;
            case BossState.ShadowLock:
                stompApplied = false;
                break;
            case BossState.Stomp:
                stompApplied = false;
                break;
            case BossState.Vulnerable:
                stompApplied = true;
                break;
        }
    }

    private void SetDamageableLayer(bool damageable)
    {
        if (enemyLayerIndex < 0)
            return;

        gameObject.layer = damageable ? enemyLayerIndex : originalFootLayer;
    }

    public void OnBossDamaged(float damageAmount, float currentHealth, float maxHealth)
    {
        Debug.Log($"BossFoot tomou {damageAmount} de dano. Vida do boss: {currentHealth}/{maxHealth}");
    }

    public void OnBossDeath()
    {
        if (bossDefeated)
            return;

        bossDefeated = true;
        SetDamageableLayer(false);
        SetFootColliderEnabled(false);

        if (footBody != null)
            footBody.linearVelocity = Vector3.zero;

        if (shadowTelegraph != null)
            shadowTelegraph.Hide();

        if (footRenderer != null)
            footRenderer.enabled = false;

        Debug.Log("BossFoot derrotado.");

        if (victoryPlant != null)
            victoryPlant.GrowPlant();

        gameObject.SetActive(false);
    }

    private void FindPlayer()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                player = GameObject.Find("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerRenderer = player.GetComponentInChildren<SpriteRenderer>();
            }
        }

        if (playerRenderer == null && playerTransform != null)
            playerRenderer = playerTransform.GetComponentInChildren<SpriteRenderer>();
    }

    private void ConfigureVisualsFromPlayer()
    {
        if (visualsConfigured || playerRenderer == null)
            return;

        if (footRenderer != null)
            footRenderer.enabled = true;

        float playerHeight = Mathf.Max(0.01f, playerRenderer.bounds.size.y);
        float playerWidth = Mathf.Max(0.01f, playerRenderer.bounds.size.x);

        ApplySortingFromPlayer();
        ApplyFootScale(playerHeight);
        ConfigureFootCollider();
        ApplyShadowScale(playerWidth);
        ApplyShadowYOffset();

        visualsConfigured = true;
    }

    private void ApplySortingFromPlayer()
    {
        if (playerRenderer == null)
            return;

        int playerBaseOrder = playerRenderer.sortingOrder;
        if (Application.isPlaying && playerSortingOrderOverride >= 0)
        {
            if (!storedPlayerSortingOrder)
            {
                originalPlayerSortingOrder = playerRenderer.sortingOrder;
                storedPlayerSortingOrder = true;
            }

            playerRenderer.sortingOrder = playerSortingOrderOverride;
            playerBaseOrder = playerSortingOrderOverride;
        }

        if (footRenderer != null)
        {
            footRenderer.sortingLayerID = playerRenderer.sortingLayerID;
            footRenderer.sortingOrder = playerBaseOrder + footSortingOffset;
        }

        if (shadowTelegraph != null && shadowTelegraph.SpriteRenderer != null)
        {
            shadowTelegraph.SpriteRenderer.sortingLayerID = playerRenderer.sortingLayerID;
            shadowTelegraph.SpriteRenderer.sortingOrder = playerBaseOrder + shadowSortingOffset;
        }

        if (debugLogs)
        {
            int footOrder = footRenderer != null ? footRenderer.sortingOrder : -999;
            int shadowOrder = shadowTelegraph != null && shadowTelegraph.SpriteRenderer != null ? shadowTelegraph.SpriteRenderer.sortingOrder : -999;
            Debug.Log($"BossFoot sorting stack -> player:{playerRenderer.sortingOrder} shadow:{shadowOrder} foot:{footOrder}");
        }
    }

    private void ApplyFootScale(float playerHeight)
    {
        if (footRenderer == null || footRenderer.sprite == null)
            return;

        float spriteHeight = Mathf.Max(0.01f, footRenderer.sprite.bounds.size.y);
        float desiredHeight = playerHeight * footHeightMultiplier;
        float scale = desiredHeight / spriteHeight;
        footVisual.localScale = new Vector3(scale, scale, scale);
        footBaseOffset = footRenderer.bounds.extents.y;
    }

    private void ApplyShadowScale(float playerWidth)
    {
        if (shadowTelegraph == null || shadowTelegraph.SpriteRenderer == null || shadowTelegraph.SpriteRenderer.sprite == null)
            return;

        float spriteWidth = Mathf.Max(0.01f, shadowTelegraph.SpriteRenderer.sprite.bounds.size.x);
        float desiredWidth = playerWidth * shadowWidthMultiplier;
        float baseScale = desiredWidth / spriteWidth;

        shadowScaleStart = baseScale * shadowStartScaleMultiplier;
        shadowScaleMin = baseScale * shadowMinScaleMultiplier;
        shadowScaleMax = baseScale * shadowMaxScaleMultiplier;

        if (autoStompRadius)
            stompRadius = (desiredWidth * 0.5f) + stompRadiusPadding;
    }

    private void ApplyShadowStartScale()
    {
        if (shadowTelegraph == null)
            return;

        if (shadowScaleStart <= 0f)
            shadowScaleStart = 0.14f;

        shadowTelegraph.SetScale(ScaleVector(shadowScaleStart));
    }

    private void ApplyShadowYOffset()
    {
        if (shadowTelegraph != null)
            shadowTelegraph.SetYOffset(shadowYOffset);
    }

    private void ResolveGroundReference()
    {
        if (groundReference != null)
        {
            groundCollider = groundReference.GetComponent<Collider>();
            return;
        }

        GameObject groundObj = GameObject.Find("meshNuvem");
        if (groundObj != null)
        {
            groundReference = groundObj.transform;
            groundCollider = groundObj.GetComponent<Collider>();
            if (debugLogs)
                Debug.Log($"BossFoot ground reference: meshNuvem collider={(groundCollider != null ? groundCollider.GetType().Name : "none")}");
            return;
        }
    }

    private void UpdateGroundYFromGround()
    {
        if (groundReference == null)
        {
            currentGroundY = playerTransform != null ? playerTransform.position.y + groundYOffset : groundY;
            currentGroundNormal = Vector3.up;
            return;
        }

        float fallbackY = groundReference.position.y + groundYOffset;
        Vector3 origin = playerTransform != null ? playerTransform.position : groundReference.position;
        origin.y += groundRaycastHeight;

        Ray downRay = new Ray(origin, Vector3.down);

        if (groundCollider != null && groundCollider.Raycast(downRay, out RaycastHit groundHit, groundRaycastDistance))
        {
            currentGroundY = groundHit.point.y + groundYOffset;
            currentGroundNormal = groundHit.normal.sqrMagnitude > 0.001f ? groundHit.normal.normalized : Vector3.up;
            if (debugLogs)
                Debug.Log($"BossFoot ground collider hit: {groundHit.collider.name} y={currentGroundY:F2}");
            return;
        }

        int mask = groundLayer.value == 0 ? ~playerLayer.value : groundLayer.value;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundRaycastDistance, mask, QueryTriggerInteraction.Ignore))
        {
            currentGroundY = hit.point.y + groundYOffset;
            currentGroundNormal = hit.normal.sqrMagnitude > 0.001f ? hit.normal.normalized : Vector3.up;
            if (debugLogs)
                Debug.Log($"BossFoot fallback ground ray hit: {hit.collider.name} y={currentGroundY:F2}");
        }
        else
        {
            currentGroundY = fallbackY;
            currentGroundNormal = groundReference.up.sqrMagnitude > 0.001f ? groundReference.up.normalized : Vector3.up;
            if (debugLogs)
                Debug.Log($"BossFoot ground ray miss. Using fallback y={currentGroundY:F2}");
        }
    }

    private void SetSpawnPoint()
    {
        if (spawnAnchor == null)
        {
            GameObject anchorObj = GameObject.Find("BossSpawn");
            if (anchorObj != null)
                spawnAnchor = anchorObj.transform;
        }

        if (spawnAnchor != null)
        {
            spawnXZ = new Vector3(spawnAnchor.position.x, 0f, spawnAnchor.position.z);
            return;
        }

        if (playerTransform != null)
        {
            Vector3 direction = spawnDirectionFromPlayer;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f)
                direction = Vector3.left;
            direction.Normalize();

            float separation = spawnSeparationPadding;
            if (playerRenderer != null)
                separation += playerRenderer.bounds.extents.x;
            if (footRenderer != null)
                separation += footRenderer.bounds.extents.x;

            Vector3 basePos = playerTransform.position + (direction * separation) + spawnOffsetFromPlayer;
            spawnXZ = new Vector3(basePos.x, 0f, basePos.z);
            if (debugLogs)
                Debug.Log($"BossFoot spawn from player snapshot: {spawnXZ}");
            return;
        }

        if (groundReference != null)
        {
            Vector3 basePos = groundReference.position + spawnOffsetFromPlayer;
            spawnXZ = new Vector3(basePos.x, 0f, basePos.z);
            if (debugLogs)
                Debug.Log($"BossFoot spawn from ground fallback: {spawnXZ}");
            return;
        }
        else
        {
            spawnXZ = new Vector3(transform.position.x, 0f, transform.position.z);
        }
    }

    private void ShowFootAtWorld(Vector3 worldPosition)
    {
        if (footRenderer != null)
            footRenderer.enabled = true;

        if (Application.isPlaying && enablePhysicalFoot && footBody != null)
            footBody.position = worldPosition;

        footVisual.position = worldPosition;
    }

    private void ShowFootAtGround(Vector3 targetXZ)
    {
        ShowFootAtWorld(new Vector3(targetXZ.x, GetGroundedFootCenterY(), targetXZ.z));
    }

    private void ShowShadowAtGround(Vector3 targetXZ)
    {
        if (shadowTelegraph == null)
            return;

        shadowTelegraph.SetGroundNormal(currentGroundNormal);
        shadowTelegraph.ShowAt(new Vector3(targetXZ.x, currentGroundY, targetXZ.z));
    }

    private float GetGroundedFootCenterY()
    {
        return currentGroundY + footBaseOffset + footGroundContactOffset;
    }

    private void RefreshEditorPreview()
    {
        if (!previewIdlePoseInEditor)
            return;

        EnsureReferences();
        visualsConfigured = false;
        FindPlayer();
        ResolveGroundReference();
        ConfigureVisualsFromPlayer();
        SetSpawnPoint();
        UpdateGroundYFromGround();

        shadowCurrentPos = new Vector3(spawnXZ.x, currentGroundY, spawnXZ.z);
        PlaceFootAtGround(spawnXZ);
        ShowShadowAtGround(shadowCurrentPos);
        ApplyShadowStartScale();
        if (footRenderer != null)
            footRenderer.enabled = true;
    }

    private static Vector3 ScaleVector(float scale)
    {
        return new Vector3(scale, scale, scale);
    }

    private GUIStyle GetBossHealthBarLabelStyle()
    {
        if (bossHealthBarLabelStyle == null)
        {
            bossHealthBarLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
        }

        bossHealthBarLabelStyle.normal.textColor = bossHealthBarTextColor;
        bossHealthBarLabelStyle.fontSize = Mathf.Clamp(Mathf.RoundToInt(bossHealthBarSize.y * 0.55f), 10, 20);
        return bossHealthBarLabelStyle;
    }

    private static void DrawSolidRect(Rect rect, Color color)
    {
        Color previousColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private static void DrawBorder(Rect rect, float thickness, Color color)
    {
        DrawSolidRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        DrawSolidRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
        DrawSolidRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        DrawSolidRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
    }
}
