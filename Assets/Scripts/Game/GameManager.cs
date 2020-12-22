using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        public Cell[] boardCells;
        public PlayerNames[] playerNames;
        public TokenSelectorText selectorText;

        public Dictionary<int, string> playerTurns;

        private Dictionary<string, int> playerLocations;
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

            playerLocations = new Dictionary<string, int>();
        }

        private void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                initialPlayerMoneys = PlayerPrefs.GetInt("InitMoney", 5000);
            }
        }

        public void FirstStart()
        {
            currentTurn = 0;
            currentPlayerName = playerTurns[currentTurn];
            selectorText.photonView.RPC("CurrentTurn", RpcTarget.All,currentPlayerName);
            UIManager.Instance.ItIsNoOnesTurn();
            UIManager.Instance.ItIsSomeonesTurn(currentPlayerName);
            for(int i = 0; i < playerTurns.Count; i++)
            {
                playerLocations.Add(playerTurns[i], 0);
                playerNames[i].UpdatePlayerNames(playerTurns[i], initialPlayerMoneys);
            }
        }

        public void RollDice()
        {
            int result = UIManager.Instance.RollDice();
            photonView.RPC("RollDiceToMaster", RpcTarget.MasterClient, result);
        }

        [PunRPC]
        public void RollDiceToMaster(int result)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if(transform.GetChild(i).GetComponent<PhotonView>().Owner.NickName == currentPlayerName)
                {
                    for(int j=0;j<boardCells.Length;j++)
                    {
                        Debug.Log(boardCells[j].location + " - " + (playerLocations[currentPlayerName] + result).ToString());
                        if (boardCells[j].location == (playerLocations[currentPlayerName] + result)%40)
                        {
                            
                            photonView.RPC("CallMasterLocation", RpcTarget.MasterClient, result);
                            transform.GetChild(i).GetComponent<TokenBoardObject>().MoveToken(boardCells[j].transform.position);
                            photonView.RPC("NextPlayer", RpcTarget.MasterClient);
                            return;
                        }
                    }
                }
            }
        }

        [PunRPC]
        public void CallMasterLocation(int add)
        {
            playerLocations[currentPlayerName] += add;
            playerLocations[currentPlayerName] = playerLocations[currentPlayerName] % 40;
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


        public void Quitt()
        {
            Application.Quit();
        }
    }
}
