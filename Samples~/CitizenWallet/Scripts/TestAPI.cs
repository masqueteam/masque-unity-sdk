using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;
using System;
using MasqueSDK;

public class TestAPI : MonoBehaviour
{
    [Header("UI web3Login")]
    public GameObject root_web3;
    public GameObject button_login;
    [Header("UI Token")]
    public TMP_Dropdown dropdown_nft;
    public TMP_Text text_nxn;
    public TMP_Text text_tokens;
    public TMP_Text text_accountAddress;
    public RawImage imageNFTShow;
    public List<DataNFT> metadataList = new List<DataNFT>();
 
    int nftIndex = 0;
    public Transform transform_show;
    GameObject petNFT;

    public int idPet;


    [Header("UI Transfer")]
    public InputField inputField_Transfer_MasqueID;
    public TMP_Text text_token;
    public GameObject gameObject_Transfer_button;
    [Header("UI Exchange")]
    public InputField inputField_Transfer_Exchange;
    public void StartTokens()
    {
        MasqueAPI.instance.GetMyTokens((accountData) =>
        {
            text_accountAddress.text = $"accountAddress={accountData.accountAddress}";
            text_nxn.text = $"{accountData.nativeBalances.name}={accountData.nativeBalances.balance}";
            string tokens = "";
            foreach (var token in accountData.tokenBalances) {
                tokens += $"{token.name}={token.balance}  \n";
            }
            text_tokens.text = tokens;

        });
    }

    private void OnEnable()
    {
        root_web3.SetActive(false);
        Login();
    }
    public void Login()
    {
        button_login.SetActive(false);
        MasqueAPI.instance.CitizenLogin((token)=> {
            print(token);
            root_web3.SetActive(true);
            button_login.SetActive(false);
        });
    }
    public void StartNFTs()
    {
        MasqueAPI.instance.GetMyNFTs(dataNFTs =>
        {
            metadataList = dataNFTs;
            dropdown_nft.ClearOptions();
            List<TMP_Dropdown.OptionData> optionDataList = new List<TMP_Dropdown.OptionData>(); ;
            foreach (var dataNFT in dataNFTs)
            {
                Sprite sprite = Sprite.Create(dataNFT.png, new Rect(0.0f, 0.0f, dataNFT.png.width, dataNFT.png.height), new Vector2(0.5f, 0.5f), 100.0f);
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData("NFT ID #" + dataNFT.id, sprite);
                optionDataList.Add(optionData);

            }
            dropdown_nft.AddOptions(optionDataList);
            if(dataNFTs.Count > 0)
            OnDropdownChange(0);
        });
    }
    public void OnDropdownChange(int index)
    {
        nftIndex = index;
        if (petNFT != null) Destroy(petNFT);
        petNFT = MasqueAPI.instance.CreateGltfGameObject(metadataList[nftIndex].gltf, transform_show);
        imageNFTShow.texture = metadataList[nftIndex].png;

    }
    string sendToAddress;
    public void ButtonFindUser()
    {
        MasqueAPI.instance.GetAddressByMasqueId(inputField_Transfer_MasqueID.text, dataUser =>
        {
            if(dataUser != null)
            {
                sendToAddress = dataUser.address;
                text_token.text = $"address = {dataUser.address}\n\nuser_id = {dataUser.user_id}";
                gameObject_Transfer_button.SetActive(true);
            }
            else
            {
                text_token.text  = "not found ";
                gameObject_Transfer_button.SetActive(false);
            }
        });
    }
    public TMP_Text text_verses;
    public void ButtonGetVerseList()
    {
        MasqueAPI.instance.GetVerseList((dataVerses) => {
            string verses = "";
            foreach (var dataVerse in dataVerses)
            {
                verses += $"ID={dataVerse.verseId} Name={dataVerse.verseName}\n";
            }
            text_verses.text = verses;
        });
    }
    public void ButtonOpenCitizen()
    {
        MasqueAPI.instance.CitizenOpenCitizen(() => print("opencitizen"));
    }

    public void ButtonOpenWallet()
    {
        MasqueAPI.instance.CitizenOpenWallet(() => print("openwallet"));
    }
    public void ButtonTranfer()
    {
        MasqueAPI.instance.CitizenTransfer(sendToAddress, metadataList[nftIndex],()=> print("Tranfer"));
    }
    public InputField verse_id;
    public void ButtonGoToVerseId()
    {
        MasqueAPI.instance.GoToVerseByVerseId(verse_id.text);
    }

    public void ButtonExchange()
    {
        MasqueAPI.instance.CitizenMintUARI(int.Parse(inputField_Transfer_Exchange.text), ()=> print("Exchange"));
    }
    public void ButtonBuyNFT()
    {
        MasqueAPI.instance.CitizenBuyNFT(()=> print("BuyNFT"));
    }
    public void ButtonAirDrop()
    {
        MasqueAPI.instance.CitizenAirDropNXN(() => print("AirDrop"));
    }
    public void ButtonTranscation()
    {
        MasqueAPI.instance.CitizenOpenTranscation(() => print("Transcation"));
    }
}
