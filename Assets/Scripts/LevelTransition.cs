using UnityEngine;
using System.Collections;

public class LevelTransition : MonoBehaviour {
	public GameObject leftLevel;
	public GameObject rightLevel;

	private CameraController cameraController;
	private SongManager songManager;
	private GameObject player;
	private bool inside;

	void Start() {
		cameraController = GameObject.Find("Main Camera").GetComponent<CameraController>();
		songManager = GameObject.Find("Dropdown").GetComponent<SongManager>();
		player = GameObject.Find("Player");
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.transform.gameObject == player) {
			cameraController.manualPositioning = true;
			cameraController.SetPositionManual(transform.position);
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if (other.transform.gameObject == player) {
			cameraController.manualPositioning = false;
			cameraController.SetBoundaries(player.transform.position.x < transform.position.x ? leftLevel : rightLevel);
			songManager.Play(player.transform.position.x < transform.position.x ? 2 : 1);
		}
	}
}
