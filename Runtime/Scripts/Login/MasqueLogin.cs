using System;
using System.Collections.Generic;
using UnityEngine;

namespace MasqueSDK
{
    public class MasqueLogin : MonoBehaviour
    {
        string apiGetindvidaID = "https://masque-dev.adldigitalservice.com/api/v3/masque-be/individualId/";
        string apiGetMasqueId = "https://masque-iot.adldigitalservice.com/api/v3/masque-be/masqueId/";

        public List<CApiGetLoginData> IndividualLoginData;
        public CApiGetLoginData loginData;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }
        string GetUrlByIndividualId(string id)
        {
            /*
            if (id.Contains("test"))
            {
                return $"https://masque-lab.adldigitalservice.com/api/media-services/database/test_individualid/{id}.json";
            }
            else
            {
                return apiGetindvidaID + id;
            }
            */
            return apiGetindvidaID + id;
        }
        string GetUrlByMasqueId(string id)
        {
            /*
            if (id.Contains("test"))
            {
                return $"https://masque-lab.adldigitalservice.com/api/media-services/database/test_masqueId/{id}.json";
            }
            else
            {
                return apiGetMasqueId + id;
            }
            */
            return apiGetMasqueId + id;
        }
        public void GetApiLoginDataByIndividualId(string id, Action<List<CApiGetLoginData>> doNext)
        {
            StartCoroutine(RestAPI.GetData(GetUrlByIndividualId(id), (api) =>
            {
                CApiGetIndividualId apiData = JsonUtility.FromJson<CApiGetIndividualId>(api);
                IndividualLoginData = apiData.data;
                Debug.Log(JsonUtility.ToJson(IndividualLoginData, true));
                doNext(IndividualLoginData);
            })
        );
        }

        public void GetApiLoginDataByMasqueId(string id, Action<CApiGetLoginData> doNext)
        {
            StartCoroutine(RestAPI.GetData(GetUrlByMasqueId(id), (api) =>
            {
                CApiGetMasqueId apiData = JsonUtility.FromJson<CApiGetMasqueId>(api);
                loginData = apiData.data;
                Debug.Log(JsonUtility.ToJson(loginData, true));
                doNext(loginData);
            })
        );
        }
    }
    [System.Serializable]
    public class CApiGetIndividualId
    {
        public string resultCode;
        public string resultDescription;
        public List<CApiGetLoginData> data;

    }
    [System.Serializable]
    public class CApiGetMasqueId
    {
        public string resultCode;
        public string resultDescription;
        public CApiGetLoginData data;
    }
    [System.Serializable]
    public class CApiGetLoginData
    {
        public string name;
        public string masqueId;
        public string description;
        public string requestId;
        public string thumbnail;
        public string displayPic;
        public string gltf;
        public string link;
    }
}