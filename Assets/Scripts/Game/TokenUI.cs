using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monopoly.Game
{
    public class TokenUI : MonoBehaviour
    {
        public GameObject glow;
        public bool isChecked = false;

        public void TokenClicking()
        {
            if (!TokenSelectionManager.Instance.localPlayerSelected)
            {
                if (isChecked)
                {
                    glow.SetActive(false);
                    isChecked = false;

                }
                else
                {
                    for (int i = 0; i < TokenSelectionManager.Instance.tokens.Length; i++)
                    {
                        TokenSelectionManager.Instance.tokens[i].glow.SetActive(false);
                        TokenSelectionManager.Instance.tokens[i].isChecked = false;
                    }
                    glow.SetActive(true);
                    isChecked = true;
                }
            }
        }
    }
}
