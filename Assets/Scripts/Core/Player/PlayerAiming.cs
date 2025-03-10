using Unity.Netcode;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform turretTransform;

    void LateUpdate()
    {
        if (!IsOwner) return;

        var mouseWorld = Camera.main.ScreenToWorldPoint(inputReader.AimPosition);
        // Turretposition is world position of the turret transform, not local position
        turretTransform.up = (Vector2)(mouseWorld - turretTransform.position).normalized;
    }
}
