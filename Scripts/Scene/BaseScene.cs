using UnityEngine;

public abstract class BaseScene : MonoBehaviour
{
    public Define.Scene SceneType { get; protected set; } = Define.Scene.Unknown;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Init();
    }

    protected virtual void Init()
    {

    }

    public abstract void Clear();
}
