using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

// 서버(Firebase Functions)와 연동하여 유저의 세이브포인트 저장/불러오기 기능
public class SavePointManager : MonoBehaviour
{
    [Header("설정")]
    // Firebase Functions API 엔드포인트 (POST/GET 공용)
    public string apiBaseUrl = "https://us-central1-team-55ee2.cloudfunctions.net/api/savepoint";

    // 로그인과 연동 가능. 현재는 테스트용 유저 ID
    public string user_id = "testuser";

    [Header("UI")]
    // 상태 메시지 출력용 텍스트
    public TMP_Text debugText;

    // 현재 유저가 마지막으로 저장한 스테이지 ID
    // 외부에서는 읽기만 가능
    // 내부에서만 수정 가능
    public int CurrentStageId { get; private set; } = -1;

    // 세이브 기능: 현재 스테이지 ID를 서버에 저장
    public void SaveStage(int stage_id)
    {
        StartCoroutine(SaveStageCoroutine(stage_id));
    }
    // 서버에 POST 요청을 보내 세이브 데이터를 저장
    IEnumerator SaveStageCoroutine(int stage_id)
    {
        string url = $"{apiBaseUrl}";
        // 전송할 데이터 구조
        SavePointData data = new SavePointData
        {
            userId = user_id,
            stageId = stage_id
        };
        // JSON 직렬화
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            // 서버에 요청 보내기
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Save success");
                if (debugText) debugText.text = "Save success!";
            }
            else
            {
                Debug.LogError($"Save failed: {www.error}");
                if (debugText) debugText.text = $"Save failed: {www.error}";
            }
        }
    }

    // 서버로부터 저장된 스테이지 정보를 불러오기
    public void LoadStage()
    {
        StartCoroutine(LoadStageCoroutine());
    }
    // 서버에 GET 요청을 보내 저장된 스테이지 데이터를 받아옴
    IEnumerator LoadStageCoroutine()
    {
        string url = $"{apiBaseUrl}?userId={user_id}";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;
                SavePointData data = JsonUtility.FromJson<SavePointData>(json);

                CurrentStageId = data.stageId;

                Debug.Log($"Loaded stage: {data.stageId}");
                if (debugText) debugText.text = $"Loaded stage: {data.stageId}";

                // TODO: 스테이지 복구 or 씬 이동 처리 추가
            }
            else
            {
                Debug.LogError($"Load failed: {www.error}");
                if (debugText) debugText.text = $"Load failed: {www.error}";
            }
        }
    }

    // 서버와 통신에 사용할 데이터 구조
    [System.Serializable]
    public class SavePointData
    {
        public string userId;
        public int stageId;
    }

    // UI 버튼에서 호출할 수 있도록 래퍼 메서드 구성
    public void OnClickSaveStage1()
    {
        SaveStage(1001);
    }

    public void OnClickSaveStage2()
    {
        SaveStage(1002);
    }
}