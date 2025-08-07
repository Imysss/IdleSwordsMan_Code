using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                if (FindObjectsOfType<T>().Length > 1)
                {
                    Debug.LogError($"[Singleton] 중복 인스턴스 발견: {typeof(T)}");
                }

                if (instance == null)
                {
                    GameObject singletonObject = new GameObject($"(singleton) {typeof(T)}");
                    instance = singletonObject.AddComponent<T>();
                    DontDestroyOnLoad(singletonObject);
                }
            }

            return instance;
        }
    }
}
