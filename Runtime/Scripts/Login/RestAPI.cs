using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace MasqueSDK
{
    internal class RestAPI : MonoBehaviour
    {
        public static IEnumerator GetData(string url, Action<string> action)
        {
            Debug.Log(url);
            UnityWebRequest webRequest = UnityWebRequest.Get(url);

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                Debug.Log(webRequest.downloadHandler.text);
                action(webRequest.downloadHandler.text);
            }
        }

        public static IEnumerator PostData(string url, Action<string> action)
        {
            WWWForm form = new WWWForm();
            form.AddField("individualId", "aridversecu");
            form.AddField("orgId", "test_masque");
            form.AddField("user-unique-id", "userUniqId102");
            form.AddField("accountAddress", "address001");
            form.AddField("redirectUrl", "https://www.ais.th/");

            UnityWebRequest webRequest = UnityWebRequest.Post(url, form);

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                Debug.Log(webRequest.downloadHandler.text);
                action(webRequest.downloadHandler.text);
            }
        }
    }
}