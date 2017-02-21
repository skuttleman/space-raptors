using UnityEngine;
using System.Collections;

public class MeleeAttack : MonoBehaviour {
	public int meleeRate;
	public float chargeSpeed;
	public float chargeRange;
	public float damageRange;
	public bool smackem;


	private int meleeCooldown;
	private GameObject player;
	private GameObject myPlatform;
	private GameObject playerPlatform;
	private GameObject arm;
	private float chargeDistance;

	// Use this for initialization
	void Start () {
		player = GameObject.Find("Player");
		arm = GetChild("MeleeCollision");
		arm.SetActive(false);
		smackem = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (!smackem) return;
		playerPlatform = Methods.onTaggedObject(player, 0.1f, "Platform");
		myPlatform = Methods.onTaggedObject(this.gameObject, 0.1f, "Platform");
		float speed = GetComponent<FollowAI>().capabilities.speed;

		if (meleeCooldown != 0) {
			if (GetComponent<FollowAI>().getem) meleeCooldown--;
			if (playerPlatform && myPlatform && playerPlatform == myPlatform) {
				// Back up after attack
				Methods.ObjectWithCorners corners = new Methods.ObjectWithCorners(myPlatform);
				Vector2 relevant = transform.localScale.x < 0 ? corners.corners.topLeft : corners.corners.topRight;
				if (Mathf.Abs(transform.position.x - relevant.x) > speed * Time.deltaTime) {
					transform.Translate(new Vector2(transform.localScale.x * speed / 3f * Time.deltaTime, 0));
				}
			}
		} else if (GetComponent<FollowAI>().getem) {
			if (playerPlatform && myPlatform && playerPlatform == myPlatform) {
				if (DistanceToPlayerX() <= chargeRange) {
					// ATTACK!
					StartCoroutine(Attack());
				} else {
					transform.Translate(new Vector2(-transform.localScale.x * speed * Time.deltaTime, 0));
				}
			}
		}
	}

	private void FacePlayer() {
		transform.localScale = new Vector3(
			player.transform.position.x < transform.position.x ? 1f : -1f,
			transform.localScale.y,
			transform.localScale.z);
	}

	private float DistanceToPlayerX() {
		return Mathf.Abs(player.transform.position.x - transform.position.x);
	}

	private IEnumerator Attack() {
		FollowAI follow = GetComponent<FollowAI>();
		MainCannon cannon = GetComponent<MainCannon>();
		if (follow != null) follow.getem = false;
		if (cannon != null) cannon.shootem = false;
		FacePlayer();
		arm.SetActive(true);
		yield return new WaitForFixedUpdate();
		chargeDistance = chargeRange;
		while (Methods.distance(player.transform.position, transform.position) > damageRange && chargeDistance > 0) {
			float distance = chargeSpeed * -transform.localScale.x * Time.deltaTime;
			chargeDistance -= Mathf.Abs(distance);
			transform.Translate(new Vector2(distance, 0));
			yield return new WaitForFixedUpdate();
		}
		// yield Attack Animation
		yield return new WaitForSeconds(0.2f);
		meleeCooldown = meleeRate;
		arm.SetActive(false);
		if (follow != null) {
			follow.getem = true;
		}
		if (cannon != null) {
			cannon.shootem = true;
		}
	}

	private GameObject GetChild(string name) {
		Transform[] transforms = this.gameObject.GetComponentsInChildren<Transform>();
		foreach (Transform t in transforms) {
			if (t.gameObject.name == name) {
				return t.gameObject;
			}
		}
		return null;
	}
}
