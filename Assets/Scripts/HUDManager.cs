using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUDManager : MonoBehaviour {
	public Slider armorSlider;
	public Slider healthSlider;
	public Slider stealthSlider;
	public Text pointsCounter;
	public Image weaponIcon;
	public Text ammoCounter;

	public void UpdateArmor(int value) {
		armorSlider.value = value;
	}

	public void UpdateArmor(float value) {
		UpdateArmor(Mathf.RoundToInt(value));
	}

	public void UpdateHealth(int value) {
		healthSlider.value = value;
	}

	public void UpdateHealth(float value) {
		UpdateHealth(Mathf.RoundToInt(value));
	}

	public void UpdateStealth(int value) {
		stealthSlider.value = value;
	}

	public void UpdateStealth(float value) {
		UpdateStealth(Mathf.RoundToInt(value));
	}

	public void UpdatePoints(int value) {
		pointsCounter.text = Methods.leftPad(value.ToString(), "0", 5);
	}

	public void UpdateWeapon(Sprite image) {
		weaponIcon.sprite = image;
	}

	public void UpdateAmmo(int value) {
		ammoCounter.text = Methods.leftPad(value.ToString(), "0", 3);
	}
}
