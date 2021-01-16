using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Monopoly.Login;
using Photon.Pun;
using Monopoly.Game;

namespace Monopoly.GameBoard
{
    [Serializable]
    public struct CityInfo
    {
        public string cityName;
        public int cityPrice, location;
    }

    public class BoardManager : MonoBehaviourPunCallbacks
    {
        public static BoardManager Instance;

        public CityInfo[] defaultCityInfos;
        public Vector3[] cityUIPoses, cityUIRots, cityGamePoses, cityGameRots;
        public CityUI[] settingsCityUIs;
        public GameCity[] inGameCities;

        private CityInfo[] currentCityInfos;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            if (!PhotonNetwork.IsMasterClient) gameObject.SetActive(false);
        }

        private void Start()
        {
            currentCityInfos = new CityInfo[22];
            if (!PlayerPrefs.HasKey("BoardCityName0"))
            {
                for(int i = 0; i < 22; i++)
                {
                    PlayerPrefs.SetString("BoardCityName" + i.ToString(), defaultCityInfos[i].cityName);
                    PlayerPrefs.SetInt("BoardCityPrice" + i.ToString(), defaultCityInfos[i].cityPrice);
                    PlayerPrefs.SetInt("BoardCityLocation" + i.ToString(), defaultCityInfos[i].location);

                    currentCityInfos[i].cityName = defaultCityInfos[i].cityName;
                    currentCityInfos[i].cityPrice = defaultCityInfos[i].cityPrice;
                    currentCityInfos[i].location = defaultCityInfos[i].location;

                    settingsCityUIs[i].UpdateCitySettings(defaultCityInfos[i].cityName, defaultCityInfos[i].cityPrice.ToString(), defaultCityInfos[i].location);
                }
            }
            else
            {
                if(currentCityInfos == null)
                {
                    currentCityInfos = new CityInfo[22];
                }

                for (int i = 0; i < 22; i++)
                {
                    currentCityInfos[i].cityName = PlayerPrefs.GetString("BoardCityName" + i.ToString());
                    currentCityInfos[i].cityPrice = PlayerPrefs.GetInt("BoardCityPrice" + i.ToString());
                    currentCityInfos[i].location = PlayerPrefs.GetInt("BoardCityLocation" + i.ToString());

                    settingsCityUIs[i].UpdateCitySettings(currentCityInfos[i].cityName, currentCityInfos[i].cityPrice.ToString(), currentCityInfos[i].location);
                }
            }
        }

        public Vector3 GetLocInfo(int type, int locType, int index)
        {
            int newIndex = 0;
            for (int i = 0; i < settingsCityUIs.Length; i++)
            {
                if (settingsCityUIs[i].cityLocation < index)
                {
                    newIndex++;
                }
            }

            if (type == 0)
            {
                if (locType == 0) return cityUIPoses[newIndex];
                else return cityUIRots[newIndex];
            }
            else
            {
                if (locType == 0) return cityGamePoses[newIndex];
                else return cityGameRots[newIndex];
            }
        }

        public void ChangeOneCity(int index, string name, int price, int location)
        {
            currentCityInfos[index].cityName = name;
            currentCityInfos[index].cityPrice = price;
            currentCityInfos[index].location = location;
            PlayerPrefs.SetString("BoardCityName" + index.ToString(), name);
            PlayerPrefs.SetInt("BoardCityPrice" + index.ToString(), price);
            PlayerPrefs.SetInt("BoardCityLocation" + index.ToString(), location);
        }

        public void SaveCities()
        {
            for (int i = 0; i < 22; i++)
            {
                PlayerPrefs.SetString("BoardCityName" + i.ToString(), settingsCityUIs[i].cityName.text);

                string tointt = settingsCityUIs[i].cityPrice.text;
                int intprice = int.Parse(tointt.Substring(1));
                PlayerPrefs.SetInt("BoardCityPrice" + i.ToString(), intprice);

                PlayerPrefs.SetInt("BoardCityLocation" + i.ToString(), settingsCityUIs[i].cityLocation);

                ChangeOneCity(i, settingsCityUIs[i].cityName.text, intprice, settingsCityUIs[i].cityLocation);
            }
        }

        public void ResetCities()
        {
            for (int i = 0; i < 22; i++)
            {
                PlayerPrefs.SetString("BoardCityName" + i.ToString(), defaultCityInfos[i].cityName);
                PlayerPrefs.SetInt("BoardCityPrice" + i.ToString(), defaultCityInfos[i].cityPrice);
                PlayerPrefs.SetInt("BoardCityLocation" + i.ToString(), defaultCityInfos[i].location);

                settingsCityUIs[i].UpdateCitySettings(defaultCityInfos[i].cityName, defaultCityInfos[i].cityPrice.ToString(), defaultCityInfos[i].location);
                ChangeOneCity(i, defaultCityInfos[i].cityName, defaultCityInfos[i].cityPrice, defaultCityInfos[i].location);
            }
        }

        public void PublishBoardChanges()
        {
            int k = 0;
            Cell[] cells = GameManager.Instance.boardCells;
            for(int i = 0; i < cells.Length; i++)
            {
                if(k < 22)
                {
                    if (cells[i].gameObject.name == inGameCities[k].gameObject.name)
                    {
                        inGameCities[k].UpdatePreSettings(currentCityInfos[k].cityName, currentCityInfos[k].cityPrice, currentCityInfos[k].location);

                        k++;
                    }
                }
            }
        }
    }
}
