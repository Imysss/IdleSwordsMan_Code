using UnityEngine;

public class GoldDummyEffectController : MonoBehaviour
{
    [SerializeField] private GameObject goldDropPrefab;

    public void DropGoldOnHit()
    {
        int dropCount = 20; // 보스 처치 시 20개 한 번에 뿌림
        for (int i = 0; i < dropCount; i++)
        {
            SpawnGold();
        }
    }

    public void DropGoldBurst(int count = 20)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnGold();
        }
    }

    private void SpawnGold()
    {
        GameObject gold = Managers.Pool.Pop(goldDropPrefab);
        if (gold != null)
        {
            gold.transform.position = transform.position;

            // 팡팡 튀는 이펙트
            Rigidbody2D rb = gold.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                float xForce = Random.Range(-2f, 2f);
                float yForce = Random.Range(2.5f, 4f);
                rb.linearVelocity = new Vector2(xForce, yForce);
            }

            gold.GetComponent<GoldDrop>().Init();
        }
    }
}