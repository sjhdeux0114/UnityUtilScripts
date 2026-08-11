using System;
using UnityEngine;


namespace UnityStandardAssets.Utility
{
    public class FollowTarget : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 7.5f, 0f);
        public float Speed = 0.3f;

        private void LateUpdate()
        {
            Vector3 pos = target.position + offset;

            transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * Speed);


        }
    }
}
