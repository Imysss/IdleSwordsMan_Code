using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

//리소스 접근 통합: Load, Instantiate, Destroy, Preload, Clear 같은 기능을 하나의 클래스로 묶어 중앙 집중식 리소스 관리가 가능
//Addressable 대응 구조: Unity Addressables API를 추상화하여 LoadAsync, LoadAllAsync로 관리 -> Addressables 의존성을 감추고 호출부를 단순화
//풀링 시스템 연계: Instantiate, Destroy에서 풀 매니저와 직접 연결하여 메모리 효율성과 성능을 고려한 구조
//사전 로딩: preload 라벨 기반으로 리소스 일괄 비동기 로딩 -> 겍임 시작 시 필요한 리소스를 선로드하여 로딩 지연 최소화

//장점
//1. 중앙 집중 관리: 모든 리소스 및 해제를 한 곳에서 처리하여 유지보수 용이
//2. 런타임 성능 최적화: Addressables + Object Pooling 결합으로 메모리 효율 향상
//3. 유연한 확장성: T 제네릭과 Sprite 변형 지원으로 다양한 타입 처리 가능
//4. 의도가 명확한 구조: 명시적인 Load, Instantiate, Destroy, Clear 함수 코드 분리로 코드 가독성 높임

public class ResourceManager 
{
    //메모리 상에 로딩된 리소스를 캐싱하여 중복 로드 방지
    Dictionary<string, UnityEngine.Object> _resources = new Dictionary<string, UnityEngine.Object>();
    //preload 완료 여부를 외부에서 확인할 수 있도록 설정
    public bool IsPreloadComplete = false;

    public void Preload()
    {
        Managers.Resource.LoadAllAsync<Object>("preload", (key, count, totalCount) =>
        {
            //preload가 완료되었는지 확인하는 콜백
            if (count == totalCount)
            {
                Debug.Log("Preload Complete");
                IsPreloadComplete = true;
            }
        });
    }

  
    
    public T Load<T>(string key) where T : UnityEngine.Object
    {
        //key 유효성 검사
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("[ResourceManager] Key is null or empty");
            return null;
        }

        //리소스 캐싱 여부 확인
        if (_resources.TryGetValue(key, out Object resource))
        {
            return resource as T;
        }

        //sprite 리소스는 .sprite 확장자 처리
        if (typeof(T) == typeof(Sprite))
        {
            key = key + ".sprite";
            if (_resources.TryGetValue(key, out Object tmp))
            {
                return tmp as T;
            }
        }

        return null;
    }
    
    public GameObject Instantiate(string key, Transform parent = null, bool pooling = false)
    {
        GameObject prefab = Load<GameObject>($"{key}");
        if (prefab == null)
        {
            Debug.Log($"Failed to load prefab {key}");
            return null;
        }
        
        //pooling
        if (pooling)
        {
            return Managers.Pool.Pop(prefab);
        }

        GameObject go = Object.Instantiate(prefab, parent);
        go.name = prefab.name;  //이름 유지 (1), (2) 이런 거 안 생기게
        return go;
    }

    public void Destroy(GameObject go)
    {
        if (go == null)
            return;
        
        //pooling
        if (Managers.Pool.Push(go))
            return;
        
        Object.Destroy(go);
    }

    public void Clear()
    {
        _resources.Clear();
    }

    #region Addressable

    public void LoadAsync<T>(string key, Action<T> callback = null) where T : UnityEngine.Object
    {
        //sprite 리소스 키 보정 처리
        //이유: Addressables에서는 .sprite 리소스를 로드할 때 종종 Texture2D로 인식하는 경우가 있어서 Sprite로 변환해서 사용하는 패턴이 필요함
        string loadKey = key;
        if (key.Contains(".sprite"))
        {
            loadKey = $"{key}[{key.Replace(".sprite", "")}]";
        }

        var asyncOperation = Addressables.LoadAssetAsync<T>(loadKey);
        asyncOperation.Completed += (op) =>
        {
            //이미 로드되어 있으면 재사용
            if (_resources.TryGetValue(key, out Object resource))
            {
                callback?.Invoke(resource as T);
                return;
            }

            //새 리소스 캐싱 후 콜백
            _resources.Add(key, op.Result);
            callback?.Invoke(op.Result);
        };
    }

    public void LoadAllAsync<T>(string label, Action<string, int, int> callback) where T : UnityEngine.Object
    {
        var opHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
        opHandle.Completed += (op) =>
        {
            int loadCount = 0;
            int totalCount = op.Result.Count;

            foreach (var result in op.Result)
            {
                //sprite 타입 별도 처리
                if (result.PrimaryKey.Contains(".sprite"))
                {
                    LoadAsync<Sprite>(result.PrimaryKey, (obj =>
                    {
                        loadCount++;
                        callback?.Invoke(result.PrimaryKey, loadCount, totalCount);
                    }));
                }
                else
                {
                    LoadAsync<T>(result.PrimaryKey, (obj) =>
                    {
                        loadCount++;
                        callback?.Invoke(result.PrimaryKey, loadCount, totalCount);
                    });
                }
            }
        };
    }
    #endregion
    // Addressables 시스템을 통해 Sprite를 비동기로 로드하고, 결과를 콜백으로 반환
    // 로딩된 Sprite는 내부 캐시에 저장되어 중복 요청을 방지
    public void LoadSprite(string spriteName, Action<Sprite> callback)
    {
        // 이미 메모리에 로딩되어 있는 Sprite가 있다면, 캐시에서 즉시 반환
        if (_resources.TryGetValue(spriteName, out Object cached) && cached is Sprite sprite)
        {
            callback?.Invoke(sprite); // 콜백에 캐시된 Sprite 전달
            return;
        }

        // Addressables를 사용해 Sprite 비동기 로딩 시작
        Addressables.LoadAssetAsync<Sprite>(spriteName).Completed += (op) =>
        {
            // 로드 성공 시
            if (op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                _resources[spriteName] = op.Result; // 결과를 캐시에 저장
                callback?.Invoke(op.Result);        // 콜백에 결과 전달
            }
            // 로드 실패 시
            else
            {
                Debug.LogError($"[ResourceManager] Failed to load sprite: {spriteName}"); // 에러 로그 출력
                callback?.Invoke(null); // 실패 시 null 전달
            }
        };
    }

}
