using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

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
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float brakeForce = 40f;
    
    [SerializeField] private VisualEffect fireParticles;
    [SerializeField] private string fireRateProperty = "Rate";
    [SerializeField] private string fireActiveProperty = "IsActive";
    [SerializeField] private ParticleSystem smokeParticles;
    [SerializeField] private float minSmokeSpeed = 2f;
    
    private Vector2 moveInput;
    private float sprintPressure = 0f;
    private float brakePressure = 0f;
    private bool isFireVfxPlaying;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        groundChecker = GetComponent<GroundChecker>();
        cam = Camera.main;
    }

    private void Start()
    {
        moveAction = playerInput?.actions.FindAction("Move");
        sprintAction = playerInput?.actions.FindAction("Sprint");
        brakeAction = playerInput?.actions.FindAction("Brake");
        
        fireParticles?.Stop();
    }

    private void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        sprintPressure = sprintAction?.ReadValue<float>() ?? 0f;
        brakePressure = brakeAction?.ReadValue<float>() ?? 0f;
        
        if (fireParticles)
        {
            var shouldEmitFire = sprintPressure > 0.01f && groundChecker.IsGrounded();
            var fireRate = Mathf.Lerp(0f, 1f, sprintPressure);

            if (!string.IsNullOrWhiteSpace(fireRateProperty) && fireParticles.HasFloat(fireRateProperty))
            {
                fireParticles.SetFloat(fireRateProperty, fireRate);
                Gamepad.current?.SetMotorSpeeds(fireRate * 0.5f, fireRate * 0.5f);
            }

            if (!string.IsNullOrWhiteSpace(fireActiveProperty) && fireParticles.HasBool(fireActiveProperty))
            {
                fireParticles.SetBool(fireActiveProperty, shouldEmitFire);
            }

            if (shouldEmitFire && !isFireVfxPlaying)
            {
                fireParticles.Play();
                isFireVfxPlaying = true;
            }
            else if (!shouldEmitFire && isFireVfxPlaying)
            {
                fireParticles.Stop();
                isFireVfxPlaying = false;
            }
        }
        
        if (smokeParticles)
        {
            var emission = smokeParticles.emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(Mathf.Lerp(0f, 100f, brakePressure));
            
            var horizontalVelocity = rb ? rb.linearVelocity : Vector3.zero;
            horizontalVelocity.y = 0f;
            var speed = horizontalVelocity.magnitude;
            
            if (brakePressure > 0.01f && groundChecker.IsGrounded() && speed > minSmokeSpeed)
            {
                if (!smokeParticles.isEmitting)
                {
                    smokeParticles.Play();
                }
            }
            else
            {
                if (smokeParticles.isEmitting)
                {
                    smokeParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        var moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        var cameraForward = cam.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();
        var cameraRight = cam.transform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        var acceleration = groundChecker.IsGrounded() ? 20f : 10f;
        
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
