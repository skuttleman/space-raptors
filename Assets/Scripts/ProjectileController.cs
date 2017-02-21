using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour {
	public int damage;
	public float speed;
	public Animator animator;
	public Vector3 direction;
	public int weaponId;
	public AudioSource hitSound;
	public float angleY;

	private Vector3 boundsMin;
	private Vector3 boundsMax;

	void Start() {
		transform.localScale = new Vector3(
			direction.x < 0f ? -1f : 1f,
			direction.y < 0f ? -1f : 1f,
			transform.localScale.z
		);
		boundsMin = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f));
		boundsMax = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
	}

	void Update() {
		if (animator.GetBool("hit") == false) {
			transform.Translate(
				speed * direction.x * Time.deltaTime,
				speed * direction.y * Time.deltaTime,
				speed * direction.z * Time.deltaTime
			);
		}
		if (transform.position.x > boundsMax.x || transform.position.x < boundsMin.x) {
			DestroyThis();
		}
	}

	void OnTriggerEnter2D(Collider2D collision) {
		if (collision.gameObject.tag == "Player" && weaponId != 2 && weaponId != 4) {
			return;
		} else if (collision.gameObject.tag == "Enemy" && weaponId == 2) {
			return;
		} else if (collision.gameObject.tag == "MiniBoss" && (weaponId == 4 || weaponId == 5)) {
			return;
		} else {
			animator.SetBool("hit", true);
			hitSound.Play();
			collision.gameObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
		}
	}

	void DestroyThis() {
		Destroy(gameObject);
	}
}
