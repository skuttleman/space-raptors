using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour {
	private GameObject player;

	void Start () {
		Time.timeScale = 0;
		player = GameObject.Find("Player");
	}

	public PauseMenu(int item) {
		if (item == 1) {
			Time.timeScale = 1;
			player.GetComponent<PlayerController>().paused = false;
			Destroy(gameObject);
		} else if (item == 3) {
			SceneManager.LoadScene("Menu");
		} else if (item == 4) {
			Application.Quit();
		}
	}
}
