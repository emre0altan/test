using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Monopoly.Game;
using Photon.Pun;
using Photon.Realtime;

namespace Monopoly.Login
{
    public class LobbyManager : MonoBehaviourPunCallbacks
    {
        public static LobbyManager Instance;

        public RoomForGrid roomForGridPrefab;
        public Transform roomGrid;
        public GameObject loadingPanel, lobbyCanvas, roomCanvas, loginCanvas;
        public InputField roomNameField;
        public string selectedRoomName;

        private List<RoomInfo> cachedRoomList;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            cachedRoomList = new List<RoomInfo>();
        }

        public void UpdateRoomGrid()
        {
            for(int i = 0; i < roomGrid.childCount; i++)
            {
                Destroy(roomGrid.GetChild(i).gameObject);
            }
            for(int i = 0; i < cachedRoomList.Count; i++)
            {
                RoomForGrid tmpGridRoom = Instantiate(roomForGridPrefab, roomGrid);
                tmpGridRoom.roomNameText.text = cachedRoomList[i].Name;
                tmpGridRoom.currentPlayerCount.text = cachedRoomList[i].PlayerCount + "/4";
            }
            Debug.Log("UpdateRoomGrid called");
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            base.OnRoomListUpdate(roomList);
            foreach (RoomInfo info in roomList)
            {
                if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
                {
                    if (cachedRoomList.Contains(info))
                    {
                        cachedRoomList.Remove(info);
                    }
                    continue;
                }

                if (cachedRoomList.Contains(info))
                {
                    cachedRoomList[cachedRoomList.FindIndex(i => i.Name == info.Name)] = info;
                }
                else
                {
                    cachedRoomList.Add(info);
                }
            }
            UpdateRoomGrid();
            Debug.Log("OnRoomListUpdate called");
        }

        public void CreateRoom()
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 4;
            PhotonNetwork.CreateRoom(roomNameField.text, roomOptions);
            roomNameField.text = "";
            loadingPanel.SetActive(true);
        }

        public override void OnCreatedRoom()
        {
            base.OnCreatedRoom();
            loadingPanel.SetActive(false);
            lobbyCanvas.SetActive(false);
            roomCanvas.SetActive(true);
            Debug.Log("OnCreatedRoom called");
        }

        public void JoinRoom()
        {
            if(selectedRoomName != "")
            {
                PhotonNetwork.JoinRoom(selectedRoomName);
                loadingPanel.SetActive(true);
            }
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            loadingPanel.SetActive(false);
            lobbyCanvas.SetActive(false);
            roomCanvas.SetActive(true);
            Debug.Log("OnJoinedRoom called");

            RoomManager.Instance.EnteredRoom();

            TokenSelectionManager.Instance.photonView.TransferOwnership(PhotonNetwork.LocalPlayer);

        }

        public void MyLeaveLobby()
        {
            PhotonNetwork.LeaveLobby();
            loadingPanel.SetActive(true);
        }

        public override void OnLeftLobby()
        {
            base.OnLeftLobby();
            loadingPanel.SetActive(false);
            lobbyCanvas.SetActive(false);
            loginCanvas.SetActive(true);
            Debug.Log("On left lobby called");
        }
    }
}