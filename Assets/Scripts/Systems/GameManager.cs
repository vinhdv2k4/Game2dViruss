using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HUST
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance {get; private set; }
        public string transitionedFromScene;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
            DontDestroyOnLoad(gameObject);
        }
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}

