using System;
using NaughtyAttributes;
using UnityEngine;

[ExecuteAlways]
public class GroundSync : MonoBehaviour
{
    [SerializeField, Layer] private string layerName = "Ground";
    [SerializeField] private Transform player;
    [SerializeField] private Transform cameraTransform; // Référence à la caméra
    [SerializeField, Range(1f, 30f)] private float rotationSmoothSpeed = 15f;
    [SerializeField, Range(0.01f, 1f)] private float normalLerpSpeed = 0.15f;
    [SerializeField, Range(0f, 10f)] private float minNormalAngle = 1f; // Seuil d'angle minimum pour ignorer les petites variations

    private Vector3 normalFromGround = Vector3.up;
    private Vector3 targetNormal = Vector3.up;
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
            targetNormal = hit.normal;
        }
        else
        {
            targetNormal = Vector3.up;
        }

        // Interpolation douce de la normale
        if (Vector3.Angle(normalFromGround, targetNormal) > minNormalAngle)
        {
            normalFromGround = Vector3.Slerp(normalFromGround, targetNormal, normalLerpSpeed);
        }
        else
        {
            normalFromGround = Vector3.Slerp(normalFromGround, targetNormal, normalLerpSpeed * 0.25f); // Lissage plus lent pour les petites variations
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
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSmoothSpeed);
        }
    }

    private void LateUpdate()
    {
        if (player)
        {
            transform.position = player.position;
        }
    }
}
