using UnityEngine;

public class PlayerBulletManager : MonoBehaviour {
	private static float bulletSpeed = 500f;

    [SerializeField] private GameObject explosion;

    public static GameObject Create(Vector3 pos, Vector3 target, GameObject bulletPrefab) {
		Vector3 dir = (target - pos).normalized;

		Quaternion lookDir = Quaternion.LookRotation(Vector3.Cross(dir, Vector3.up), Vector3.Cross(dir, -Vector3.forward));

		GameObject bullet = Instantiate(bulletPrefab, pos, lookDir);

		bullet.GetComponent<Rigidbody>().velocity = dir * bulletSpeed * Time.fixedDeltaTime;

		bullet.GetComponent<PlayerBulletManager>().Destroy(2f);

        return bullet;
    }

    public void Destroy() {
        gameObject.SetActive(false);

        Instantiate(explosion, transform.position, Quaternion.identity);

        Destroy(gameObject, 0.2f);
    }
    public void Destroy(float timeTilDeath) {
        Invoke("Destroy", timeTilDeath);
    }
}
