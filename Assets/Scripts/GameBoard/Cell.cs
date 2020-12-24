using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monopoly.GameBoard
{
    public class Cell : MonoBehaviour
    {
        public int location, cellType;//0 for cornerGO,1 for in jail, 2 for parking, 3 for go jail, 4 for normal property, 5 for state property, 6 for card cell,
                                      //7 for tax cell
        public int taxAmount;
    }
}