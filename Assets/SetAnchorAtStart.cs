using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class SetAnchorAtStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.AddComponent<ARAnchor>();
    }
}
