using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HUST
{
    public class Enzyme1 : Enemy
    {
        // // Start is called before the first frame update
        // void Start()
        // {
        //     rb.gravityScale = 12f;
        // }

        protected override void Start()
        {
            base.Start();
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            if (!isRecoiling)
            {
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(PlayerMovement.Instance.transform.position.x, transform.position.y),
                speed * Time.deltaTime);
            }
        }

        public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
        {
            base.EnemyHit(_damageDone, _hitDirection, _hitForce);
        }
    }

}
