using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MasqueSDK
{
    public class MasqueAvatarTransform : MonoBehaviour
    {
        public HumanBodyBones humanBodyBones;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale = Vector3.one;

        public void Setup(Animator animator)
        {
            transform.parent = animator.GetBoneTransform(humanBodyBones);
            transform.localPosition = position;
            transform.localEulerAngles = rotation;
            transform.localScale = scale;
        }
        void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
                position = transform.localPosition;
            rotation = transform.localEulerAngles;
            scale = transform.localScale = scale;
        }
    }

}
