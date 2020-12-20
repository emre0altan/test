using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monopoly.Input
{
    public class InputHandler : MonoBehaviour
    {
        public static InputHandler Instance;
        public bool isPressed = false, isReleased = true;

        private void Awake()
        {
            if (Instance)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            TouchManager.Instance.onTouchBegan += PressTouch;
            TouchManager.Instance.onTouchEnded += ReleaseTouch;
        }

        public void PressTouch(TouchInput touchInput)
        {
            isPressed = true;
            isReleased = false;
        }

        public void ReleaseTouch(TouchInput touchInput)
        {
            isPressed = false;
            isReleased = true;
        }
    }
}