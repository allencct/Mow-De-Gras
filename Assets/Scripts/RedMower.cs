using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Base code borrowed from http://wiki.unity3d.com/index.php/SimpleTankController

public class RedMower : MonoBehaviour {
	enum direction {forward,reverse,stop};
	CharacterController controller;
	public float topSpeedForward = 3.0f; 					// top speed of forward
	public float topSpeedReverse = 1.0f; 					// top speed of reverse
	public float accelerationRate = 3f; 					// rate at which top speed is reached
	public float decelerationRate = 2f; 					// rate at which speed is lost when not accelerating
	public float brakingDecelerationRate = 4f;				// rate at which speed is lost when braking (input opposite of current direction)
	public float stoppedTurnRate = 2.0f;					// rate at which object turns when stopped
	public float topSpeedForwardTurnRate = 1.0f;			// rate at which object turns at top forward speed
	public float topSpeedReverseTurnRate = 2.0f;			// rate at which object turns at top reverse speed
	public bool stickyThrottle = false; 				// true to disable loss of speed if no input is provided
	public float stickyThrottleDelay = 0.35f;				// delay between change of direction when sticky throttle is enabled
	public float yRotation = 0.0f;
	
	private float currentSpeed = 0.0f; 					// stores current speed
	private float currentTopSpeed; 		// stores top speed of current direction
	private direction currentDirection = direction.stop; 	// stores current direction
	private bool isBraking = false; 					// true if input is braking
	private bool isAccelerating = false; 				// true if input is accelerating
	private float stickyDelayCount = 9999.0f;				// current sticky delay count

	private float clipRadius = 1.0f;

	//spin out stuff
	int timeTracker=0;
	float rotationTracker;
	bool hitRock=false;
	public AudioClip rockSound;

	//grass stuff
	float nextSoundTime=0;
	public AudioClip grassSound;
	GameObject clippings;
	bool isClipping=false;

	//poo stuff
	public AudioClip pooSound;
	bool hitPoo=false;
	GameObject pellets;

	//camera stuff
	//GameObject camera1;
	//GameObject camera2;

	//powerup stuff
	float timeLeft;
	int powerUp = 0;
	public GameObject manager;
	public GameObject otherMow;
	public AudioClip powerUpSound;
	private Vector3 scale;


	// Use this for initialization
	void Start () {
		controller = GetComponent<CharacterController> ();
		currentTopSpeed = topSpeedForward;

		/*for (int y = 0; y < 50; y++) {
			for (int x = 0; x < 60; x++) {
				Vector3 pos = new Vector3((x-65f), 0f, (y-25f));
				Instantiate(blade, pos, Quaternion.identity);
			}
		}*/


		//camera1 = GameObject.Find ("Camera1");
		//camera2 = GameObject.Find ("Camera2");

		clippings = GameObject.Find ("Clippings");
		clippings.SetActive (false);

		pellets = GameObject.Find ("Pellets");
		pellets.SetActive (false);
	}

	void FixedUpdate () {

		if (GameManager.gameStarted) {

			checkBlueGrass();
			checkRedGrass();

			if (hitRock) {

				timeTracker++;
				if(timeTracker>30){
					yRotation+=36;
					rotationTracker+=36;
					if (rotationTracker>360){
						hitRock=false;
						timeTracker=0;
					}
				}
			}
			if(isClipping){
				timeTracker++;
				clippings.SetActive(true);
				if(timeTracker>50){
					isClipping=false;
					clippings.SetActive(false);
					timeTracker=0;
				}
			}
			if(hitPoo){
				timeTracker++;
				pellets.SetActive(true);
				if(timeTracker>50){
					hitPoo=false;
					pellets.SetActive(false);
					timeTracker=0;
				}
			}

			// direction to move this update
			Vector3 moveDirection = Vector3.zero;
			// direction requested this update
			direction requestedDirection = direction.stop;

			// WASD controls only
			if (Input.GetKey("a") || Input.GetKey("d")) {
				// simulate loss of turn rate at speed
				float currentTurnRate = Mathf.Lerp((currentDirection == direction.forward ? topSpeedForwardTurnRate : topSpeedReverseTurnRate), stoppedTurnRate, (1- (currentSpeed/currentTopSpeed)));
				float rotAxis = (Input.GetKey("a")) ? -0.75f : 0.75f;
				yRotation += rotAxis * currentTurnRate;
				transform.eulerAngles = new Vector3 (0, yRotation, 0);
			}

			// based on input, determine requested action
			if (Input.GetKey("w")) { // requesting forward
				requestedDirection = direction.forward;
				isAccelerating = true;
			} else if (Input.GetKey("s")) { // requesting reverse
				requestedDirection = direction.reverse;
				isAccelerating = true;
			} else {
				requestedDirection = currentDirection;
				isAccelerating = false;
			}
			
			isBraking = false;
			
			if (currentDirection == direction.stop) { // engage new direction
				stickyDelayCount += Time.deltaTime;
				// if we are not sticky throttle or if we have hit the delay then change direction
				if (!stickyThrottle || stickyDelayCount > stickyThrottleDelay) {
					// make sure we can go in the requsted direction
					if (requestedDirection == direction.reverse && topSpeedReverse > 0 || 
					    requestedDirection == direction.forward && topSpeedForward > 0) {
						
						currentDirection = requestedDirection;
					}
				}
			} else if (currentDirection != requestedDirection) { // requesting a change of direction, but not stopped so we are braking
				isBraking = true;
				isAccelerating = false;
			}
			
			// setup top speeds and move direction
			if (currentDirection == direction.forward) {
				moveDirection = Vector3.forward;
				currentTopSpeed = topSpeedForward;
			} else if (currentDirection == direction.reverse) {
				moveDirection = (-1 * Vector3.forward);
				currentTopSpeed = topSpeedReverse;
			} else if (currentDirection == direction.stop) {
				moveDirection = Vector3.zero;
			}
			
			if (isAccelerating) {
				//if we havent hit top speed yet, accelerate
				if (currentSpeed < currentTopSpeed){ 
					currentSpeed += (accelerationRate * Time.deltaTime);     
				}
			} else {
				// if we are not accelerating and still have some speed, decelerate
				if (currentSpeed > 0) {
					// adjust deceleration for braking and implement sticky throttle
					float currentDecelerationRate = (isBraking ? brakingDecelerationRate : (!stickyThrottle ? decelerationRate : 0));
					currentSpeed -= (currentDecelerationRate * Time.deltaTime);  
				}
			}
			
			// if our speed is below zero, stop and initialize
			if (currentSpeed < 0 || (currentSpeed == 0 && currentDirection != direction.stop)) {
				SetStopped();
			} else if (currentSpeed > currentTopSpeed) { // limit the speed to the current top speed
				currentSpeed = currentTopSpeed;
			}
			
			moveDirection = transform.TransformDirection(moveDirection);
			moveDirection.z = moveDirection.z * (Time.deltaTime * currentSpeed);
			moveDirection.x = moveDirection.x * (Time.deltaTime * currentSpeed);
			controller.Move(moveDirection);
		
			//powerup
			if (powerUp != 0) {
				timeLeft -= Time.deltaTime;
				if (timeLeft < 5.5) {
					GUIManager.powerUpDisplay("");
					if (timeLeft < 0) {
						switch(powerUp){
							case 1:
								transform.localScale = scale;
								clipRadius = 1.0f;
								break;
							case 2:
								otherMow.transform.localScale = scale;
								otherMow.SendMessage ("setClipRadius", 1.0f);
								break;
							default:
								break;
						}
						powerUp = 0;
					}
				}else{
					int flicker = (int)((timeLeft-5.5) * 10);
					switch(powerUp){
						case 1:
							switch(flicker%2){
								case 0:
									transform.localScale += new Vector3(.2f,.2f,.2f);
									break;
								case 1:
									transform.localScale -= new Vector3(.2f,.2f,.2f);
									break;
							}
							break;
						case 2:
							switch(flicker%2){
								case 0:
									otherMow.transform.localScale -= new Vector3(.15f,.15f,.15f);
									break;
								case 1:
									otherMow.transform.localScale += new Vector3(.15f,.15f,.15f);
									break;
							}
							break;
						default:
							if(flicker < 11)
								manager.SendMessage ("bombOver");
							break;
					}
				}
			}

			// FIXED Y POSITION
			GameObject redMower = GameObject.Find("RedMower");
			redMower.transform.position = new Vector3(redMower.transform.position.x, 0.5f, redMower.transform.position.z);
		}

	}

	float GetCurrentSpeed() {
		return currentSpeed;
	}
	
	float GetCurrentTopSpeed() {
		return currentTopSpeed;
	}
	
	direction GetCurrentDirection() {
		return currentDirection;
	}
	
	bool GetIsBraking() {
		return isBraking;
	}
	
	bool GetIsAccelerating() {
		return isAccelerating;
	}
	
	void SetStopped() {
		currentSpeed = 0;
		currentDirection = direction.stop;
		isAccelerating = false;
		isBraking = false;
		stickyDelayCount = 0;
	}

	void OnTriggerEnter(Collider other){

		if (other.gameObject.name == "BlueMower") {
			currentSpeed*=0.5f;
			yRotation+=120f;
			audio.PlayOneShot (rockSound);
		}
		if (other.gameObject.tag == "Rock") {
			if(currentSpeed>0.5f){
				GameManager.redScore-=100;
				//GUIManager.SetRedScore(GameManager.redScore);
				//Debug.Log ("It's a rock!");
				currentSpeed *= 0.2f;
				rotationTracker = 0;
				audio.PlayOneShot (rockSound);
				hitRock = true;
			}
		}
		if (other.gameObject.tag == "Poo") {
			GameManager.redScore-=300;
			//GUIManager.SetRedScore(GameManager.redScore);
			//Debug.Log ("It's poop again!");
			Destroy (other.gameObject);
			currentSpeed*= 0.05f;
			audio.PlayOneShot(pooSound);
			hitPoo=true;
		}
		if (other.gameObject.tag == "PowerUp") {
			//other.gameObject.SetActive (false);
			audio.PlayOneShot(powerUpSound);
			Destroy (other.gameObject);
			timeLeft = 7.0f;
			manager.SendMessage ("pwrUpPicked");
			powerUp = Random.Range (1,5);
			switch(powerUp){
				case 1:
					scale = transform.localScale;
					transform.localScale += new Vector3(1.5f,1.5f,1.5f);
					clipRadius = 2f;
					GUIManager.powerUpDisplay("RED GROWING!!!");
					break;
				case 2:
					scale = otherMow.transform.localScale;
					otherMow.SendMessage ("setClipRadius", 0.7f);
					GUIManager.powerUpDisplay("BLUE SHRINKING!!!");
					break;
				case 3:
					manager.SendMessage ("modBlueGrass", "grow");
					GUIManager.powerUpDisplay("GROWING BLUE GRASS!!!");
					break;
				case 4:
					manager.SendMessage ("modRedGrass", "kill");
				GUIManager.powerUpDisplay("BOMBING RED GRASS!!!");
					break;
			}
		}
	}

	void checkBlueGrass(){
		Vector3 currentPosition = controller.transform.position;
		Dictionary<Vector3,GameObject> bgg = GameManager.getGrassGrid ("blue");
		
		foreach(KeyValuePair<Vector3,GameObject> grassKeyValuePair in bgg){
			if(grassKeyValuePair.Value!=null){	//if grass exists
				if (Vector3.Distance(grassKeyValuePair.Key,currentPosition)<clipRadius){ //detect grass
					GUIManager.mowingOpp(Color.red);
					//GameObject.Destroy (grassKeyValuePair.Value); //destroy grass
					manager.SendMessage ("removeBlueGrass", grassKeyValuePair.Key);
					GameManager.redScore--; //increment score
					//GUIManager.SetRedScore(GameManager.redScore); //update score
					if(Time.time>=nextSoundTime) { //play sound
						audio.PlayOneShot(grassSound);
						nextSoundTime = Time.time + (grassSound.length/4f);
					}
					if(clippings.activeInHierarchy==false){ //show particles
						isClipping=true;
					}
				}
			}
		}
	}
	void checkRedGrass(){
		Vector3 currentPosition = controller.transform.position;
		Dictionary<Vector3,GameObject> rgg = GameManager.getGrassGrid ("red");
		
		foreach(KeyValuePair<Vector3,GameObject> grassKeyValuePair in rgg){
			if(grassKeyValuePair.Value!=null){	//if grass exists
				if (Vector3.Distance(grassKeyValuePair.Key,currentPosition)<clipRadius){ //detect grass
					//GameObject.Destroy (grassKeyValuePair.Value); //destroy grass
					manager.SendMessage ("removeRedGrass", grassKeyValuePair.Key);
					GameManager.redScore++; //increment score
					//GUIManager.SetRedScore(GameManager.redScore); //update score
					if(Time.time>=nextSoundTime) { //play sound
						audio.PlayOneShot(grassSound);
						nextSoundTime = Time.time + (grassSound.length/4f);
					}
					if(clippings.activeInHierarchy==false){ //show particles
						isClipping=true;
					}
				}
			}
		}
	}
	void setClipRadius(float radius){
		clipRadius = radius;
	}
}

