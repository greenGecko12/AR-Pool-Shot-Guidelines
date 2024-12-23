using UnityEngine;
using UnityEngine.InputSystem;

namespace Simulation_Model.Scripts
{
    public class ForceDebugger : MonoBehaviour
    {

        [SerializeField] private SharedVariables sharedVariables;
        [SerializeField] private GameObject cueBall;
        [SerializeField] private GameObject table;
        [SerializeField] private GameObject controller;

        private Camera _camera;

        private bool mouseLastMoved = false;
        private Rigidbody _cueBallrb;
        private Plane _plane;
        private float _inputCooldown;
        

        private void Start()
        {
            _camera = Camera.main;
            _plane = new Plane(Vector3.up, table.transform.position);
            _cueBallrb = cueBall.GetComponent<Rigidbody>();
        }

        public void FixedUpdate()
        {
            if (Mouse.current.leftButton.IsPressed() || Keyboard.current.spaceKey.IsPressed() || OVRInput.Get(OVRInput.Button.One))
            {
                if (_inputCooldown > 0) return;
                _inputCooldown = 1f;
                var force = Vector3.zero;
                if (!mouseLastMoved)
                {
                    Debug.Log("Mouse not moved");
                    force = ProjectionHelper.GetForceFromCueBallToPoint(cueBall, controller.GetComponent<PlayerPoint>().contactPos);
                }
                else
                {
                    Debug.Log("Mouse moved");
                    var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

                    if (_plane.Raycast(ray, out var enter))
                    {
                        force = ProjectionHelper.GetForceFromCueBallToPoint(cueBall, ray.GetPoint(enter));
                    }
                }

                if (_cueBallrb.linearVelocity.magnitude <= sharedVariables.movementThreshold)
                {
                    cueBall.GetComponent<Rigidbody>().AddForce(force);
                }

                
            }
        }

        private void Update()
        {
            if (_inputCooldown > 0) _inputCooldown -= Time.deltaTime;

            if (Mouse.current.delta.IsActuated())
            {
                mouseLastMoved = true;
            }
            else if (OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch) != Vector3.zero)
            {
                mouseLastMoved = false;
            }
        }
    }
}
