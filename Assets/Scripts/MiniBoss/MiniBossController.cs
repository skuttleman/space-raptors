using UnityEngine;
using System.Collections;

public class MiniBossController : MonoBehaviour {
	public int health;

	private int totalHealth;
	private int stage;
	private GameObject player;
	private FollowAI follow;
	private MeleeAttack melee;
	private MainCannon mainCannon;

	void Start() {
		player = GameObject.Find("Player");
		totalHealth = health;
		follow = GetComponent<FollowAI>();
		mainCannon = GetComponent<MainCannon>();
		melee = GetComponent<MeleeAttack>();
		follow.enabled = false;
		mainCannon.enabled = false;
		SetStage();
	}

	void FixedUpdate() {
		if (player.GetComponent<PlayerController>().health <= 0) {
			follow.getem = false;
			melee.smackem = false;
			mainCannon.shootem = false;
		}
	}

	private void TakeDamage(int damage) {
		health -= damage;
		if (health <= 0) {
			Destroy(this.gameObject);
		} else {
			this.gameObject.SendMessage("DisplayDamage");
			SetStage();
		}
	}

	private void SetStage() {
		int newStage = Mathf.RoundToInt(4f - (health * 3f / totalHealth));
		if (stage != newStage) {
			stage = newStage;
			switch(stage) {
				case 1:
					GameObject.Find("Main Camera").GetComponent<CameraController>().SetZoom(15f);
					follow.enabled = true;
					follow.getem = true;
					break;
				case 2:
					mainCannon.enabled = true;
					mainCannon.shootem = true;
					break;
				case 3:
					mainCannon.destroyem = true;
					break;
				default:
					break;
			}
		}
	}
}
