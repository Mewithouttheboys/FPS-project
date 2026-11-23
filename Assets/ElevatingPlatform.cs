using System;
using System.Collections.Generic;
using UnityEngine;

public class ElevatingPlatform : MonoBehaviour
{
    [Serializable]
    public class FloorData
    {
        [Tooltip("Where the platform stops for this floor.")]
        public Transform stopPoint;

        [Tooltip("Enemies that must die on the previous floor before moving to this stopPoint.")]
        public List<Ghoul> requiredEnemies = new List<Ghoul>();
    }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private float activationDelay = 1.0f;

    [Header("Floor Sequence (Top to Bottom or Bottom to Top)")]
    [Tooltip("Add your 8 floor destinations and the enemies required to unlock each move.")]
    [SerializeField] private List<FloorData> floors = new List<FloorData>();

    private int currentFloorIndex = 0;
    private bool isUnlocked = false;
    private bool isMoving = false;
    private float delayTimer = 0f;

    private void Start()
    {
        if (floors.Count > 0)
        {
            SetupFloor(0);
        }
    }

    private void SetupFloor(int index)
    {
        if (index >= floors.Count) return;

        currentFloorIndex = index;
        isUnlocked = false;
        isMoving = false;

        FloorData currentFloor = floors[currentFloorIndex];

        // Subscribe to all enemies assigned to the current floor
        foreach (Ghoul enemy in currentFloor.requiredEnemies)
        {
            if (enemy != null)
            {
                enemy.OnEnemyDied += HandleEnemyDied;
            }
        }

        // Edge case: If a floor has 0 enemies assigned, unlock immediately
        if (currentFloor.requiredEnemies.Count == 0)
        {
            UnlockCurrentFloor();
        }
    }

    private void HandleEnemyDied(Ghoul deadEnemy)
    {
        deadEnemy.OnEnemyDied -= HandleEnemyDied;

        FloorData currentFloor = floors[currentFloorIndex];
        currentFloor.requiredEnemies.Remove(deadEnemy);

        // Once every enemy on this floor is dead, unlock movement to the next stop
        if (currentFloor.requiredEnemies.Count == 0)
        {
            UnlockCurrentFloor();
        }
    }

    private void UnlockCurrentFloor()
    {
        isUnlocked = true;
        delayTimer = activationDelay;
    }

    private void FixedUpdate()
    {
        if (!isUnlocked || currentFloorIndex >= floors.Count) return;

        Transform destination = floors[currentFloorIndex].stopPoint;
        if (destination == null) return;

        // Brief pause after the last enemy dies before moving
        if (!isMoving)
        {
            delayTimer -= Time.fixedDeltaTime;
            if (delayTimer <= 0f)
            {
                isMoving = true;
            }
            return;
        }

        // Move toward the current target floor
        transform.position = Vector3.MoveTowards(
            transform.position,
            destination.position,
            moveSpeed * Time.fixedDeltaTime
        );

        // Check if the platform has arrived at this floor's stopPoint
        if (Vector3.Distance(transform.position, destination.position) < 0.01f)
        {
            transform.position = destination.position;
            isMoving = false;
            isUnlocked = false;

            // Advance to the next floor and start listening to its enemies
            SetupFloor(currentFloorIndex + 1);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(null);
        }
    }
}