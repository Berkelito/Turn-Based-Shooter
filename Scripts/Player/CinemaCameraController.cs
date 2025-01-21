using Unity.Cinemachine;
using UnityEngine;

public class CinemaCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;

    public void ForceCameraPosition(Vector3 position)
    {
        cinemachineCamera.ForceCameraPosition(position, cinemachineCamera.transform.rotation);
    }
}
