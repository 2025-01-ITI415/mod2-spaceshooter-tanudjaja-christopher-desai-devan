using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoundsCheck))]
public class ProjectileHero : MonoBehaviour
{
    private BoundsCheck bndCheck;
    private Renderer rend;

    [Header("Dynamic")]
    public Rigidbody rigid;

    [SerializeField]
    private eWeaponType _type;

    [Header("Bomb Explosion Settings")]
    public float explosionRadius = 5f;
    public GameObject explosionEffectPrefab;
    public float explosionDelay = 2f;
    private bool hasCollided = false;

    public eWeaponType type
    {
        get { return _type; }
        set { SetType(value); }
    }

    void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        rend = GetComponent<Renderer>();
        rigid = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (explosionEffectPrefab == null)
        {
            explosionEffectPrefab = Resources.Load<GameObject>("DefaultExplosion");
            if (explosionEffectPrefab == null)
            {
                Debug.LogWarning($"No explosion effect prefab assigned to {gameObject.name} in Start()!");
            }
        }
    }

    void Update()
    {
        if (bndCheck.LocIs(BoundsCheck.eScreenLocs.offUp))
        {
            Destroy(gameObject);
        }
    }

    public void SetType(eWeaponType eType)
    {
        _type = eType;
        WeaponDefinition def = Main.GET_WEAPON_DEFINITION(_type);
        rend.material.color = def.projectileColor;

        if (_type == eWeaponType.bomb)
        {
            transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            if (def.explosionEffectPrefab != null)
            {
                explosionEffectPrefab = def.explosionEffectPrefab;
            }
            else
            {
                Debug.LogWarning($"WeaponDefinition for {_type} has no explosionEffectPrefab!");
            }
        }
    }

    public Vector3 vel
    {
        get { return rigid.velocity; }
        set { rigid.velocity = value; }
    }

    void OnCollisionEnter(Collision coll)
    {
        if (_type == eWeaponType.bomb && !hasCollided)
        {
            hasCollided = true;
            rigid.velocity = Vector3.zero;

            if (rend != null)
            {
                rend.enabled = false;
            }

            Explode();
            StartCoroutine(DelayedDestruction(0.5f));
        }
        else if (_type != eWeaponType.bomb)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator DelayedDestruction(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public void Explode()
    {
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($"No explosion effect prefab assigned to {gameObject.name} in Explode()!");
        }

        float damage = Main.GET_WEAPON_DEFINITION(_type).damageOnHit;
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null && enemy.IsOnScreen)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                float damagePercent = 1f - (distance / explosionRadius);
                if (damagePercent > 0)
                {
                    enemy.TakeDamage(damage * damagePercent);
                }
            }
        }
    }
}
