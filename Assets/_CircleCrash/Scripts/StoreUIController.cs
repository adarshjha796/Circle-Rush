using UnityEngine;
using System.Collections;
using UnityEngine.UI;

#if EASY_MOBILE
using EasyMobile;
#endif

namespace SgLib
{
    public class StoreUIController : MonoBehaviour
    {
        public GameObject coinPackPrefab;
        public Transform productList;

        // Use this for initialization
        void Start()
        {
            if (InAppPurchaser.Instance != null && InAppPurchaser.Instance.coinPacks != null)
            {
                for (int i = 0; i < InAppPurchaser.Instance.coinPacks.Length; i++)
                {
                    InAppPurchaser.CoinPack pack = InAppPurchaser.Instance.coinPacks[i];
                    GameObject newPack = Instantiate(coinPackPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    Transform newPackTf = newPack.transform;
                    newPackTf.Find("CoinValue").GetComponent<Text>().text = pack.coinValue.ToString();
                    newPackTf.Find("Price/PriceString").GetComponent<Text>().text = pack.priceString;
                    newPackTf.SetParent(productList, true);
                    newPackTf.localScale = Vector3.one;

                    // This is to make sure it displays correctly even when the canvas is set to Camera Overlay.
                    newPackTf.localPosition = Vector3.zero;
                    newPackTf.localEulerAngles = Vector3.zero;

                    // Add button listener
                    newPackTf.GetComponent<Button>().onClick.AddListener(() =>
                        {
                            Utilities.ButtonClickSound();

                            #if EASYMOBILE && UNITY_EDITOR
                            Debug.Log("Please test in-app purchases on a real device.");
                            #elif EASY_MOBILE
                            InAppPurchaser.Instance.Purchase(pack.productName);
                            #else
                            Debug.Log("In app purchase is not available.");
                            #endif
                        });
                }
            }
        }
    }
}
