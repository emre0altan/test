using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Monopoly.GameBoard;

namespace Monopoly.Login
{
    public class CityUI : MonoBehaviour
    {
        public GameObject glow;
        public Text cityName, cityPrice;
        public int cityLocation;
        public Image cityUpImage;

        private bool selected;
        private RectTransform rectTransform;


        public void UpdateCitySettings(string name, string price, int location)
        {
            cityName.text = name;
            cityPrice.text = "$" + price;
            cityLocation = location;
        }

        private void OnEnable()
        {
            rectTransform = GetComponent<RectTransform>();
            rectTransform.anchoredPosition = BoardManager.Instance.GetLocInfo(0, 0, cityLocation);
            rectTransform.eulerAngles = BoardManager.Instance.GetLocInfo(0, 1, cityLocation);
        }

        public void SelectCity()
        {
            if (!selected)
            {
                if(GameSettingsManager.Instance.firstSelected == null)
                {
                    selected = true;
                    glow.gameObject.SetActive(true);
                    GameSettingsManager.Instance.firstSelected = this;
                }
                else if(GameSettingsManager.Instance.firstSelected != null && GameSettingsManager.Instance.secondSelected == null)
                {
                    selected = true;
                    glow.gameObject.SetActive(true);
                    GameSettingsManager.Instance.secondSelected = this;
                }
                else
                {
                    selected = true;
                    glow.gameObject.SetActive(true);
                    GameSettingsManager.Instance.firstSelected.DisableSelection();
                    GameSettingsManager.Instance.firstSelected = GameSettingsManager.Instance.secondSelected;
                    GameSettingsManager.Instance.secondSelected = this;
                }
            }
            else
            {
                if(GameSettingsManager.Instance.firstSelected == this)
                {
                    selected = false;
                    glow.gameObject.SetActive(false);
                    GameSettingsManager.Instance.firstSelected = GameSettingsManager.Instance.secondSelected;
                    GameSettingsManager.Instance.secondSelected = null;
                }
                else if(GameSettingsManager.Instance.secondSelected == this)
                {
                    selected = false;
                    glow.gameObject.SetActive(false);
                    GameSettingsManager.Instance.secondSelected = null;
                }
            }

            GameSettingsManager.Instance.UpdateCityButtons();
        }

        public void DisableSelection()
        {
            selected = false;
            glow.gameObject.SetActive(false);
        }
    }
}
