using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eWeaponType
{
    none,
    blaster,
    spread,
    Phaser,
    bomb,
    Laser,
    shield
}

[System.Serializable]
public class WeaponDefinition
{
    public eWeaponType type = eWeaponType.bomb;
    public string letter;
    public Color powerUpColor = Color.white;
    public GameObject weaponModelPrefab;
    public GameObject projectilePrefab;
    public Color projectileColor = Color.white;
    public float damageOnHit = 0;
    public float damagePerSec = 0;
    public float delayBetweenShots = 0;
    public float velocity = 0;
    public float explosionRadius = 5f;
}

public class Weapon : MonoBehaviour
{
    static public Transform PROJECTILE_ANCHOR;

    [Header("Dynamic")]
    [SerializeField]
    private eWeaponType _type = eWeaponType.bomb;
    public WeaponDefinition def;
    public float nextShotTime;

    private GameObject weaponModel;
    private Transform shotPointTrans;

    void Start()
    {
        if (PROJECTILE_ANCHOR == null)
        {
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }

        shotPointTrans = transform.GetChild(0);
        SetType(_type);

        Hero hero = GetComponentInParent<Hero>();
        if (hero != null) hero.fireEvent += Fire;
    }

    public eWeaponType type
    {
        get { return (_type); }
        set { SetType(value); }
    }

    public void SetType(eWeaponType wt)
    {
        _type = wt;
        if (type == eWeaponType.none)
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
        }

        def = Main.GET_WEAPON_DEFINITION(_type);
        if (def == null)
        {
            Debug.LogError($"WeaponDefinition for {_type} is NULL!");
            return;
        }
        if (def.weaponModelPrefab == null)
        {
            Debug.LogError($"weaponModelPrefab for {_type} is NULL in WeaponDefinition!");
            return;
        }

        if (weaponModel != null) Destroy(weaponModel);
        weaponModel = Instantiate<GameObject>(def.weaponModelPrefab, transform);
        weaponModel.transform.localPosition = Vector3.zero;
        weaponModel.transform.localScale = Vector3.one;

        nextShotTime = 0;
    }

    private void Fire()
    {
        if (!gameObject.activeInHierarchy) return;
        if (Time.time < nextShotTime) return;

        ProjectileHero p;
        Vector3 vel = Vector3.up * def.velocity;

        switch (type)
        {
            case eWeaponType.blaster:
                p = MakeProjectile();
                p.vel = vel;
                break;

            case eWeaponType.spread:
                p = MakeProjectile();
                p.vel = vel;
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(10, Vector3.back);
                p.vel = p.transform.rotation * vel;
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(-10, Vector3.back);
                p.vel = p.transform.rotation * vel;
                break;

            case eWeaponType.bomb:
                p = MakeProjectile();
                p.vel = vel;
                break;
        }

        nextShotTime = Time.time + def.delayBetweenShots;
    }

    private ProjectileHero MakeProjectile()
    {
        if (def == null || def.projectilePrefab == null)
        {
            Debug.LogError($"ProjectilePrefab for {_type} is NULL in WeaponDefinition!");
            return null;
        }

        GameObject go = Instantiate<GameObject>(def.projectilePrefab, PROJECTILE_ANCHOR);
        ProjectileHero p = go.GetComponent<ProjectileHero>();

        Vector3 pos = shotPointTrans.position;
        pos.z = 0;
        p.transform.position = pos;
        p.type = type;

        if (type == eWeaponType.bomb && def.explosionRadius > 0)
        {
            p.explosionRadius = def.explosionRadius;
        }

        return p;
    }
}
