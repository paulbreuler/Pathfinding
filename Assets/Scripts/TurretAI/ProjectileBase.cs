using UnityEngine;
using System.Collections;

public abstract class ProjectileBase : MonoBehaviour {
    public float speed = 5;
    public abstract void FireProjectile(GameObject projectileSpawn, GameObject target, int damage);
}
