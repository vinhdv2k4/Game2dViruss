using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HUST
{
    public class PlayerStateList : MonoBehaviour
    {
        public bool isJumping = false;
        public bool isDashing = false;
        public bool isHealing;
        public bool isCasting;

        // public bool alive ;
        // public bool Invincible;
        public bool recoilingX, recoilingY;
        public bool lookingRight;
        public bool invincible;
        public bool cutscene = false;
    }
}

