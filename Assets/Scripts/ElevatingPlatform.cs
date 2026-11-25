using System;
using System.Collections.Generic;
using UnityEngine;

public class ElevatingPlatform : MonoBehaviour
{
    public static bool IsPlayerMovementLocked { get; private set; } = false;

    [Serializable]
    public class FloorData
    {
        [Tooltip("Where the platform stops for this floor.")]
        public Transform stopPoint;

        [Tooltip("Enemies that must die on the previous floor before moving to this stopPoint.")]
        public List<FloorEnemy> requiredEnemies = new List<FloorEnemy>();
    }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private float activationDelay = 1.0f;

    [Tooltip("If true, the elevator waits until the player steps onto it after enemies are cleared.")]
    [SerializeField] private bool requirePlayerBoarded = true;

    [Header("Floor Sequence")]
    [SerializeField] private List<FloorData> floors = new List<FloorData>();

    private int currentFloorIndex = 0;
    private bool isUnlocked = false;
    private bool isMoving = false;
    private bool playerOnPlatform = false;
    private float delayTimer = 0f;

    private void Start()
    {
        IsPlayerMovementLocked = false;

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
        UpdateMovementLock();

        FloorData currentFloor = floors[currentFloorIndex];

        foreach (FloorEnemy enemy in currentFloor.requiredEnemies)
        {
            if (enemy != null)
            {
                enemy.OnEnemyDied += HandleEnemyDied;
            }
        }

        if (currentFloor.requiredEnemies.Count == 0)
        {
            UnlockCurrentFloor();
        }
    }

    private void HandleEnemyDied(FloorEnemy deadEnemy)
    {
        deadEnemy.OnEnemyDied -= HandleEnemyDied;

        FloorData currentFloor = floors[currentFloorIndex];
        currentFloor.requiredEnemies.Remove(deadEnemy);

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

        if (!isMoving)
        {
            // Wait until the player steps onto the platform before counting down
            if (requirePlayerBoarded && !playerOnPlatform)
            {
                delayTimer = activationDelay; // Reset timer if they step off early
                return;
            }

            delayTimer -= Time.fixedDeltaTime;
            if (delayTimer <= 0f)
            {
                isMoving = true;
                UpdateMovementLock();
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
            UpdateMovementLock();

            SetupFloor(currentFloorIndex + 1);
        }
    }

    private void UpdateMovementLock()
    {
        IsPlayerMovementLocked = isMoving && playerOnPlatform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnPlatform = true;
            other.transform.SetParent(transform);
            UpdateMovementLock();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnPlatform = false;
            other.transform.SetParent(null);
            UpdateMovementLock();
        }
    }

    private void OnDisable()
    {
        IsPlayerMovementLocked = false;
    }
}