using UnityEngine;
using UnityEngine.InputSystem;


// Doesn't currently work 
public class OVRCameraController : MonoBehaviour
{
    private Vector2 moveInput;   // For arrow key movement
    private Vector2 rotateInput; // For WASD rotation
    private float verticalMove;  // For PageUp/PageDown vertical movement

    [SerializeField] public Transform cameraRigRoot; // Assign the OVRCameraRig root or its parent in the inspector
    [SerializeField] public SharedVariables sharedVariables; // For moveSpeed and rotationSpeed

    void Update()
    {
        if (cameraRigRoot == null)
        {
            Debug.LogWarning("Camera Rig Root is not assigned!");
            return;
        }

        // Movement using arrow keys
        float moveX = moveInput.x * sharedVariables.moveSpeed * Time.deltaTime;
        float moveZ = moveInput.y * sharedVariables.moveSpeed * Time.deltaTime;

        // Vertical movement (PageUp/PageDown)
        float moveY = verticalMove * sharedVariables.moveSpeed * Time.deltaTime;

        // Apply movement to the OVRCameraRig's root or parent
        cameraRigRoot.Translate(new Vector3(moveX, moveY, moveZ), Space.Self);

        // Rotation using WASD keys
        float rotateX = rotateInput.y * sharedVariables.rotationSpeed * Time.deltaTime; // W/S for look up/down
        float rotateY = rotateInput.x * sharedVariables.rotationSpeed * Time.deltaTime; // A/D for look left/right

        // Apply rotation to the OVRCameraRig's root or parent
        cameraRigRoot.Rotate(new Vector3(rotateX, rotateY, 0f), Space.Self);
    }

    // Input System callbacks
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>(); // Read movement input from arrow keys
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        rotateInput = context.ReadValue<Vector2>(); // Read rotation input from WASD
    }

    public void OnVerticalMove(InputAction.CallbackContext context)
    {
        verticalMove = context.ReadValue<float>(); // Read vertical movement (PageUp/PageDown equivalent)
    }
}
