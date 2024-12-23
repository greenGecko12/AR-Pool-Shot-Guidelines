using UnityEngine;

[CreateAssetMenu(fileName = "SharedVariables", menuName = "Game/Shared Variables")]


public class SharedVariables : ScriptableObject
{
    [Header("Ball settings")]
    public float movementThreshold = 0.01f;

    // note - this bit is not currently working
    [Header("Camera settings")]
    public float moveSpeed = 10f;
    public float rotationSpeed = 100f;

    [Header("Trajectory settings")]
    public float maxTrajectoryDistance = 30f;
    public int maxBounces = 1;

}