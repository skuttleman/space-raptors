﻿#pragma strict
// Constants
static var ACTIONS :int = 3;


// Inspector Variables
public class Capabilities {
	var speed :float;
	var jumpForce :float;
	var attackPower :float;
	var maxVelocity :float;
	var meleeRange :float;
}
public class Reactions {
	var reactionTime :float;
	var distThreshold :float;
	var meleeFreq :float;
	var randomLikelihood :int;
}
public class Pointers {
	var rigidBody :Rigidbody2D;
	var animator :Animator;
	var player :GameObject;
}
var capabilities :Capabilities;
var reactions :Reactions;
var pointers :Pointers;


// Script Variables
private var randomActions :String[];
private var meleeCoolDown :float = 1f;
private var reactionCoolDown :float = 1f;
private var ready :boolean = true;
private var turnable :boolean = true;



function Start () {
	randomActions = new String[ACTIONS];
	randomActions[0] = 'Growl';
	randomActions[1] = 'Stomp';
	randomActions[2] = 'Hiss';
}

function FixedUpdate () {
	if (turnable) FacePlayer();
	
	if (ready) {
		if (meleeCoolDown > 0f) meleeCoolDown -= Time.deltaTime;
		if (reactionCoolDown > 0f) reactionCoolDown -= Time.deltaTime;
		var dist :float = distanceCalc(transform.position, pointers.player.transform.position);
		if (meleeCoolDown <= 0f && dist <= capabilities.meleeRange) {
			StartCoroutine('MeleeAttack');
		} else if (reactionCoolDown <= 0f) {
			
			var decide = Random.Range(0, reactions.randomLikelihood);
			if (false && !decide) {
				DoSomethingRandom();
// skip for now
			} else {
				
				var myPlatform :GameObject = onPlatform(pointers.player, 0.1);
				var playerPlatform :GameObject = onPlatform(pointers.player, 0.1);
				if (myPlatform == playerPlatform || !myPlatform || !playerPlatform) {
					var moveAmount = -transform.localScale.x * capabilities.speed * Time.deltaTime;
					transform.Translate(new Vector2(moveAmount, 0));
				}
				// if player.platform = self.platform - move to player
				
				// recursive calculate platform path to player to build move instructions
				// follow instructions while player is on same platform or in the air
			}
			
		}
	}
	
}

function distanceCalc(point1 :Vector2, point2 :Vector2): float {
	return Mathf.Sqrt(Mathf.Pow(point1.x - point2.x, 2.0) + Mathf.Pow(point1.y - point2.y, 2.0));
}

function FacePlayer() {
	var scale :float = -xScale(0.1);
	if (scale) transform.localScale = new Vector3(scale, 1f, 1f);
	else Debug.Log(scale);
}

function xScale(tolerance :float) :float {
	if (pointers.player.transform.position.x < transform.position.x - tolerance) return -1f;
	else if (pointers.player.transform.position.x > transform.position.x + tolerance) return 1f;
	else return 0f;
}

function MeleeAttack() {
	ready = false;
	var renderer :SpriteRenderer = GetComponent(SpriteRenderer);
	renderer.color = Color.yellow;
	yield WaitForSeconds(1);
	renderer.color = Color.white;
	meleeCoolDown = reactions.meleeFreq;
	ready = true;
}

function DoSomethingRandom() {
	var action :int = Random.Range(0, ACTIONS);
	StartCoroutine(randomActions[action]);
	reactionCoolDown = reactions.reactionTime;
}

function Growl() {
	ready = false;
	var renderer :SpriteRenderer = GetComponent(SpriteRenderer);
	renderer.color = Color.red;
	yield WaitForSeconds(2);
	renderer.color = Color.white;
	ready = true;
}

function Stomp() {
	ready = false;
	var renderer :SpriteRenderer = GetComponent(SpriteRenderer);
	renderer.color = Color.green;
	yield WaitForSeconds(5);
	renderer.color = Color.white;
	ready = true;
}

function Hiss() {
	ready = false;
	var renderer :SpriteRenderer = GetComponent(SpriteRenderer);
	renderer.color = Color.blue;
	yield WaitForSeconds(4);
	renderer.color = Color.white;
	ready = true;
}

function onPlatform(object :GameObject, tolerance :float) :GameObject {
	var collider :BoxCollider2D = object.GetComponent(BoxCollider2D);
	var platforms :GameObject[] = GameObject.FindGameObjectsWithTag('Platform');
	for (var i :int = 0; i < platforms.Length; i++) {
		var thickness :float = platforms[i].transform.localScale.y / 2f;
		var vec :Vector3 = object.transform.position - collider.bounds.extents - new Vector3(0, thickness, 0);
		if (Mathf.Abs(vec.y - platforms[i].transform.position.y) <= tolerance) return platforms[i];
	}
	return null;
}
