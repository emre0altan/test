using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


namespace Monopoly.Game
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        public PhotonView photonView;
        public bool isButtonPressing = false;
        public Button[] gameCanvasButtons;
        public Sprite[] diceSprites;
        public Image firstDice, secondDice;
        public GameObject diceTable;
        public Text diceText;

        private void Awake()
        {
            if (Instance)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        public void ItIsNoOnesTurn()
        {
            photonView.RPC("SayNotMyTurn", RpcTarget.All);
        }

        [PunRPC]
        public void SayNotMyTurn()
        {
            for(int i = 0; i < gameCanvasButtons.Length; i++)
            {
                gameCanvasButtons[i].interactable = false;
            }
        }

        public void ItIsSomeonesTurn(string name)
        {
            for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if(PhotonNetwork.PlayerList[i].NickName == name)
                {
                    photonView.RPC("SayMyTurn", PhotonNetwork.PlayerList[i]);
                }
            }
        }

        [PunRPC]
        public void SayMyTurn()
        {
            for (int i = 0; i < gameCanvasButtons.Length; i++)
            {
                gameCanvasButtons[i].interactable = true;
            }
        }

        public int RollDice()
        {
            int x1, x2;
            x1 = Random.Range(1, 7);
            x2 = Random.Range(1, 7);

            photonView.RPC("RollDiceAll", RpcTarget.All, x1 , x2);
            StartCoroutine(DisableDice());
            return x1 + x2;
        }

        IEnumerator DisableDice()
        {
            yield return new WaitForSeconds(5);
            photonView.RPC("DisableDiceAll", RpcTarget.All);
        }

        [PunRPC]
        public void RollDiceAll(int x1, int x2)
        {
            diceTable.SetActive(true);
            firstDice.sprite = diceSprites[x1-1];
            secondDice.sprite = diceSprites[x2-1];
            diceText.text = "Result: " + (x1 + x2).ToString();
        }
        [PunRPC]
        public void DisableDiceAll()
        {
            diceTable.SetActive(false);
        }
    }
}
