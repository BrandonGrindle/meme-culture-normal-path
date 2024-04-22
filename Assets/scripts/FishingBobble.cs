using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class FishingBobble : MonoBehaviour
{
    public List<NPCBehavior> CaughtNCPS = new List<NPCBehavior>();
    public NPCBehavior NPCControl = null;
    public FishPoint FishingSpot = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FishPoint"))
        {
            FishingSpot = other.GetComponent<FishPoint>();
            if (FishingSpot != null)
            {
                Debug.Log("Catching Fish");
                StartCoroutine(FishingSpot.FishBite(this.gameObject));
            }
        }
    }
}
