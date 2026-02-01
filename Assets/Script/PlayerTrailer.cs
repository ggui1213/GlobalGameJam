using System;
using UnityEngine;

namespace Script
{
    public class PlayerTrailer : MonoBehaviour
    {
        public bool IsTrailing { get; private set; } = false;

        private void FixedUpdate()
        {
            if (IsTrailing)
            {
                MovementTrialManager.Instance.AddNewPositionToCurrentTrial(transform.position);
            }
        }

        public void StartTrailing()
        {
            IsTrailing = true;
            MovementTrialManager.Instance.InitiateNewTrial();
        }
        
        public void StopTrailing()
        {
            IsTrailing = false;
        }
    }
}