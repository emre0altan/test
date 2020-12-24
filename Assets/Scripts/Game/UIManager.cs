using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Monopoly.GameBoard;

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

        public Transform viewInfoUp, railRoadViewInfo, electricViewInfo, waterViewInfo;
        public Text[] viewInfoTexts, railRoadTexts, electricTexts, waterTexts;

        private void Awake()
        {
            if (Instance)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        public void PressedToButton() { isButtonPressing = true; }
        public void ReleasedAButton() { isButtonPressing = false; }

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
            gameCanvasButtons[0].interactable = true;
            TokenBoardObject[] tokenBoardObjects = GameManager.Instance.transform.GetComponentsInChildren<TokenBoardObject>();
            for (int i = 0; i < tokenBoardObjects.Length; i++)
            {
                if (tokenBoardObjects[i].photonView.Owner.NickName == name && tokenBoardObjects[i].isInJail)
                {
                    gameCanvasButtons[0].interactable = false;
                    if (GameManager.Instance.playerMoneys[GameManager.Instance.currentPlayerName] > 50)
                    {
                        gameCanvasButtons[7].interactable = true;
                    }
                    tokenBoardObjects[i].remainJailTurns--;
                    if (tokenBoardObjects[i].remainJailTurns == 0)
                    {
                        tokenBoardObjects[i].isInJail = false;
                    }
                }
            }
        }

        public void OpenButtonsAccToCell(Player x, int cellType, bool isBuyable, string owner)
        {
            photonView.RPC("ButtonsForTurn", x, x.NickName, cellType, isBuyable, owner);
        }

        [PunRPC]
        public void ButtonsForTurn(string name, int cellType, bool isBuyable, string owner)
        {
            gameCanvasButtons[1].interactable = true;
            
            gameCanvasButtons[4].interactable = true;
            if (isBuyable && owner == "")
            {
                gameCanvasButtons[2].interactable = true;
            }
            else if (isBuyable && owner == name)
            {
                gameCanvasButtons[3].interactable = true;
            }

            for (int i = 0; i < GameManager.Instance.gameCities.Length; i++)
            {
                if (GameManager.Instance.gameCities[i].owner == name)
                {
                    if (GameManager.Instance.gameCities[i].isMortgaged)
                    {
                        gameCanvasButtons[6].interactable = true;
                    }
                    else
                    {
                        gameCanvasButtons[5].interactable = true;
                    }
                }
            }
            Debug.Log(name + " - " + cellType + " - " + isBuyable + " - " + owner);
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
            yield return new WaitForSeconds(3);
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

        public void ShowViewInformation(GameCity gameCity, int type)//0 for normal
        {
            electricViewInfo.parent.gameObject.SetActive(false);
            viewInfoUp.parent.gameObject.SetActive(false);

            if (type == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (i < gameCity.currentHouseCount)
                    {
                        viewInfoUp.GetChild(i).gameObject.SetActive(true);
                    }
                    else
                    {
                        viewInfoUp.GetChild(i).gameObject.SetActive(false);
                    }
                }
                viewInfoTexts[0].text = gameCity.cityName;
                viewInfoTexts[1].text = "Owner: " + gameCity.owner;
                viewInfoTexts[2].text = "Rent: $" + gameCity.rent;
                if (gameCity.isMortgaged) viewInfoTexts[3].text = "Mortgaged";
                else viewInfoTexts[3].text = "Unmortgaged";
                viewInfoTexts[4].text = "Price of 1 house: $" + gameCity.priceOfHouse;
                viewInfoTexts[5].text = "$" + gameCity.price;

                viewInfoUp.parent.gameObject.SetActive(true);
                viewInfoUp.GetComponent<Image>().color = gameCity.transform.GetChild(0).GetComponent<SpriteRenderer>().color;
            }
            else
            {
                if (gameCity.gameObject.name.CompareTo("Electric") == 0)
                {
                    electricTexts[0].text = gameCity.cityName;
                    electricTexts[1].text = "Owner: " + gameCity.owner;
                    electricTexts[2].text = "Rent: $" + gameCity.rent;
                    electricTexts[3].text = "$" + gameCity.price;

                    electricViewInfo.gameObject.SetActive(true);
                    railRoadViewInfo.gameObject.SetActive(false);
                    waterViewInfo.gameObject.SetActive(false);
                    electricViewInfo.parent.gameObject.SetActive(true);
                }
                else if (gameCity.gameObject.name.CompareTo("Water") == 0)
                {
                    waterTexts[0].text = gameCity.cityName;
                    waterTexts[1].text = "Owner: " + gameCity.owner;
                    waterTexts[2].text = "Rent: $" + gameCity.rent;
                    waterTexts[3].text = "$" + gameCity.price;

                    electricViewInfo.gameObject.SetActive(false);
                    railRoadViewInfo.gameObject.SetActive(false);
                    waterViewInfo.gameObject.SetActive(true);
                    electricViewInfo.parent.gameObject.SetActive(true);
                }
                else
                {
                    railRoadTexts[0].text = gameCity.cityName;
                    railRoadTexts[1].text = "Owner: " + gameCity.owner;
                    railRoadTexts[2].text = "Rent: $" + gameCity.rent;
                    railRoadTexts[3].text = "$" + gameCity.price;

                    electricViewInfo.gameObject.SetActive(false);
                    railRoadViewInfo.gameObject.SetActive(true);
                    waterViewInfo.gameObject.SetActive(false);
                    electricViewInfo.parent.gameObject.SetActive(true);
                }
            }
        }
    }
}
