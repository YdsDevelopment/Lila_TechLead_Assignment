using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TicTacToe.Utils
{
    public class CameraViewPortHandler : MonoBehaviour
    {
        private float aspectRatio = 1.0f;
        public float refAspectRation = 0.45f;
        public Camera m_mainCamera;

        public void Awake()
        {
            if (m_mainCamera == null)
            {
                m_mainCamera = GetComponent<Camera>();
            }
            UpdateCameraViewPort();
        }

        public void Start()
        {
            
        }

        [ContextMenu("UpdateView")]
        public void UpdateCameraViewPort()
        {
            if(m_mainCamera == null)
            {
                m_mainCamera = GetComponent<Camera>();
            }
            m_mainCamera.rect = new Rect(0, 0, 1, 1);

            aspectRatio = m_mainCamera.aspect;
            float htmultiplier = (aspectRatio / refAspectRation) * 1.0f;
            Debug.Log("Current Aspect : " + aspectRatio + " Mul : " + htmultiplier + " ref asp " + refAspectRation);
            if (htmultiplier >= 1.00f)
                return;

            Rect refRect = m_mainCamera.rect;
            
            float ht = 1 * htmultiplier;
            Debug.Log("Updated Current Aspect : " + aspectRatio + " Mul : " + htmultiplier + " ref asp " + refAspectRation);

            m_mainCamera.rect = new Rect(refRect.x, (1 - ht)/ 2.0f, refRect.width, ht);
        }
    }
}