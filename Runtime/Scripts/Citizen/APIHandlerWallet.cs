using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class TokenOfOwner
{
    public string tokenId;
    public string uriData;
}

[Serializable]
public class NFTBalance
{
    public string balance;
    public string tokenAddress;
    public string name;
    public string symbol;
    public string icon_url;
    public string type;
    public TokenOfOwner[] tokenOfOwner;
}

[Serializable]
public class NewApiResponse
{
    public string accountAddress;
    public NFTBalance[] nftBalances;
}

[Serializable]
public class NFTMetadata
{
    public string name;
    public string description;
    public string image;
    public string gltf;
    public Attribute[] attributes;
    public string tokenAddress;
}

[Serializable]
public class Attribute
{
    public string trait_type;
    public string value;
    public string display_type;
    public string description;
}

namespace MasqueSDK
{
    internal class APIHandlerWallet : MonoBehaviour
    {
        public List<DataNFT> metadataList = new List<DataNFT>();
        int currentNumber = 0;
        // public static APIHandlerWallet instance;


        Action<List<DataNFT>> last_done;

        string metadata;
        public void GetData(Action<List<DataNFT>> done)
        {
            StopAllCoroutines();
            last_done = done;
            Debug.Log("เริ่มดึงข้อมูลจาก API...");

            APICitizen.instance.GetWalletNFT((data) =>
            {
                if (metadata == data)
                {
                    Debug.Log("metadata 1");
                    last_done.Invoke(metadataList);
                    return;
                }
                metadataList.Clear();
                metadata = data;
                NewApiResponse api = JsonUtility.FromJson<NewApiResponse>(data);
                if (api == null)
                {
                    Debug.Log("metadata 1");
                    last_done.Invoke(metadataList);
                }

                else
                    StartCoroutine(ProcessNFTMetadataSequentially(api));
            });
        }

        public IEnumerator IGetData(string url, string tokenId, string mytokenAddress, Action<DataNFT> action)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"มีข้อผิดพลาดในการดึงข้อมูล: {request.error}");
            }
            else
            {
                // Debug.Log($"ได้รับข้อมูลจาก {url} สำเร็จ");
                NFTMetadata metadata = JsonUtility.FromJson<NFTMetadata>(request.downloadHandler.text);

                currentNumber++;
                DataNFT newItem = new DataNFT
                {
                    name = metadata.name,
                    id = tokenId,
                    description = metadata.description,
                    tokenAddress = mytokenAddress,
                    attributes = metadata.attributes,
                    gltf = metadata.gltf,

                };



                string imageUrl = metadata.image;
                if (Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute)) // ตรวจสอบว่า URL ถูกต้องหรือไม่
                {
                    bool isDone = false;

                    APILoad.instance.GetImage(imageUrl, (texture) =>
                    {
                        if (texture != null)
                        {

                            Debug.Log("Load Image Complete" + imageUrl);
                            newItem.png = texture;
                        }
                        else
                        {
                            Debug.Log($"Failed to load texture from {imageUrl}");
                        }
                        isDone = true;
                    });

                    while (isDone == false)
                    {
                        yield return null;
                    }
                }

                action?.Invoke(newItem);

            }




        }

        private IEnumerator ProcessNFTMetadataSequentially(NewApiResponse data)
        {
            Debug.Log("กำลังประมวลผลข้อมูลกระเป๋า...");
            foreach (var nftBalance in data.nftBalances)
            {
                TokenOfOwner[] tokenList = nftBalance.tokenOfOwner;
                string tokenAddress = nftBalance.tokenAddress;
                for (int currentIndex = 0; currentIndex < tokenList.Length; currentIndex++)
                {

                    TokenOfOwner owner = tokenList[currentIndex];

                    yield return StartCoroutine(IGetData(owner.uriData, owner.tokenId, tokenAddress, (nftData) => metadataList.Add(nftData)));

                }
            }

            Debug.Log("Load All Data Inventory Complete");
            last_done.Invoke(metadataList);

        }

    }
}
[System.Serializable]
public class DataNFT
{
    public string name;
    public string id;
    public string description;
    public string tokenAddress;

    public Attribute[] attributes;
    public string gltf;
    public Texture2D png;
}
