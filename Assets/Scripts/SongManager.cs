using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SongManager : MonoBehaviour {
	public AudioSource[] songs;

	void Start() {
		if (SceneManager.GetActiveScene().name == "MiniBossFight") {
			Play(3);
		} else {
			Play(2);
		}
	}

	public void Play(int num) {
		StopAll();
		if (num >= 0 && num < songs.Length) {
			songs[num].Play();
		}
	}

	public void StopAll() {
		Methods.forEach(songs, song => song.Stop());
	}
}
