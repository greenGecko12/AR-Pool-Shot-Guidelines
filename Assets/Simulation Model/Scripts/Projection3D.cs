using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Simulation_Model.Scripts
{
    public class Projection3D : MonoBehaviour
    {
        private Scene _simScene;
        private PhysicsScene _physicsScene;
        private readonly Dictionary<Transform, Transform> _physicsObjects = new();

        [SerializeField] private Transform objectBalls;
        [SerializeField] private Transform table;
        [SerializeField] private GameObject controller;
        [SerializeField] private int maxFrameIts;
        [SerializeField] private SharedVariables sharedVariables;


        private bool mouseLastMoved = false;
        private Camera _camera;
        private Plane _plane;

        private void Start()
        {
            _camera = Camera.main;
            _plane = new Plane(Vector3.up, table.position);
            CreatePhysicsScene();
        }

        private void Update()
        {
            foreach (var obj in _physicsObjects)
            {
                obj.Value.SetPositionAndRotation(obj.Key.position, obj.Key.rotation);
            }

            // checks if either the mouse or the Oculus controllers have moved since the last frame
            if (Mouse.current.delta.IsActuated())
            {
                mouseLastMoved = true;
            }
            else if (OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch) != Vector3.zero)
            {
                mouseLastMoved = false;
            }
        }

        // creating ghost versions of all the objects and adding it to the simulation scene
        private void CreatePhysicsScene()
        {
            // Physics.simulationMode = SimulationMode.Script;
            // scene with its own isolated 3D physics simulation
            _simScene = SceneManager.CreateScene("SimulationScene", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
            _physicsScene = _simScene.GetPhysicsScene();

            foreach (Transform obstacle in objectBalls) // note, this doesn't include the cue ball
            {
                var ghostObstacle = Instantiate(obstacle.gameObject, obstacle.position, obstacle.rotation);
                // ghostObstacle.GetComponent<Renderer>().enabled = false;
                SceneManager.MoveGameObjectToScene(ghostObstacle, _simScene);
                _physicsObjects.Add(obstacle, ghostObstacle.transform);
            }
            var ghostTable = Instantiate(table.gameObject, table.position, table.rotation);
            // ghostTable.GetComponent<Renderer>().enabled = false;
            SceneManager.MoveGameObjectToScene(ghostTable, _simScene);
        }

        public void SimulateTrajectory(GameObject cueBall)
        {
            var ghostCueBall = Instantiate(cueBall, cueBall.transform.position, cueBall.transform.rotation);
            Debug.Log(ghostCueBall.GetComponent<Rigidbody>().collisionDetectionMode);

            // ghostObj.GetComponent<Renderer>().enabled = false;
            SceneManager.MoveGameObjectToScene(ghostCueBall, _simScene);

            // brudda, not really sure what you're doing here, both branches of the if statement do the same thing?!
            var force = Vector3.zero;
            if (!mouseLastMoved)
            {
                // force from (the point where the ray from the camera hits the table bed) and the ball
                force = ProjectionHelper.GetForceFromCueBallToPoint(cueBall, controller.GetComponent<PlayerPoint>().contactPos);
            }
            else // mouse has moved
            {
                var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

                if (_plane.Raycast(ray, out var enter)) // enter stores the distance from the ray's origin to the intersection point
                {
                    force = ProjectionHelper.GetForceFromCueBallToPoint(cueBall, ray.GetPoint(enter));
                }
            }
            ghostCueBall.GetComponent<Rigidbody>().AddForce(force);

            var lineRenderer = cueBall.GetComponent<LineRenderer>();
            lineRenderer.positionCount = maxFrameIts * 2; // Adjusted to match loop iterations;

            for (var i = 0; i < maxFrameIts * 2; i++)
            {
                _physicsScene.Simulate(Time.fixedDeltaTime);
                lineRenderer.SetPosition(i, ghostCueBall.transform.position);
            }

            Destroy(ghostCueBall);
            // _ghostCueBall.transform.position = cueBall.transform.position;
        }

        public void CalculateTrajectory(GameObject cueBall)
        {
            // Set up LineRenderer
            var lineRenderer = cueBall.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 0;

            // Starting position and direction for the cue ball
            Vector3 startPosition = cueBall.transform.position;
            Vector3 direction = GetCueBallDirection(cueBall); // Initial direction from input (mouse or controller)

            float remainingDistance = sharedVariables.maxTrajectoryDistance; // Max prediction distance
            int maxBounces = sharedVariables.maxBounces; // Limit the number of bounces

            List<Vector3> trajectoryPoints = new()
            {
                startPosition // Add the starting point of the trajectory
            };

            for (int i = 0; i < maxBounces; i++)
            {
                if (remainingDistance <= 0f)
                    break;

                // Perform raycast for cue ball
                if (Physics.Raycast(startPosition, direction, out RaycastHit hit, remainingDistance))
                {
                    trajectoryPoints.Add(hit.point); // Add the collision point to the trajectory

                    if (hit.collider.CompareTag("Table"))
                    {
                        // Reflect direction for wall collisions
                        direction = Vector3.Reflect(direction, hit.normal);


                        direction = Vector3.ProjectOnPlane(direction, Vector3.up);
                        remainingDistance -= hit.distance;
                        startPosition = hit.point + direction * 0.01f; // Slight offset to avoid overlapping
                    }
                    else if (hit.collider.CompareTag("ObjectBall"))
                    {
                        // Calculate cue ball's new trajectory and object ball's trajectory
                        Vector3 cueBallNewDirection = CalculateCueBallCollision(hit, hit.collider.gameObject, cueBall);
                        direction = cueBallNewDirection; // Update cue ball's direction after collision
                        remainingDistance -= hit.distance;
                        startPosition = hit.point + direction * 0.01f; // Slight offset to avoid overlapping
                    }
                }
                else
                {
                    // If no hit, draw until max distance
                    trajectoryPoints.Add(startPosition + direction * remainingDistance);
                    break;
                }
            }

            // Apply points to LineRenderer
            lineRenderer.positionCount = trajectoryPoints.Count;
            lineRenderer.SetPositions(trajectoryPoints.ToArray());
        }

        // Function to calculate the new direction of the cue ball after hitting an object ball
        private Vector3 CalculateCueBallCollision(RaycastHit hit, GameObject objectBall, GameObject cueball)
        {
            // Cue ball velocity (assume normalized direction * speed)
            Vector3 cueBallVelocity = cueball.GetComponent<Rigidbody>().linearVelocity;

            // Object ball velocity (initially stationary)
            Vector3 objectBallPosition = objectBall.transform.position;
            Vector3 collisionPoint = hit.point;

            // Calculate collision normal
            Vector3 collisionNormal = (objectBallPosition - collisionPoint).normalized;

            // Object ball's direction after collision
            Vector3 objectBallVelocity = Vector3.Project(cueBallVelocity, collisionNormal);

            // Cue ball's new direction after losing momentum to the object ball
            Vector3 cueBallNewVelocity = cueBallVelocity - objectBallVelocity;

            // Optionally visualize the object ball's trajectory
            SimulateObjectBallTrajectory(objectBall.transform.position, objectBallVelocity.normalized);

            // Return the new normalized direction for the cue ball
            return cueBallNewVelocity.normalized;
        }

        // Function to simulate the object ball trajectory after collision
        private void SimulateObjectBallTrajectory(Vector3 startPosition, Vector3 direction)
        {
            float remainingDistance = 10f; // Limit object ball trajectory distance
            int maxBounces = 2; // Limit object ball bounces

            for (int i = 0; i < maxBounces; i++)
            {
                if (remainingDistance <= 0f)
                    break;

                if (Physics.Raycast(startPosition, direction, out RaycastHit hit, remainingDistance))
                {
                    // Visualize object ball trajectory
                    Debug.DrawLine(startPosition, hit.point, Color.black, 10f);

                    if (hit.collider.CompareTag("Table"))
                    {
                        direction = Vector3.Reflect(direction, hit.normal);
                        remainingDistance -= hit.distance;
                        startPosition = hit.point + direction * 0.01f; // Slight offset to avoid overlapping
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        // Helper function to get the initial direction of the cue ball
        private Vector3 GetCueBallDirection(GameObject cueBall)
        {
            var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (_plane.Raycast(ray, out float enter))
            {
                Vector3 targetPoint = ray.GetPoint(enter);
                Vector3 zeroY = targetPoint - cueBall.transform.position;
                zeroY.y = 0f;
                return zeroY.normalized;
            }

            return Vector3.forward; // Default direction if input is invalid
        }


    }
}
