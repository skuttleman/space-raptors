using UnityEngine;
using System.Collections;

public class MeleeCollision : MonoBehaviour {
	public int meleeDamage;

	private GameObject player;

	void Start () {
		player = GameObject.Find("Player");
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.transform.gameObject == player) {
			player.SendMessage("TakeDamage", meleeDamage);
		}
	}
}
