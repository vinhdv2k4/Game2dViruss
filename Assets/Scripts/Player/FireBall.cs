using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HUST
{
    public class FireBall : MonoBehaviour
    {
        [SerializeField] private float damage;
        [SerializeField] private float hitForce;
        [SerializeField] private float speed;
        [SerializeField] private float lifeTime = 1;
        // Start is called before the first frame update
        void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        private void FixedUpdate()
        {
            transform.position += speed * transform.right;
        }

        private void OnTriggerEnter2D(Collider2D _other)
        {
            if (_other.CompareTag("Enemy"))
            {
                _other.GetComponent<Enemy>().EnemyHit(damage, (_other.transform.position - transform.position).normalized, -hitForce);
                Destroy(gameObject);
            }
        }
    }

}
