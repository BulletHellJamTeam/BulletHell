using UnityEngine;

public class BatBulletManager : MonoBehaviour {
	private static float bulletSpeed = 250f;

    [SerializeField] private GameObject explosion;
    private float bulletDamage = 15f;

    public static GameObject Create(Vector3 pos, Vector3 target, GameObject bulletPrefab) {
		Vector3 dir = (target - pos).normalized;

		Quaternion lookDir = Quaternion.LookRotation(Vector3.Cross(dir, Vector3.up), Vector3.Cross(dir, -Vector3.forward));

		GameObject bullet = Instantiate(bulletPrefab, pos, lookDir);

		bullet.GetComponent<Rigidbody>().velocity = dir * bulletSpeed * Time.fixedDeltaTime;

		bullet.GetComponent<BatBulletManager>().Destroy(2f);

        return bullet;
    }

    public float GetDamage() { return bulletDamage; }

    public void Destroy() {
        gameObject.SetActive(false);

        GameObject expl = Instantiate(explosion, transform.position, Quaternion.identity);

        Destroy(expl, 0.2f);
        Destroy(gameObject, 0.2f);
    }

    public void Destroy(float timeTilDeath) {
        Invoke("Destroy", timeTilDeath);
    }
}
