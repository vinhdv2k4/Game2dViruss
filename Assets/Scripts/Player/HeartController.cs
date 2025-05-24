using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HUST
{
    public class HeartController : MonoBehaviour
    {
        private PlayerMovement player;
        private GameObject[] heartContainers;
        private Image[] heartFills;
        public Transform heartsParent;
        public GameObject heartContainerPrefab;

        // Start is called before the first frame update
        void Start()
        {
            player = PlayerMovement.Instance;
            heartContainers = new GameObject[PlayerMovement.Instance.maxHealth];
            heartFills = new Image[PlayerMovement.Instance.maxHealth];

            PlayerMovement.Instance.onHealthChangedCallBack += UpdateHeartsHUD;

            InstantiateHeartContainers();
            UpdateHeartsHUD();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void SetHeartContainers()
        {
            for (int i = 0; i < heartContainers.Length; i++)
            {
                if (i < PlayerMovement.Instance.maxHealth)
                {
                    heartContainers[i].SetActive(true);
                }
                else
                {
                    heartContainers[i].SetActive(false);
                }
            }
        }

        // private void SetFilledHearts()
        // {
        //     for (int i = 0; i < heartFills.Length; i++)
        //     {
        //         if (i < PlayerMovement.Instance.maxHealth)
        //         {
        //             heartFills[i].fillAmount = 1;
        //         }
        //         else
        //         {
        //             heartFills[i].fillAmount = 0;
        //         }
        //     }
        // }
        private void SetFilledHearts()
        {
            int currentHealth = PlayerMovement.Instance.Health;

            for (int i = 0; i < heartFills.Length; i++)
            {
                if (i < currentHealth)
                {
                    heartFills[i].fillAmount = 1;
                }
                else
                {
                    heartFills[i].fillAmount = 0;
                }
            }
        }

        private void InstantiateHeartContainers()
        {
            for (int i = 0; i < PlayerMovement.Instance.maxHealth; i++)
            {
                GameObject temp = Instantiate(heartContainerPrefab);
                temp.transform.SetParent(heartsParent, false);
                heartContainers[i] = temp;
                heartFills[i] = temp.transform.Find("HeartFill").GetComponent<Image>();
            }
        }

        private void UpdateHeartsHUD()
        {
            SetHeartContainers();
            SetFilledHearts();
        }
    }
}
