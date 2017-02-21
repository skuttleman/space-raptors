using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadMiniBoss : MonoBehaviour {
	void OnTriggerEnter2D(Collider2D other) {
		if (other.transform.gameObject.name == "Player") {
			other.transform.position = new Vector2(-25.29f, -7.69f);
			SceneManager.LoadScene("MiniBossFight");
		}
	}
}
