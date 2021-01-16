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
        public Text playerNameText, moneyDiff;
        public string playerName;
        public int playerMoney;

        private int lastMoneyDiff = 0;

        public void UpdatePlayerNames(string name, int money)
        {
            photonView.RPC("SendUpdatePlayerNames", RpcTarget.All, name, money);
        }

        [PunRPC]
        public void SendUpdatePlayerNames(string name, int money)
        {
            if (money != playerMoney)
            {
                moneyDiff.gameObject.SetActive(true);
                lastMoneyDiff += money - playerMoney;

                if(lastMoneyDiff > 0)
                {
                    moneyDiff.text = "+" + lastMoneyDiff.ToString();
                    moneyDiff.color = Color.green;
                }
                else
                {
                    moneyDiff.text = lastMoneyDiff.ToString();
                    moneyDiff.color = Color.red;
                }

                StopCoroutine("DisableMoneyDiff");
                StartCoroutine("DisableMoneyDiff");
            }

            playerNameText.text = name + " - $" + money.ToString();

            playerName = name;
            playerMoney = money;
        }

        IEnumerator DisableMoneyDiff()
        {
            yield return new WaitForSeconds(3);
            lastMoneyDiff = 0;
            moneyDiff.gameObject.SetActive(false);
        }
    }
}