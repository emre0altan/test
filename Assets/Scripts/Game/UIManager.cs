using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Monopoly.GameBoard;

namespace Monopoly.Game
{
    [System.Serializable]
    public struct TradeOffer
    {
        public Player from, to;
        public string[] fromCities, toCities;
        public int fromMoney, toMoney;
    }

    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        public PhotonView photonView;
        public bool isButtonPressing = false;
        public Button[] gameCanvasButtons;
        public Sprite[] diceSprites;
        public Image firstDice, secondDice, cardPanel;
        public GameObject diceTable, tradePanel, tradePanelOthers, tradeProperty, bankrupt;
        public Text diceText, tradeOtherText, tradePanelPlayerName, waitingOfferedTrade, offeredTrade;
        public InputField myTradeMon, otherTradeMon;

        public Transform viewInfoUp, railRoadViewInfo, electricViewInfo, waterViewInfo, tradeOtherContentTra, tradeMineContentTra;
        public Text[] viewInfoTexts, railRoadTexts, electricTexts, waterTexts, tradeButtonsTexts;
        public TradeOffer currentTradeOffer;

        Player selectedPlayerForTrade;

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

        [PunRPC]
        public void SendBankrupt()
        {
            bankrupt.SetActive(true);
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
            gameCanvasButtons[0].interactable = true;
            TokenBoardObject[] tokenBoardObjects = GameManager.Instance.transform.GetComponentsInChildren<TokenBoardObject>();
            for (int i = 0; i < tokenBoardObjects.Length; i++)
            {
                if (tokenBoardObjects[i].photonView.Owner.NickName == PhotonNetwork.LocalPlayer.NickName && tokenBoardObjects[i].isInJail)
                {
                    gameCanvasButtons[0].interactable = false;
                    gameCanvasButtons[1].interactable = true;
                    if (GameManager.Instance.playerMoneys[GameManager.Instance.currentPlayerName] > 50)
                    {
                        gameCanvasButtons[7].interactable = true;
                    }
                }
            }
            
        }

        public void OpenButtonsAccToCell(Player x, int cellType, bool isBuyable, string owner, bool isdouble)
        {
            photonView.RPC("ButtonsForTurn", x, x.NickName, cellType, isBuyable, owner, isdouble);
        }

        [PunRPC]
        public void ButtonsForTurn(string name, int cellType, bool isBuyable, string owner, bool isdouble)
        {
            if (isdouble) gameCanvasButtons[0].interactable = true;
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
        }

        public int RollDice(out bool isdouble)
        {
            int x1, x2;
            x1 = Random.Range(1, 7);
            x2 = Random.Range(1, 7);

            photonView.RPC("RollDiceAll", RpcTarget.All, x1 , x2);
            StartCoroutine(DisableDice());
            if (x1 == x2) isdouble = true;
            else isdouble = false;
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

        public void CardCell(int cardType, int cardIndex)
        {
            photonView.RPC("CardCellAll", RpcTarget.All, cardType, cardIndex);
        }

        [PunRPC]
        public void CardCellAll(int cardType, int cardIndex)
        {
            if(cardType == 0)
            {
                cardPanel.sprite = GameManager.Instance.chanceCards[cardIndex].cardImage;
            }
            else
            {
                cardPanel.sprite = GameManager.Instance.communityCards[cardIndex].cardImage;
            }

            cardPanel.gameObject.SetActive(true);
        }

        [PunRPC]
        public void CloseCardAll()
        {
            cardPanel.gameObject.SetActive(false);
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

        public void TradeUI(Player player1, Player player2 = null)//0 for trade norm,1 for trade others
        {
            photonView.RPC("TradeOthers", RpcTarget.All, player1.NickName);
        }


        [PunRPC]
        public void TradeOthers(string name)
        {
            if(PhotonNetwork.LocalPlayer.NickName != name)
            {
                tradePanel.transform.parent.gameObject.SetActive(true);
                tradePanel.SetActive(false);
                tradePanelOthers.SetActive(true);
                waitingOfferedTrade.transform.parent.gameObject.SetActive(false);
                offeredTrade.transform.parent.gameObject.SetActive(false);
                tradeOtherText.text = name + " is trading...";
            }
            else
            {
                tradePanel.transform.parent.gameObject.SetActive(true);
                tradePanel.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
                tradePanel.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
                tradePanel.SetActive(true);
                tradePanelOthers.SetActive(false);
                waitingOfferedTrade.transform.parent.gameObject.SetActive(false);
                offeredTrade.transform.parent.gameObject.SetActive(false);
            }
        }

        public void UpdateTradeButtonNames()
        {
            for(int i = 0; i < 3; i++)
            {
                if(i < PhotonNetwork.PlayerListOthers.Length)
                {
                    tradeButtonsTexts[i].text = PhotonNetwork.PlayerListOthers[i].NickName;
                }
                else
                {
                    tradeButtonsTexts[i].transform.parent.gameObject.SetActive(false);
                }
            }

            Player selectedPlayer = PhotonNetwork.LocalPlayer;
            List<GameCity> selectedProperties = new List<GameCity>();

            for (int i = 0; i < GameManager.Instance.boardCells.Length; i++)
            {
                if(GameManager.Instance.boardCells[i].cellType == 4 || GameManager.Instance.boardCells[i].cellType == 5)
                {
                    if (GameManager.Instance.boardCells[i].GetComponent<GameCity>().owner == selectedPlayer.NickName)
                    {
                        selectedProperties.Add(GameManager.Instance.boardCells[i].GetComponent<GameCity>());
                    }
                }
            }
            GameObject newprop;
            for(int i = 0; i < tradeMineContentTra.childCount; i++)
            {
                Destroy(tradeMineContentTra.GetChild(i).gameObject);
            }
            for (int i = 0; i < selectedProperties.Count; i++)
            {
                newprop = Instantiate(tradeProperty, tradeMineContentTra);
                newprop.transform.GetChild(0).GetComponent<Text>().text = selectedProperties[i].cityName;
                newprop.transform.GetChild(1).GetComponent<Text>().text = "Price: " + selectedProperties[i].price.ToString();
                newprop.transform.GetChild(2).GetComponent<Text>().text = "Rent: " + selectedProperties[i].rent.ToString();
                if (selectedProperties[i].currentHouseCount > 0) newprop.transform.GetChild(3).gameObject.SetActive(true);
                if (selectedProperties[i].currentHouseCount > 1) newprop.transform.GetChild(4).gameObject.SetActive(true);
                if (selectedProperties[i].currentHouseCount > 2) newprop.transform.GetChild(5).gameObject.SetActive(true);
                if (selectedProperties[i].currentHouseCount > 3) newprop.transform.GetChild(6).gameObject.SetActive(true);
            }
        }

        public void OpenSelectedPlayerAttributes(int index)
        {
            selectedPlayerForTrade = PhotonNetwork.PlayerListOthers[index];
            tradePanelPlayerName.text = selectedPlayerForTrade.NickName + " properties";
            List<GameCity> selectedProperties = new List<GameCity>();

            for (int i = 0; i < GameManager.Instance.boardCells.Length; i++)
            {
                if (GameManager.Instance.boardCells[i].cellType == 4 || GameManager.Instance.boardCells[i].cellType == 5)
                {
                    if (GameManager.Instance.boardCells[i].GetComponent<GameCity>().owner == selectedPlayerForTrade.NickName)
                    {
                        selectedProperties.Add(GameManager.Instance.boardCells[i].GetComponent<GameCity>());
                    }
                }
            }
            GameObject newprop;
            for (int i = 0; i < tradeOtherContentTra.childCount; i++)
            {
                Destroy(tradeOtherContentTra.GetChild(i).gameObject);
            }
            for (int i = 0; i < selectedProperties.Count; i++)
            {
                newprop = Instantiate(tradeProperty, tradeOtherContentTra);
                newprop.transform.GetChild(0).GetComponent<Text>().text = selectedProperties[i].cityName;
                newprop.transform.GetChild(1).GetComponent<Text>().text = "Price: " + selectedProperties[i].price.ToString();
                newprop.transform.GetChild(2).GetComponent<Text>().text = "Rent: " + selectedProperties[i].rent.ToString();
                if (selectedProperties[i].currentHouseCount > 0) newprop.transform.GetChild(3).gameObject.SetActive(true);
                if (selectedProperties[i].currentHouseCount > 1) newprop.transform.GetChild(4).gameObject.SetActive(true);
                if (selectedProperties[i].currentHouseCount > 2) newprop.transform.GetChild(5).gameObject.SetActive(true);
                if (selectedProperties[i].currentHouseCount > 3) newprop.transform.GetChild(6).gameObject.SetActive(true);
            }

        }

        public void SendTrade()
        {
            tradePanel.SetActive(false);
            waitingOfferedTrade.transform.parent.gameObject.SetActive(true);
            waitingOfferedTrade.text = selectedPlayerForTrade.NickName + " is deciding...";

            List<string> myGameCities = new List<string>(), otherGameCities = new List<string>();

            for(int i = 0; i < tradeMineContentTra.childCount; i++)
            {
                if (tradeMineContentTra.GetChild(i).GetChild(7).gameObject.activeSelf)
                {
                    myGameCities.Add(tradeMineContentTra.GetChild(i).GetChild(0).GetComponent<Text>().text);
                }
            }
            for (int i = 0; i < tradeOtherContentTra.childCount; i++)
            {
                if (tradeOtherContentTra.GetChild(i).GetChild(7).gameObject.activeSelf)
                {
                    otherGameCities.Add(tradeOtherContentTra.GetChild(i).GetChild(0).GetComponent<Text>().text);
                }
            }
            int myMon, otherMon;
            if (string.IsNullOrEmpty(myTradeMon.text)) myMon = 0; 
            else myMon = int.Parse(myTradeMon.text);

            if (string.IsNullOrEmpty(otherTradeMon.text)) otherMon = 0; 
            else otherMon = int.Parse(otherTradeMon.text);

            currentTradeOffer = new TradeOffer();
            currentTradeOffer.from = PhotonNetwork.LocalPlayer;
            currentTradeOffer.to = selectedPlayerForTrade;
            currentTradeOffer.fromMoney = myMon;
            currentTradeOffer.toMoney = otherMon;

            currentTradeOffer.fromCities = new string[myGameCities.Count];
            for(int i = 0; i < myGameCities.Count; i++)
            {
                currentTradeOffer.fromCities[i] = myGameCities[i];
            }

            currentTradeOffer.toCities = new string[otherGameCities.Count];
            for (int i = 0; i < otherGameCities.Count; i++)
            {
                currentTradeOffer.toCities[i] = otherGameCities[i];
            }

            photonView.RPC("TradeOfferedOthers", RpcTarget.Others, PhotonNetwork.LocalPlayer, selectedPlayerForTrade, currentTradeOffer.fromCities, myMon, currentTradeOffer.toCities, otherMon);
        }

        [PunRPC]
        public void TradeOfferedOthers(Player from, Player to, string[] fromGameCities, int fromMoney, string[] toGameCities, int toMoney)
        {
            currentTradeOffer = new TradeOffer();
            currentTradeOffer.from = from;
            currentTradeOffer.to = to;
            currentTradeOffer.fromCities = fromGameCities;
            currentTradeOffer.toCities = toGameCities;
            currentTradeOffer.fromMoney = fromMoney;
            currentTradeOffer.toMoney = toMoney;

            if (to.IsLocal)
            {
                tradePanelOthers.SetActive(false);
                string texx;
                texx = from.NickName + " is offering to you ";
                for(int i = 0; i < fromGameCities.Length; i++)
                {
                    if (i != 0) texx += ", ";
                    texx += fromGameCities[i];
                }

                texx += " and " + fromMoney + " dollar in exchange for ";
                for(int i = 0; i < toGameCities.Length; i++)
                {
                    if (i != 0) texx += ", ";
                    texx += toGameCities[i];
                }
                texx += " and " + toMoney + " dollar.";
                offeredTrade.text = texx;
                offeredTrade.transform.parent.gameObject.SetActive(true);

            }
            else
            {
                string texx;
                texx = from.NickName + " is offering ";
                for (int i = 0; i < fromGameCities.Length; i++)
                {
                    if (i != 0) texx += ", ";
                    texx += fromGameCities[i];
                }

                texx += " and " + fromMoney + " dollar in exchange for ";
                texx += to.NickName + "'s ";
                for (int i = 0; i < toGameCities.Length; i++)
                {
                    if (i != 0) texx += ", ";
                    texx += toGameCities[i];
                }
                texx += " and " + toMoney + " dollar.";
                tradeOtherText.text = texx;
            }
        }

        public void AcceptTrade()
        {
            if (!PhotonNetwork.IsMasterClient) photonView.RPC("AcceptTradeMaster", RpcTarget.MasterClient);
            else AcceptTradeMaster();
        }

        [PunRPC]
        public void AcceptTradeMaster()
        {
            if (GameManager.Instance.playerMoneys[currentTradeOffer.from.NickName] > currentTradeOffer.fromMoney - currentTradeOffer.toMoney)
            {
                GameManager.Instance.playerMoneys[currentTradeOffer.from.NickName] -= currentTradeOffer.fromMoney - currentTradeOffer.toMoney;
                for (int j = 0; j < GameManager.Instance.playerNames.Length; j++)
                {
                    if (GameManager.Instance.playerNames[j].playerName == currentTradeOffer.from.NickName)
                    {
                        GameManager.Instance.playerNames[j].UpdatePlayerNames(currentTradeOffer.from.NickName, GameManager.Instance.playerMoneys[currentTradeOffer.from.NickName]);
                    }
                }

                Debug.Log(currentTradeOffer.to.NickName);
                GameManager.Instance.playerMoneys[currentTradeOffer.to.NickName] -= currentTradeOffer.toMoney - currentTradeOffer.fromMoney;
                for (int j = 0; j < GameManager.Instance.playerNames.Length; j++)
                {
                    if (GameManager.Instance.playerNames[j].playerName == currentTradeOffer.to.NickName)
                    {
                        GameManager.Instance.playerNames[j].UpdatePlayerNames(currentTradeOffer.to.NickName, GameManager.Instance.playerMoneys[currentTradeOffer.to.NickName]);
                    }
                }


                for (int j = 0; j < currentTradeOffer.fromCities.Length; j++)
                {
                    for (int i = 0; i < GameManager.Instance.gameCities.Length; i++)
                    {
                        if (GameManager.Instance.gameCities[i].cityName == currentTradeOffer.fromCities[j])
                        {
                            GameManager.Instance.gameCities[i].photonView.RPC("UpdateOwner", RpcTarget.All, currentTradeOffer.to.NickName);
                            Debug.Log("CITY FOUND");
                        }
                    }
                }

                for (int j = 0; j < currentTradeOffer.toCities.Length; j++)
                {
                    for (int i = 0; i < GameManager.Instance.gameCities.Length; i++)
                    {
                        Debug.Log(GameManager.Instance.gameCities[i].name + " - " + currentTradeOffer.toCities[j]);
                        if (GameManager.Instance.gameCities[i].cityName == currentTradeOffer.toCities[j])
                        {
                            GameManager.Instance.gameCities[i].photonView.RPC("UpdateOwner", RpcTarget.All, currentTradeOffer.from.NickName);
                        }
                    }
                }
            }

            DenyTrade();
        }

        public void DenyTrade()
        {
            photonView.RPC("DenyTradeAll", RpcTarget.All);
        }

        [PunRPC]
        public void DenyTradeAll()
        {
            tradePanel.transform.parent.gameObject.SetActive(false);
            tradePanel.SetActive(false);
            tradePanelOthers.SetActive(false);
            waitingOfferedTrade.transform.parent.gameObject.SetActive(false);
            offeredTrade.transform.parent.gameObject.SetActive(false);
            tradePanel.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
            tradePanel.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);

        }

        public void TradeEnded()
        {
            photonView.RPC("TradeEndAll", RpcTarget.All);
        }

        [PunRPC]
        public void TradeEndAll()
        {
            tradePanel.SetActive(false);
            tradePanelOthers.SetActive(false);
            tradePanel.transform.parent.gameObject.SetActive(false);
        }
    }
}
