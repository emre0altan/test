using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Photon.Pun;
using Monopoly.GameBoard;
using DG.Tweening;

namespace Monopoly.Game
{
    public class GameManager : MonoBehaviour
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

        private void Awake()
        {
            if (Instance)
            {
                Destroy(this);
                return;
            }
            Instance = this;


        }

        private void Start()
        {
            initialPlayerMoneys = PlayerPrefs.GetInt("InitMoney", 100000);
        }

        public void FirstStart()
        {
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

        #region RollDice
        public void RollDice()
        {
            int result = UIManager.Instance.RollDice();
            StartCoroutine(RollDiceDelay(result));
        }

        IEnumerator RollDiceDelay(int result)
        {
            yield return new WaitForSeconds(2);
            photonView.RPC("RollDiceToMaster", RpcTarget.MasterClient, result);
        }

        [PunRPC]
        public void RollDiceToMaster(int result)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).GetComponent<PhotonView>().Owner.NickName == currentPlayerName)
                {
                    for (int j = 0; j < boardCells.Length; j++)
                    {
                        Debug.Log(boardCells[j].location + " - " + (playerLocations[currentPlayerName] + result).ToString());
                        if (boardCells[j].location == (playerLocations[currentPlayerName] + result) % 40)
                        {

                            transform.GetChild(i).GetComponent<TokenBoardObject>().MoveToken(boardCells[j].transform.position);

                            if (playerLocations[currentPlayerName] + result > 39)
                            {
                                playerMoneys[currentPlayerName] += 500;
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

                            transform.GetChild(i).GetComponent<TokenBoardObject>().MoveToken(boardCells[j].transform.position);

                            //IF BUYABLE
                            if (boardCells[j].cellType == 4 || boardCells[j].cellType == 5)
                            {
                                GameCity tmpGameCity = boardCells[j].GetComponent<GameCity>();
                                UIManager.Instance.OpenButtonsAccToCell(transform.GetChild(i).GetComponent<PhotonView>().Owner, boardCells[j].cellType, true, tmpGameCity.owner);

                                if (tmpGameCity.owner != "" && tmpGameCity.owner != currentPlayerName)
                                {
                                    playerMoneys[currentPlayerName] -= tmpGameCity.rent;
                                    for (int k = 0; k < playerNames.Length; k++)
                                    {
                                        if (playerNames[k].playerName == currentPlayerName)
                                        {
                                            playerNames[k].UpdatePlayerNames(currentPlayerName, playerMoneys[currentPlayerName]);
                                        }
                                    }
                                }
                            }
                            //IF IT IS A TAX CELL
                            else if(boardCells[j].cellType == 6 || boardCells[j].cellType == 7)
                            {
                                UIManager.Instance.OpenButtonsAccToCell(transform.GetChild(i).GetComponent<PhotonView>().Owner, boardCells[j].cellType, false, "");
                                playerMoneys[currentPlayerName] -= boardCells[j].taxAmount;
                                for (int k = 0; k < playerNames.Length; k++)
                                {
                                    if (playerNames[k].playerName == currentPlayerName)
                                    {
                                        playerNames[k].UpdatePlayerNames(currentPlayerName, playerMoneys[currentPlayerName]);
                                    }
                                }
                            }
                            else if(boardCells[j].cellType == 3)
                            {
                                UIManager.Instance.OpenButtonsAccToCell(transform.GetChild(i).GetComponent<PhotonView>().Owner, boardCells[j].cellType, false, "");
                                transform.GetChild(i).GetComponent<TokenBoardObject>().MoveToken(boardCells[29].transform.position);
                                transform.GetChild(i).GetComponent<TokenBoardObject>().isInJail = true;
                                transform.GetChild(i).GetComponent<TokenBoardObject>().remainJailTurns = 3;
                            }
                            else
                            {
                                UIManager.Instance.OpenButtonsAccToCell(transform.GetChild(i).GetComponent<PhotonView>().Owner, boardCells[j].cellType, false, "");
                            }


                            return;
                        }
                    }
                }
            }
        }


        #endregion

        public void SwitchTurn()
        {
            photonView.RPC("NextPlayer", RpcTarget.MasterClient);
        }



        [PunRPC]
        public void NextPlayer()
        {
            currentTurn++;
            if (currentTurn >= playerTurns.Count) currentTurn = currentTurn % playerTurns.Count;
            currentPlayerName = playerTurns[currentTurn];
            selectorText.photonView.RPC("CurrentTurn", RpcTarget.All, currentPlayerName);
            UIManager.Instance.ItIsNoOnesTurn();
            UIManager.Instance.ItIsSomeonesTurn(currentPlayerName);
        }

        public void PurchasePlace()
        {
            photonView.RPC("PurchasePlaceMaster", RpcTarget.MasterClient);
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
                    transform.GetChild(i).GetComponent<TokenBoardObject>().isInJail = false;
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
