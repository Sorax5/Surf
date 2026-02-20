using NaughtyAttributes;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [SerializeField, Layer] private string layerName = "Ground";
    
    private LayerMask layerMask;

    private bool isGrounded = false;

    private void Awake()
    {
        layerMask = LayerMask.GetMask(layerName);
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    private void OnCollisionStay(Collision collision)
    {
        // Vérifie si le collider touché est bien sur le layer du sol
        if (((1 << collision.gameObject.layer) & layerMask) != 0)
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Si on quitte le contact avec le sol, on n'est plus au sol
        if (((1 << collision.gameObject.layer) & layerMask) != 0)
        {
            isGrounded = false;
        }
    }

    private void OnDrawGizmos() 
    {
        // Affiche simplement la position du joueur pour éviter les erreurs
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
