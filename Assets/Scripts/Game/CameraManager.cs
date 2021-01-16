using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Monopoly.Input;
using DG.Tweening;
using UnityEngine.UI;

namespace Monopoly.Game
{
    public class CameraManager : MonoBehaviour
    {
        public CinemachineVirtualCamera cinemachineFreeLook;
        public int zoomPhase = 0;
        public Button zoomIn, zoomOut;

        private CinemachineOrbitalTransposer orbitalTransposer;

        private void Start()
        {
            CinemachineCore.GetInputAxis = LookingAroundRoof;
            orbitalTransposer = cinemachineFreeLook.GetCinemachineComponent<CinemachineOrbitalTransposer>();
            ChangeOffset(zoomPhase);
        }

        public void ZoomIn()
        {
            if (zoomPhase >= 2) return;
            zoomPhase++;
            ChangeOffset(zoomPhase);
            if (zoomPhase == 2) zoomIn.interactable = false;
            else if (zoomPhase == 1) zoomOut.interactable = true;
        }

        public void ZoomOut()
        {
            if (zoomPhase <= 0) return;
            zoomPhase--;
            ChangeOffset(zoomPhase);
            if (zoomPhase == 1) zoomIn.interactable = true;
            else if (zoomPhase == 0) zoomOut.interactable = false;
        }

        public void ChangeOffset(int q)
        {
            if(q == 0)
            {
                DOTween.To(() => orbitalTransposer.m_FollowOffset, x => orbitalTransposer.m_FollowOffset = x, new Vector3(-2.46f, 15.8f, -18.31f), 0.2f).SetEase(Ease.Linear);
            }
            else if(q == 1)
            {
                DOTween.To(() => orbitalTransposer.m_FollowOffset, x => orbitalTransposer.m_FollowOffset = x, new Vector3(-2.46f, 11.68f, -17.9f), 0.2f).SetEase(Ease.Linear);
            }
            else if (q == 2)
            {
                DOTween.To(() => orbitalTransposer.m_FollowOffset, x => orbitalTransposer.m_FollowOffset = x, new Vector3(-2.46f, 8.1f, -15.91f), 0.2f).SetEase(Ease.Linear);
            }
        }


        public float LookingAroundRoof(string axisName)
        {
            if (!UIManager.Instance.isButtonPressing && TouchManager.Instance._touch.Phase == TouchInput.TouchPhase.Moved)
            {
                if (axisName == "Mouse X")
                {
                    if (InputHandler.Instance.isPressed)
                    {
                        return UnityEngine.Input.GetAxis("Mouse X");
                    }
                    else
                    {
                        return 0;
                    }
                }
                else if (axisName == "Mouse Y")
                {
                    if (InputHandler.Instance.isPressed)
                    {
                        return UnityEngine.Input.GetAxis("Mouse Y");
                    }
                    else
                    {
                        return 0;
                    }
                }
                return UnityEngine.Input.GetAxis(axisName);
            }
            else return 0;
        }

    }
}