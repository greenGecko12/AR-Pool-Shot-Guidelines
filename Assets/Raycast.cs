using UnityEngine;

public class PlayerPoint : MonoBehaviour
{
    private readonly int _raylength = 200;
    private LineRenderer _lineRenderer;
    private GameObject _line;
    public Vector3 contactPos;

    // Initialisation
    void Start()
    {
        contactPos = new();

        // Create and configure the LineRenderer only once
        _line = new("Line");
        _lineRenderer = _line.AddComponent<LineRenderer>();
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startWidth = 0.01f;
        _lineRenderer.endWidth = 0.01f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, _raylength))
        {
            if (hit.collider.CompareTag("Table"))
            {
                contactPos = hit.point;
                _line.transform.position = transform.position;
                _lineRenderer.SetPosition(0, transform.position);
                _lineRenderer.SetPosition(1, hit.point);

            }
        }
    }
}