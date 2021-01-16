using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Monopoly.Game;
using Photon.Pun;
using Photon.Realtime;
using Photon.Chat;
using ExitGames.Client.Photon;

namespace Monopoly.Login
{
    public class LobbyManager : MonoBehaviourPunCallbacks, IChatClientListener
    {
        public static LobbyManager Instance;

        public RoomForGrid roomForGridPrefab;
        public Transform roomGrid;
        public GameObject loadingPanel, lobbyCanvas, roomCanvas, loginCanvas, inviteUI;
        public InputField roomNameField;
        public string selectedRoomName, invitedRooomName;
        public Text inviteText;

        private List<RoomInfo> cachedRoomList;
        private ChatClient chatClient;

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

        public void ConnectToChat()
        {
            chatClient = new ChatClient(this);
            chatClient.ChatRegion = "EU";
            chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.AppVersion, new Photon.Chat.AuthenticationValues(PlayerPrefs.GetString("Username", Random.Range(0, 10000).ToString())));
        }

        public void SubscribeToChat()
        {
            ChannelCreationOptions channelCreationOptions = new ChannelCreationOptions();
            channelCreationOptions.PublishSubscribers = true;
            channelCreationOptions.MaxSubscribers = 20;

            chatClient.Subscribe("A",default, default, channelCreationOptions);
        }

        private void Update()
        {
            if (chatClient != null)
            {
                chatClient.Service();
                Debug.Log(chatClient.CanChatInChannel("A"));
            }
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

        public void ShowInvite(string roomname, string playername)
        {
            inviteUI.SetActive(true);
            invitedRooomName = roomname;
            inviteText.text = playername + " invited you to: " + roomname;
        }

        public void JoinInvitedRoom()
        {
            if (invitedRooomName != "")
            {
                PhotonNetwork.JoinRoom(invitedRooomName);
                loadingPanel.SetActive(true);
            }
        }

        public void InviteAll()
        {
            string[] names = new string[2];
            names[0] = PhotonNetwork.CurrentRoom.Name;
            names[1] = PhotonNetwork.LocalPlayer.NickName;
            if (chatClient.PublishMessage("A", names)) Debug.Log("message sent");
            else Debug.Log("message couldnt sent");
        }

        public void DebugReturn(DebugLevel level, string message)
        {
        }

        public void OnDisconnected()
        {
        }

        public void OnChatStateChange(ChatState state)
        {
        }

        public void OnGetMessages(string channelName, string[] senders, object[] messages)
        {
            string[] namess = messages[0] as string[];

            if (channelName == "A" && !PhotonNetwork.InRoom)
            {
                ShowInvite(namess[0],namess[1]);
            }
        }

        public void OnPrivateMessage(string sender, object message, string channelName)
        {
        }

        public void OnSubscribed(string[] channels, bool[] results)
        {
            Debug.Log("User subscribed to: " + channels);
        }

        public void OnUnsubscribed(string[] channels)
        {
            Debug.Log("User unsubscribed to: " + channels);
        }

        public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
        {
        }

        public void OnUserSubscribed(string channel, string user)
        {
            Debug.Log(user + " subscribed to: " + channel);
        }

        public void OnUserUnsubscribed(string channel, string user)
        {
            Debug.Log(user + " unsubscribed to: " + channel);
        }
    }
}