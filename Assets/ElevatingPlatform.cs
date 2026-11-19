using System.Collections.Generic;
using UnityEngine;

public class ElevatingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Where the platform should move to (e.g., an empty GameObject at the top).")]
    [SerializeField] private Transform targetDestination;
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private float activationDelay = 1.0f;

    [Header("Enemies")]
    [Tooltip("Assign all enemies that must be defeated to unlock this elevator.")]
    [SerializeField] private List<Ghoul> Enemies = new List<Ghoul>();

    private Vector3 startPosition;
    private bool isUnlocked = false;
    private bool isMoving = false;
    private float delayTimer = 0f;

    private void Start()
    {
        startPosition = transform.position;

        // Subscribe to the death event of every enemy on this floor
        foreach (Ghoul enemy in Enemies)
        {
            if (enemy != null)
            {
                enemy.OnEnemyDied += HandleEnemyDied;
            }
        }

        // Edge case: If no enemies were assigned, unlock immediately
        if (Enemies.Count == 0)
        {
            UnlockPlatform();
        }
    }

    private void HandleEnemyDied(Ghoul deadEnemy)
    {
        // Unsubscribe to prevent memory leaks and remove from active list
        deadEnemy.OnEnemyDied -= HandleEnemyDied;
        Enemies.Remove(deadEnemy);

        // Check if all enemies on this floor are cleared
        if (Enemies.Count == 0)
        {
            UnlockPlatform();
        }
    }

    private void UnlockPlatform()
    {
        isUnlocked = true;
        delayTimer = activationDelay;
    }

    private void FixedUpdate()
    {
        if (!isUnlocked || targetDestination == null) return;

        // Wait for the brief delay after the last enemy dies so it doesn't feel jarring
        if (!isMoving)
        {
            delayTimer -= Time.fixedDeltaTime;
            if (delayTimer <= 0f)
            {
                isMoving = true;
            }
            return;
        }

        // Smoothly move the platform toward the destination
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetDestination.position,
            moveSpeed * Time.fixedDeltaTime
        );
    }

    // Keep the player moving smoothly with the platform
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