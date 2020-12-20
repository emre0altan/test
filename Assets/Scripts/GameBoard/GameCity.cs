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

        public TextMeshPro cityNameTMP, cityPriceTMP; 
        public PhotonView photonView;


        public void UpdatePreSettings(string newname, int newprice, int newlocation)
        {
            photonView.RPC("GameCityRPC", RpcTarget.All, newname, newprice, newlocation);
            cityName = newname;
            price = newprice;
            location = newlocation;
        }

        [PunRPC]
        public void GameCityRPC(string name, int price, int location)
        {
            cityNameTMP.text = name;
            cityPriceTMP.text = "$" + price.ToString();
            transform.localPosition = BoardManager.Instance.GetLocInfo(1, 0, location);
            transform.localEulerAngles = BoardManager.Instance.GetLocInfo(1, 1, location);
        }
    }
}