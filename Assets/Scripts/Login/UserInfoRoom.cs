using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace Monopoly.Login
{
    public class UserInfoRoom : MonoBehaviour
    {
        public Text userName;
        public GameObject check;
        public PhotonView photonView;

        private void Start()
        {
            userName.text = photonView.Owner.NickName;
            transform.SetParent(GameObject.Find("Users").transform);
        }

        public void CheckForReady()
        {
            if(photonView.IsMine) photonView.RPC("CheckMyReady", RpcTarget.AllBuffered);
        }
        
        [PunRPC]
        public void CheckMyReady()
        {
            check.SetActive(!check.activeSelf);
        }
    }
}
