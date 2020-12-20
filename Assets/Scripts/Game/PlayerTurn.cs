using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace Monopoly.Game
{
    public class PlayerTurn : MonoBehaviour
    {
        public Text playername;

        [PunRPC]
        public void ShowOrder(int index, string name)
        {
            playername.text = (index + 1).ToString() + " - " + name;
        }
    }
}
