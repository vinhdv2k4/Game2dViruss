using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HUST
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

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

        public SceneFader sceneFader;

        void Start()
        {
            sceneFader = GetComponentInChildren<SceneFader>();
        }
    }
}
