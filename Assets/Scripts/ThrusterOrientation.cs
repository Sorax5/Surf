using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThrusterOrientation : MonoBehaviour
{
    public PlayerInput playerInput;
    public List<GameObject> thrusters;
    
    [Header("Réglages")]
    public float intensiteAngle = 35f;
    public float vitesse = 10f;

    private InputAction moveAction;
    private List<Quaternion> rotationsInitiales;

    private void Awake()
    {
        rotationsInitiales = new List<Quaternion>(thrusters.Count);
    }

    private void Start()
    {
        if (playerInput)
        {
            moveAction = playerInput.actions.FindAction("Move");
        }
        
        if (thrusters != null)
        {
            foreach (var thruster in thrusters)
            {
                if (!thruster)
                {
                    continue;
                }
                
                rotationsInitiales.Add(thruster.transform.localRotation);
            }
        }

        if (moveAction == null)
        {
            Debug.LogError("ERREUR : Action 'Move' introuvable !");
        }

        if (thrusters is { Count: 0 })
        {
            Debug.LogWarning("ATTENTION : Liste de thrusters vide !");
        }
    }

    private void Update()
    {
        if (moveAction == null)
        {
            return;
        }
        
        var input = moveAction.ReadValue<Vector2>();

        for (var i = 0; i < thrusters.Count; i++)
        {
            if (!thrusters[i])
            {
                continue;
            }

            Quaternion cible;
            if (input.sqrMagnitude > 0.01f)
            {
                var rotX = input.y * intensiteAngle;
                var rotZ = -input.x * intensiteAngle;

                cible = rotationsInitiales[i] * Quaternion.Euler(rotX, 0, rotZ);
            }
            else
            {
                cible = rotationsInitiales[i];
            }
            
            thrusters[i].transform.localRotation = Quaternion.Slerp(
                thrusters[i].transform.localRotation, 
                cible, 
                Time.deltaTime * vitesse
            );
        }
    }
}