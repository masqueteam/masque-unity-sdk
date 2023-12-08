using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace MasqueSDK
{
    internal class APICitizen : MonoBehaviour
    {
        // eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzc2lkIjoiODFhZTUyY2YtZTQxOC00ZjQ1LTkyOWEtYzg1YTAyZDE5ZTE2Iiwia2V5IjoibWFzcXVlMTAtMTE3MjA5NTU2NjcxMjg0MjU3Mjg4IiwiaWF0IjoxNzAxODM2NTY1LCJleHAiOjE3MDE5MjI5NjV9.0LsGgGTT8F7IjHVqUi83C_dKRwm0JVEEQ8yFbL4qUXmjSbz41TXooLakUeX4SCiwB-i4z9i2-DtiHNK7TeH-ORRxLAT-JFXvjTZCk4yT3M0nB4rwa2Aot1AoLvKvI-cHFgSYgyTXKtAY8a1V-wKjaXSQtJuj39UUrBoFpI2WQsExEEJTTQJ5eKVbH4yd7sfV0yOzyRQeabH_VnaN5pIbTHqt28-X9rilGXjv_NgvKc-XDRfiRXKqoXxHb5NDd0aLm6N9S8NE6zUscDbV4iIKMNPnmFAI65qnk6XacFZof0BQ12N3Dh6N-ICwBMBBDZLGIJ3Q8fJjAmvM1mpjJJS1og&redirect_uri=https:%2F%2Fmasque-lab.adldigitalservice.com%2Fcitizen%2Fcitizen
        private string myAddress = "0xF01395A25B1B5c77C2D49DbD307a17F683b63501";
        public static APICitizen instance;
        private void Awake()
        {
            instance = this;
        }

        public void GetWalletNFT(Action<string> onComplete)
        {

            StartCoroutine(IEGetWalletNFT(onComplete));
        }
        IEnumerator IEGetWalletNFT(Action<string> onComplete)
        {
            myAddress = Masque.masqueAccountAddress;
            using (UnityWebRequest request = UnityWebRequest.Get("https://masque-lab.adldigitalservice.com/services/citizen/nextclan/wallet/nfts/" + myAddress))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("Error: " + request.error);

                    onComplete?.Invoke("");
                }
                else
                {
                    string responseText = request.downloadHandler.text;
                    Debug.Log(responseText);
                    onComplete?.Invoke(responseText);
                }
            }
        }
    }
}
