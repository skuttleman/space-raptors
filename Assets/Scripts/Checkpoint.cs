using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour {
	public Vector2 respawnPosition;

	void OnTriggerEnter2D(Collider2D other) {
		if (other.transform.gameObject.name == "Player") {
			GameObject.Find("CheckpointController")
				.GetComponent<CheckpointController>()
				.Save(respawnPosition, this.gameObject);
			Destroy(this.gameObject);
		}
	}
}
