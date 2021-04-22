using UnityEngine;

public class BatController : MonoBehaviour {
    public enum BatState { ENTERING, FIGHTING };

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
    private float batSpeed = 5f;

    BatState state = BatState.ENTERING;

    private void Awake() {
        rigidbodyRef = GetComponent<Rigidbody>();
        targetPos = rigidbodyRef.transform.position;
    }

    private void Start() {
		LeftMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0.25f, 0f, 0f)).x;
		RightMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0.75f, 0f, 0f)).x;

		BottomMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0.25f, 0f)).y;
		TopMoveBound = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0.75f, 0f)).y;

        newPathTimer = newPathTime;
    }

    private void FixedUpdate() {
        newPathTimer += Time.fixedDeltaTime;

        // every x seconds, path to a new random place within bounds
        if (newPathTimer > newPathTime) {
            // set a new destination
            targetPos = new Vector3(
                Random.Range(LeftMoveBound, RightMoveBound),
                Random.Range(BottomMoveBound, TopMoveBound),
                0f
                );

            print(targetPos);

            newPathTimer = 0f;
            newPathTime = Random.Range(newPathTimeMin, newPathTimeMax);
        }

        // move bat!
        Vector3 moveDir = targetPos - rigidbodyRef.transform.position;
        Vector3 moveDist = moveDir.normalized * batSpeed * Time.fixedDeltaTime;

        if (moveDir.magnitude > 0.5f) //rigidbodyRef.MovePosition(rigidbodyRef.transform.position + moveDist);
            rigidbodyRef.AddForce(moveDist);

        if (state != BatState.FIGHTING) return;

        attackTimer += Time.fixedDeltaTime;

        // every y seconds use one of two different basic attacks
        if (attackTimer > attackTime) {
            // atk1 - fire three projectiles at player
            // atk2 - fire a circle of projectiles in all directions

            print("attack");

            attackTimer = 0f;
            attackTime = Random.Range(attackTimeMin, attackTimeMax);
        }
    }
}
