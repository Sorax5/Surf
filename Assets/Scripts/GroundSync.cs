using System;
using NaughtyAttributes;
using UnityEngine;

[ExecuteAlways]
public class GroundSync : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform player;
    [SerializeField] private Transform cameraTransform;

    [Header("Settings")]
    [SerializeField, Range(1f, 30f)] private float rotationSmoothSpeed = 15f;
    [SerializeField, Range(1f, 20f)] private float normalSmoothSpeed = 10f; // Changé pour être basé sur le temps
    [SerializeField] private float raycastDistance = 2f;
    [SerializeField] private float raycastOffset = 0.5f;

    private Vector3 currentNormal = Vector3.up;

    private void LateUpdate()
    {
        if (!player)
        {
            return;
        }
        
        transform.position = player.position;
        
        var ray = new Ray(transform.position + Vector3.up * raycastOffset, Vector3.down);
        var targetNormal = Vector3.up;
        
        if (Physics.Raycast(ray, out var hit, raycastDistance + raycastOffset, groundLayer))
        {
            targetNormal = hit.normal;
        }
        
        currentNormal = Vector3.Slerp(currentNormal, targetNormal, Time.deltaTime * normalSmoothSpeed);

        var cameraForward = cameraTransform ? cameraTransform.forward : Vector3.forward;
        var projectedForward = Vector3.ProjectOnPlane(cameraForward, currentNormal);

        if (!(projectedForward.sqrMagnitude > 0.001f))
        {
            return;
        }
        
        var targetRotation = Quaternion.LookRotation(projectedForward.normalized, currentNormal);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + currentNormal);
        
        Gizmos.color = Color.red;
        var ray = new Ray(transform.position + Vector3.up * raycastOffset, Vector3.down);
        Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * (raycastDistance + raycastOffset));
    }
}
