using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuChoice : MonoBehaviour {
	public MenuChoice (int item) {
		if (item == 1) {
			SceneManager.LoadScene("Backstory");
		} else if (item == 4) {
			Application.Quit();
		}
	}
}
