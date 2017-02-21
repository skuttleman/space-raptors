using UnityEngine;
using System.Collections;

public class MainCannon : MonoBehaviour {
	public bool shootem;
	public bool destroyem;
	public float shootToleranceY;
	public int cannonCoolDown;
	public GameObject mainProjectile;
	public GameObject platformProjectile;
	public int cannonTimer;

	private bool shooting;
	private GameObject myPlatform;
	private GameObject player;
	private GameObject playerPlatform;
	private GameObject lastProjectile;

	// Use this for initialization
	void Start () {
		player = GameObject.Find("Player");
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!shootem) return;
		myPlatform = Methods.onTaggedObject(this.gameObject, 0.1f, "Platform");
		playerPlatform = Methods.onTaggedObject(player, 0.1f, "Platform");

		if (cannonTimer != 0 && !shooting) cannonTimer--;

		if (cannonTimer == 0 && myPlatform != null) {
			shooting = true;
			cannonTimer = cannonCoolDown;
			FollowAI follow = GetComponent<FollowAI>();
			MeleeAttack attack = GetComponent<MeleeAttack>();

			follow.getem = false;
			attack.smackem = false;
			FacePlayer();

			if (Mathf.Abs(transform.position.y - player.transform.position.y) <= shootToleranceY) {
				StartCoroutine(AimAndShoot());
			} else {
				if (playerPlatform && destroyem && lastProjectile != platformProjectile) {
					StartCoroutine(PlatformShoot(playerPlatform));
				} else {
					StartCoroutine(AimAndShoot());
				}
			}
		}
	}

	private IEnumerator AimAndShoot() {
		lastProjectile = mainProjectile;
		//Animate aimining...
		SpriteRenderer renderer = GetComponent<SpriteRenderer>();
		renderer.color = new Color(0, 0, 1, 1);
		yield return new WaitForSeconds(3);

		GameObject blast = (GameObject) Instantiate(mainProjectile, new Vector2(transform.position.x, transform.position.y), Quaternion.identity);
		blast.GetComponent<ProjectileController>().direction.x = -transform.localScale.x;

		yield return new WaitForSeconds(0.5f);
		renderer.color = new Color(1, 1, 1, 1);

		FollowAI follow = GetComponent<FollowAI>();
		MeleeAttack attack = GetComponent<MeleeAttack>();
		shooting = false;
		follow.getem = true;
		attack.smackem = true;
	}

	private IEnumerator PlatformShoot(GameObject platform) {
		lastProjectile = platformProjectile;
		//Animate aimining...
		SpriteRenderer renderer = GetComponent<SpriteRenderer>();
		renderer.color = new Color(0, 1, 0, 1);
		yield return new WaitForSeconds(3);

		Vector2 direction = platform.transform.position - transform.position;
		float denom = Mathf.Abs(direction.x) > Mathf.Abs(direction.y) ? direction.x : direction.y;
		direction.x /= denom;
		direction.y /= denom;

		GameObject blast = (GameObject) Instantiate(platformProjectile, new Vector2(transform.position.x, transform.position.y), Quaternion.identity);
		blast.GetComponent<ProjectileController>().direction = direction;

		yield return new WaitForSeconds(0.5f);
		renderer.color = new Color(1, 1, 1, 1);

		FollowAI follow = GetComponent<FollowAI>();
		MeleeAttack attack = GetComponent<MeleeAttack>();
		shooting = false;
		follow.getem = true;
		attack.smackem = true;
	}

	private void FacePlayer() {
		transform.localScale = new Vector3(
			player.transform.position.x < transform.position.x ? 1f : -1f,
			transform.localScale.y,
			transform.localScale.z);
	}
}
