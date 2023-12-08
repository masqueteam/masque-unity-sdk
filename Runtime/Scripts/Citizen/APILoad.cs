using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

public class APILoad : MonoBehaviour
{
    
    public static APILoad instance;
    private void Start()
    {
        instance = this;


    }
    public IEnumerator GetData(string url, Action<string[]> action)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();
        if(request.result != UnityWebRequest.Result.Success)
        {

        }else
        {
         //   print(request.downloadHandler.text);
            CGetImageAPI Data = JsonUtility.FromJson<CGetImageAPI>(request.downloadHandler.text);

            action(Data.Image);
          
        }


    }
    private bool IsValidUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.Log("URL is null or empty.");
            return false;
        }

        Uri uriResult;
        bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        if (!result)
            Debug.Log("Invalid URL: " + url);

        return result;
    }
    public void GetImage(string url, Action<Texture2D> callback)
    {
        if (!IsValidUrl(url))
        {
            callback(null);
            return;
        }
        StartCoroutine(IGetImage(url, callback));
    }

    // IEnumerator for downloading the image
    private IEnumerator IGetImage(string url, Action<Texture2D> callback)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(request.error);
            callback(null);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            callback(texture);
        }
    }

    bool isLoad = true;
    public IEnumerator LibGetAssetBundle(string URL, Action<GameObject[]> actionGame, Action<float> progress)
    {

        print(">>Incoming >> GetAssetBundle   :  " + URL);
        AssetBundle mBundle = null;
        GameObject[] LoadModel;
        string[] modelAss;
        if (mBundle != null)
        {
            mBundle.Unload(false); //scene is unload from here
        }
        /*
        while (!Caching.ready)
            yield return null;
        */
        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(URL);


        isLoad = true;

        StartCoroutine(Loadprogress(www, progress));

        yield return www.SendWebRequest();
        isLoad = false;


        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("ErrorGetAssetBundle :  " + www.error);
        }
        else
        {
            mBundle = DownloadHandlerAssetBundle.GetContent(www);

            modelAss = mBundle.GetAllAssetNames();
            LoadModel = new GameObject[modelAss.Length];
            for (int i = 0; i < modelAss.Length; i++)
            {
                LoadModel[i] = mBundle.LoadAsset(modelAss[i]) as GameObject;
            }
            print(">>Outgoing DownLoad >" + LoadModel[0].name + "< Succese");
            actionGame(LoadModel);
            mBundle.Unload(false);

        }
        yield return new WaitForEndOfFrame();

    }
    IEnumerator Loadprogress(UnityWebRequest www, Action<float> progress)
    {
        print(">>StartLoadPro");
        while (isLoad)
        {
            progress(www.downloadProgress * 100);


            Debug.Log(string.Format("Progress - {0}%. from {1}", www.downloadProgress * 100, www.url));
            yield return new WaitForSeconds(.25f);
        }

        progress(100);
        print(">>EndLoadPro");
        yield return new WaitForSeconds(.25f);
    }

}

[System.Serializable]
public class CGetImageAPI
{
    public string[] Image;
}
