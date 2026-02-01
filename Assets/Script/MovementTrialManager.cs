using UnityEngine;

namespace Script
{
    
    public class MovementTrialManager : MonoBehaviour
    {
        public enum FadeMode
        {
            DiscreteSteps, // map trails to fixed steps = maxTrialCount-1
            EvenlySpaced   // spread evenly across current trails (oldest -> min, newest -> max)
        }

        [SerializeField, Tooltip("Including current trail")] private int maxTrialCount = 4;
        
        [SerializeField, Range(0f,1f)] private float trailMaxOpacity = 0.5f;

        [SerializeField, Range(0f, 1f),
         Tooltip("This should Not be 0, keep in mind, the min opacity is the last time before it disappear")]
        private float trailMinOpacity = 0.1f;

        [SerializeField, Tooltip("Choose how trail opacities are distributed")]
        private FadeMode fadeMode = FadeMode.DiscreteSteps;
        
        private LineRenderer[] _lineRenderers;
        private LineRenderer _currentTrail;
        private int _currentTrailCount;
        
        public static MovementTrialManager Instance;

        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            _lineRenderers = new LineRenderer[maxTrialCount];
        }
        
        public void EndCurrentTrial()
        {
            _currentTrail = null;
        }

        // Helper: compute start alpha for trail at index i (0 oldest .. n-1 newest)
        private float ComputeStartAlphaForIndex(int i, int n)
        {
            if (n <= 0) return trailMinOpacity;
            if (fadeMode == FadeMode.EvenlySpaced)
            {
                if (n == 1) return trailMaxOpacity;
                float t = (float)i / (n - 1); // 0..1 oldest->newest
                return Mathf.Lerp(trailMinOpacity, trailMaxOpacity, t);
            }
            else // DiscreteSteps
            {
                int steps = Mathf.Max(1, maxTrialCount - 1);
                if (n == 1) return trailMaxOpacity;
                // map i (0..n-1) to discrete steps (0..steps) so oldest => 0, newest => steps
                int step = Mathf.RoundToInt((float)i * steps / (n - 1));
                float t = (float)step / steps; // 0..1
                return Mathf.Lerp(trailMinOpacity, trailMaxOpacity, t);
            }
        }
        
        public void InitiateNewTrial()
        {
            Debug.Log("Trial Initiated");
            if (_currentTrailCount >= maxTrialCount)
            {
                LineRenderer oldestTrail = _lineRenderers[0];
                Destroy(oldestTrail.gameObject);
                for (int i = 1; i < maxTrialCount; i++)
                {
                    _lineRenderers[i - 1] = _lineRenderers[i];
                }
                // clear the freed slot at the end
                _lineRenderers[maxTrialCount - 1] = null;
                _currentTrailCount--;
            }
            GameObject newTrailObject = new GameObject("MovementTrialTrail");
            LineRenderer newTrail = newTrailObject.AddComponent<LineRenderer>();
            newTrail.positionCount = 0;
            newTrail.material = new Material(Shader.Find("Sprites/Default"));
            newTrail.widthMultiplier = 0.05f;
            // initialize with default gradient, will be overwritten below
            var startColor = Color.gray;
            startColor.a = trailMaxOpacity;
            var endColor = Color.white;
            endColor.a = trailMinOpacity;
            newTrail.startColor = startColor;
            newTrail.endColor = endColor;
            _lineRenderers[_currentTrailCount] = newTrail;
            _currentTrailCount++;
            _currentTrail = newTrail;
            
            // Update opacities of all trails so they form a smooth fade from oldest -> newest
            int n = _currentTrailCount;
            if (n > 0)
            {
                float ratio = (trailMaxOpacity > 0f) ? (trailMinOpacity / trailMaxOpacity) : 0f;
                for (int i = 0; i < n; i++)
                {
                    var lr = _lineRenderers[i];
                    if (lr == null) continue;

                    float startAlpha = ComputeStartAlphaForIndex(i, n);
                    float endAlpha = startAlpha * ratio;

                    var sColor = lr.startColor;
                    sColor.a = Mathf.Clamp01(startAlpha);
                    lr.startColor = sColor;

                    var eColor = lr.endColor;
                    eColor.a = Mathf.Clamp01(endAlpha);
                    lr.endColor = eColor;
                }
            }
        }
        
        public void AddNewPositionToCurrentTrial(Vector3 newPosition)
        {
            if (_currentTrail == null)
                throw new System.Exception("No current trail. Call InitiateNewTrial first.");
            _currentTrail.positionCount += 1;
            _currentTrail.SetPosition(_currentTrail.positionCount - 1, newPosition);
        }
    }
}
