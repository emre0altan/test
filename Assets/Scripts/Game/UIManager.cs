using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monopoly.Game
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
        public bool isButtonPressing = false;

        private void Awake()
        {
            if (Instance)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }
    }
}
