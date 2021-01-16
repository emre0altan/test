using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace Monopoly.Login
{
    public class LoginManager : MonoBehaviourPunCallbacks
    {
        public static LoginManager Instance;

        public InputField userNameField;
        public GameObject loadingPanel, loginCanvas, lobbyCanvas;


        private void Awake()
        {
            if(Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            if (loginCanvas.activeSelf)
            {
                loadingPanel.SetActive(false);
                LobbyManager.Instance.ConnectToChat();
                Debug.Log("On connected to master called");
            }
        }

        public void Login()
        {
            if(userNameField.text != "")
            {
                PlayerPrefs.SetString("Username", userNameField.text);
                PhotonNetwork.LocalPlayer.NickName = userNameField.text;
                loadingPanel.SetActive(true);
                PhotonNetwork.JoinLobby();
                LobbyManager.Instance.SubscribeToChat();
            }
        }

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            if (loginCanvas.activeSelf)
            {
                loadingPanel.SetActive(false);
                loginCanvas.SetActive(false);
                lobbyCanvas.SetActive(true);
                Debug.Log("On joined lobby called");
            }
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}
