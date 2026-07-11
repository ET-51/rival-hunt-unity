using UnityEngine;

/// Spawns gems at random points inside the arena, up to a max alive count.
public class GemSpawner : MonoBehaviour
{
    [SerializeField] private Gem gemPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private float spawnInterval = 0.5f;
    [SerializeField] private int initialGemCount = 12;
    [SerializeField] private int maxAliveGems = 30;
    [SerializeField] private Vector2 arenaHalfSize = new Vector2(8f, 4.5f);

    private float timer;
    private int aliveGems;

    private void Start()
    {
        int count = Mathf.Min(initialGemCount, maxAliveGems);
        for (int i = 0; i < count; i++)
        {
            SpawnGem();
        }
    }

    private void OnEnable()
    {
        Gem.OnGemCollected += HandleGemCollected;
    }

    private void OnDisable()
    {
        Gem.OnGemCollected -= HandleGemCollected;
    }

    private void Update()
    {
        if (MatchController.Instance == null) return;
        if (MatchController.Instance.State == MatchState.Ended) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval && aliveGems < maxAliveGems)
        {
            timer = 0f;
            SpawnGem();
        }
    }

    private void SpawnGem()
    {
        Vector2 pos = new Vector2(
            Random.Range(-arenaHalfSize.x, arenaHalfSize.x),
            Random.Range(-arenaHalfSize.y, arenaHalfSize.y));

        Gem gem = Instantiate(gemPrefab, pos, Quaternion.identity);
        gem.Init(player);
        aliveGems++;
    }

    private void HandleGemCollected()
    {
        aliveGems = Mathf.Max(0, aliveGems - 1);
    }
}
