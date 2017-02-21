using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

public class CameraController : MonoBehaviour {
	public float zoomSpeed;
	public Vector2 positionSpeed;
	public GameObject startingBounds;
	public bool manualPositioning;
	public Vector2 playerOffset;

	private float zoom;
	private Vector3 position;
	private GameObject player;
	private bool shaking;
	private Bounds boundary;
	private Camera cameraObj;
	private Vector3 targetPosition;
	private float targetZoom;

	void Start () {
		cameraObj = GetComponent<Camera>();
		targetZoom = cameraObj.orthographicSize;
		zoom = targetZoom;
		player = GameObject.Find("Player");
		transform.position = new Vector3(
			player.transform.position.x,
			player.transform.position.y,
			-10f
		);
		if (SceneManager.GetActiveScene().name == "MiniBossFight") {
			SetZoom(15);
		}
		SetBoundaries(startingBounds);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		// Zoom
		if (zoom != targetZoom) {
			if (Mathf.Abs(zoom - targetZoom) <= zoomSpeed * Time.deltaTime) zoom = targetZoom;
			else zoom += zoom > targetZoom ? -zoomSpeed * Time.deltaTime : zoomSpeed * Time.deltaTime;
			cameraObj.orthographicSize = zoom;
		}

		// Set Position to Player
		if (!manualPositioning) {
			Vector2 position = new Vector2(
				player.transform.position.x + (player.transform.localScale.x * playerOffset.x),
				player.transform.position.y + (player.transform.localScale.y * playerOffset.y)
			);
			SetPosition(position);
		}

		// Move Camera
		if (!shaking) {
			if (Mathf.Abs(transform.position.x - targetPosition.x) <= positionSpeed.x * Time.deltaTime) {
				transform.position.Set(targetPosition.x, transform.position.y, transform.position.z);
			} else {
				transform.position.Set(
					calculatePosition(transform.position.x, targetPosition.x, positionSpeed.x),
					transform.position.y,
					transform.position.z);
			}
			if (Mathf.Abs(transform.position.y - targetPosition.y) <= positionSpeed.y * Time.deltaTime) {
				transform.position.Set(
					transform.position.x,
					targetPosition.y,
					transform.position.z
				);
			} else {
				transform.position.Set(
					transform.position.x,
					calculatePosition(transform.position.y, targetPosition.y, positionSpeed.y),
					transform.position.z);
			}
		}
	}

	public void SetZoom(float zoom, float speed) {
		this.zoomSpeed = speed;
		this.targetZoom = zoom;
	}

	public void SetZoom(float zoom) {
		SetZoom(zoom, zoomSpeed);
	}

	public void SetPosition(Vector2 position, Vector2 speed, bool stayInBounds) {
		positionSpeed = speed;

		if (stayInBounds && (boundary.extents.x != 0 || boundary.extents.y != 0)) {
			Vector2 minBound = MinBound(position);
			Vector2 maxBound = MaxBound(minBound);

			position = new Vector2(
				Mathf.Min(maxBound.x, minBound.x),
				Mathf.Min(maxBound.y, minBound.y)
			);
		}

		targetPosition = position;
		targetPosition.z = -10f;
	}

	public void SetPosition(Vector2 position, Vector2 speed) {
		SetPosition(position, speed, true);
	}

	void SetPosition(Vector2 position) {
		SetPosition(position, this.positionSpeed, true);
	}

	public void SetPositionManual(Vector2 position) {
		this.targetPosition = position;
		this.targetPosition.z = -10f;
	}

	public IEnumerator Shake(int iterations, float velocity) {
		this.shaking = true;
		Vector3 end = cameraObj.transform.localPosition;
		List<Vector3> shakes = new List<Vector3>();
		shakes.Add(new Vector3(-velocity, velocity, position.z));
		shakes.Add(new Vector3(velocity, -velocity, position.z));
		shakes.Add(new Vector3(-velocity, -velocity, position.z));
		shakes.Add(new Vector3(velocity, velocity, position.z));
		for (int i = 0; i < iterations; i++) {
			for (int j = 0; j < shakes.Count; j++) {
				cameraObj.transform.localPosition = end + shakes[j];
				yield return new WaitForFixedUpdate();
			}
		}
		this.cameraObj.transform.localPosition = end;
		this.shaking = false;
	}

	public void SetBoundaries(GameObject obj) {
		if (obj) {
			boundary = obj.GetComponent<MeshRenderer>().bounds;
		}
	}

	Vector2 MinBound(Vector2 position){
		return EitherBound(position, 1.9f, boundary.min, Mathf.Max);
	}

	Vector2 MaxBound(Vector2 position) {
		return EitherBound(position, -1.9f, boundary.max, Mathf.Min);
	}

	Vector2 EitherBound(Vector2 position, float divisor, Vector2 boundPos, Func<float, float, float> compare) {
		Vector2 centerPos = cameraObj.WorldToScreenPoint(position);
		Vector2 comparePos = new Vector2(
			centerPos.x - cameraObj.pixelWidth / divisor,
			centerPos.y - cameraObj.pixelHeight / divisor
		);
		comparePos = cameraObj.ScreenToWorldPoint(comparePos);
		comparePos.x = compare(comparePos.x, boundPos.x);
		comparePos.y = compare(comparePos.y, boundPos.y);
		comparePos = cameraObj.WorldToScreenPoint(comparePos);
		centerPos = new Vector2(
			comparePos.x + cameraObj.pixelWidth / divisor,
			comparePos.y + cameraObj.pixelHeight / divisor
		);
		return cameraObj.ScreenToWorldPoint(centerPos);
	}

	private float calculatePosition(float axis, float target, float speed) {
		float value = speed * Time.deltaTime;
		if (axis > target) {
			value = value * -1f;
		}
		return value;
	}
}
