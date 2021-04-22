using UnityEngine;
using UnityEngine.InputSystem;
using EZCameraShake;
using System.Collections;

public class PlayerController : MonoBehaviour {
	// state
    public enum PlayerState { MOVING, DASHING, MELEE };
	private PlayerState state = PlayerState.MOVING;
	private PlayerState oldState = PlayerState.MOVING;

	// stats
	private float health = 500;
	private int orbs = 0;

	// references
	[SerializeField] private Animator animRef;
	[SerializeField] private GameObject explosion;
    [SerializeField] private Material damageMaterial;
    [SerializeField] private GameObject hair;
    [SerializeField] private GameObject hair2;
    [SerializeField] private GameObject body;
    private Material originalHairMaterial;
    private Material originalBodyMaterial;
	private Rigidbody rigidBodyRef;

	// bullet prefabs
	[SerializeField] private GameObject bulletPrefab;
	[SerializeField] private GameObject bulletExplosion;
	[SerializeField] private GameObject[] Spirits = new GameObject[5];
	private float bulletTime = 0.1f, bulletTimer = 0f;
	private bool bulletsEnabled = false;

	// movement
	private Vector2 rawInputMovement, rawMousePosition;
	private float playerSpeed = 10f;
	private float maxSpeed = 10f;

	// slowdown
	private float slowdownDuration = 0.5f;
	private float slowdownTimeElapsed = 0f;

	// dashing
	[SerializeField] private GameObject echoPrefab;
	private float dashSpeed = 100f;
	private float dashTime = 0.05f, dashTimer = 0f;
	private float dashBackTime = 2.5f, dashBackTimer = 0f;
	private float dashEchoTime = 0.001f, dashEchoTimer = 0f;
	private bool justDashed = false;

	private void Awake() {
		rigidBodyRef = gameObject.GetComponent<Rigidbody>();

        originalHairMaterial = hair.GetComponent<SkinnedMeshRenderer>().material;
        originalBodyMaterial = body.GetComponent<SkinnedMeshRenderer>().material;
	}

	private void FixedUpdate() {
		if (state == PlayerState.MOVING) {
			Vector3 inputMovement = rawInputMovement.normalized * playerSpeed;

			rigidBodyRef.AddForce(inputMovement);

			if(rigidBodyRef.velocity.magnitude > maxSpeed){
			     rigidBodyRef.velocity = Vector3.ClampMagnitude(rigidBodyRef.velocity, maxSpeed);
			}

			if (inputMovement == Vector3.zero && slowdownTimeElapsed < slowdownDuration) {
				rigidBodyRef.velocity = Vector3.Lerp(rigidBodyRef.velocity, Vector3.zero, slowdownTimeElapsed / slowdownDuration);
				slowdownTimeElapsed += Time.fixedDeltaTime;
			} else {
				slowdownTimeElapsed = 0f;
			}
		} else if (state == PlayerState.DASHING) {
			dashTimer += Time.fixedDeltaTime;
			dashEchoTimer += Time.fixedDeltaTime;

			if (dashEchoTimer > dashEchoTime) {
				GameObject echo = (GameObject)Instantiate(echoPrefab, rigidBodyRef.transform.position, Quaternion.identity);
				Destroy(echo, 0.5f);

				dashEchoTimer = 0f;
			}
			
			if (dashTimer > dashTime) {
				rigidBodyRef.velocity = Vector3.zero;

				state = oldState;
			}
		}

		if (justDashed) {
			dashBackTimer += Time.fixedDeltaTime;

			if (dashBackTimer > dashBackTime) justDashed = false;
		}

		if (bulletsEnabled) {
			if (bulletTimer > bulletTime) {
				Vector3 mousePosition = new Vector3(rawMousePosition.x, rawMousePosition.y, 0f);
				Vector3 target = Camera.main.ScreenToWorldPoint(mousePosition);
				target = new Vector3(target.x, target.y, 0f);

				for (int i=0; i<Spirits.Length; i++) {
					if (Spirits[i].activeSelf) {
						PlayerBulletManager.Create(Spirits[i].transform.position, target, bulletPrefab);
                    }
                } bulletTimer = 0f;
            } bulletTimer += Time.fixedDeltaTime;
        }

		animRef.SetFloat("VelocityX", rigidBodyRef.velocity.x);
		animRef.SetFloat("VelocityY", rigidBodyRef.velocity.y);
	}

	public void OnMovement(InputAction.CallbackContext value) {
		rawInputMovement = value.ReadValue<Vector2>();
	}

	public void OnMouse(InputAction.CallbackContext value) {
		rawMousePosition = value.ReadValue<Vector2>();
	}

	public void OnLeftClick(InputAction.CallbackContext value) {
		if (state == PlayerState.MELEE) return;

		if (value.started) bulletsEnabled = true;
		if (value.canceled) bulletsEnabled = false;
	}

	public void OnRightClick(InputAction.CallbackContext value) {
		if (bulletsEnabled) return;

		if (value.started && state != PlayerState.MELEE && state != PlayerState.DASHING) {
			StartCoroutine(MeleeAttack());
		}
	}

	private IEnumerator MeleeAttack() {
		// oldState = state;
		// state = PlayerState.MELEE;

		// rigidBodyRef.velocity = Vector3.zero;

		// animRef.SetBool("Melee", true);

		// AnimatorStateInfo animState = animRef.GetCurrentAnimatorStateInfo(0);
		// while (!animState.IsName("BHJ_AttackCombo1")) {
		// 	animState = animRef.GetCurrentAnimatorStateInfo(0);
		// 	yield return new WaitForSeconds(0.1f);
		// }

		// yield return new WaitForSeconds(animState.length - animState.normalizedTime - 0.8f);

		// animRef.SetBool("Melee", false);
		// state = oldState;

		yield return null;
	}

	public void OnDash(InputAction.CallbackContext value) {
		if (value.started && state != PlayerState.DASHING && state != PlayerState.MELEE) Dash();
	}

	private void Dash() {
		oldState = state;
		state = PlayerState.DASHING;

		dashTimer = 0f;
		dashEchoTimer = 0f;

		CameraShaker.Instance.ShakeOnce(2f, 2f, 0.1f, 0.5f);

		if (justDashed) {
			rigidBodyRef.velocity = Vector3.left * dashSpeed;
			justDashed = false;
		} else {
			rigidBodyRef.velocity = Vector3.right * dashSpeed;
			dashBackTimer = 0f;
			justDashed = true;
		}
	}

    public void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Orb")) {
			orbs++;
			Destroy(other.gameObject);
        } else if (other.gameObject.CompareTag("EnemyBullet")) {
			BatBulletManager pbm = other.gameObject.GetComponent<BatBulletManager>();

            health -= pbm.GetDamage();

            pbm.Destroy();

            if (health <= 0f) {
                SelfDestruct();
            } else {
                body.GetComponent<SkinnedMeshRenderer>().material = damageMaterial;
                hair.GetComponent<SkinnedMeshRenderer>().material = damageMaterial;
                hair2.GetComponent<SkinnedMeshRenderer>().material = damageMaterial;

                Invoke("ResetMaterial", 0.2f);
            }
		} else if (other.gameObject.CompareTag("BossBullet")) {
			BossBulletManager pbm = other.gameObject.GetComponent<BossBulletManager>();

            health -= pbm.GetDamage();

            pbm.Destroy();

            if (health <= 0f) {
                SelfDestruct();
            } else {
                body.GetComponent<SkinnedMeshRenderer>().material = damageMaterial;
                hair.GetComponent<SkinnedMeshRenderer>().material = damageMaterial;
                hair2.GetComponent<SkinnedMeshRenderer>().material = damageMaterial;

                Invoke("ResetMaterial", 0.2f);
            }
		}
    }

    private void ResetMaterial() {
        body.GetComponent<SkinnedMeshRenderer>().material = originalBodyMaterial;
        hair.GetComponent<SkinnedMeshRenderer>().material = originalHairMaterial;
        hair2.GetComponent<SkinnedMeshRenderer>().material = originalHairMaterial;
    }
    
    public void SelfDestruct() {
        gameObject.SetActive(false);

        GameObject exp = Instantiate(explosion, transform.position, Quaternion.identity);

        Destroy(exp, 0.5f);
        Destroy(gameObject, 0.5f);
    }

}
