using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace Monopoly.Login
{
    public class RoomForGrid : MonoBehaviour
    {
        public Text roomNameText, currentPlayerCount;
        public GameObject checkImage;

        private GameObject tmpGO;

        [PunRPC]
        public void UpdatePlayerCount(int count)
        {
            currentPlayerCount.text = count.ToString() + "/4";
        }

        public void SelectRoom()
        {
            if (checkImage.activeSelf)
            {
                checkImage.SetActive(false);
                LobbyManager.Instance.selectedRoomName = "";
            }
            else
            {
                for (int i = 0; i < transform.parent.childCount; i++)
                {
                    tmpGO = transform.parent.GetChild(i).GetChild(2).GetChild(0).gameObject;
                    if (tmpGO.activeSelf) tmpGO.SetActive(false);
                }
                checkImage.SetActive(true);
                LobbyManager.Instance.selectedRoomName = roomNameText.text;
            }
        }
    }
}