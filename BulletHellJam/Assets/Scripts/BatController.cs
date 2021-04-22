using UnityEngine;

public class BatController : MonoBehaviour {
    // state
    public enum BatState { ENTERING, FIGHTING };
    BatState state = BatState.ENTERING;

    // stats
    float health = 100;

    // timers
    private float newPathTimeMin = 0.2f, newPathTimeMax = 1.5f, newPathTime = 0f;
    private float newPathTimer = 0f;
    private float attackTimeMin = 1f, attackTimeMax = 3f, attackTime = 1.5f;
    private float attackTimer = 0f;

    // bounds
	private float LeftMoveBound, RightMoveBound, BottomMoveBound, TopMoveBound;

    // movement data
    private Rigidbody rigidbodyRef;
    private Vector3 targetPos;
    private float batSpeed = 2.5f;

    // visuals
    [SerializeField] private GameObject explosion;
    [SerializeField] private Material damageMaterial;
    [SerializeField] private GameObject body;
    [SerializeField] private GameObject wings;
    private Material originalMaterial;

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

            print(targetPos);

            newPathTimer = 0f;
            newPathTime = Random.Range(newPathTimeMin, newPathTimeMax);
        } newPathTimer += Time.fixedDeltaTime;

        if (state != BatState.FIGHTING) return;

        // every y seconds use one of two different basic attacks
        if (attackTimer > attackTime) {
            // atk1 - fire three projectiles at player
            // atk2 - fire a circle of projectiles in all directions

            print("attack");

            attackTimer = 0f;
            attackTime = Random.Range(attackTimeMin, attackTimeMax);
        } attackTimer += Time.fixedDeltaTime;
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

        GameObject exp = Instantiate(explosion, transform.position, Quaternion.identity);

        Destroy(exp, 0.5f);
        Destroy(gameObject, 0.5f);
    }
}
