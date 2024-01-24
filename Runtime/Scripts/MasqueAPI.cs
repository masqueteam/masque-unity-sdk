using GLTFast;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using UnityEngine;
using UnityEngine.Networking;

namespace MasqueSDK
{
    public class MasqueAPI : MonoBehaviour
    {
        APIHandlerToken handlerToken;
        APIHandlerWallet handlerWallet;

        private static MasqueAPI _instance;
        public static MasqueAPI instance
        {
            get
            {
                if (_instance == null)
                {
                    // สร้าง GameObject ใหม่
                    GameObject apiObject = new GameObject("MasqueSDK");
                    _instance = apiObject.AddComponent<MasqueAPI>();

                    DontDestroyOnLoad(apiObject);
                }
                return _instance;
            }
        }

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this);
                handlerToken = gameObject.AddComponent<APIHandlerToken>();
                handlerWallet = gameObject.AddComponent<APIHandlerWallet>();
                gameObject.AddComponent<APICitizen>();
                gameObject.AddComponent<APILoad>();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void CitizenLogin(Action<string> onComplete, Action<string> onError = null)
        {
            StopAllCoroutines();
            string url = Application.absoluteURL;
            if (string.IsNullOrEmpty(Application.absoluteURL))
            {
                StartCoroutine(CreateLoginSession(onComplete, onError));
            }
            else
            {
                string mtoken = HttpUtility.ParseQueryString(new Uri(url).Query).Get("mtoken");
                if(string.IsNullOrEmpty(mtoken))
                    StartCoroutine(CreateLoginSession(onComplete, onError));
                else
                    StartCoroutine(IEDecode(mtoken, onComplete, onError));
            }
        }
        public void GetMyNFTs(Action<List<DataNFT>> onComplete)
        {
            handlerWallet.GetData((data) =>
            {
                onComplete(data);
            });
        }

        public void GetMyTokens(Action<AccountData> onComplete)
        {
            handlerToken.GetData((Token) =>
            {
                onComplete(Token);
            });
        }

        public GameObject CreateGltfGameObject(string url, Transform parent)
        {
            GameObject gltfPrefabs = Resources.Load<GameObject>("DummyLoadGlft");
            gltfPrefabs.GetComponent<GltfAsset>().Url = url;
            return Instantiate(gltfPrefabs, parent);
        }

        public void CitizenAirDropNXN(Action onComplete)
        {
            CitizenPopupScript.OpenPopup("citizen-airdrop", "");
            onComplete.Invoke();
        }
        public void CitizenOpenTranscation(Action onComplete)
        {
            CitizenPopupScript.OpenPopup("tx", Masque.masqueAccountAddress);
            onComplete.Invoke();
        }
        public void CitizenOpenCitizen(Action onComplete)
        {
            CitizenPopupScript.OpenPopup("citizen", Masque.masqueAccountAddress);
            onComplete.Invoke();
        }
        public void CitizenOpenWallet(Action onComplete)
        {
            CitizenPopupScript.OpenPopup("wallet", Masque.masqueAccountAddress);
            onComplete.Invoke();
        }

        public void CitizenMintUARI(int uari, Action onComplete)
        {
            CitizenPopupScript.OpenPopup("get-uari", uari.ToString());
            onComplete.Invoke();
        }
        public void CitizenBuyNFT(Action onComplete)
        {
            CitizenPopupScript.OpenPopup("buy", "");
            onComplete.Invoke();
        }
        public void GoToVerseByVerseId(string verseId)
        {
            CitizenPopupScript.OpenPopup("verse", verseId);
        }
        public void CitizenTransfer(string sendToAddress, DataNFT dataNFT, Action onComplete)
        {
            JObject data = new JObject();

            data["sendTo"] = sendToAddress;
            data["tokenAddress"] = dataNFT.tokenAddress;
            data["tokenId"] = dataNFT.id;

            CitizenPopupScript.OpenPopup("transfer", data.ToString());
            onComplete();
        }
        public void GetAddressByMasqueId(string masqueId, Action<DataUser> onComplete)
        {
            if (masqueId != "")
            {
                StartCoroutine(IEGetDataUser(masqueId, onComplete));
            }
        }
        public void GetVerseList(Action<List<DataVerse>> onComplete)
        {
            StartCoroutine(IEGetVerseList(onComplete));
        }
        IEnumerator IEGetVerseList(Action<List<DataVerse>> onComplete)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get("https://masque-lab.adldigitalservice.com/services/citizen/verses"))
            {
                List<DataVerse> dataVerses = new List<DataVerse>();
                yield return webRequest.SendWebRequest();
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    JArray jsonArray = JArray.Parse(jsonResponse);

                    foreach (JObject item in jsonArray)
                    {
                        DataVerse dataVerse = new DataVerse()
                        {
                            verseId = (string)item["id"],
                            verseName = (string)item["name"],
                            verseUrl = (string)item["url"],
                        };
                        dataVerses.Add(dataVerse);
                    }
                    Debug.Log(webRequest.downloadHandler.text);
                }
                else
                {
                    Debug.Log(webRequest.error);
                }
                onComplete(dataVerses);
            }
        }
        IEnumerator IEGetDataUser(string masqueId, Action<DataUser> onComplete)
        {
            {
                using (UnityWebRequest webRequest = UnityWebRequest.Get("https://masque-iot.adldigitalservice.com/api/v3/masque-be/masqueid/" + masqueId))
                {
                    // Request and wait for the desired page.
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log(webRequest.downloadHandler.text);
                        string jsonResponse = webRequest.downloadHandler.text;
                        JObject json = JObject.Parse(jsonResponse);


                        if (json["data"]["accountAddress"] != null)
                        {
                            DataUser user = new DataUser()
                            {
                                user_id = (string)json["data"]["masqueId"],
                                address = (string)json["data"]["accountAddress"]

                            };
                            onComplete(user);
                        }
                        else
                        {
                            print("AccountAddress Not Found");
                            onComplete(null);
                        }
                    }
                    else
                    {
                        onComplete(null);
                        print("MasqueID Not Found");
                    }
                }
            }
        }
        IEnumerator IEGetMasqueId(string masqueId, Action onComplete, Action<string> onError)
        {
            {
                using (UnityWebRequest webRequest = UnityWebRequest.Get("https://masque-iot.adldigitalservice.com/api/v3/masque-be/masqueid/" + masqueId))
                {
                    // Request and wait for the desired page.
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log(webRequest.downloadHandler.text);
                        string jsonResponse = webRequest.downloadHandler.text;
                        JObject json = JObject.Parse(jsonResponse);


                        if (json["data"]["accountAddress"] != null)
                        {
                            Masque.masqueAvatarUrl = (string)json["data"]["gltf"];
                            Masque.masqueName = (string)json["data"]["name"];
                            Masque.masqueAccountAddress = (string)json["data"]["accountAddress"];
                            onComplete.Invoke();
                        }
                        else
                        {
                            onError?.Invoke("AccountAddress Not Found");
                            print("AccountAddress Not Found");
                        }
                    }
                    else
                    {
                        onError?.Invoke(webRequest.error);
                        print("MasqueID Not Found");
                    }
                }
            }
        }
        IEnumerator CreateLoginSession(Action<string> onComplete, Action<string> onError)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get("https://masque-lab.adldigitalservice.com/services/citizen/createSession");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = webRequest.downloadHandler.text;
                JObject json = JObject.Parse(jsonResponse);

                string sessionId = json["uid"].ToString();
                string secretToken = json["secretToken"].ToString(); // รับ secret token
                Debug.Log($"Session created with UID: {sessionId}");
                CitizenPopupScript.OpenPopup("startLogin", sessionId);

                yield return CheckLoginStatus(sessionId, secretToken, onComplete, onError);
            }
            else
            {
                onError?.Invoke(webRequest.error);
                Debug.Log($"Error: {webRequest.error}");
            }
        }

        IEnumerator CheckLoginStatus(string sessionId, string secretToken, Action<string> onComplete, Action<string> onError)
        {
            string checkStatusUrl = $"https://masque-lab.adldigitalservice.com/services/citizen/checkLoginStatus?uid={sessionId}&secretToken={secretToken}";

            while (true)
            {
                UnityWebRequest webRequest = UnityWebRequest.Get(checkStatusUrl);
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    JObject json = JObject.Parse(jsonResponse);
                    //Debug.Log($"Login status: {jsonResponse}");
                    string status = json["status"].ToString();
                    if (status == "loggedIn")
                    {
                        string token = json["token"].ToString();
                        //string masqueId = json["masqueId"].ToString();
                        //APICitizen.myAddress = json["accountAddress"].ToString();
                        yield return IEDecode(token, onComplete, onError);
                        break;
                    }
                }
                else
                {
                    Debug.Log($"Error: {webRequest.error}");
                    onError?.Invoke(webRequest.error);
                    break; // หยุด loop หากมีข้อผิดพลาด
                }

                yield return new WaitForSeconds(5); // ตรวจสอบทุก 5 วินาที
            }
        }

        IEnumerator IEDecode(string token, Action<string> onComplete, Action<string> onError)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get("https://masque-lab.adldigitalservice.com/services/citizen/api/decode?mtoken=" + token))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string res = webRequest.downloadHandler.text;
                    //Debug.Log("Login By GameToken : " + res);
                    JObject json = JObject.Parse(res);
                    string masqueId = json["masqueId"].ToString();
                    string accountAddress = json["accountAddress"].ToString();
                    Masque.masqueAvatarUrl = json["gltf"].ToString();
                    Masque.masqueName = json["name"].ToString();
                    Masque.masqueAccountAddress = accountAddress;
                    //Masque.masqueAccountAddress = accountAddress;
                    Masque.masqueId = masqueId;
                    //yield return IEGetMasqueId(masqueId,() => onComplete(token),onError);
                    onComplete.Invoke(token);

                    // Citizen.isConnect = true;
                    // loginShowIDScript.ClickButtonByMasqueId();
                }
                else
                {
                    Debug.Log(webRequest.error);
                    onError?.Invoke(webRequest.error);
                }
            }
        }
    }
    [System.Serializable]
    public class DataVerse
    {
        public string verseId;
        public string verseName;
        public string verseUrl;
    }
    [System.Serializable]
    public class DataUser
    {
        public string name;
        public string user_id;
        public string address;
    }
}
