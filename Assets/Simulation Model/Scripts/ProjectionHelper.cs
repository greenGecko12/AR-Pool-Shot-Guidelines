using UnityEngine;

namespace Simulation_Model.Scripts
{
      public static class ProjectionHelper
      {
            private const float ForceMultiplier = 100f;
            
            // Get a force vector between cue ball and mouse
            public static Vector3 GetForceFromCueBallToPoint(GameObject cueBall, Vector3 point)
            {
            
                  var forceToApply = (point - cueBall.transform.position) * ForceMultiplier;
                  forceToApply.y = 0f;
                  return forceToApply;
            }
      }
}