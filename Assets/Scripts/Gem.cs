using System;
using UnityEngine;

/// A collectible XP gem. Pulls toward the player when close, grants XP on contact.
[RequireComponent(typeof(Collider2D))]
public class Gem : MonoBehaviour
{
    [SerializeField] private float xpValue = 3f;
    [SerializeField] private float magnetRadius = 1.5f;
    [SerializeField] private float magnetSpeed = 8f;

    // Static event: the RivalSimulator listens to know when the player "starves" it.
    public static event Action OnGemCollected;

    private Transform player;
    private Vector3 baseScale;
    private bool collected;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    // The spawner passes the player reference in, so gems don't search the scene.
    public void Init(Transform playerTransform)
    {
        player = playerTransform;
    }

    private void Update()
    {
        if (player == null) return;

        float pulse = 1f + Mathf.Sin(Time.time * 5f) * 0.08f;
        transform.localScale = baseScale * pulse;

        // Simple magnet: drift toward the player inside the radius.
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist < magnetRadius)
        {
            transform.position = Vector2.MoveTowards(
                transform.position, player.position, magnetSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player")) return;

        XPSystem playerXp = other.GetComponent<XPSystem>();
        if (playerXp == null || MatchController.Instance == null) return;

        collected = true;

        // MatchController is the single source for race-wide XP modifiers.
        float finalValue = xpValue * MatchController.Instance.PlayerGemMultiplier;
        playerXp.AddXp(finalValue);

        OnGemCollected?.Invoke();
        Destroy(gameObject);
    }
}
