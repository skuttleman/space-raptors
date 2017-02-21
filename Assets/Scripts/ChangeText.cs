using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class ChangeText : MonoBehaviour {

	string[] storyText;
	Text UIText;

	private int currentLine;
	private bool keyPress;

	void Start () {
		currentLine = 0;
		Next();
	}

	void Update () {
		if (Input.anyKey) {
			if (!keyPress) {
				KeyPressed();
			}
		}
	}

	IEnumerable AnimateText (string text) {
		for (int i = 0; i < text.Length; i++) {
			UIText.text += text[i];
			yield return new WaitForSeconds(0.05f);
		}
		currentLine ++;
		yield return new WaitForSeconds(1f);
		Next();
	}

	IEnumerable KeyPressed () {
		keyPress = true;
		StopCoroutine("AnimateText");
		if (UIText.text.Length < storyText[currentLine].Length) {
			UIText.text = storyText[currentLine];
			yield return new WaitForSeconds(1f);
		}
		currentLine++;
		Next();
	}

	void Next () {
		keyPress = false;
		if (currentLine < storyText.Length) {
			UIText.text = "";
			StartCoroutine("AnimateText", storyText[currentLine]);
		} else {
			SceneManager.LoadScene("Main");
		}
	}
}
