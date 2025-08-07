using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Data; //  SaveData 클래스 네임스페이스 사용

public class UserDatabaseManager : MonoBehaviour
{
    // 서버 API URL들
    private string saveUrl = "https://us-central1-team-55ee2.cloudfunctions.net/api/save";
    private string loadUrl = "https://us-central1-team-55ee2.cloudfunctions.net/api/save/load";

    private string deleteUrl = "https://us-central1-team-55ee2.cloudfunctions.net/api/save/delete";

    private Coroutine saveDelayCoroutine;
    private string pendingFirebaseUid;
    private SaveData pendingSaveData;
    

    /// <summary>
    ///  UID + SaveData를 함께 서버에 저장
    /// </summary>
    public IEnumerator SaveDataToServer(string firebaseUid, SaveData saveData)
    {
        string saveDataJson = JsonConvert.SerializeObject(saveData);

        var body = new
        {
            userId = firebaseUid,
            saveData = JsonConvert.DeserializeObject(saveDataJson) // 중첩 JSON 구조 유지
        };

        string jsonBody = JsonConvert.SerializeObject(body);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest(saveUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log($"[Save] 서버 저장 성공: {request.downloadHandler.text}");
        }
        else
        {
            Debug.LogError($"[Save] 서버 저장 실패: {request.error}");
        }
    }

    /// <summary>
    ///  UID로 서버에서 SaveData를 불러오기
    /// </summary>
    public IEnumerator LoadDataFromServer(string userId, System.Action<SaveData> onSuccess)
    {
        var body = new { userId = userId };
        string jsonBody = JsonConvert.SerializeObject(body);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new UnityWebRequest(loadUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[Load] 서버 불러오기 성공: {request.downloadHandler.text}");
            try
            {
                ServerLoadResponse res = JsonConvert.DeserializeObject<ServerLoadResponse>(request.downloadHandler.text);
                onSuccess?.Invoke(res.saveData);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Load] JSON 파싱 실패: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"[Load] 서버 불러오기 실패: {request.error}");
        }
    }

    // 서버 저장을 지연 호출 (3초 Debounce)
    public void RequestSaveWithDelay(string firebaseUid, SaveData saveData)
    {
        // 최신 데이터 기억
        pendingFirebaseUid = firebaseUid;
        pendingSaveData = saveData;

        // 기존 딜레이 코루틴 취소 후 재시작
        if (saveDelayCoroutine != null)
            StopCoroutine(saveDelayCoroutine);

        saveDelayCoroutine = StartCoroutine(DelaySaveRoutine());
    }

    private IEnumerator DelaySaveRoutine()
    {
        yield return new WaitForSeconds(3f); //  저장 딜레이 시간

        if (!string.IsNullOrEmpty(pendingFirebaseUid) && pendingSaveData != null)
        {
            yield return StartCoroutine(SaveDataToServer(pendingFirebaseUid, pendingSaveData));
        }

        saveDelayCoroutine = null;
    }

    /// <summary>
    /// 특정 userId의 세이브 데이터를 서버에서 삭제
    /// </summary>
    public IEnumerator DeleteGuestData(string userId)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", userId);

        UnityWebRequest request = UnityWebRequest.Post(deleteUrl, form);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[Delete] 게스트 데이터 삭제 성공: {userId}");
        }
        else
        {
            Debug.LogError($"[Delete] 게스트 데이터 삭제 실패: {request.error}");
        }
    }


    // 서버 응답 구조 정의
    [System.Serializable]
    public class ServerLoadResponse
    {
        public SaveData saveData;
    }

    [System.Serializable]
    public class ServerResponse
    {
        public string status;
        public string message;
    }
}