using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SpriteRenderer))]
public class FogOfWarController : MonoBehaviour {

    // Constants 
    private static readonly int ExploredTextureId = Shader.PropertyToID("_ExploredTex");
    private static readonly int PlayerPositionId = Shader.PropertyToID("_PlayerPosition");
    private static readonly int WorldMinId = Shader.PropertyToID("_WorldMin");
    private static readonly int WorldSizeId = Shader.PropertyToID("_WorldSize");
    private static readonly int VisionRadiusId = Shader.PropertyToID("_VisionRadius");

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private BoxCollider2D mapBounds;

    [Header("Vision")]
    [Min(0.1f)]
    [SerializeField] private float visionRadius = 6f;

    [Header("Explored texture")]
    [Min(32)]
    [SerializeField] private int textureWidth = 512;

    [Min(32)]
    [SerializeField] private int textureHeight = 512;

    [Header("Exploration drawing")]
    [SerializeField, Min(0.01f)]
    private float revealStampSpacing = 0.1f;

    [SerializeField, Range(0.01f, 1f)]
    private float revealEdgeSoftness = 0.2f;

    private SpriteRenderer fogRenderer;
    private Material fogMaterial;

    private Texture2D exploredTexture;
    private Color32[] exploredPixels;

    private Bounds worldBounds;
    private Vector2 lastRevealPosition;
    private bool hasRevealedInitialPosition;

    private void Awake() {
        fogRenderer = GetComponent<SpriteRenderer>();
        fogMaterial = fogRenderer.material;
        InitializeFog();
    }

    private void Update() {
        if (player == null)
            return;

        UpdateCurrentVision();
        RevealMovement(lastRevealPosition, player.position);

        lastRevealPosition = player.position;
    }

    private void OnDestroy() {
        if (exploredTexture != null) {
            Destroy(exploredTexture);
        }

        if (fogMaterial != null) {
            Destroy(fogMaterial);
        }
    }

    private void InitializeFog() {
        if (player == null) {
            Debug.LogError("Player is not assigned", this);
            enabled = false;
            return;
        }

        if (mapBounds == null) {
            Debug.LogError("Map bounds is not assigned", this);
            enabled = false;
            return;
        }

        worldBounds = mapBounds.bounds;

        //CalculateTextureSize();
        CreateExploredTexture();
        ConfigureFogRenderer();
        ConfigureFogMaterial();

        lastRevealPosition = player.position;

        RevealInitialArea();
        UpdateCurrentVision();
    }

    private void RevealInitialArea() {
        bool changed = DrawSoftCircle(
            player.position,
            visionRadius
        );

        if (!changed)
            return;

        exploredTexture.SetPixels32(exploredPixels);

        exploredTexture.Apply(
            updateMipmaps: false,
            makeNoLongerReadable: false
        );
    }

    private void CreateExploredTexture() {
        exploredTexture = new Texture2D(
            textureWidth,
            textureHeight,
            TextureFormat.RGBA32,
            mipChain: false,
            linear: true
        );

        exploredTexture.name = "Runtime Explored Mask";

        exploredTexture.filterMode = FilterMode.Bilinear;

        exploredTexture.wrapMode = TextureWrapMode.Clamp;

        exploredPixels = new Color32[textureWidth * textureHeight];

        Color32 unexploredColor = new Color32(0, 0, 0, 255);

        for (int i = 0; i < exploredPixels.Length; i++) {
            exploredPixels[i] = unexploredColor;
        }

        exploredTexture.SetPixels32(exploredPixels);
        exploredTexture.Apply(
            updateMipmaps: false,
            makeNoLongerReadable: false
        );
    }

    private void ConfigureFogRenderer() {
        Vector3 boundsCenter = worldBounds.center;
        Vector3 boundsSize = worldBounds.size;

        transform.position = new Vector3(boundsCenter.x, boundsCenter.y, transform.position.z);
        transform.localScale = new Vector3(boundsSize.x, boundsSize.y, 1f);
    }

    private void ConfigureFogMaterial() {
        fogMaterial.SetTexture(ExploredTextureId, exploredTexture);
        fogMaterial.SetVector(WorldMinId, worldBounds.min);
        fogMaterial.SetVector(WorldSizeId, worldBounds.size);
        fogMaterial.SetFloat(VisionRadiusId, visionRadius);
    }

    private void RevealMovement(
        Vector2 fromWorldPosition,
        Vector2 toWorldPosition
    ) {
        float distance = Vector2.Distance(
            fromWorldPosition,
            toWorldPosition
        );

        if (distance <= Mathf.Epsilon)
            return;

        int stampCount = Mathf.Max(
            1,
            Mathf.CeilToInt(distance / revealStampSpacing)
        );

        bool textureChanged = false;

        for (int i = 1; i <= stampCount; i++) {
            float t = i / (float)stampCount;

            Vector2 stampPosition = Vector2.Lerp(
                fromWorldPosition,
                toWorldPosition,
                t
            );

            textureChanged |= DrawSoftCircle(
                stampPosition,
                visionRadius
            );
        }

        if (!textureChanged)
            return;

        exploredTexture.SetPixels32(exploredPixels);

        exploredTexture.Apply(
            updateMipmaps: false,
            makeNoLongerReadable: false
        );
    }

    private void UpdateCurrentVision() {
        Vector3 playerPosition = player.position;

        fogMaterial.SetVector(PlayerPositionId, playerPosition);
        fogMaterial.SetFloat(VisionRadiusId, visionRadius);
    }

    private bool DrawSoftCircle(
        Vector2 worldPosition,
        float worldRadius
    ) {
        Vector2 normalizedPosition =
            WorldToNormalizedPosition(worldPosition);

        int centerX = Mathf.RoundToInt(
            normalizedPosition.x * (textureWidth - 1)
        );

        int centerY = Mathf.RoundToInt(
            normalizedPosition.y * (textureHeight - 1)
        );

        float pixelsPerWorldUnitX =
            textureWidth / worldBounds.size.x;

        float pixelsPerWorldUnitY =
            textureHeight / worldBounds.size.y;

        int radiusX = Mathf.Max(
            1,
            Mathf.CeilToInt(worldRadius * pixelsPerWorldUnitX)
        );

        int radiusY = Mathf.Max(
            1,
            Mathf.CeilToInt(worldRadius * pixelsPerWorldUnitY)
        );

        int minX = Mathf.Max(0, centerX - radiusX);
        int maxX = Mathf.Min(textureWidth - 1, centerX + radiusX);

        int minY = Mathf.Max(0, centerY - radiusY);
        int maxY = Mathf.Min(textureHeight - 1, centerY + radiusY);

        float hardEdge = Mathf.Clamp01(
            1f - revealEdgeSoftness
        );

        bool changed = false;

        for (int y = minY; y <= maxY; y++) {
            for (int x = minX; x <= maxX; x++) {
                float normalizedX =
                    (x - centerX) / (float)radiusX;

                float normalizedY =
                    (y - centerY) / (float)radiusY;

                float normalizedDistance = Mathf.Sqrt(
                    normalizedX * normalizedX +
                    normalizedY * normalizedY
                );

                if (normalizedDistance > 1f)
                    continue;

                /*
                 * Внутри основного круга revealAmount = 1.
                 * На внешнем краю постепенно становится 0.
                 */
                float revealAmount = 1f - Mathf.SmoothStep(
                    hardEdge,
                    1f,
                    normalizedDistance
                );

                byte revealByte = (byte)Mathf.RoundToInt(
                    revealAmount * 255f
                );

                int pixelIndex = y * textureWidth + x;

                byte currentValue =
                    exploredPixels[pixelIndex].r;

                /*
                 * Исследованность может только увеличиваться.
                 * Уже открытая область никогда не закрывается.
                 */
                if (revealByte <= currentValue)
                    continue;

                exploredPixels[pixelIndex] = new Color32(
                    revealByte,
                    revealByte,
                    revealByte,
                    255
                );

                changed = true;
            }
        }

        return changed;
    }

    private Vector2 WorldToNormalizedPosition(Vector2 worldPosition) {
        float normalizedX = Mathf.InverseLerp(
            worldBounds.min.x,
            worldBounds.max.x,
            worldPosition.x
        );

        float normalizedY = Mathf.InverseLerp(
            worldBounds.min.y,
            worldBounds.max.y,
            worldPosition.y
        );

        return new Vector2(normalizedX, normalizedY);
    }
}
