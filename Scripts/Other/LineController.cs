using UnityEngine;

public class LineController : MonoBehaviour
{
    public Vector3 targetPosition;
    public Transform targetTransform;

    private LineRenderer lineRenderer;
    

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        lineRenderer.SetPosition(0, Vector3.zero);
        transform.rotation = Quaternion.identity;
        if (targetTransform == null)
        {
            lineRenderer.SetPosition(1, targetPosition - transform.position);
        }
        else
        {
            lineRenderer.SetPosition(1, targetTransform.position);
        }
    }
}
