using System.Collections.Generic;
using UnityEngine;

namespace Script
{
    
    public class MovementTrialManager : MonoBehaviour
    {
        [SerializeField, Tooltip("Including current trail")] private int maxTrialCount = 4;
        
        [SerializeField, Range(0f,1f)] private float trailMaxOpacity = 0.5f;

        [SerializeField, Range(0f, 1f),
         Tooltip("This should Not be 0, keep in mind, the min opacity is the last time before it disappear")]
        private float trailMinOpacity = 0.1f;
        
        private LineRenderer[] _lineRenderers;
        private LineRenderer _currentTrail;
        private int _currentTrailCount = 0;
        
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
                _currentTrailCount--;
            }
            GameObject newTrailObject = new GameObject("MovementTrialTrail");
            LineRenderer newTrail = newTrailObject.AddComponent<LineRenderer>();
            newTrail.positionCount = 0;
            newTrail.material = new Material(Shader.Find("Sprites/Default"));
            newTrail.widthMultiplier = 0.05f;
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
                // preserve the ratio between start and end alpha so each trail keeps its gradient look
                float ratio = (trailMaxOpacity > 0f) ? (trailMinOpacity / trailMaxOpacity) : 0f;
                for (int i = 0; i < n; i++)
                {
                    var lr = _lineRenderers[i];
                    if (lr == null) continue;

                    float startAlpha;
                    float endAlpha;
                    if (n == 1)
                    {
                        // only one trail -> full opacity range
                        startAlpha = trailMaxOpacity;
                        endAlpha = trailMinOpacity;
                    }
                    else
                    {
                        // i == 0 => oldest (min), i == n-1 => newest (max)
                        float t = (float)i / (n - 1);
                        startAlpha = Mathf.Lerp(trailMinOpacity, trailMaxOpacity, t);
                        endAlpha = startAlpha * ratio;
                    }

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
