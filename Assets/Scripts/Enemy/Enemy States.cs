
using UnityEngine;

namespace HUST
{
    public class EnemyStates : MonoBehaviour
    {
        protected EnemyState currentEnemyState;
        public Enemy enemy;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
        }

        public virtual EnemyState GetCurrentEnemyState
        {
            get => currentEnemyState;
            set
            {
                if (currentEnemyState != value)
                {
                    currentEnemyState = value;
                    enemy.ChangeCurrentAnimation();
                }
            }
        }

        public void ChangeState(EnemyState newState)
        {
            GetCurrentEnemyState = newState;
        }
    }
}
