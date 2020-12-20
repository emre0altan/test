using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace Monopoly.Game
{
    public class TokenSelectorText : MonoBehaviour
    {
        public Text selectorText;
        public PhotonView photonView;


        [PunRPC]
        public void CurrentSelector(string playername)
        {
            selectorText.text = playername + " selects token";
        }
    }
}