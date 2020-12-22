using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace Monopoly.Game
{
    public class PlayerNames : MonoBehaviour
    {
        public PhotonView photonView;
        public Text playerNameText;
        public string playerName;
        public int playerMoney;

        public void UpdatePlayerNames(string name, int money)
        {
            photonView.RPC("SendUpdatePlayerNames", RpcTarget.All, name, money);
        }

        [PunRPC]
        public void SendUpdatePlayerNames(string name, int money)
        {
            playerNameText.text = name + " - $" + money.ToString();
            playerName = name;
            playerMoney = money;
        }
    }
}