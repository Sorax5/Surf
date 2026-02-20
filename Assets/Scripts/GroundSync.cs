using System;
using NaughtyAttributes;
using UnityEngine;

[ExecuteAlways]
public class GroundSync : MonoBehaviour
{
    [SerializeField, Layer] private string layerName = "Ground";
    [SerializeField] private Transform player;
    [SerializeField] private Transform cameraTransform; // Référence à la caméra
    
    private Vector3 normalFromGround;
    private RaycastHit hit;
    
    private LayerMask layerMask;

    private void Awake()
    {
        layerMask = LayerMask.GetMask(layerName);
    }

    private void Update()
    {
        var ray = new Ray(transform.position + Vector3.up * 0.5f, Vector3.down);
        if (Physics.Raycast(ray, out hit, 10f, layerMask))
        {
            normalFromGround = hit.normal;
        }
        else
        {
            normalFromGround = Vector3.up;
        }
    }

    private void FixedUpdate()
    {
        // Utiliser la direction de la caméra projetée sur le plan du sol
        Vector3 cameraForward = cameraTransform ? cameraTransform.forward : Vector3.forward;
        var projectedForward = Vector3.ProjectOnPlane(cameraForward, normalFromGround).normalized;

        if (projectedForward.sqrMagnitude > 0.01f)
        {
            var targetRotation = Quaternion.LookRotation(projectedForward, normalFromGround);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 15f);
        }
    }

    private void LateUpdate()
    {
        if (player)
        {
            transform.position = player.position;
        }
    }

    private void OnDrawGizmos() 
    {
        /*Gizmos.color = Color.red;
        Gizmos.DrawLine(hit.point, hit.point + hit.normal);
        
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(hit.point, 0.1f);*/
    }
}
