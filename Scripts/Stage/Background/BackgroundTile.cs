using UnityEngine;

public class BackgroundTile : MonoBehaviour
{
    private float _width;

    private void Awake()
    {
        // 타일의 가로 길이를 계산해서 저장해둠
        _width = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    public float GetWidth()
    {
        return _width;
    }
}