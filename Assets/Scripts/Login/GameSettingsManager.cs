using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Monopoly.Login
{
    public class GameSettingsManager : MonoBehaviour
    {
        public static GameSettingsManager Instance;

        public bool isSettingsOpened = false, isChangingBoard = false;
        public Image board, boardMask, editCityUpImage;
        public GameObject boardPanel, editButton, swapButton;
        public InputField editCityName, editCityPrice;
        public CityUI firstSelected, secondSelected;
        public Slider initMoneySlider, mortgageRatioSlider;
        public Text initMoneyText, mortgageRatioText;

        public float lastPinchDelta = 0, initSize = 0, currentSize = 0;

        private Vector3[] cityPoses, cityRots;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;

        }

        private void Start()
        {
            cityPoses = new Vector3[board.rectTransform.childCount];
            cityRots = new Vector3[board.rectTransform.childCount];
            Transform tmpTraa;
            for (int i = 0; i < cityPoses.Length; i++)
            {
                tmpTraa = board.rectTransform.GetChild(i);
                cityPoses[i] = tmpTraa.position;
                cityRots[i] = tmpTraa.eulerAngles;
            }
            if (!PlayerPrefs.HasKey("InitMoney"))
            {
                GameDifficulty(1);
            }
        }


        #region BoardMovement
        public void BoardZoomNMoveInput(TouchInput touchInput)
        {
            if (isChangingBoard)
            {
                ChangeBoardPos(touchInput.DeltaScreenPosition);
            }
        }

        public void ChangeBoardSize(float d)
        {
            float newwScale = board.rectTransform.localScale.x + d * 0.1f;
            if (newwScale < 0.1f) return;
            else if (newwScale > 3f) return;
            board.rectTransform.localScale = new Vector3(newwScale, newwScale, newwScale);
            currentSize = board.rectTransform.sizeDelta.x * newwScale;

            ChangeBoardPos(Vector2.zero);
        }

        public void ChangeBoardPos(Vector2 del)
        {
            Vector2 lastPos = board.rectTransform.anchoredPosition + del;
            if (currentSize < initSize)
            {
                lastPos.x = Mathf.Clamp(lastPos.x, -(initSize - currentSize) * 0.5f, (initSize - currentSize) * 0.5f);
                lastPos.y = Mathf.Clamp(lastPos.y, -(initSize - currentSize) * 0.5f, (initSize - currentSize) * 0.5f);
                board.rectTransform.anchoredPosition = lastPos;
            }
            else
            {
                lastPos.x = Mathf.Clamp(lastPos.x, -(currentSize - initSize) * 0.5f, (currentSize - initSize) * 0.5f);
                lastPos.y = Mathf.Clamp(lastPos.y, -(currentSize - initSize) * 0.5f, (currentSize - initSize) * 0.5f);
                board.rectTransform.anchoredPosition = lastPos;
            }
        }

        public void ResetBoard()
        {
            board.rectTransform.anchoredPosition = Vector3.zero;
            board.rectTransform.localScale = Vector3.one;
            board.rectTransform.eulerAngles = Vector3.zero;
            currentSize = initSize;
        }

        public void RotateBoard(int x)
        {
            Vector3 tmpEu = board.rectTransform.eulerAngles;
            if (x == 1) tmpEu.z += 90;
            else if (x == -1) tmpEu.z -= 90;
            board.rectTransform.eulerAngles = tmpEu;
        }

        public void ChangeBoard()
        {
            ResetBoard();
            boardPanel.SetActive(true);
            isChangingBoard = true;
            TouchManager.Instance.onTouchMoved += BoardZoomNMoveInput;
            initSize = board.rectTransform.sizeDelta.x;
            currentSize = initSize;
        }
        #endregion

        #region CityActions

        public void SwapCities()
        {
            if (firstSelected == null || secondSelected == null) return;
            Vector3 cityPos, cityRot;
            int tmploc = firstSelected.cityLocation;
            firstSelected.cityLocation = secondSelected.cityLocation;
            secondSelected.cityLocation = tmploc;

            cityPos = firstSelected.transform.position;
            cityRot = firstSelected.transform.eulerAngles;
            firstSelected.transform.position = secondSelected.transform.position;
            firstSelected.transform.eulerAngles = secondSelected.transform.eulerAngles;
            secondSelected.transform.position = cityPos;
            secondSelected.transform.eulerAngles = cityRot;
        }

        public void UpdateCityButtons()
        {
            if (firstSelected == null && secondSelected == null)
            {
                editButton.SetActive(false);
                swapButton.SetActive(false);
            }
            else if (firstSelected != null && secondSelected != null)
            {
                editButton.SetActive(false);
                swapButton.SetActive(true);
            }
            else
            {
                editButton.SetActive(true);
                swapButton.SetActive(false);
            }
        }

        public void EditCity()
        {
            CityUI selectedCty;
            if (firstSelected != null) selectedCty = firstSelected;
            else selectedCty = secondSelected;

            editCityName.placeholder.GetComponent<Text>().text = selectedCty.cityName.text;
            editCityPrice.placeholder.GetComponent<Text>().text = selectedCty.cityPrice.text;
            editCityUpImage.color = selectedCty.cityUpImage.color;
        }


        public void UpdateCityWhenEditing()
        {
            CityUI selectedCty;
            if (firstSelected != null) selectedCty = firstSelected;
            else selectedCty = secondSelected;

            selectedCty.UpdateCitySettings(editCityName.text, editCityPrice.text, selectedCty.cityLocation);
        }

        #endregion

        public void UpdateInitMoney()
        {
            PlayerPrefs.SetInt("InitMoney", Mathf.FloorToInt(initMoneySlider.value * 10000));
            initMoneyText.text = Mathf.FloorToInt(initMoneySlider.value * 10000).ToString();
        }

        public void UpdateMortgage()
        {
            PlayerPrefs.SetInt("MortgageRatio", Mathf.FloorToInt(mortgageRatioSlider.value * 100));
            mortgageRatioText.text = "%" + Mathf.FloorToInt(mortgageRatioSlider.value * 100).ToString();
        }

        public void GameDifficulty(int x)
        {
            if(x == 0)
            {
                PlayerPrefs.SetInt("InitMoney", 2000);
                PlayerPrefs.SetInt("MortgageRatio", 70);
                PlayerPrefs.SetInt("GoMoney", 300);
            }
            else if (x == 1)
            {
                PlayerPrefs.SetInt("InitMoney", 1500);
                PlayerPrefs.SetInt("MortgageRatio", 50);
                PlayerPrefs.SetInt("GoMoney", 200);
            }
            else if (x == 2)
            {
                PlayerPrefs.SetInt("InitMoney", 1000);
                PlayerPrefs.SetInt("MortgageRatio", 30);
                PlayerPrefs.SetInt("GoMoney", 100);
            }
        }
    }
}

