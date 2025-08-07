using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FireFlySpawner : MonoBehaviour
{
    [SerializeField] private RectTransform canvasRect;  //전체 UI 기준 RectTransform
    [SerializeField] private GameObject fireflyPrefab;
    private float spawnInterval = 2f;   //주기적으로 생성
    private int minPerSpawn = 3;        //한 번에 최소 생성 개수
    private int maxPerSpawn = 7;        //최대 생성 개수

    private void Start()
    {
        InvokeRepeating(nameof(SpawnMultipleDusts), 0f, spawnInterval);
    }

    private void SpawnMultipleDusts()
    {
        int count = Random.Range(minPerSpawn, maxPerSpawn + 1);

        for (int i = 0; i < count; i++)
        {
            SpawnOneDust();
        }
    }

    private void SpawnOneDust()
    {
        GameObject firefly =  Managers.Pool.Pop(fireflyPrefab);
        firefly.transform.SetParent(canvasRect);
        //GameObject firefly = Instantiate(fireflyPrefab, canvasRect);
        RectTransform rect = firefly.GetComponentInChildren<RectTransform>();
        
        //랜덤 위치 계산
        Vector2 randomPos = new Vector2(
            Random.Range(-canvasRect.rect.width * 0.5f, canvasRect.rect.width * 0.5f),
            Random.Range(-canvasRect.rect.height * 0.5f, canvasRect.rect.height * 0.5f)
        );
        rect.anchoredPosition = randomPos;
        
        //랜덤 크기, 알파 설정
        float scale = Random.Range(0.3f, 1.2f);
        float alpha = Random.Range(0.2f, 0.8f);
        
        rect.localScale = Vector3.zero;
        
        Image img = firefly.GetComponentInChildren<Image>();
        Color color = img.color;
        img.color = new Color(color.r, color.g, color.b, 0f);
        
        Vector2 moveOffset = new Vector2(Random.Range(-30f, 30f), Random.Range(20f, 60f));
        
        //DOTween 애니메이션 연출
        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOScale(scale, 0.4f).SetEase(Ease.OutSine));
        seq.Join(img.DOFade(alpha, 0.4f));
        seq.Append(rect.DOAnchorPos(rect.anchoredPosition + moveOffset, 2.5f).SetEase(Ease.InOutSine));
        seq.Join(img.DOFade(0f, 2.5f));
        seq.AppendCallback(() =>
        {
            Managers.Resource.Destroy(firefly);
        });
    }
}
