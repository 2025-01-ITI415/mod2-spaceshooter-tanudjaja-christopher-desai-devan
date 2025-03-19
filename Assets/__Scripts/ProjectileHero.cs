using System.Collections;
using System.Collections.Generic;
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

    // Explosion related fields
    [Header("Explosion Settings")]
    public float explosionRadius = 5f;
    public GameObject explosionEffectPrefab; // Optional - for visual effects

    // This public property masks the private field _type
    public eWeaponType type
    {
        get { return (_type); }
        set { SetType(value); }
    }

    void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        rend = GetComponent<Renderer>();
        rigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (bndCheck.LocIs(BoundsCheck.eScreenLocs.offUp))
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Sets the _type private field and colors this projectile to match the 
    ///   WeaponDefinition.
    /// </summary>
    /// <param name="eType">The eWeaponType to use.</param>
    public void SetType(eWeaponType eType)
    {
        _type = eType;
        WeaponDefinition def = Main.GET_WEAPON_DEFINITION(_type);
        rend.material.color = def.projectileColor;

        // If it's a bomb, adjust the visuals/scale to make it look different
        if (_type == eWeaponType.bomb)
        {
            transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }
    }

    /// <summary>
    /// Allows Weapon to easily set the velocity of this ProjectileHero
    /// </summary>
    public Vector3 vel
    {
        get { return rigid.velocity; }
        set { rigid.velocity = value; }
    }

    void OnCollisionEnter(Collision coll)
    {
        // If this is a bomb, create an explosion instead of being destroyed immediately
        if (_type == eWeaponType.bomb)
        {
            Explode();
        }

        // The Enemy.cs will handle normal projectile collision logic
    }

    /// <summary>
    /// Creates an explosion that damages all enemies within the explosion radius
    /// </summary>
    public void Explode()
    {
        // Optional: Create explosion visual effect
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // Get the damage value for this bomb
        float damage = Main.GET_WEAPON_DEFINITION(_type).damageOnHit;

        // Find all enemies in the explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null && enemy.IsOnScreen)
            {
                // Calculate damage based on distance from explosion center
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                float damagePercent = 1f - (distance / explosionRadius);
                if (damagePercent > 0)
                {
                    // Apply damage to the enemy
                    enemy.TakeDamage(damage * damagePercent);
                }
            }
        }

        // Destroy the bomb projectile
        Destroy(gameObject);
    }
}