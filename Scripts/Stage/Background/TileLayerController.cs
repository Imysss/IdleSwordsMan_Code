// TileLayerController.cs
using System.Collections.Generic;
using UnityEngine;

public class TileLayerController : MonoBehaviour
{
    [Header("타일 프리팹")]
    [Tooltip("이 레이어에서 사용할 타일 프리팹들을 등록합니다.")]
    public List<GameObject> tilePrefabs;

    [Header("패럴랙스 설정")]
    [Tooltip("속도 배율. 1이면 기본 속도, 0.5면 절반 속도, 0이면 멈춤.")]
    [Range(0f, 2f)]
    public float parallaxMultiplier;

    // 실제 속도 
    private float currentSpeed = 0f;
    private bool isMoving = false;

    private Queue<BackgroundTile> activeTiles = new Queue<BackgroundTile>();
    private float lastTileXPosition = 0f;

    private void Start()
    {
        // 화면 너비보다 넉넉하게, 예를 들어 5개의 타일을 미리 배치합니다.
        for (int i = 0; i < 5; i++)
        {
            SpawnTileAtEnd();
        }
    }

    private void Update()
    {
        if (!isMoving) return;

        // 모든 타일을 왼쪽으로 이동
        foreach (var tile in activeTiles)
        {
            tile.transform.Translate(Vector2.left * currentSpeed * Time.deltaTime);
        }

        // 가장 왼쪽 타일이 화면 밖으로 나갔는지 확인
        BackgroundTile frontTile = activeTiles.Peek();
        // 화면 왼쪽 끝 좌표 + 타일 너비의 절반 = 타일의 중심점. 이 중심점이 화면 밖으로 나갔는지 체크
        if (frontTile.transform.position.x < (-15f - frontTile.GetWidth()))
        {
            RecycleTile();
        }
    }

    void SpawnTileAtEnd()
    {
        GameObject prefabToSpawn = tilePrefabs[Random.Range(0, tilePrefabs.Count)];
        GameObject newTileObject = Instantiate(prefabToSpawn, transform);

        float tileWidth = newTileObject.GetComponent<BackgroundTile>().GetWidth();
        newTileObject.transform.position = new Vector2(lastTileXPosition, transform.position.y);
        
        lastTileXPosition += tileWidth;
        activeTiles.Enqueue(newTileObject.GetComponent<BackgroundTile>());
    }

    void RecycleTile()
    {
        BackgroundTile recycledTile = activeTiles.Dequeue();
        // 위치를 맨 오른쪽으로 이동
        recycledTile.transform.position = new Vector2(lastTileXPosition, transform.position.y);
        lastTileXPosition += recycledTile.GetWidth();
        activeTiles.Enqueue(recycledTile);
    }

    public void SetMovingState(bool state)
    {
        isMoving = state;
    }

    public void UpdateSpeed(float masterSpeed)
    {
        currentSpeed = masterSpeed * parallaxMultiplier;
    }
}