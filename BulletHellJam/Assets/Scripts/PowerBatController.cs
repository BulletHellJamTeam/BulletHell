using System.Collections;
using UnityEngine;

public class PowerBatController : MonoBehaviour {
    // state
    public enum BatState { ENTERING, FIGHTING };
    BatState state = BatState.ENTERING;

    // stats
    float health = 200;

    // timers
    private float newPathTimeMin = 0.2f, newPathTimeMax = 1.5f, newPathTime = 0f;
    private float newPathTimer = 0f;
    private float attackTimeMin = 1f, attackTimeMax = 3f, attackTime = 1.5f;
    private float attackTimer = 0f;

    // bounds
	private float LeftMoveBound, RightMoveBound, BottomMoveBound, TopMoveBound;

    // bullets
	[SerializeField] private GameObject bulletPrefab;
	[SerializeField] private GameObject[] attack1BulletSpawners = new GameObject[3];
    private GameObject playerRef;
    private float attack1Rate = 0.5f;

    // movement data
    private Rigidbody rigidbodyRef;
    private Vector3 targetPos;
    private float batSpeed = 5f;

    // visuals
    [SerializeField] private GameObject explosion;
    [SerializeField] private Material damageMaterial;
    [SerializeField] private GameObject body;
    [SerializeField] private GameObject wings;
    private Material originalMaterial;

    // orbs
    [SerializeField] private GameObject orbPrefab;

    private void Awake() {
        rigidbodyRef = GetComponent<Rigidbody>();
        targetPos = rigidbodyRef.transform.position;

        originalMaterial = body.GetComponent<SkinnedMeshRenderer>().material;
    }

    private void Start() {
		LeftMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0.01f, 0f, 0f)).x;
		RightMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0.99f, 0f, 0f)).x;

		BottomMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0.01f, 0f)).y;
		TopMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0.99f, 0f)).y;

        playerRef = GameObject.Find("Player");

        newPathTimer = newPathTime;
    }

    private void FixedUpdate() {
        Vector3 moveDir = targetPos - rigidbodyRef.transform.position;
        Vector3 moveDist = moveDir.normalized * batSpeed * Time.fixedDeltaTime;

        if (moveDir.x > 0) rigidbodyRef.transform.rotation = Quaternion.Euler(0, 90f, 0f);
        else rigidbodyRef.transform.rotation = Quaternion.Euler(0, -90f, 0f);

        rigidbodyRef.MovePosition(rigidbodyRef.transform.position + moveDist);

        if (newPathTimer > newPathTime || moveDir.magnitude < 0.5f) {
            // set a new destination
            targetPos = new Vector3(
                Random.Range(LeftMoveBound, RightMoveBound),
                Random.Range(BottomMoveBound, TopMoveBound),
                0f
                );

            newPathTimer = 0f;
            newPathTime = Random.Range(newPathTimeMin, newPathTimeMax);
        } newPathTimer += Time.fixedDeltaTime;

        if (rigidbodyRef.transform.position.x < RightMoveBound) state = BatState.FIGHTING;

        if (state != BatState.FIGHTING) return;

        if (attackTimer > attackTime) {
            // atk1 - fire three projectiles at player
            StartCoroutine(Attack1());
            // atk2 - fire a circle of projectiles in all directions

            attackTimer = 0f;
            attackTime = Random.Range(attackTimeMin, attackTimeMax);
        } attackTimer += Time.fixedDeltaTime;
    }

    private IEnumerator Attack1() {
        for (int j=0; j<6; j++) {
			for (int i=0; i<attack1BulletSpawners.Length; i++) {
				if (attack1BulletSpawners[i].activeSelf) {
                    if (playerRef != null) BatBulletManager.Create(attack1BulletSpawners[i].transform.position, playerRef.transform.position, bulletPrefab);
                }
            } yield return new WaitForSeconds(attack1Rate); 
        } yield return null;
    }


    public void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("PlayerBullet")) {
			PlayerBulletManager pbm = other.gameObject.GetComponent<PlayerBulletManager>();

            health -= pbm.GetDamage();

            pbm.Destroy();

            if (health <= 0f) {
                SelfDestruct();
            } else {
                body.GetComponent<SkinnedMeshRenderer>().material = damageMaterial;
                wings.GetComponent<SkinnedMeshRenderer>().material = damageMaterial;

                Invoke("ResetMaterial", 0.2f);
            }
        }
    }

    private void ResetMaterial() {
        body.GetComponent<SkinnedMeshRenderer>().material = originalMaterial;
        wings.GetComponent<SkinnedMeshRenderer>().material = originalMaterial;
    }

    public void SelfDestruct() {
        gameObject.SetActive(false);

        DropOrbs(rigidbodyRef.transform.position, (int)Random.Range(15f, 30f), -0.25f, 0.25f, -0.25f, 0.25f);

        GameObject exp = Instantiate(explosion, transform.position, Quaternion.identity);

        Destroy(exp, 0.5f);
        Destroy(gameObject, 0.5f);
    }

	public void DropOrbs(Vector3 loc, int numOrbs, float minX, float maxX, float minY, float maxY) {
		for (int i=0; i<numOrbs; i++) {
			Vector3 pos = new Vector3(loc.x + Random.Range(minX, maxX), loc.y + Random.Range(minY, maxY), 0f);
			Instantiate(orbPrefab, pos, Quaternion.identity);
		}
	}
}
