using UnityEngine;
using System.Collections;

public class FlashDamage : MonoBehaviour {
	public IEnumerable DisplayDamage() {
		SpriteRenderer renderer = GetComponent<SpriteRenderer>();
		float alpha = renderer.color.a;
		renderer.color = new Color(0.9f, 0.5f, 0.5f, alpha);
		yield return new WaitForSeconds(0.333f);
		renderer.color = new Color(1f, 1f, 1f, alpha);
	}
}
