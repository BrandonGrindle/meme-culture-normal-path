using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishPoint : MonoBehaviour
{
    [Header("Catch Alert")]
    [SerializeField] private GameObject exclamationPoint;
    private GameObject UiCatch;

    [Header("Missed Fish")]
    [SerializeField] private float reactionTime = 5f;
    private GameObject UiMiss;

    [Header("Wait Time")]
    [SerializeField] private float WaitMin = 5f;
    [SerializeField] private float WaitMax = 60f;

    [Header("Possible Fish")]
    public List<Items> Fish;
    public Items caughtFish;

    private GameObject exclamationPointInstance;
    public bool catchDetected = false;

    public AudioClip drop;

    public void Awake()
    {
        UiCatch = GameObject.Find("CatchAlert");
        UiMiss = GameObject.Find("MissAlert");

        UiCatch.SetActive(false);
        UiMiss.SetActive(false);
    }

    public void CatchFish() 
    { 
        if (catchDetected)
        {
            catchDetected = false;
            StartCoroutine(UIDelay(UiCatch));
            if (Fish.Count != 0)
            {
                int RandFish = Random.Range(0, Fish.Count);
                caughtFish = Fish[RandFish];
                Debug.Log("Fish Caught: " + caughtFish);
            }
            if (exclamationPointInstance != null)
            {
                Destroy(exclamationPointInstance);
            }
        }
    }
    

    public IEnumerator FishBite(GameObject Bobble)
    {
        yield return new WaitForSeconds(Random.Range(WaitMin, WaitMax));

        Vector3 pos = Bobble.transform.position + new Vector3(0, 1, 0);
        AudioSource.PlayClipAtPoint(drop, pos, 1);
        exclamationPointInstance = Instantiate(exclamationPoint, pos, Quaternion.identity);
        
        catchDetected = true;
        StartCoroutine(CatchTime());
    }

    private IEnumerator CatchTime()
    {
        yield return new WaitForSeconds(reactionTime);

        if (catchDetected)
        {
            Debug.Log("Fish Escaped");
            catchDetected = false;
            StartCoroutine(UIDelay(UiMiss));
            if(exclamationPointInstance != null)
            {
                Destroy(exclamationPointInstance);
            }
        }
    }

    private IEnumerator UIDelay(GameObject UIElement)
    {
        UIElement.SetActive(true);
        yield return new WaitForSeconds(3);
        UIElement.SetActive(false);
    }
}
