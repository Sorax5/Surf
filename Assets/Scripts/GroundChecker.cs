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
        if (((1 << collision.gameObject.layer) & layerMask) != 0)
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & layerMask) != 0)
        {
            isGrounded = false;
        }
    }
    private void OnDrawGizmos() 
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
