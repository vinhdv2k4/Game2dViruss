using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HUST
{
    public class Enzyme1 : Enemy
    {
        [Header("Enzyme Settings:")]
        private EnemyStates currentStates;
        protected override void Start()
        {
            base.Start();
            currentStates.ChangeState(EnemyStates.Patrol_Idle);
        }
        public override void AttackPlayer()
        {
           
        }

        public override void Turn()
        {
        
        }
    }
}
