using System.Runtime.InteropServices;
using UnityEngine;

namespace MasqueSDK
{
    internal class CitizenPopupScript : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void OpenURLWithCallback(string url);

        public static void OpenPopup(string type, string body)
        {
            string url = $"https://masque-lab.adldigitalservice.com/citizen/popup?data={type}&body={body}";
#if UNITY_WEBGL && !UNITY_EDITOR
        OpenURLWithCallback(url);
#else
            Debug.Log($"Open Popup : {url}");
            Application.OpenURL(url);
#endif
        }
    }
}