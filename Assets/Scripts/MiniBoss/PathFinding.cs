using UnityEngine;
using System.Collections;

public class PathFinding : MonoBehaviour {
	public class Instructions {
		public string action;
		public Vector2[] instructions;

		public Instructions(string action, params Vector2[] instructions) {
			this.action = action;
			this.instructions = instructions;
		}
	}

	private class BuildCollection {
		public Methods.ObjectWithCorners start;
		public Methods.ObjectWithCorners target;
		public Methods.ObjectWithCorners[] platforms;
		public Methods.ObjectWithCorners[] steps;
		public Methods.ObjectWithCorners[][] possibilities;
		public Vector2 position;

		public BuildCollection(GameObject startP, GameObject targetP, string tag) {
			GameObject[] plats = GameObject.FindGameObjectsWithTag(tag);
			platforms = Methods.map(plats, plat => new Methods.ObjectWithCorners(plat));

			start = new Methods.ObjectWithCorners(startP);
			target = new Methods.ObjectWithCorners(targetP);
			steps = Methods.collect(start);
			possibilities = new Methods.ObjectWithCorners[0][];
		}

		public void stepThrough(Methods.ObjectWithCorners current, float tolerance, FollowAI.Capabilities stats) {
			int position = 0;
			// Build a viable path to target from start
			while (position >= 0 && current.gameObject != target.gameObject) {
				if (possibilities.Length <= position) {
					// a list of every platform that can be reached from current platfrom that is not already in the path
					Methods.ObjectWithCorners[] options = Methods.filter(platforms, obj => {
						return obj.gameObject != current.gameObject &&
							reachable(current, obj, stats) &&
							notIn(steps, obj);
					});
					// sort by distance to target platform with preference to higher platforms
					options = Methods.sort(options, (obj1, obj2) => {
						return (Methods.distance(target.corners.landCenter, obj1.corners.landCenter) - obj1.corners.landCenter.y) -
							(Methods.distance(target.corners.landCenter, obj2.corners.landCenter) - obj2.corners.landCenter.y);
					});
					possibilities = Methods.concat(possibilities, options);
				}
				if (possibilities[position].Length == 0) {
					possibilities = Methods.eject(possibilities);
					steps = Methods.eject(steps);
					position--;
				} else {
					if (Methods.indexOf(possibilities[position], target) >= 0) {
						current = target;
					} else {
						current = Methods.last(possibilities[position]);
						possibilities[position] = Methods.eject(possibilities[position]);
					}
					steps = Methods.concat(steps, current);
					position++;
				}
			}
		}
	}

	public static GameObject[] buildSteps(GameObject target, GameObject mover, string tag, float tolerance, FollowAI.Capabilities stats) {
		GameObject startPlatform = Methods.onTaggedObject(mover, 0.1f, tag);
		GameObject targetPlatform = Methods.onTaggedObject(target, 0.1f, tag);
		if (startPlatform == null || targetPlatform == null || startPlatform == targetPlatform) {
			return new GameObject[0];
		}
		BuildCollection collection = new BuildCollection(startPlatform, targetPlatform, tag);
		collection.stepThrough(collection.start, tolerance, stats);
		return Methods.map(collection.steps, element => element.gameObject);
	}

	public static Instructions howToGetThere(GameObject obj, GameObject start, GameObject target, float tolerance, FollowAI.Capabilities stats) {
		Methods.ObjectWithCorners objectExtend = new Methods.ObjectWithCorners(obj);
		Methods.ObjectWithCorners startExtend = new Methods.ObjectWithCorners(start);
		Methods.ObjectWithCorners targetExtend = new Methods.ObjectWithCorners(target);
		if (!PathFinding.reachable(startExtend, targetExtend, stats)) {
			return new Instructions("NONE");
		}

		float myWidth = Methods.distance(objectExtend.corners.bottomLeft, objectExtend.corners.bottomRight);
		Vector2 position;
		float yOffset;

		////
		// On the Level
		////
		if (Mathf.Abs(startExtend.corners.landCenter.y - targetExtend.corners.landCenter.y) <= tolerance) {
			if (startExtend.corners.landCenter.x > targetExtend.corners.landCenter.x) {
				// => to the left
				if (startExtend.corners.topLeft.x < targetExtend.corners.topRight.x + myWidth) {
					return new Instructions("MoveLeft", targetExtend.corners.topRight);
				} else {
					return new Instructions("JumpLeft", startExtend.corners.topLeft, startExtend.corners.landCenter, targetExtend.corners.topRight);
				}
			} else {
				// => to the right
				if (startExtend.corners.topRight.x > targetExtend.corners.topLeft.x - myWidth) {
					return new Instructions("MoveRight", targetExtend.corners.topLeft);
				} else {
					return new Instructions("JumpRight", startExtend.corners.topRight, startExtend.corners.landCenter, targetExtend.corners.topLeft);
				}
			}

			////
			// Aim toward the sky and rise
			////
		} else if (startExtend.corners.landCenter.y < targetExtend.corners.landCenter.y) {
			if (startExtend.corners.topLeft.x >= targetExtend.corners.topRight.x + (myWidth / 2f)) {
				// jump across left
				return new Instructions("JumpLeft", startExtend.corners.topLeft, targetExtend.corners.landCenter, targetExtend.corners.landCenter);
			} else if (startExtend.corners.topRight.x <= targetExtend.corners.topLeft.x - (myWidth  / 2f)) {
				// jump acrros right
				return new Instructions("JumpRight", startExtend.corners.topRight, targetExtend.corners.landCenter, targetExtend.corners.landCenter);
			} else if (startExtend.corners.topRight.x > targetExtend.corners.topRight.x + myWidth) {
				// jump up left
				position = new Vector2(targetExtend.corners.topRight.x, startExtend.corners.landCenter.y);
				return new Instructions("JumpLeft", position, targetExtend.corners.landCenter, targetExtend.corners.landCenter);
			} else if (startExtend.corners.topLeft.x < targetExtend.corners.topLeft.x + myWidth) {
				// jump up right
				position = new Vector2(targetExtend.corners.topLeft.x, startExtend.corners.landCenter.y);
				return new Instructions("JumpRight", position, targetExtend.corners.landCenter, targetExtend.corners.landCenter);
			} else if (startExtend.corners.topLeft.x <= targetExtend.corners.topLeft.x) {
				// jump around left
				return new Instructions("JumpAroundLeft", startExtend.corners.topLeft, targetExtend.corners.topLeft);
			} else if (startExtend.corners.topRight.x >= targetExtend.corners.topRight.x) {
				// jump around right
				return new Instructions("JumpAroundRight", startExtend.corners.topRight, targetExtend.corners.topRight);
			} else return new Instructions("PATHFINDING ERROR: jump, but no go");

			////
			// I was up above it
			////
		} else {
			if (startExtend.corners.topLeft.x > targetExtend.corners.topLeft.x + myWidth) {
				// Fall or Jump Left?
				yOffset = targetExtend.corners.landCenter.y - startExtend.corners.landCenter.y;
				if (startExtend.corners.topLeft.x < targetExtend.corners.topRight.x + myWidth + yOffset) {
					// fall left
					return new Instructions("FallLeft", targetExtend.corners.topRight);
				} else {
					// jump left
					return new Instructions("JumpLeft", startExtend.corners.topLeft, startExtend.corners.landCenter, targetExtend.corners.topLeft);
				}
			} else if (startExtend.corners.topRight.x < targetExtend.corners.topRight.x - myWidth) {
				// Fall or Jump Right?
				yOffset = targetExtend.corners.landCenter.y - startExtend.corners.landCenter.y;
				if (startExtend.corners.topRight.x > targetExtend.corners.topLeft.x - myWidth - yOffset) {
					// fall right
					return new Instructions("FallRight", targetExtend.corners.topLeft);
				} else {
					return new Instructions("JumpRight", startExtend.corners.topRight, startExtend.corners.landCenter, targetExtend.corners.topRight);
				}
			} else if (Methods.distance(startExtend.corners.topLeft, targetExtend.corners.topLeft) <=
				Methods.distance(startExtend.corners.topRight, targetExtend.corners.topLeft)
			) {
				return new Instructions("FallAroundLeft", targetExtend.corners.topLeft);
			} else {
				return new Instructions("FallAroundRight", targetExtend.corners.topRight);
			}
		}
	}

	private static bool reachable(Methods.ObjectWithCorners start, Methods.ObjectWithCorners end, FollowAI.Capabilities stats) {
		return end.corners.topLeft.y <= start.corners.topLeft.y + stats.jumpHeight &&
			end.corners.topLeft.x <= start.corners.topRight.x + stats.jumpWidth &&
			end.corners.topRight.x >= start.corners.topLeft.x - stats.jumpWidth;
	}

	private static bool notIn(Methods.ObjectWithCorners[] array, Methods.ObjectWithCorners obj) {
		return Methods.find(array, item => item.gameObject == obj.gameObject) == null;
	}
}
