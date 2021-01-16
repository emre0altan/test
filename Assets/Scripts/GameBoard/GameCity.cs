using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

namespace Monopoly.GameBoard
{
    public class GameCity : MonoBehaviour
    {
        public string cityName, owner;
        public int price, rent, currentHouseCount, priceOfHouse, location;
        public bool isMortgaged, isContract;
        public GameObject[] houses;

        public TextMeshPro cityNameTMP, cityPriceTMP; 
        public PhotonView photonView;
        public bool isFullColor;
        public int propCount;


        public void UpdatePreSettings(string newname, int newprice, int newlocation)
        {
            photonView.RPC("GameCityRPC", RpcTarget.All, newname, newprice, newlocation);
        }

        [PunRPC]
        public void GameCityRPC(string newname, int newprice, int newlocation)
        {
            cityNameTMP.text = newname;
            cityPriceTMP.text = "$" + newprice.ToString();
            Debug.Log(newlocation + " - " + newname);
            transform.localPosition = BoardManager.Instance.GetLocInfo(1, 0, newlocation);
            transform.localEulerAngles = BoardManager.Instance.GetLocInfo(1, 1, newlocation);

            cityName = newname;
            price = newprice;
            location = newlocation;
            rent = newprice/ 10 + (newprice * currentHouseCount) / 10;
            priceOfHouse = newprice / 2;
            GetComponent<Cell>().location = newlocation;
        }

        [PunRPC]
        public void UpdateOwner(string playerName)
        {
            owner = playerName;
        }

        [PunRPC]
        public void UpdateHouses()
        {
            if (currentHouseCount < houses.Length)
            {
                currentHouseCount++;
                for (int i = 0; i < houses.Length; i++)
                {
                    if (i < currentHouseCount) houses[i].SetActive(true);
                    else houses[i].SetActive(false);
                }
            }
        }
    }
}