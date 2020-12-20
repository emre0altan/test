using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Monopoly.GameBoard;
using Monopoly.Game;
using Photon.Pun;
using Photon.Realtime;

namespace Monopoly.Login
{
    public class RoomManager : MonoBehaviourPunCallbacks
    {
        public static RoomManager Instance;

        public UserInfoRoom userInfoRoomPrefab;
        public Transform userInfoGrid;
        public GameObject loadingPanel, roomCanvas, gameSettingsCanvas, gameCanvas, lobbyCanvas, gameSettingsButton, startGameButton, tokenSelectCanvas;
        public Text roomName;
        public Button startButton;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            startButton = startGameButton.GetComponent<Button>();
        }

        private void Update()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (PhotonNetwork.InRoom)
                {
                    for(int i = 0; i < userInfoGrid.childCount; i++)
                    {
                        if (!userInfoGrid.GetChild(i).GetComponent<UserInfoRoom>().check.activeSelf)
                        {
                            startButton.interactable = false;
                            return;
                        }
                    }
                    startButton.interactable = true;
                }
            }
        }

        public void EnteredRoom()
        {
            UserInfoRoom tmpUserInfo = PhotonNetwork.Instantiate("UserInfo", Vector3.zero, Quaternion.identity).GetComponent<UserInfoRoom>();
            roomName.text = "Room - " + PhotonNetwork.CurrentRoom.Name;
            if (PhotonNetwork.IsMasterClient)
            {
                gameSettingsButton.SetActive(true);
                startGameButton.SetActive(true);
            }
            else
            {
                gameSettingsButton.SetActive(false);
                startGameButton.SetActive(false);
            }
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            base.OnMasterClientSwitched(newMasterClient);
            if (PhotonNetwork.IsMasterClient)
            {
                gameSettingsButton.SetActive(true);
                startGameButton.SetActive(true);
            }
            else
            {
                gameSettingsButton.SetActive(false);
                startGameButton.SetActive(false);
            }
        }

        public void MyLeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
            loadingPanel.SetActive(true);
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            Debug.Log("LeftRoom");
            PhotonNetwork.LeaveLobby();
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            if (roomCanvas.activeSelf)
            {
                PhotonNetwork.JoinLobby();
                Debug.Log("On connected to master called");
            }
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            if (roomCanvas.activeSelf)
            {
                loadingPanel.SetActive(false);
                roomCanvas.SetActive(false);
                lobbyCanvas.SetActive(true);
                Debug.Log("On joined lobby called");
            }
        }

        public void OpenGameSettings()
        {
            GameSettingsManager.Instance.isSettingsOpened = true;
        }

        public void StartGame()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                BoardManager.Instance.PublishBoardChanges();
                TokenSelectionManager.Instance.GotoTokenScreen();
            }
        }
    }
}