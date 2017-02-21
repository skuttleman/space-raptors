using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour {
	public int health;
	public float speed;
	public float jumpForce;
	public Rigidbody2D rigidBody;
	public float maxVelocity;
	public float turnDeadZone;
	public Animator animator;
	public Vector2 shotOffset;
	public GameObject currentWeapon;
	public float defaultCooldown;
	public GameObject[] weapons;
	public float switchCooldown;
	public int armor;
	public int collisionDamage;
	public float stealthTime;
	public bool stealth;
	public float stealthCooldown;
	public GameObject bootDust;
	public int[] ammo;
	public GameObject pauseMenu;
	public bool paused;

	private int score;
	private HUDManager hudManager;
	private SoundFXManager soundFXManager;
	private AudioSource audioSource;
	private Animator weaponAnimator;
	private GameObject weaponProjectile;
	private float shotCooldown;
	private Vector2 previousPosition;
	private Sprite weaponIcon;
	private int ammoType;

	void Start () {
		this.shotCooldown = 0;
		this.hudManager = GameObject.Find("HUDCanvas").GetComponent<HUDManager>();
		this.soundFXManager = GameObject.Find("SoundFX").GetComponent<SoundFXManager>();
		this.audioSource = gameObject.GetComponent<AudioSource>();
		previousPosition = new Vector2(0, 0);
		GameObject.Find("CheckpointController").GetComponent<CheckpointController>().Load();
	}

	void OnLevelWasLoaded () {
		this.hudManager = GameObject.Find("HUDCanvas").GetComponent<HUDManager>();
		InitHUD();
	}

	public void OnSpawn() {
		this.weaponAnimator = currentWeapon.GetComponent<Animator>();
		this.weaponProjectile = currentWeapon.GetComponent<WeaponDetails>().projectile;
		this.weaponIcon = currentWeapon.GetComponent<WeaponDetails>().weaponIcon;
		this.ammoType = currentWeapon.GetComponent<WeaponDetails>().ammoType - 1;
	}

	void Awake () {
		DontDestroyOnLoad(transform.gameObject);
	}

	void SetAnimation (string name, bool state) {
		this.animator.SetBool(name, state);
		this.weaponAnimator.SetBool(name, state);
	}

	void FixedUpdate() {
		if (stealthTime != 0f && stealth) {
			stealthTime -= Time.deltaTime;
			hudManager.UpdateStealth(stealthTime);
			if (stealthTime <= 0) {
				stealth = false;
				SetVisibility(1f);
			}
		}

		if (shotCooldown > 0f) {
			shotCooldown -= Time.deltaTime;
		}
		if (stealthCooldown > 0f) {
			stealthCooldown -= Time.deltaTime;
		}
		if (switchCooldown > 0f) {
			switchCooldown -= Time.deltaTime;
		}

		if (Input.GetAxis("Cancel") != 0f) {
			if (!paused) {
				paused = true;
				Instantiate(pauseMenu);
			}
		}

		float direction = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
		if (!animator.GetBool("dead")) {
			bool moved = !Mathf.Approximately(transform.position.x, previousPosition.x) ||
				!Mathf.Approximately(transform.position.y, previousPosition.y);
			bool tried = !Mathf.Approximately(direction, 0) && !Mathf.Approximately(transform.position.x, previousPosition.x);
			transform.Translate(new Vector2(direction, tried ? 0.1f : 0f));
			previousPosition = transform.position;
		}
		if (direction > turnDeadZone) {
			transform.localScale = new Vector3(1f, 1f, 1f);
			SetAnimation("walking", true);
			shotOffset.x = 2;
		} else if (direction < -turnDeadZone) {
			transform.localScale = new Vector3(-1f, 1f, 1f);
			SetAnimation("walking", true);
			shotOffset.x = -2;
		} else {
			SetAnimation("walking", false);
		}

		if (Input.GetAxis("Jump") > 0
			&& (!animator.GetBool("jumping") && !animator.GetBool("falling"))
			&& rigidBody.velocity.y == 0
			&& !animator.GetBool("dead"))
		{
			rigidBody.AddForce(new Vector2(0, jumpForce));
			SetAnimation("walking", false);
			SetAnimation("jumping", true);
			soundFXManager.Play(audioSource, "action", "player_jump");
			GameObject dust = (GameObject) Instantiate(bootDust, transform.position, Quaternion.identity);
			Destroy(dust, dust.GetComponent<ParticleSystem>().duration);
		} else if (rigidBody.velocity.y < 0) {
			SetAnimation("jumping", false);
			SetAnimation("falling", true);
		} else if (rigidBody.velocity.y == 0 && !animator.GetBool("jumping")) {
			SetAnimation("jumping", false);
			SetAnimation("falling", false);
		}

		float shoot = Input.GetAxis("Fire1");
		if (ammo[ammoType] > 0 && shotCooldown <= 0f && shoot != 0f && !animator.GetBool("dead")) {
			animator.SetTrigger("shoot");
			weaponAnimator.SetTrigger("shoot");
			GameObject newShot = (GameObject) Instantiate(weaponProjectile, new Vector2(gameObject.transform.position.x + shotOffset.x, gameObject.transform.position.y + shotOffset.y), Quaternion.identity);
			newShot.GetComponent<ProjectileController>().direction.x = transform.localScale.x;
			shotCooldown = defaultCooldown;
			ammo[ammoType]--;
			hudManager.UpdateAmmo(ammo[ammoType]);
		}

		int weaponSwitch = 0;

		if (Input.GetAxis("Stealth") != 0f && stealthCooldown <= 0) {
			stealthCooldown = defaultCooldown;
			SpriteRenderer renderer = GetComponent<SpriteRenderer>();
			if (stealth) {
				stealth = false;
				renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 1f);
			} else if (!stealth && stealthTime != 0f) {
				stealth = true;
				renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0.2f);
			}
		}

		if (Input.GetAxis("Fire2") != 0f) {
			weaponSwitch = -1;
		} else if (Input.GetAxis("Fire3") != 0f) {
			weaponSwitch = 1;
		}
		if (switchCooldown <= 0 && weaponSwitch != 0 && !animator.GetBool("dead")) {
			switchCooldown = defaultCooldown;
			int current = 0;
			for (var i = 0; i < weapons.Length; i++) {
				if (weapons[i].name == currentWeapon.name.Replace("(Clone)", "")) {
					current = i;
					break;
				}
			}
			if (current + weaponSwitch >= 0 && current + weaponSwitch < weapons.Length && !animator.GetBool("dead")) {
				SwitchWeapon(weapons[current + weaponSwitch]);
			}
		}
	}

	void ItemPickup (GameObject newItem) {
		if (newItem.tag == "Weapon") {
			System.Array.Resize(ref weapons, weapons.Length + 1);
			weapons[weapons.Length - 1] = newItem;
			ammo[newItem.GetComponent<WeaponDetails>().ammoType - 1] += 20;
			SwitchWeapon(newItem);
		} else if (newItem.tag == "Powerup") {
			if (newItem.name.Contains("Health") && newItem.name.Contains("25")) {
				HealDamage(25);
				soundFXManager.Play(audioSource, "item_pickup", "health_25");
			} else if (newItem.name.Contains("Health") && newItem.name.Contains("50")) {
				HealDamage(50);
				soundFXManager.Play(audioSource, "item_pickup", "health_50");
			} else if (newItem.name.Contains("Health") && newItem.name.Contains("Max")) {
				HealDamage(100);
				soundFXManager.Play(audioSource, "item_pickup", "health_max");
			} else if (newItem.name.Contains("Armor") && newItem.name.Contains("30")) {
				IncreaseArmor(30);
				soundFXManager.Play(audioSource, "item_pickup", "powerup");
			} else if (newItem.name.Contains("Armor") && newItem.name.Contains("100")) {
				IncreaseArmor(100);
				soundFXManager.Play(audioSource, "item_pickup", "powerup");
			} else if (newItem.name.Contains("Ammo") && newItem.name.Contains("1")) {
				IncreaseAmmo(15, 0);
				soundFXManager.Play(audioSource, "item_pickup", "powerup");
			} else if (newItem.name.Contains("Ammo") && newItem.name.Contains("2")) {
				IncreaseAmmo(10, 1);
				soundFXManager.Play(audioSource, "item_pickup", "powerup");
			}
		}
	}

	void OnCollisionEnter2D (Collision2D collision) {
//			if (collision.gameObject.tag == "Enemy") {
//				TakeDamage(collisionDamage);
//				soundFXManager.Play(audioSource, "explosion", "small");
//			} else
		if (collision.gameObject.tag == "Spikes") {
			if (armor > 0) {
				TakeDamage(collisionDamage * 1);
				soundFXManager.Play(audioSource, "explosion", "small");
			} else {
				TakeDamage(collisionDamage * 3);
				soundFXManager.Play(audioSource, "explosion", "medium");
			}
		}
	}

//TODO: Refactor IncreaseArmor and HealDamage into a PowerUp function
	void IncreaseArmor (int heal) {
		Debug.Log(heal.ToString() + " armor pickup");
		armor += heal;
		if (armor > 100) {
			armor = 100;
		}
		hudManager.UpdateArmor(armor);
	}

	void HealDamage (int heal) {
		health += heal;
		if (health > 100) {
			health = 100;
		}
		hudManager.UpdateHealth(health);
	}

	void IncreaseAmmo (int value, int ammoId) {
		ammo[ammoId] += value;
		if (ammoId == currentWeapon.GetComponent<WeaponDetails>().ammoType - 1) {
			hudManager.UpdateAmmo(ammo[ammoId]);
		}
	}

	void TakeDamage (int damage) {
		animator.SetTrigger("hit");
		weaponAnimator.SetTrigger("hit");
		if (armor != 0) {
			armor -= damage;
			if (armor < 0) {
				armor = 0;
			}
		} else {
			health -= damage;
			if (health <= 0) {
				SetAnimation("dead", true);
			}
		}
		if (health > 0) {
//			this.gameObject.SendMessage("DisplayDamage");
			hudManager.UpdateArmor(armor);
			hudManager.UpdateHealth(health);

		} else {
			hudManager.UpdateArmor(0);
			hudManager.UpdateHealth(0);
		}
		transform.position = new Vector3(
			transform.position.x - (transform.localScale.x / 4),
			transform.position.y,
			transform.position.z
		);
	}

	public void SwitchWeapon(GameObject weapon) {
		Methods.destroyChildren(this.gameObject);
		GameObject newWeapon = (GameObject) Instantiate(weapon, gameObject.transform.position, Quaternion.identity);
		newWeapon.transform.parent = gameObject.transform;
		newWeapon.transform.localScale = new Vector3(
			newWeapon.transform.localScale.x * transform.localScale.x,
			newWeapon.transform.localScale.y,
			newWeapon.transform.localScale.z);
		currentWeapon = newWeapon;
		weaponAnimator = currentWeapon.GetComponent<Animator>();
		weaponAnimator.enabled = true;
		weaponProjectile = currentWeapon.GetComponent<WeaponDetails>().projectile;
		weaponIcon = currentWeapon.GetComponent<WeaponDetails>().weaponIcon;
		ammoType = currentWeapon.GetComponent<WeaponDetails>().ammoType -1;
		hudManager.UpdateWeapon(weaponIcon);
		hudManager.UpdateAmmo(ammo[ammoType]);
	}

	void GetPoints (int points) {
		score += points;
		hudManager.UpdatePoints(score);

	}

	void Die () {
		Destroy(this.gameObject);
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public int GetScore() {
		return this.score;
	}

	public void ResetScore(int score) {
		this.score = score;
	}

	public void InitHUD() {
		hudManager.UpdateHealth(health);
		hudManager.UpdateArmor(armor);
		hudManager.UpdateStealth(stealthTime);
		hudManager.UpdatePoints(score);
		hudManager.UpdateWeapon(weaponIcon);
		hudManager.UpdateAmmo(ammo[ammoType]);
	}

	private void SetVisibility(float alpha) {
		SpriteRenderer renderer = GetComponent<SpriteRenderer>();
		renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, alpha);
	}
}
