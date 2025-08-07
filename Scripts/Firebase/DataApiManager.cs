using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Data;

public class DataApiManager
{
    // 공통 메서드 (POST 요청)
    private IEnumerator PostJsonRequest<T>(string url, System.Action<T> onSuccess)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes("{}"));

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            T data = JsonConvert.DeserializeObject<T>(json);
            onSuccess?.Invoke(data);
        }
        else
        {
            Debug.LogError($"[API ERROR] {url} - {request.error}");
        }
    }

    //  SkillData
    public void LoadSkillData(System.Action<SkillDataLoader> onSuccess)
    {
        string url = "https://us-central1-team-55ee2.cloudfunctions.net/api/skill/list";
        CoroutineManager.StartCoroutine(PostJsonRequest(url, onSuccess));
    }

    public void LoadGearData(System.Action<GearDataLoader> onSuccess)
    {
        string url = "https://us-central1-team-55ee2.cloudfunctions.net/api/gear-data/list";
        CoroutineManager.StartCoroutine(PostJsonRequest(url, onSuccess));
    }

    // 이하 동일하게 각 API 정의
}