
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    [Header("Pickup Settings")]
    public GameObject pickupPrefab;
    public int maxPickups = 12;
    public float spawnInterval = 2f;

    [Header("Spawn Area")]
    public Vector2 xRange = new Vector2(-7f, 7f);
    public Vector2 zRange = new Vector2(-7f, 7f);
    public float spawnY = 0.5f;

    [Header("Other")]
    public Transform player;
    public GameObject endGameText;
    public float minPlayerDistance = 2f;
    public float spawnCheckRadius = 0.4f;

    void Start()
    {
        InvokeRepeating(
            nameof(SpawnPickup),
            spawnInterval,
            spawnInterval
        );
    }

    void SpawnPickup()
    {
        if (pickupPrefab == null)
            return;

        // Stop spawning when game ends
        if (endGameText != null &&
            endGameText.activeInHierarchy)
            return;

        // Check current number of pickups
        GameObject[] pickups =
            GameObject.FindGameObjectsWithTag("PickUp");

        if (pickups.Length >= maxPickups)
            return;

        // Find a safe spawn position
        for (int i = 0; i < 20; i++)
        {
            Vector3 spawnPosition = new Vector3(
                Random.Range(xRange.x, xRange.y),
                spawnY,
                Random.Range(zRange.x, zRange.y)
            );

            // Avoid spawning near player
            if (player != null)
            {
                float distance = Vector2.Distance(
                    new Vector2(spawnPosition.x, spawnPosition.z),
                    new Vector2(player.position.x, player.position.z)
                );

                if (distance < minPlayerDistance)
                    continue;
            }

            // Avoid spawning inside objects
            if (Physics.CheckSphere(
                spawnPosition,
                spawnCheckRadius,
                ~0,
                QueryTriggerInteraction.Collide))
            {
                continue;
            }

            // Spawn pickup
            Instantiate(
                pickupPrefab,
                spawnPosition,
                pickupPrefab.transform.rotation
            );

            break;
        }
    }
}
