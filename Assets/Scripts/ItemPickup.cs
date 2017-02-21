using UnityEngine;
using System.Collections;

public class ItemPickup : MonoBehaviour {
	public GameObject itemType;

	public void OnTriggerEnter2D(Collider2D collision) {
		if (collision.gameObject.tag == "Player") {
			collision.gameObject.SendMessage("ItemPickup", itemType);
			Destroy(gameObject);
		}
	}
}
