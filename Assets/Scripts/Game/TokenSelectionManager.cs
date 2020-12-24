using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Monopoly.GameBoard;

namespace Monopoly.Game
{
    public class TokenSelectionManager : MonoBehaviour
    {
        public static TokenSelectionManager Instance;

        public TokenUI[] tokens;
        public bool localPlayerSelected = false;
        public PhotonView photonView;
        public GameObject roomCanvas, tokenSelectCanvas, gameCanvas;
        public PhotonView[] playOrder;
        public Button[] tokenSelectionButtons;
        public TokenSelectorText tokenSelectorText;
        public TokenBoardObject[] tokenPrefabs;
        public Transform boardStartPos;

        private Dictionary<string, int> playerTokens;//player nickname, token index
        private Dictionary<int, string> playerTurns;//player nickname, turn
        public int[] tokenSelected;
        

        private void Awake()
        {
            if (Instance)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            playerTokens = new Dictionary<string, int>();
            playerTurns = new Dictionary<int, string>();
        }


        #region TokenInit
        public void GotoTokenScreen()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("GoTokenSelection", RpcTarget.All, PhotonNetwork.PlayerList.Length);

                List<int> randlist = new List<int>();
                for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    randlist.Add(i);
                }
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    int qq = Random.Range(0, PhotonNetwork.PlayerList.Length - 1 - i);
                    playerTurns.Add(randlist[qq], PhotonNetwork.PlayerList[i].NickName);
                    randlist.RemoveAt(qq);
                    playOrder[i].RPC("ShowOrder", RpcTarget.All, i, playerTurns[i]);
                }
                GameManager.Instance.playerTurns = playerTurns;
                NextTurn();
            }
        }

        [PunRPC]
        public void GoTokenSelection(int countt)
        {
            roomCanvas.SetActive(false);
            tokenSelectCanvas.SetActive(true);
            tokenSelected = new int[countt];
        }
        #endregion

        #region TurnOperations
        public void NextTurn()
        {
            bool allready = true;
            for (int i = 0; i < playerTurns.Count; i++)
            {
                if (tokenSelected[i] == 0)
                {
                    for (int j = 0; j < PhotonNetwork.PlayerList.Length; j++)
                    {
                        if (playerTurns[i] == PhotonNetwork.PlayerList[j].NickName)
                        {
                            photonView.RPC("MyTurn", RpcTarget.All, playerTurns[i]);
                            tokenSelectorText.photonView.RPC("CurrentSelector", RpcTarget.All, playerTurns[i]);

                            return;
                        }
                    }
                    allready = false;
                }
            }

            if (allready)
            {
                StartCoroutine(GameStartRoutine());
            }
        }

        IEnumerator GameStartRoutine()
        {
            yield return new WaitForSeconds(3);
            photonView.RPC("StartToGame", RpcTarget.All);
            GameManager.Instance.FirstStart();
        }

        [PunRPC]
        public void StartToGame()
        {
            tokenSelectCanvas.SetActive(false);
            gameCanvas.SetActive(true);
            PhotonNetwork.Instantiate(tokenPrefabs[playerTokens[PhotonNetwork.LocalPlayer.NickName]].gameObject.name, boardStartPos.position, boardStartPos.rotation);
            TouchManager.Instance.onTouchBegan += GameManager.Instance.SendRaycast;
        }

        [PunRPC]
        public void MyTurn(string nickname)
        {
            if(PhotonNetwork.LocalPlayer.NickName == nickname)
            {
                for (int i = 0; i < tokenSelectionButtons.Length; i++)
                {
                    tokenSelectionButtons[i].interactable = true;
                }

                foreach (KeyValuePair<string, int> entry in playerTokens)
                {
                    tokenSelectionButtons[entry.Value].interactable = false;
                }
            }
        }

        public void SelectToken()
        {
            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i].glow.activeSelf)
                {
                    photonView.RPC("SendTokenInfo", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName, i);
                    localPlayerSelected = true;
                }
            }
            for (int i = 0; i < tokenSelectionButtons.Length; i++)
            {
                tokenSelectionButtons[i].interactable = false;
            }
            
        }

        [PunRPC]
        public void SendTokenInfo(string nickname, int index)
        {
            playerTokens.Add(nickname, index);
            tokens[index].GetComponent<Button>().interactable = false;
            for(int i = 0; i < playerTurns.Count; i++)
            {
                if(playerTurns[i] == nickname)
                {
                    tokenSelected[i] = 1;
                    break;
                }
            }
            if(PhotonNetwork.IsMasterClient) NextTurn();
        }

        public void DisableGlow()
        {
            for(int i = 0; i < tokens.Length; i++)
            {
                tokens[i].glow.SetActive(false);
            }
        }

        #endregion


    }
}