using System;
using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

namespace MasqueSDK
{
    [Serializable]
    public class Token
    {
        public string balance;
        public string tokenAddress;
        public string name;
        public string symbol;
        public string icon_url;
        public string type;
    }

    [Serializable]
    public class AccountData
    {
        public string accountAddress;
        public Token nativeBalances;
        public Token[] tokenBalances;
    }

    internal class APIHandlerToken : MonoBehaviour
    {
        string inquiryUrl = "https://masque-lab.adldigitalservice.com/services/citizen/nextclan/wallet/tokens/";
        public AccountData accountData;

        public void GetData(Action<AccountData> Token)
        {

            StartCoroutine(IGetTokenData(Token));
        }

        IEnumerator IGetTokenData(Action<AccountData> _accountData)
        {
            Debug.Log("Starting IGetTokenData Coroutine");
            string address = Masque.masqueAccountAddress;
            using (UnityWebRequest webRequest = UnityWebRequest.Get(inquiryUrl + address))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Web Request Error: " + webRequest.error);
                }
                else
                {
                    string responseText = webRequest.downloadHandler.text;
                    print("restext ======== " + responseText);
                    accountData = JsonUtility.FromJson<AccountData>(responseText);

                }
                _accountData(accountData);
            }
        }



    }

}
