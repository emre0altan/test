using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Photon.Pun;
using Monopoly.GameBoard;
using DG.Tweening;
using Photon.Realtime;

namespace Monopoly.Game
{
    [System.Serializable]
    public struct CardInfo
    {
        public Sprite cardImage;
        public bool isMoneyCard, payOthers, isRelativeMove, isGoJail;
        public int moneyChange, moveValue;
    }

    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager Instance;

        public PhotonView photonView;
        public GameCity[] gameCities;
        public Cell[] boardCells;
        public PlayerNames[] playerNames;
        public TokenSelectorText selectorText;

        public Dictionary<int, string> playerTurns;

        public Dictionary<string, int> playerLocations, playerMoneys;
        private int initialPlayerMoneys;
        public int currentTurn;
        public string currentPlayerName;
        public CardInfo[] chanceCards, communityCards;
        

        private void Awake()
        {
            if (Instance)
            {
                Destroy(this);
                return;
            }
            Instance = this;

        }

        public void FirstStart()
        {
            initialPlayerMoneys = PlayerPrefs.GetInt("InitMoney");

            playerLocations = new Dictionary<string, int>();
            playerMoneys = new Dictionary<string, int>();

            currentTurn = 0;
            currentPlayerName = playerTurns[currentTurn];
            selectorText.photonView.RPC("CurrentTurn", RpcTarget.All, currentPlayerName);
            UIManager.Instance.ItIsNoOnesTurn();
            UIManager.Instance.ItIsSomeonesTurn(currentPlayerName);
            for (int i = 0; i < playerTurns.Count; i++)
            {
                playerLocations.Add(playerTurns[i], 0);
                playerMoneys.Add(playerTurns[i], initialPlayerMoneys);
                playerNames[i].UpdatePlayerNames(playerTurns[i], initialPlayerMoneys);
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            if(currentPlayerName == otherPlayer.NickName)
            {
                photonView.RPC("NextPlayer", RpcTarget.MasterClient);
            }
        }

        #region RollDice
        public void RollDice()
        {
            bool isdouble = false;
            int result = UIManager.Instance.RollDice(out isdouble);
            StartCoroutine(RollDiceDelay(result, true, isdouble));
        }

        IEnumerator RollDiceDelay(int result, bool del, bool isDouble)
        {
            if (del) yield return new WaitForSeconds(2);
            else yield return null;
            photonView.RPC("RollDiceToMaster", RpcTarget.MasterClient, result, isDouble);
        }

        [PunRPC]
        public void RollDiceToMaster(int result, bool isdouble)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).GetComponent<PhotonView>().Owner.NickName == currentPlayerName)
                {
                    for (int j = 0; j < boardCells.Length; j++)
                    {
                        if (boardCells[j].location == (playerLocations[currentPlayerName] + result) % 40)
                        {
                            Debug.Log("Current loc: " + boardCells[j].location + " - type: " + boardCells[j].cellType);

                            transform.GetChild(i).GetComponent<TokenBoardObject>().MoveToken(boardCells[j].transform.position, false, 0);

                            if (playerLocations[currentPlayerName] + result > 39)
                            {
                                playerMoneys[currentPlayerName] += PlayerPrefs.GetInt("GoMoney");
                                for (int k = 0; k < playerNames.Length; k++)
                                {
                                    if (playerNames[k].playerName == currentPlayerName)
                                    {
                                        playerNames[k].UpdatePlayerNames(currentPlayerName, playerMoneys[currentPlayerName]);
                                    }
                                }
                            }

                            playerLocations[currentPlayerName] += result;
                            playerLocations[currentPlayerName] = playerLocations[currentPlayerName] % 40;

                            transform.GetChild(i).GetComponent<TokenBoardObject>().MoveToken(boardCells[j].transform.position, false, 0);

                            //IF BUYABLE
                            if (boardCells[j].cellType == 4 || boardCells[j].cellType == 5)
                            {
                                GameCity tmpGameCity = boardCells[j].GetComponent<GameCity>();
                                UIManager.Instance.OpenButtonsAccToCell(transform.GetChild(i).GetComponent<PhotonView>().Owner, boardCells[j].cellType, true, tmpGameCity.owner, isdouble);

                                if (tmpGameCity.owner != "" && tmpGameCity.owner != currentPlayerName)
                                {
                                    if(boardCells[j].cellType == 4)
                                    {
                                        playerMoneys[currentPlayerName] -= tmpGameCity.rent * (tmpGameCity.currentHouseCount + 1);
                                        playerMoneys[tmpGameCity.owner] += tmpGameCity.rent * (tmpGameCity.currentHouseCount + 1);

                                        if(playerMoneys[currentPlayerName] <= 0)
                                        {
                                            UIManager.Instance.photonView.RPC("SendBankrupt", transform.GetChild(i).GetComponent<PhotonView>().Owner);
                                        }
                                    }
                                    else
                                    {
                                        playerMoneys[currentPlayerName] -= tmpGameCity.rent;
                                        playerMoneys[tmpGameCity.owner] += tmpGameCity.rent;

                                        if (playerMoneys[currentPlayerName] <= 0)
                                        {
                                            UIManager.Instance.photonView.RPC("SendBankrupt", transform.GetChild(i).GetComponent<PhotonView>().Owner);
                                        }
                                    }
                                    for (int k = 0; k < playerNames.Length; k++)
                                    {
                                        if (playerNames[k].playerName == currentPlayerName)
                                        {
                                            playerNames[k].UpdatePlayerNames(currentPlayerName, playerMoneys[currentPlayerName]);
                                        }
                                        else if (playerNames[k].playerName == tmpGameCity.owner)
                                        {
                                            playerNames[k].UpdatePlayerNames(tmpGameCity.owner, playerMoneys[tmpGameCity.owner]);
                                        }
                                    }
                                }

                                if (boardCells[j].cellType == 4) UIManager.Instance.ShowViewInformation(tmpGameCity, 0);
                                else UIManager.Instance.ShowViewInformation(tmpGameCity, 1);
                            }
                            //IF IT IS A TAX CELL
                            else if(boardCells[j].cellType == 7)
                            {
                                UIManager.Instance.OpenButtonsAccToCell(transform.GetChild(i).GetComponent<PhotonView>().Owner, boardCells[j].cellType, false, "", isdouble);
                                playerMoneys[currentPlayerName] -= boardCells[j].taxAmount;
                                for (int k = 0; k < playerNames.Length; k++)
                                {
                                    if (playerNames[k].playerName == currentPlayerName)
                                    {
                                        playerNames[k].UpdatePlayerNames(currentPlayerName, playerMoneys[currentPlayerName]);
                                    }
                                }
                            }
                            //IF IT IS GO TO JAIL
                            else if(boardCells[j].cellType == 3)
                            {
                                UIManager.Instance.OpenButtonsAccToCell(transform.GetChild(i).GetComponent<PhotonView>().Owner, boardCells[j].cellType, false, "", isdouble);
                                transform.GetChild(i).GetComponent<TokenBoardObject>().MoveToken(boardCells[29].transform.position, true, 3);
  
                                playerLocations[currentPlayerName] = 10;
                            }
                            //IF IT IS CARD CELL
                            else if(boardCells[j].cellType == 6)
                            {
                                Debug.Log("CARD CELL");
                                if (boardCells[j].gameObject.CompareTag("ChanceCard"))
                                {
                                    int carrand = Random.Range(0, chanceCards.Length);
                                    UIManager.Instance.CardCell(0, carrand);
                                    StartCoroutine(CardRoutine(0, carrand));
                                    Debug.Log("CHANCE CARD CELL");
                                }
                                else
                                {
                                    int carrand = Random.Range(0, communityCards.Length);
                                    UIManager.Instance.CardCell(1, carrand);
                                    StartCoroutine(CardRoutine(1, carrand));
                                    Debug.Log("COMM CARD CELL");
                                }
                                UIManager.Instance.OpenButtonsAccToCell(transform.GetChild(i).GetComponent<PhotonView>().Owner, boardCells[j].cellType, false, "", isdouble);
                            }
                            else
                            {
                                UIManager.Instance.OpenButtonsAccToCell(transform.GetChild(i).GetComponent<PhotonView>().Owner, boardCells[j].cellType, false, "", isdouble);
                            }


                            return;
                        }
                    }
                }
            }
        }

        IEnumerator CardRoutine(int cardType, int cardIndex)
        {
            yield return new WaitForSeconds(1f);

            CardInfo currCard;
            if (cardType == 0) currCard = chanceCards[cardIndex];
            else currCard = communityCards[cardIndex];

            if(currCard.isMoneyCard && !currCard.payOthers)
            {
                Debug.Log("1111");
                playerMoneys[currentPlayerName] += currCard.moneyChange;

                for(int q = 0;q < transform.childCount; q++)
                {
                    if (transform.GetChild(q).GetComponent<PhotonView>().Owner.NickName == currentPlayerName)
                    {
                        if (playerMoneys[currentPlayerName] <= 0)
                        {
                            UIManager.Instance.photonView.RPC("SendBankrupt", transform.GetChild(q).GetComponent<PhotonView>().Owner);
                        }
                    }
                }
                for (int k = 0; k < playerNames.Length; k++)
                {
                    if (playerNames[k].playerName == currentPlayerName)
                    {
                        playerNames[k].UpdatePlayerNames(currentPlayerName, playerMoneys[currentPlayerName]);
                    }
                }
            }
            else if (currCard.isMoneyCard)
            {
                Debug.Log("2222");
                for (int k = 0; k < playerNames.Length; k++)
                {
                    if (playerNames[k].playerName == currentPlayerName)
                    {
                        playerMoneys[currentPlayerName] += currCard.moneyChange;
                        playerNames[k].UpdatePlayerNames(currentPlayerName, playerMoneys[currentPlayerName]);
                    }
                    else if(k < transform.childCount)
                    {
                        playerMoneys[playerNames[k].playerName] -= currCard.moneyChange;
                        playerNames[k].UpdatePlayerNames(playerNames[k].playerName, playerMoneys[playerNames[k].playerName]);
                    }
                }

                for (int q = 0; q < transform.childCount; q++)
                {
                    if (transform.GetChild(q).GetComponent<PhotonView>().Owner.NickName == currentPlayerName)
                    {
                        if (playerMoneys[currentPlayerName] <= 0)
                        {
                            UIManager.Instance.photonView.RPC("SendBankrupt", transform.GetChild(q).GetComponent<PhotonView>().Owner);
                        }
                    }
                }

            }
            else if (currCard.isGoJail)
            {
                Debug.Log("3333");
                StartCoroutine(RollDiceDelay(10-playerLocations[currentPlayerName], false, false));
            }
            else if (currCard.isRelativeMove)
            {
                Debug.Log("4444");
                StartCoroutine(RollDiceDelay(currCard.moveValue, false, false));
            }

            yield return new WaitForSeconds(0.5f);
            UIManager.Instance.photonView.RPC("CloseCardAll", RpcTarget.All);
        }


        #endregion

        public void SwitchTurn()
        {
            photonView.RPC("NextPlayer", RpcTarget.MasterClient);
            TokenBoardObject tokenBoardObjecttemp;
            for (int i = 0; i < GameManager.Instance.transform.childCount; i++)
            {
                tokenBoardObjecttemp = GameManager.Instance.transform.GetChild(i).GetComponent<TokenBoardObject>();
                if (tokenBoardObjecttemp.photonView.IsMine)
                {
                    if(tokenBoardObjecttemp.remainJailTurns > 1)
                    {
                        tokenBoardObjecttemp.MoveToken(tokenBoardObjecttemp.transform.position, true, --tokenBoardObjecttemp.remainJailTurns);
                    }
                    else
                    {
                        tokenBoardObjecttemp.MoveToken(tokenBoardObjecttemp.transform.position, false, 0);
                    }
                }
            }
        }



        [PunRPC]
        public void NextPlayer()
        {
            for(int i = 0; i < 4; i++)
            {
                currentTurn++;
                if (currentTurn >= playerTurns.Count) currentTurn = currentTurn % playerTurns.Count;
                currentPlayerName = playerTurns[currentTurn];
                if (playerMoneys[currentPlayerName] > 0) break;

                if (i == 3) photonView.RPC("CloseGame", RpcTarget.All);
            }
            selectorText.photonView.RPC("CurrentTurn", RpcTarget.All, currentPlayerName);
            UIManager.Instance.ItIsNoOnesTurn();
            UIManager.Instance.ItIsSomeonesTurn(currentPlayerName);
        }

        [PunRPC]
        public void CloseGame()
        {
            Debug.Log("GAME CLOSED");
            Application.Quit();
        }

        public void PurchasePlace()
        {
            photonView.RPC("PurchasePlaceMaster", RpcTarget.MasterClient);
            for (int i = 0; i < boardCells.Length; i++)
            {
                if (boardCells[i].location == playerLocations[currentPlayerName] && boardCells[i].cellType == 4)
                {
                    UIManager.Instance.gameCanvasButtons[3].interactable = true;
                }
            }
        }

        [PunRPC]
        public void PurchasePlaceMaster()
        {
            Debug.Log("LOC:" + playerLocations[currentPlayerName]);
            for (int i = 0; i < boardCells.Length; i++)
            {
                GameCity gameCityTmp = boardCells[i].GetComponent<GameCity>();
                if (boardCells[i].location == playerLocations[currentPlayerName])
                {
                    if (playerMoneys[currentPlayerName] >= gameCityTmp.price)
                    {
                        playerMoneys[currentPlayerName] -= gameCityTmp.price;
                        gameCityTmp.photonView.RPC("UpdateOwner", RpcTarget.All, currentPlayerName);
                        for (int j = 0; j < playerNames.Length; j++)
                        {
                            if (playerNames[j].playerName == currentPlayerName)
                            {
                                playerNames[j].UpdatePlayerNames(currentPlayerName, playerMoneys[currentPlayerName]);
                            }
                        }
                    }
                }
            }
        }

        public void BuyHouse()
        {
            photonView.RPC("BuyHouseMaster", RpcTarget.MasterClient);
            for (int i = 0; i < boardCells.Length; i++)
            {
                if (boardCells[i].location == playerLocations[currentPlayerName] && boardCells[i].cellType == 4)
                {
                    if(boardCells[i].GetComponent<GameCity>().currentHouseCount >= 4)
                    {
                        UIManager.Instance.gameCanvasButtons[3].interactable = false;
                    }
                }
            }
        }

        [PunRPC]
        public void BuyHouseMaster()
        {
            for (int i = 0; i < boardCells.Length; i++)
            {
                GameCity gameCityTmp = boardCells[i].GetComponent<GameCity>();
                if (boardCells[i].location == playerLocations[currentPlayerName])
                {
                    if (playerMoneys[currentPlayerName] >= gameCityTmp.priceOfHouse)
                    {
                        playerMoneys[currentPlayerName] -= gameCityTmp.priceOfHouse;

                        gameCityTmp.photonView.RPC("UpdateHouses", RpcTarget.All);

                        for (int j = 0; j < playerNames.Length; j++)
                        {
                            if (playerNames[j].playerName == currentPlayerName)
                            {
                                playerNames[j].UpdatePlayerNames(currentPlayerName, playerMoneys[currentPlayerName]);
                            }
                        }
                    }
                }
            }
        }

        public void Trade()
        {
            photonView.RPC("TradeMaster", RpcTarget.MasterClient);
        }

        [PunRPC]
        public void TradeMaster()
        {
            for(int i = 0;i< PhotonNetwork.PlayerList.Length; i++)
            {
                if(PhotonNetwork.PlayerList[i].NickName == currentPlayerName)
                {
                    UIManager.Instance.TradeUI(PhotonNetwork.PlayerList[i]);
                }
            }
        }


        public void GetOuOfJail()
        {
            photonView.RPC("GetOutOfJailMaster", RpcTarget.MasterClient);
        }

        [PunRPC]
        public void GetOutOfJailMaster()
        {
            playerMoneys[currentPlayerName] -= 50;

            for (int j = 0; j < playerNames.Length; j++)
            {
                if (playerNames[j].playerName == currentPlayerName)
                {
                    playerNames[j].UpdatePlayerNames(currentPlayerName, playerMoneys[currentPlayerName]);
                }
            }

            for(int i = 0; i < transform.childCount; i++)
            {
                if(transform.GetChild(i).GetComponent<PhotonView>().Owner.NickName == currentPlayerName)
                {
                    transform.GetChild(i).GetComponent<TokenBoardObject>().MoveToken(transform.GetChild(i).position, false, 0);
                }
            }
        }

        public void SendRaycast(TouchInput touchInput)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(touchInput.FirstScreenPosition);

            if (Physics.Raycast(ray, out hit))
            {
                Transform objectHit = hit.transform;

                if (objectHit.gameObject.layer == 8)
                {
                    if (objectHit.GetComponent<Cell>().cellType == 4) UIManager.Instance.ShowViewInformation(objectHit.GetComponent<GameCity>(), 0);
                    else UIManager.Instance.ShowViewInformation(objectHit.GetComponent<GameCity>(), 1);
                }
            }
        }

        public void Quitt()
        {
            Application.Quit();
        }
    }
}
