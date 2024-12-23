using UnityEngine;

namespace Simulation_Model.Scripts
{
    public class CueBall : MonoBehaviour
    {
        [SerializeField] private SharedVariables sharedVariables;
        [SerializeField] private GameObject trajectoryManager;

        private Rigidbody _rb;
        private Projection3D _projector;

        void Start ()
        {
            _rb = gameObject.GetComponent<Rigidbody>();
            _projector = trajectoryManager.GetComponent<Projection3D>();
            
        }

        // Update is called once per frame
        void Update()
        {
            // there isn't much point in calculating the trajectory when the cue ball is moving
            if (_rb.linearVelocity.magnitude <= sharedVariables.movementThreshold)
            { 
                _projector.SimulateTrajectory(gameObject);
                //_projector.CalculateTrajectory(gameObject);

            }
        }
    }
}
