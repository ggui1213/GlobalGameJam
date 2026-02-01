using UnityEngine;
using UnityEngine.UI;

namespace Script
{
    public class LevelEndPoint : MonoBehaviour
    {
        [SerializeField] private string nextLevelName;
        [SerializeField] private GameObject arrowIndicatorPrefab;
        [SerializeField, Tooltip("Relative Position, from -1 to 1 on both axis")] private Rect indicatorVanishScreenBound;
        
        public string NextLevelName => nextLevelName;
        
        [SerializeField] private Canvas playerUICanvas;
        private RectTransform _arrowIndicatorTransform;
        private Image _arrowIndicatorImage;

        // New: normalized indicator rect settings (center in -1..1, size as proportions, default centered with 0.9x0.9)
        [SerializeField, Tooltip("Indicator rect center in normalized screen coords (-1..1). Default 0,0")]
        private Vector2 indicatorRectCenterNormalized = Vector2.zero;
        [SerializeField, Tooltip("Inner rect size in normalized screen units (width,height). Indicator hidden when endpoint inside this rect.")]
        private Vector2 innerRectSizeNormalized = new Vector2(0.2f, 0.2f);
        [SerializeField, Tooltip("Outer rect size in normalized screen units (width,height). Indicator clamped to this rect when endpoint is outside.")]
        private Vector2 outerRectSizeNormalized = new Vector2(0.9f, 0.9f);

        // New: size interpolation settings
        [SerializeField, Range(0f, 1f), Tooltip("How much distance from center affects indicator size. 0 = always min size, 1 = full effect.")]
        private float sizeInfluence = 1f; // slider 0..1
        [SerializeField, Tooltip("Indicator size in pixels when interpolation factor is 0")]
        private float minIndicatorSize = 40f;
        [SerializeField, Tooltip("Indicator size in pixels when interpolation factor is 1")]
        private float maxIndicatorSize = 80f;

        private void Start()
        {
            if (arrowIndicatorPrefab == null || playerUICanvas == null)
            {
                Debug.LogWarning($"LevelEndPoint: missing arrowIndicatorPrefab or playerUICanvas on {gameObject.name}; indicator will be disabled.");
                enabled = false;
                return;
            }

            var go = Instantiate(arrowIndicatorPrefab, playerUICanvas.transform);
            _arrowIndicatorTransform = go.GetComponent<RectTransform>();
            _arrowIndicatorImage = go.GetComponent<Image>();
            if (_arrowIndicatorImage != null)
                _arrowIndicatorImage.enabled = false;
        }

        // Make sure there's an arrow pointing to the next level in the game
        void Update()
        {
            Camera cam = Player.Instance.PlayerCamera;

            // Use Vector3 to check if target is behind the camera (z < 0)
            Vector3 sp3 = cam.WorldToScreenPoint(transform.position);
            Vector2 sp;
            if (sp3.z < 0f)
            {
                // Target is behind camera. Produce a screen direction by projecting
                // a point from the camera in the direction of the endpoint to the far plane.
                Vector3 dir = (transform.position - cam.transform.position).normalized;
                Vector3 farPoint = cam.WorldToScreenPoint(cam.transform.position + dir * 1000f);
                sp = new Vector2(farPoint.x, farPoint.y);
            }
            else
            {
                sp = new Vector2(sp3.x, sp3.y);
            }

            // ===== 1. 归一化屏幕坐标 (-1 ~ 1) =====
            Vector2 normalized = new(
                (sp.x / Screen.width)  * 2f - 1f,
                (sp.y / Screen.height) * 2f - 1f
            );

            // Define inner & outer rects (in normalized -1..1 space) around the configured center.
            Rect innerRect = new Rect(indicatorRectCenterNormalized - innerRectSizeNormalized * 0.5f, innerRectSizeNormalized);
            Rect outerRect = new Rect(indicatorRectCenterNormalized - outerRectSizeNormalized * 0.5f, outerRectSizeNormalized);

            // If endpoint is inside inner rect -> indicator should disappear
            if (innerRect.Contains(normalized))
            {
                if (_arrowIndicatorImage != null)
                    _arrowIndicatorImage.enabled = false;
                return;
            }

            // indicator only appears in the region between innerRect and outerRect
            _arrowIndicatorImage.enabled = true;

            Vector2 indicatorNormalizedPos;
            if (outerRect.Contains(normalized))
            {
                // If inside outer rect (but not inner) -> indicator is at actual position
                indicatorNormalizedPos = normalized;
            }
            else
            {
                // Outside outer rect: clamp to outer rect boundary along the ray from center
                Vector2 dir = normalized - indicatorRectCenterNormalized;
                if (dir == Vector2.zero)
                {
                    indicatorNormalizedPos = indicatorRectCenterNormalized;
                }
                else
                {
                    float halfW = outerRect.width * 0.5f;
                    float halfH = outerRect.height * 0.5f;

                    float tX = dir.x != 0f ? (dir.x > 0f ? halfW : -halfW) / dir.x : float.PositiveInfinity;
                    float tY = dir.y != 0f ? (dir.y > 0f ? halfH : -halfH) / dir.y : float.PositiveInfinity;

                    float t = Mathf.Min(tX, tY);
                    t = Mathf.Max(0f, t);

                    indicatorNormalizedPos = indicatorRectCenterNormalized + dir * t;
                    indicatorNormalizedPos.x = Mathf.Clamp(indicatorNormalizedPos.x, -1f, 1f);
                    indicatorNormalizedPos.y = Mathf.Clamp(indicatorNormalizedPos.y, -1f, 1f);
                }
            }

            // 将 normalized (-1..1) 映射回屏幕坐标
            Vector2 indicatorPos = new(
                (indicatorNormalizedPos.x + 1f) * 0.5f * Screen.width,
                (indicatorNormalizedPos.y + 1f) * 0.5f * Screen.height
            );

            if (_arrowIndicatorImage != null && _arrowIndicatorImage.rectTransform != null)
            {
                _arrowIndicatorImage.rectTransform.position = indicatorPos;

                // ===== 4. 可选：旋转指向目标 =====
                Vector2 dirToTarget = sp - indicatorPos;
                float angle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
                _arrowIndicatorImage.rectTransform.rotation = Quaternion.Euler(0, 0, angle - 90f);
            }

            // ===== 5. Size interpolation based on distance from inner to outer rect =====
            // Compute radial magnitudes for inner and outer rect corners as reference.
            float innerHalfW = innerRect.width * 0.5f;
            float innerHalfH = innerRect.height * 0.5f;
            float outerHalfW = outerRect.width * 0.5f;
            float outerHalfH = outerRect.height * 0.5f;
            float innerMax = Mathf.Sqrt(innerHalfW * innerHalfW + innerHalfH * innerHalfH);
            float outerMax = Mathf.Sqrt(outerHalfW * outerHalfW + outerHalfH * outerHalfH);

            float mag = (normalized - indicatorRectCenterNormalized).magnitude;

            // If outside outer rect (or behind camera), treat as max size
            float sizeT;
            if (mag >= outerMax)
            {
                sizeT = 1f;
            }
            else
            {
                // map mag in [innerMax, outerMax] to t in [0,1]
                float denom = Mathf.Max(1e-6f, outerMax - innerMax);
                sizeT = Mathf.Clamp01((mag - innerMax) / denom);
            }

            // sizeT == 0 => at inner boundary -> min size; sizeT == 1 => at/after outer boundary -> max size
            float size = Mathf.Lerp(minIndicatorSize, maxIndicatorSize, sizeT);
            if (_arrowIndicatorTransform != null)
            {
                _arrowIndicatorTransform.sizeDelta = new Vector2(size, size);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // Load the next level
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextLevelName);
            }
        }
    }
}