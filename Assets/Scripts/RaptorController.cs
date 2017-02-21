using UnityEngine;
using System.Collections;

public class RaptorController : MonoBehaviour {
	private class Obstacles {
		public RaycastHit2D forward;
		public RaycastHit2D upward;
		public RaycastHit2D downward;
		public RaycastHit2D backward;
		
		public Obstacles(RaycastHit2D forward, RaycastHit2D upward, RaycastHit2D downward, RaycastHit2D backward) {
			this.forward = forward;
			this.upward = upward;
			this.downward = downward;
			this.backward = backward;
		}
	}

	public int health;
	public float speed;
	public Vector2 shotOffset;
	public GameObject currentWeapon;
	public int shootingCooldown;
	public Animator animator;
	public float activationDistance;
	public float sightDistance;
	public int waitAfterShooting;
	public float patrolSpeed;
	public float jumpForce;
	public float maxPlayerProximity;
	public int stealthTimeout;
	public int pointValue;
	public float willShootY;
	public GameObject[] drops;

	private int shotCooldown;
	private int stealthReset;
	private int moveWait;
	private GameObject player;
	private float faceSensitivity = 0.5f;
	private bool awareOfPlayer;
	private BoxCollider2D myCollider;
	private float roughRadius;
	private float turnWait;
	private Rigidbody2D body;
	private SoundFXManager SoundFXManager;
	private AudioSource audioSource;
	private Vector2 lastKnownPos;
	private float lastSpin;
	private float lastJump;
	private SoundFXManager soundFXManager;

	void Start() {
		player = GameObject.Find("Player");
		shotCooldown = 5;
		moveWait = 5;
		turnWait = patrolSpeed;
		awareOfPlayer = false;
		myCollider = this.gameObject.GetComponent<BoxCollider2D>();
		body = this.gameObject.GetComponent<Rigidbody2D>();
		roughRadius = Mathf.Sqrt(Mathf.Pow(myCollider.bounds.extents.x * 2, 2f) + Mathf.Pow(myCollider.bounds.extents.y * 2, 2f));
		soundFXManager = GameObject.Find("SoundFX").GetComponent<SoundFXManager>();
		audioSource = gameObject.GetComponent<AudioSource>();
		lastKnownPos = player.transform.position;
	}

	void OnCollisionEnter2D(Collision2D other) {
		bool wall = false;
		Methods.forEach(other.contacts, contact => {
			if (Mathf.Abs(contact.point.x - transform.position.x) > Mathf.Abs(contact.point.y - transform.position.y)) {
				wall = true;
			}
		});
		if (wall && other.transform.gameObject == player) {
			transform.Translate(new Vector2(0.1f * -transform.localScale.x, 0f));
		}
	}

	void FixedUpdate() {
		// Only update if within activationDistance
		if (Methods.distance(player.transform.position, transform.position) > activationDistance) {
			return;
		}

		// CoolDowns
		if (awareOfPlayer) {
			CoolDowns();
		}

		// Knows of Player
		Obstacles obstacles = ScanTerrain();
		if (!awareOfPlayer) {
			awareOfPlayer = CanSeePlayer();
		}

		// Patrol
		if (!awareOfPlayer) {
			Patrol(obstacles);
		}

		// Follow and Attack
		if (awareOfPlayer) {
			FollowAttack(obstacles);
		}
	}

	public void TakeDamage(int damage) {
		awareOfPlayer = true;
		Face(player.transform.position);
		health -= damage;
		if (health <= 0) {
			Destroy(gameObject);
			player.SendMessage("GetPoints", pointValue);
			RandomDrop();
		} else {
			this.gameObject.SendMessage("DisplayDamage");
		}
	}

	public void RandomDrop () {
		GameObject drop;
		int randomizer = Random.Range(0, 8);
		if (randomizer == 0) {
			return;
		} else if (randomizer == 2) {
			if (player.GetComponent<PlayerController>().weapons.Length > 1) {
				drop = Instantiate(drops[0]);
			} else {
				drop = Instantiate(drops[1]);
			}
		} else if (randomizer == 3) {
			drop = Instantiate(drops[2]);
		} else {
			drop = Instantiate(drops[1]);
		}
		if (drop != null) {
			drop.transform.position = transform.position;
		}
	}

	private void CoolDowns() {
		if (shotCooldown > 0) shotCooldown--;
		if (moveWait > 0) moveWait--;
		if (stealthReset >0) stealthReset--;
		lastSpin += Time.deltaTime;
		lastJump += Time.deltaTime;
	}

	private Obstacles ScanTerrain() {
		RaycastHit2D forward = Methods.RaycastClosest(transform.position, new Vector2(-transform.localScale.x, 0), transform);
		RaycastHit2D upward = Methods.RaycastClosest(transform.position, new Vector2(-transform.localScale.x, 1f), transform);
		RaycastHit2D downward = Methods.RaycastClosest(transform.position, new Vector2(-transform.localScale.x, -1f), transform);
		RaycastHit2D backward = Methods.RaycastClosest(transform.position, new Vector2(transform.localScale.x, 0), transform);
		return new Obstacles(forward, upward, downward, backward);
	}

	private bool CanSeePlayer() {
		Vector2 direction = player.transform.position - transform.position;
		float divisor = Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.y));
		direction.x /= divisor;
		direction.y /= divisor;
		RaycastHit2D ray = Methods.RaycastClosest(transform.position, direction, transform);
		return ray.transform.gameObject == player &&
			Mathf.Approximately(direction.x, -transform.localScale.x) &&
			Methods.distance(transform.position, player.transform.position) <= sightDistance &&
			!player.GetComponent<PlayerController>().stealth;
	}

	private void Patrol(Obstacles obstacles) {
		float distanceToGround = Methods.distance(obstacles.downward.point, transform.position);
		float distanceToWall = Methods.distance(obstacles.forward.point, transform.position);
		if (distanceToGround <= roughRadius * 2f && distanceToWall >= roughRadius / 1.5f) {
			AnimateMove(new Vector2(speed / 2f * -transform.localScale.x * Time.deltaTime, 0.1f));
		} else if (turnWait != 0f) {
			animator.SetBool("walking", false);
			turnWait --;
		}
		else {
			animator.SetBool("walking", false);
			turnWait = patrolSpeed;
			FlipScaleX();
		}
	}

	private void Face(Vector2 position) {
		if (position.x - transform.position.x > faceSensitivity) {
			SetScaleX(-1f);
		} else if (transform.position.x - position.x > faceSensitivity) {
			SetScaleX(1f);
		}
	}

	private void AnimateMove(Vector2 amount) {
		if (OnGround()) {
			animator.SetBool("walking", true);
		}
		transform.Translate(amount);
	}

	private bool OnGround() {
		GameObject ret = Methods.onSomething(this.gameObject, 0.1f);
		return Mathf.Abs(GetComponent<Rigidbody2D>().velocity.y) < 0.5f && (ret == null || ret.name != "Player");
	}

	private void FollowAttack(Obstacles obstacles) {
		bool visible = !player.GetComponent<PlayerController>().stealth;
		if (visible && CanSeePlayer()) {
			lastKnownPos = player.transform.position;
			stealthReset = stealthTimeout;
		}
		GameObject onSomething = Methods.onSomething(this.gameObject, 0.1f);
		GameObject playerOn = Methods.onSomething(player, 0.1f);
		if (stealthReset == 0 && (visible ||
			(onSomething != null && onSomething.name != "Player") ||	
			(playerOn != null && playerOn.name != this.gameObject.name))
		) {
			awareOfPlayer = false;
		}

		if (shotCooldown == 0 && Mathf.Abs(lastKnownPos.y - transform.position.y) <= willShootY) {
			Face(lastKnownPos);
			Shoot();
		} else if (moveWait == 0) {
			if (Mathf.Abs(transform.position.y - lastKnownPos.y) * 3f > Mathf.Abs(transform.position.x - lastKnownPos.x) &&
				transform.position.y > lastKnownPos.y
			) {
				GetDown(obstacles);
			} else if (OnGround() &&
				(lastKnownPos.y - transform.position.y > 0.2 ||
					(obstacles.forward && obstacles.forward.distance < roughRadius * 3f && obstacles.forward.transform.gameObject != player) ||
					(!obstacles.downward || (obstacles.downward.distance > roughRadius * 2f && lastKnownPos.y > obstacles.downward.point.y)))
			) {
				Face(lastKnownPos);
				Jump();
			} else if (Mathf.Abs(transform.position.x - lastKnownPos.x) > maxPlayerProximity) {
				Face(lastKnownPos);
				Jump();
				AnimateMove(new Vector2(transform.localScale.x * speed * Time.deltaTime * -1f, 0.1f));
			}
		} else {
			animator.SetBool("walking", false);
		}
	}

	private void FlipScaleX() {
		SetScaleX(transform.localScale.x * -1f);
	}

	private void SetScaleX(float x) {
		transform.localScale = new Vector3(
			x,
			transform.localScale.y,
			transform.localScale.z
		);
	}

	private void Shoot() {
		animator.SetBool("walking", false);
		animator.SetTrigger("shoot");
		GameObject newShot = (GameObject) Instantiate(currentWeapon, new Vector2(gameObject.transform.position.x + (shotOffset.x * transform.localScale.x), gameObject.transform.position.y + shotOffset.y), Quaternion.identity);
		newShot.GetComponent<ProjectileController>().direction.x = -transform.localScale.x;
		shotCooldown = shootingCooldown;
		moveWait = waitAfterShooting;
	}

	private void GetDown(Obstacles obstacles) {
		if (obstacles.forward.distance <= roughRadius || obstacles.upward.distance <= roughRadius) {
			TurnAround();
		}
		if (obstacles.downward.distance > roughRadius || lastKnownPos.y > transform.position.y) {
			Jump();
		}
		AnimateMove(new Vector2(speed * -transform.localScale.x * Time.deltaTime, OnGround() ? 0.1f : 0f));
	}

	private void Jump() {
		if (OnGround() && lastJump > 1f) {
			lastJump = 0f;
			animator.SetBool("walking", false);
			body.AddForce(new Vector2(transform.localScale.x * 50f, jumpForce));
			soundFXManager.Play(audioSource, "action", "raptor_jump");
		}
	}

	private void TurnAround() {
		if (lastSpin > 1f) {
			lastSpin = 0f;
			FlipScaleX();
		}
	}
}
