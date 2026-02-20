using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GroundChecker))]
public class PlayerController : MonoBehaviour
{
    // Références
    private PlayerInput playerInput;
    private Rigidbody rb;
    private GroundChecker groundChecker;
    private Camera cam;

    // Input Actions
    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction brakeAction;

    // Paramètres
    [SerializeField] private float gravityMultiplier = 3f; // Gravité supplémentaire en l'air
    [SerializeField] private float sprintMultiplier = 2f;   // Multiplicateur de vitesse pour le sprint
    [SerializeField] private float brakeForce = 40f;        // Force de freinage
    
    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private ParticleSystem smokeParticles;
    [SerializeField] private float minSmokeSpeed = 2f; // Vitesse minimale pour activer la fumée

    // État
    private Vector2 moveInput;
    private float sprintPressure = 0f;
    private float brakePressure = 0f;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        groundChecker = GetComponent<GroundChecker>();
        cam = Camera.main;
    }

    private void Start()
    {
        moveAction = playerInput.actions.FindAction("Move");
        sprintAction = playerInput.actions.FindAction("Sprint");
        brakeAction = playerInput.actions.FindAction("Brake");
        // Suppression de la gestion des particules dans les events
    }

    private void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        sprintPressure = sprintAction?.ReadValue<float>() ?? 0f;
        brakePressure = brakeAction?.ReadValue<float>() ?? 0f;
        
        if (fireParticles)
        {
            var emission = fireParticles.emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(Mathf.Lerp(0f, 200f, sprintPressure));
            var shouldPlay = sprintPressure > 0.01f && groundChecker.IsGrounded();
            if (shouldPlay)
            {
                if (!fireParticles.isEmitting)
                    fireParticles.Play();
            }
            else
            {
                if (fireParticles.isEmitting)
                    fireParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
        
        if (smokeParticles)
        {
            var emission = smokeParticles.emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(Mathf.Lerp(0f, 100f, brakePressure));
            // Calcul de la vitesse horizontale
            Vector3 horizontalVelocity = rb ? rb.linearVelocity : Vector3.zero;
            horizontalVelocity.y = 0f;
            float speed = horizontalVelocity.magnitude;
            // On ne joue les particules de fumée que si le joueur freine, est au sol, et va plus vite que minSmokeSpeed
            var shouldPlay = brakePressure > 0.01f && groundChecker.IsGrounded() && speed > minSmokeSpeed;
            if (shouldPlay)
            {
                if (!smokeParticles.isEmitting)
                    smokeParticles.Play();
            }
            else
            {
                if (smokeParticles.isEmitting)
                    smokeParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        
    }

    private void FixedUpdate()
    {
        // Calcul direction
        var moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        var cameraForward = cam.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();
        var cameraRight = cam.transform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        var acceleration = groundChecker.IsGrounded() ? 20f : 10f;

        // Sprint : forcer le déplacement dans la direction forward caméra, force proportionnelle à la pression
        if (sprintAction != null && sprintPressure > 0.01f && groundChecker.IsGrounded())
        {
            acceleration *= Mathf.Lerp(1f, sprintMultiplier, sprintPressure);
            moveDirection = cameraForward;
        }
        else
        {
            moveDirection = cameraForward * moveDirection.z + cameraRight * moveDirection.x;
        }
        
        if (brakeAction != null && brakePressure > 0.01f && groundChecker.IsGrounded())
        {
            var horizontalVelocity = rb.linearVelocity;
            horizontalVelocity.y = 0f;
            var brakeAmount = Mathf.Lerp(0f, brakeForce, brakePressure);
            rb.AddForce(-horizontalVelocity.normalized * brakeAmount, ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * acceleration, ForceMode.Acceleration);
        }
        
        if (!groundChecker.IsGrounded() && rb.linearVelocity.y > -20f)
        {
            rb.linearVelocity += Vector3.down * 2f;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        var moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        Gizmos.DrawLine(transform.position, transform.position + moveDirection);
    }
}
