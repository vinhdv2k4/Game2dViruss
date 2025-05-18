using HUST;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TV
{
    public class EnemyStates : MonoBehaviour
    {
        protected EnemyStates currentEnemyState;
        public Enemy enemy;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
        }
        public virtual EnemyStates GetCurrentEnemyState
        {
            get { return currentEnemyState; }
            set
            {
                if (currentEnemyState != value)
                {
                    currentEnemyState = value;
                    enemy.ChangeCurrentAnimation();
                }
            }
        }
        protected void ChangeState(EnemyStates newState)
        {
            GetCurrentEnemyState = newState;
        }
    }
}
