﻿using GLTFast;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace MasqueSDK
{
    [RequireComponent(typeof(Animator))]
    public class MasqueAvatarLoader : MonoBehaviour
    {
        static internal Dictionary<string, GltfImport> avatarTemp = new Dictionary<string, GltfImport>();
        public bool isLoadByLoginData = true;
        public string masqueAvatarUrl;
        public UnityEvent onLoading;
        public UnityEvent<GameObject> onComplete;
        public UnityEvent onFail;
        //public UnityEvent<string> onNameUpdate;

        public enum Mode
        {
            ShowSomeObject, HideSomeObject
        }
        public Mode mode = Mode.ShowSomeObject;
        public GameObject[] selectObject;

        public IEnumerator Start()
        {/*
            if (!isLoadByLoginData)
              LoadAvatar();*/
            if (isLoadByLoginData)
            {
                Debug.Log("Auto Load Avatar");
                while (string.IsNullOrEmpty(Masque.masqueAvatarUrl))
                    yield return null;

                masqueAvatarUrl = Masque.masqueAvatarUrl;
                //onNameUpdate?.Invoke(Masque.masqueName);
                LoadAvatar(masqueAvatarUrl);
            }
            //else
            //    LoadAvatar("https://models.readyplayer.me/63da89bc9b552e12bccafb58.glb");
        }

        public void LoadAvatar(string url, bool isUseTemp = true)
        {
            onLoading?.Invoke();
            ClearChild();
            masqueAvatarUrl = url;
            GltfLoad(masqueAvatarUrl, transform, isUseTemp);
        }

        /*
        public void SetAvatarName(string avatarName)
        {
            onNameUpdate?.Invoke(avatarName);
        }
        */

        /*
        async void GltfLoad(string url, Transform parent)
        {
            var gltfImport = new GltfImport();
            await gltfImport.Load(url);
            var instantiator = new GameObjectInstantiator(gltfImport, parent);

            Debug.Log("instantiator : "+ instantiator);
            var success = await gltfImport.InstantiateMainSceneAsync(instantiator);

            if (success)
            {
                if (model != null)
                    Destroy(model);
                model = instantiator.SceneTransform.gameObject;
 
                GenerateAnimator();
                onComplete?.Invoke();
            }
            else
            {
                onFail?.Invoke();
                Debug.LogError("Loading glTF failed!");
            }
        }
        */

        private CancellationTokenSource cancellationTokenSource;

        async void GltfLoad(string url, Transform parent, bool isUseTemp)
        {
            cancellationTokenSource = new CancellationTokenSource();
            GltfImport gltfImport = null;

            if (avatarTemp.ContainsKey(url))
            {
                gltfImport = avatarTemp[url];
            }
            else
            {
                gltfImport = new GltfImport();
                try
                {
                    avatarTemp[url] = gltfImport;  // เก็บ GltfImport ลงแคชเมื่อโหลดสำเร็จ
                    await gltfImport.Load(url, null, cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    Debug.Log($"Load glTF from URL {url} was canceled.");
                    return;
                }
                catch (Exception e)
                {
                    Debug.Log($"Failed to load glTF from URL {url}. Error: {e.Message}");
                    Failed();
                    return;
                }
            }

            InstantiationSettings setting = new InstantiationSettings
            {
                SceneObjectCreation = SceneObjectCreation.Always
            };
            var instantiator = new GameObjectInstantiator(gltfImport, parent, null, setting);

            bool success;
            try
            {
                while (!gltfImport.LoadingDone)
                    await Task.Yield();

                success = await gltfImport.InstantiateMainSceneAsync(instantiator, cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"Instantiation of glTF from URL {url} was canceled.");
                return;
            }
            catch (Exception e)
            {
                Debug.Log($"Failed to instantiate glTF from URL {url}. Error: {e.Message}");
                Failed();
                return;
            }

            if (success)
            {
                if (model != null)
                    Destroy(model);

                model = instantiator.SceneTransform.gameObject;
                StartCoroutine(GenerateAnimator());
            }
            else
            {
                Failed();
            }
        }

        private void OnDisable()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
            }
        }

        void Complete()
        {
            try
            {
                onComplete?.Invoke(model);
            }
            catch (Exception e)
            {
                Debug.Log($"OnCompleted failed! Error: {e.Message}");
            }
        }
        void Failed()
        {
            try
            {
                onFail?.Invoke();
            }
            catch (Exception e)
            {
                Debug.Log($"OnFailed! Error: {e.Message}");
            }
        }

        MasqueAvatarTransform[] masqueAvatarTransforms;
        void ClearChild()
        {
            masqueAvatarTransforms = transform.GetComponentsInChildren<MasqueAvatarTransform>(true);
            /*
            for (int i = 0; i < masqueAvatarTransforms.Length; i++)
            {
                masqueAvatarTransforms[i].transform.parent = null;
            }
            */

            /*
            SkinnedMeshRenderer[] skin = transform.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < skin.Length; i++)
            {
                skin[i].gameObject.SetActive(false);
            }
            */

            /*
            foreach (Transform child in transform)
            {
                // Destroy(child.gameObject);
                child.gameObject.SetActive(false);
            }
            */



            if (mode == Mode.ShowSomeObject)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(false);
                }
                for (int i = 0; i < selectObject.Length; i++)
                {
                    selectObject[i].SetActive(true);
                }
            }
            else
            {
                for (int i = 0; i < selectObject.Length; i++)
                {
                    selectObject[i].SetActive(false);
                }
            }

        }
        GameObject model;

        IEnumerator GenerateAnimator()
        {
            Debug.Log("Start Generate :" + model.name);
            //model = transform.Find("Scene").gameObject;

            //if has animation destroy it.
            Animation animation = GetComponentInChildren<Animation>();
            if (animation)
            {
                Destroy(animation);
            }

            if (masqueAvatarUrl.Contains("https://masque-lab.adldigitalservice.com") || masqueAvatarUrl.Contains("https://masque-dev.adldigitalservice.com") || masqueAvatarUrl.Contains("https://masque-iot.adldigitalservice.com"))
            {
                if (model.transform.Find("bone_masque0_root/hips_ARI"))
                    yield return SetupAnimator("Avatars/MasqueAvatar_ARI");
                else if (model.transform.Find("bone_masque0_root/hips_ARI_M"))
                    yield return SetupAnimator("Avatars/MasqueAvatar_ARI_M");
                else if (model.transform.Find("bone_masque0_root/hips/spine.001") || model.transform.Find("amature_masque0/hips/spine.001"))
                    yield return SetupAnimator("Avatars/MasqueAvatar_CU");
                else if (model.transform.Find("bone_masque0_root/mixamorig:Hips"))
                    yield return SetupAnimator("Avatars/MasqueAvatar_Demo");
                else if (model.transform.Find("bone_masque0_root/hips/spine/chest"))
                    yield return SetupAnimator("Avatars/MasqueAvatar_Stranded");
                else
                    yield return SetupAnimator("Avatars/MasqueAvatar_NewCollection01");
            }
            /*
            if (masqueAvatarUrl.Contains("http://20.212.54.87:4006/public/3D/masque-gltf/") || masqueAvatarUrl.Contains("https://masque-dev.adldigitalservice.com"))
            {
                if (model.transform.Find("bone_masque0_root/hips/spine.001"))
                    SetupAnimator("Avatars/MasqueAvatar_CU");
                else if (model.transform.Find("bone_masque0_root/mixamorig:Hips"))
                    SetupAnimator("Avatars/MasqueAvatar_Demo");
                else if (model.transform.Find("bone_masque0_root/hips/spine"))
                    SetupAnimator("Avatars/MasqueAvatar_Stranded");
            }
            */
            else // readyme
            {
                //model.SetActive(false);
                model.transform.localScale = Vector3.one * 0.8f;
                string jsonUrl = masqueAvatarUrl.Replace("glb", "json");
                yield return DownloadMetaData(jsonUrl);
            }


            Complete();
        }

        IEnumerator SetupAnimator(string avatarPath)
        {


            Avatar animationAvatar = Resources.Load<Avatar>(avatarPath);
            Debug.Log(animationAvatar.name);
            Animator animator = GetComponent<Animator>();

            Transform rootBone = animator.GetBoneTransform(HumanBodyBones.Hips);
            rootBone.name = "hide_root_bone";

            //animator.runtimeAnimatorController = animatorController;
            animator.avatar = animationAvatar;
            //animator.applyRootMotion = false;

            for (int i = 0; i < masqueAvatarTransforms.Length; i++)
            {
                masqueAvatarTransforms[i].Setup(animator);
            }

            //if (sourceGameObject)
            //    sourceGameObject.SetActive(false);
            //StartCoroutine(Reload(animator));

            model.SetActive(false);
            yield return null;

            animator.Rebind();

            model.SetActive(true);
            yield return null;


            if (headPoint)
            {
                Vector3 headPos = headPoint.position;
                headPos.y = animator.GetBoneTransform(HumanBodyBones.Head).position.y;
                headPoint.position = headPos;
            }

        }
        public Transform headPoint;
        /*
        IEnumerator Reload(Animator animator)
        {
            model.SetActive(false);
            yield return null;
            animator.Rebind();
            model.SetActive(true);
        }
        */
        public IEnumerator DownloadMetaData(string url)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("Failed Download metadata.");
                }
                else
                {
                    LoadMetaData(request.downloadHandler.text);
                }
            }
        }
        public void LoadMetaData(string json)
        {
            var metaData = JsonConvert.DeserializeObject<AvatarMetadata>(json);

            // TODO: when metadata for half-body avatars are fixed, make the check
            // if (metaData.OutfitGender == OutfitGender.None || metaData.BodyType == BodyType.None)
            if (metaData.BodyType == BodyType.None)
            {
                //OnFailed?.Invoke(FailureType.MetadataParseError, "Failed to parse metadata.");
                Debug.Log("Failed to parse metadata.");
            }
            else
            {
                string path;
                if (metaData.OutfitGender == OutfitGender.Feminine)
                {
                    path = "Avatars/FeminineAnimationAvatar";
                }
                else if (metaData.OutfitGender == OutfitGender.Masculine)
                {
                    path = "Avatars/MasculineAnimationAvatar";
                }
                else
                {
                    Debug.Log("RPM Avatar Error");
                    return;
                }
                model.SetActive(true);
                SetupAnimator(path);
            }
        }
    }

    internal struct AvatarMetadata
    {
        public BodyType BodyType;
        public OutfitGender OutfitGender;
    }
    internal enum BodyType
    {
        None,
        [Description("fullbody")]
        FullBody,
        [Description("halfbody")]
        HalfBody
    }

    internal enum OutfitGender
    {
        None,
        [Description("masculine")]
        Masculine,
        [Description("feminine")]
        Feminine,
        [Description("neutral")]
        Neutral
    }

}