using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Data;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine.SceneManagement;

public class SaveLoadManager : MonoBehaviour
{
    private string _path;
    public bool hasSaveData { get; private set; }

    public SaveData SaveData { get; private set; }
    
    private string _previousGuestId;  // SaveData 이전 userId 저장용
    private float _saveCooldown = 10f;      // 최소 저장 간격 (초)
    private float _lastSaveTime = -999f;    // 마지막 저장 시각

    //private void Awake()
    private void Start()
    {
        _path = Path.Combine(Application.persistentDataPath, "_saveData.json");
        Load();
#if UNITY_EDITOR

#endif
    }

    public void Save()
    {
        // 타이틀 씬에서는 저장 불가
        if (SceneManager.GetActiveScene().name == "TitleScene") return;
        
        //  0. 저장 쿨타임 적용
        if (Time.time - _lastSaveTime < _saveCooldown)
        {
            Debug.Log("[Save] 쿨타임 이내 호출, 서버 저장 생략");
            return;
        }
        _lastSaveTime = Time.time;

        // 1. SaveData 자체가 null일 경우 저장 생략
        if (SaveData == null)
        {
            Debug.LogWarning("[Save] SaveData가 null입니다. 저장을 생략합니다.");
            return;
        }

        // 2. 로컬 저장
        string json = JsonConvert.SerializeObject(SaveData, Formatting.Indented);
        File.WriteAllText(_path, json);
        //Debug.Log("[Save] 로컬 저장 완료");

        Debug.Log($"[Save] 서버에 전송될 JSON:\n{json}");

        // 3. GuestManager 인스턴스 체크
        if (Managers.Guest == null)
        {
            Debug.LogWarning("[Save] GuestManager.Instance가 아직 초기화되지 않았습니다.");
            return;
        }
        
        string userId = Managers.Guest.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("[Save] UID가 없어 서버 저장 생략");
            return;
        }

        SaveData.userId = userId;
        //Debug.Log($"[Save] 서버 저장 시도 - UID: {userId}");

        // 4. UserDatabaseManager 인스턴스 체크
        
        if (Managers.UserDatabase == null)
        {
            Debug.LogWarning("[Save] UserDatabaseManager.Instance가 아직 초기화되지 않았습니다.");
            return;
        }

        // 5. 서버 저장 코루틴 실행
        StartCoroutine(SaveAndCleanupGuest(userId, SaveData));
    }

    private void Load()
    {
        if (File.Exists(_path))
        {
            string json = File.ReadAllText(_path);
            SaveData = JsonConvert.DeserializeObject<SaveData>(json);
            // 역직렬화 후 currencyData가 null이면 새로 생성
            if (SaveData.currencyData == null)
            {
                Debug.LogWarning("currencyData가 null입니다. 새로 생성합니다.");
                SaveData.currencyData = new CurrencyData();
            }
            hasSaveData = true;
            Debug.Log("저장 데이터 불러오기 완료");

            //  기존 guest ID 저장
            if (SaveData != null && SaveData.userId.StartsWith("guest-"))
                _previousGuestId = SaveData.userId;
        }
        else
        {
            // 저장된 파일이 없으면 새 데이터 생성
            SaveData = new SaveData();
            hasSaveData = false;
            Debug.Log("저장 데이터 없음; 새로운 저장 데이터를 생성합니다.");
        }
    }
    
    // 서버에서 저장 데이터 불러오기
    public void LoadFromServer()
    {
        //string userId = Firebase.Auth.FirebaseAuth.DefaultInstance?.CurrentUser?.UserId;
        // if (!string.IsNullOrEmpty(userId))
        // {
        //     Debug.Log($"[Load] 서버에서 저장 데이터 요청 - UID: {userId}");
        //     
        //     StartCoroutine(Managers.UserDatabase.LoadDataFromServer(userId, ApplyServerSave));
        // }
        // else
        // {
        //     SaveData = new SaveData();
        //     hasSaveData = false;
        //     Debug.LogWarning("[Load] 로그인 UID가 없어 서버 불러오기 생략");
        // }
    }

    public void ApplyServerSave(SaveData serverData)
    {
        SaveData = serverData;
        hasSaveData = true;

        string json = JsonConvert.SerializeObject(SaveData, Formatting.Indented);
        File.WriteAllText(_path, json);

        Debug.Log("서버 저장 데이터 적용 및 로컬 백업 완료");
    }

    private IEnumerator SaveAndCleanupGuest(string userId, SaveData saveData)
    {
        //Debug.Log($"[Debug] SaveAndCleanupGuest 호출됨 - userId: {userId}");
        // 1. 서버 저장 
        yield return StartCoroutine(Managers.UserDatabase.SaveDataToServer(userId, saveData));

        //Debug.Log($"[Debug] _previousGuestId: {_previousGuestId}, 현재 userId: {userId}");

        // 2. 이전 게스트 ID와 현재 UID가 다르면, 게스트 삭제 시도
        if (!string.IsNullOrEmpty(_previousGuestId) && _previousGuestId != userId)
        {
            Debug.Log($"[마이그레이션] 이전 게스트 데이터 삭제 시도: {_previousGuestId}");
            yield return StartCoroutine(Managers.UserDatabase.DeleteGuestData(_previousGuestId));
            _previousGuestId = null; // 삭제 후 초기화
        }
    }

    public void DeleteSaveData()
    {
        if (File.Exists(_path))
        {
            string json = File.ReadAllText(_path);
            File.Delete(_path);
        }
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            // 백그라운드로 갈 때 저장
            Save();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            // 포커스 잃을 때 저장
            Save();
        }
    }
}
