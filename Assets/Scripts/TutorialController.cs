using UnityEngine;
using System.Collections;

public class TutorialController : MonoBehaviour {
	public GameObject tutorialHUD;
	public GameObject nextOverlay;

	void OnTriggerEnter2D (Collider2D collision) {
		if (collision.gameObject.name == "Player") {
			Methods.destroyChildren(tutorialHUD);
			GameObject newOverlay = (GameObject) Instantiate(nextOverlay);
			newOverlay.transform.parent = tutorialHUD.transform;
			newOverlay.transform.localPosition = new Vector3(0f, 0f, 0f);
			Destroy(this.gameObject);
		}
	}
}
