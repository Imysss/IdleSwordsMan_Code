using System.Collections;
using UnityEngine;

public class FireworkTest : MonoBehaviour
{
    public GameObject projectilePrefab;
    public GameObject explosionPrefab;

    public void LaunchFireworks()
    {
        StartCoroutine(LaunchSequence());
    }

    private IEnumerator LaunchSequence()
    {
        // 화면 기준 위치 계산
        Vector3 center = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));

        Vector3[] starts = new Vector3[]
        {
            Camera.main.ViewportToWorldPoint(new Vector3(0.1f, 0.1f, 10f)),
            Camera.main.ViewportToWorldPoint(new Vector3(0.9f, 0.1f, 10f)),
            Camera.main.ViewportToWorldPoint(new Vector3(0.15f, 0.1f, 10f)),
            Camera.main.ViewportToWorldPoint(new Vector3(0.85f, 0.1f, 10f)),
        };

        Vector3[] targets = new Vector3[]
        {
            center + new Vector3(-1.2f, 2.5f, 0f),
            center + new Vector3(1.2f, 2.5f, 0f),
            center + new Vector3(-0.6f, 2.7f, 0f),
            center + new Vector3(0.6f, 2.7f, 0f),
        };

        for (int i = 0; i < starts.Length; i++)
        {
            SpawnFirework(starts[i], targets[i]);
            yield return new WaitForSeconds(0.2f); // 순차 발사 간격
        }
    }

    private void SpawnFirework(Vector3 origin, Vector3 target)
    {
        GameObject proj = Instantiate(projectilePrefab, origin, Quaternion.identity);
        proj.GetComponent<ParticleSystem>().Play();
        StartCoroutine(MoveAndExplode(proj, target));
    }

    private IEnumerator MoveAndExplode(GameObject proj, Vector3 target)
    {
        float duration = 1.0f;
        float time = 0f;
        Vector3 start = proj.transform.position;

        while (time < duration)
        {
            time += Time.deltaTime;
            proj.transform.position = Vector3.Lerp(start, target, time / duration);
            yield return null;
        }

        Destroy(proj);

        GameObject explode = Instantiate(explosionPrefab, target, Quaternion.identity);
        explode.GetComponent<ParticleSystem>().Play();
    }
}
