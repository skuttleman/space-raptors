using UnityEngine;
using System.Collections;

public class PlatformDestroy : MonoBehaviour {
	public void TakeDamage(int damage) {
		if (damage != 0) {
			Destroy(this.gameObject);
		}
	}
}
