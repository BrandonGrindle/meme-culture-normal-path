using StarterAssets;
using System.Collections;
using System.Collections.Generic;
//using UnityEditor.PackageManager;
using UnityEngine;


public class WeaponController : MonoBehaviour
{
    public static WeaponController instance;

    [Header("Weapon Stats")]
    //[SerializeField] private float AttackCooldown = 2.4f;
    [SerializeField] private float AttackRange = .5f;
    [SerializeField] private GameObject WeaponOrigin;
    [SerializeField] private int weaponDmg = 5;

    public bool canAttack = true;
    private int layerMask;
    public Camera _mainCamera;

    public bool attacking;
    private void Awake()
    {
        instance = this;
        layerMask = ~LayerMask.GetMask("Player");
    }


    public void Attack(Animator anim, int AnimID)
    {
        Vector3 attackDirection = _mainCamera.transform.forward;
        Vector3 origin = WeaponOrigin.transform.position;

        if (Vector3.Dot(WeaponOrigin.transform.forward, attackDirection) > 0)
        {
            Ray raycast = new Ray(origin, attackDirection);
            if (Physics.Raycast(raycast, out RaycastHit hit, AttackRange, layerMask))
            {
                anim.SetBool(AnimID, true);
                if (hit.collider.CompareTag("Enemy"))
                {
                    EnemyAI EnemyScript = hit.collider.gameObject.GetComponentInParent<EnemyAI>();
                    if (EnemyScript != null)
                    {
                        EnemyScript.DamageTaken(weaponDmg);
                    }
                    else
                    {
                        Debug.Log("no enemy script found");
                    }
                }
            }
            else
            {
                Debug.Log("Nothing was hit by the attack.");
            }
        }
        else
        {
            Debug.Log("Attack direction is invalid (backwards through player).");
        }
    }
}
