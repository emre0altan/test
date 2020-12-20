using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Monopoly.Input;

namespace Monopoly.Game
{
    public class CameraManager : MonoBehaviour
    {
        private void Start()
        {
            CinemachineCore.GetInputAxis = LookingAroundRoof;
        }


        public float LookingAroundRoof(string axisName)
        {
            if (!UIManager.Instance.isButtonPressing)
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