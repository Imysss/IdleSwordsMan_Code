using UnityEngine;

public class WorldCanvas : MonoBehaviour
{
    public static WorldCanvas Instance { get; private set; }

    [field: SerializeField]
    public Canvas UICanvas { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (UICanvas == null)
            UICanvas = GetComponentInChildren<Canvas>();
    }
}