using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public static int redScore;
	public static int blueScore;

	public static float redPercent;
	public static float bluePercent;

	public static bool gameStarted;

	float startTime;
	public static int gameTime;
	
	public GameObject redGrass;
	public GameObject blueGrass;
	public GameObject bomb;
	public Transform pwrUpTemplate;

	private bool pwrUp;
	public static float pwrUpTime;
	GameObject power;
	private GameObject bombFX;

	public static Dictionary<Vector3, GameObject> redGrassGrid = new Dictionary<Vector3,GameObject>();
	public static Dictionary<Vector3, GameObject> blueGrassGrid = new Dictionary<Vector3,GameObject>();
	//private BoolArray isBlueCut = new BoolArray(); 

	private int w = 60;
	private int h = 70;
	private bool[,] isBlueCut; 
	private bool[,] isRedCut; 

	public float grassBombRadius = 15.0f;

	int redMeterTotalPos = -41;
	int redMeterTotalWidth = 50;

	int blueMeterTotalPos = 41;
	int blueMeterTotalWidth = 50;

	void Start() {
		isBlueCut = new bool[h, w];
		isRedCut = new bool[h, w];
		GameEventManager.GameStart += GameStart;
		GameEventManager.GameOver += GameOver;
		gameStarted = false;
		gameObject.SetActive(false);
	}

	void Update() {

		/*if (Input.GetKey(KeyCode.Return)) {
			if (gameStarted) {
				GameEventManager.TriggerGameOver();
				gameStarted = false;
			}
			else {
				GameEventManager.TriggerGameStart();
			}
			return;
		}*/

		float timeElapsed = (float)Time.time - startTime;
		GUIManager.setTime(Mathf.Ceil(gameTime - timeElapsed));
		if (timeElapsed > gameTime) {
			GameEventManager.TriggerGameOver();
		}
		if (!pwrUp) {
			if (timeElapsed > pwrUpTime + 8) {
				GUIManager.powerUpDisplay("POWERUP HAS APPEARED!!!");
				power = Instantiate(pwrUpTemplate, new Vector3(0, 3, 23*Random.Range(-1, 2)), Quaternion.identity) as GameObject;
				pwrUp = true;
			}
		}else{
			if ((float)Time.time - pwrUpTime > 12) {
				GUIManager.powerUpDisplay("");
			}
		}

		// Red Score meter
		GameObject redMeter = GameObject.Find("RedMeter");
		redPercent = percentRedCut();
		int redScoreWidth = (int)(redMeterTotalWidth * redPercent);
		redMeter.transform.localScale = new Vector3(redScoreWidth, 0.1f, 10);

		// Blue score meter
		GameObject blueMeter = GameObject.Find("BlueMeter");
		bluePercent = percentBlueCut();
		int blueScoreWidth = (int)(blueMeterTotalWidth * bluePercent);
		blueMeter.transform.localScale = new Vector3(blueScoreWidth, 0.1f, 10);
	}

	void GameStart() {
		// reset vars
		gameTime = 90;
		startTime = (float)Time.time;
		redScore = 0;
		blueScore = 0;
		gameStarted = true;
		pwrUpTime = 0;
		pwrUp = false;

		gameObject.SetActive(true);

		GUIManager.grassLoadDisplay("Loading...");

		initRedGrass();
		initBlueGrass();

		GUIManager.grassLoadDisplay("");

		GameObject.Find("RedMower").transform.position = new Vector3(-4f, 0.25f, -22f);
		GameObject.Find("RedMower").transform.rotation = new Quaternion (0, 0, 0, 0);

		GameObject.Find("BlueMower").transform.position = new Vector3(4f, 0.25f, -22f);
		GameObject.Find("BlueMower").transform.rotation = new Quaternion (0, 0, 0, 0);

		GameObject redMeter = GameObject.Find("RedMeter");
		redMeter.transform.localScale = new Vector3(0, 0.1f, 10);

		GameObject blueMeter = GameObject.Find("BlueMeter");
		blueMeter.transform.localScale = new Vector3(0, 0.1f, 10);
	}

	void GameOver() {
		gameObject.SetActive(false);
		gameStarted = false;
	}

	void initRedGrass() {

		foreach(GameObject grass in GameObject.FindGameObjectsWithTag("RedGrass")){
			Destroy (grass);
		}
		for (int i = 0; i < h; i++) {
			for(int j = 0; j < w; j++){
				isRedCut[i,j] = true;
			}
		}

		for (int y = -10; y < 60; y++) {
			for (int x = 0; x < 60; x++) {
				Vector3 pos = new Vector3((x-65f), 0f, (y-25f));
				//GameObject rg = (GameObject)Instantiate(redGrass, pos, Quaternion.identity);
				//redGrassGrid.Add (pos,rg);
				addRedGrass(pos);
			}
		}
	}

	void initBlueGrass() {
		foreach(GameObject grass in GameObject.FindGameObjectsWithTag("BlueGrass")){
			Destroy (grass);
		}
		for (int i = 0; i < h; i++) {
			for(int j = 0; j < w; j++){
				isBlueCut[i,j] = true;
			}
		}

		for (int y = -10; y < 60; y++) {
			for (int x = 0; x < 60; x++) {
				Vector3 pos = new Vector3((x+6f), 0f, (y-25f));
				//GameObject bg = (GameObject)Instantiate(blueGrass, pos, Quaternion.identity);
				//blueGrassGrid.Add (pos,bg);
				addBlueGrass(pos);
			}
		}
	}

	void modBlueGrass(string cmd){
		float randX = Random.Range (6+grassBombRadius, 66-grassBombRadius);
		float randY = Random.Range (-35+grassBombRadius, 35-grassBombRadius);
		//Vector3 pos = new Vector3((randX), 0f, (randY));
		//print ("ry: " + randY);
		//print ("rx: " + randX);
		bombFX = (GameObject)Instantiate(bomb, new Vector3(randX, 2, randY), Quaternion.identity);
		if (cmd.Equals ("grow")) {
			for (int i = 5; i < 65; i++) {
				for(int j = -34; j < 34; j++){
					if(Vector2.Distance(new Vector2(randX, randY),new Vector2(i,j))<grassBombRadius){
						//if(j*j + i*i <= grassBombRadius*grassBombRadius){
						addBlueGrass(new Vector3(i, 0, j));
					}
				}
			}
		}else{
			for (int i = 5; i < 65; i++) {
				for(int j = -34; j < 34; j++){
					if(Vector2.Distance(new Vector2(randX, randY),new Vector2(i,j))<grassBombRadius){
					//if(j*j + i*i <= grassBombRadius*grassBombRadius){
						removeBlueGrass(new Vector3(i, 0, j));
					}
				}
			}
		}
	}

	void modRedGrass(string cmd){
		float randX = Random.Range (-66+grassBombRadius, -6-grassBombRadius);
		float randY = Random.Range (-35+grassBombRadius, 35-grassBombRadius);
		//Vector3 pos = new Vector3((randX), 0f, (randY));
		//print ("ry: " + randY);
		//print ("rx: " + randX);
		bombFX = (GameObject)Instantiate(bomb, new Vector3(randX, 2, randY), Quaternion.identity);
		if (cmd.Equals ("grow")) {
			for (int i = -5; i > -65; i--) {
				for(int j = -34; j < 34; j++){
					if(Vector2.Distance(new Vector2(randX, randY),new Vector2(i,j))<grassBombRadius){
						addRedGrass(new Vector3(i, 0, j));
					}
				}
			}
		}else{
			for (int i = -5; i > -65; i--) {
				for(int j = -34; j < 34; j++){
					if(Vector2.Distance(new Vector2(randX, randY),new Vector2(i,j))<grassBombRadius){
						removeRedGrass(new Vector3(i, 0, j));
					}
				}
			}
		}
	}

	void addBlueGrass(Vector3 pos){
		int y = (int)pos.z+35;
		int x = (int)pos.x-6;																									
		
		if (isBlueCut [y, x]) {
			GameObject bg = (GameObject)Instantiate(blueGrass, pos, Quaternion.identity);
			//blueGrassGrid.Add (pos,bg);
			blueGrassGrid[pos] = bg;
			isBlueCut[y, x] = false;
		}
	}
	
	void addRedGrass(Vector3 pos){
		int y = (int)pos.z-35*-1;
		int x = (int)pos.x*-1-6;

		if (isRedCut [y, x]) {
			GameObject rg = (GameObject)Instantiate(redGrass, pos, Quaternion.identity);
			//redGrassGrid.Add (pos,rg);
			redGrassGrid[pos] = rg;
			isRedCut[y, x] = false;
		}
	}

	void removeBlueGrass(Vector3 pos){
		//print ("bey: " + pos.z);
		//print ("bex: " + pos.x);
		int y = (int)pos.z+35;
		int x = (int)pos.x-6;
		//print ("aftery: " + y);
		//print ("afterx: " + x);																										

		if (!isBlueCut [y, x]) {
			GameObject grass;
			blueGrassGrid.TryGetValue(pos, out grass);
			GameObject.Destroy (grass); 
			//blueGrassGrid.Remove (pos);
			isBlueCut[y, x] = true;
		}
	}

	void removeRedGrass(Vector3 pos){
		int y = (int)pos.z-35*-1;
		int x = (int)pos.x*-1-6;
		if (!isRedCut [y, x]) {
			GameObject grass;
			redGrassGrid.TryGetValue(pos, out grass);
			GameObject.Destroy (grass); 
			//blueGrassGrid.Remove (pos);
			isRedCut[y, x] = true;
		}
	}

	void pwrUpPicked(){
		pwrUpTime = (float)Time.time - startTime;
		pwrUp = false;
	}

	void bombOver(){
		if(bombFX != null)
			Destroy (bombFX);
		foreach(GameObject bomb in GameObject.FindGameObjectsWithTag("Bomb")){
			Destroy (bomb);
		}
	}

	public static Dictionary<Vector3,GameObject> getGrassGrid(string name){
		if (name == "blue") {
			return blueGrassGrid;
		} else if (name == "red") {
			return redGrassGrid;
		}
		return null;
	}


	public float percentRedCut() {
		float cut = 0;
		float total = 0;
		for (int i = 0; i < h; i++) {
			for(int j = 0; j < w; j++){
				if (isRedCut[i,j])
					cut++;
				total++;
			}
		}
		return (float)(cut / total);
	}


	public float percentBlueCut() {
		float cut = 0;
		float total = 0;
		for (int i = 0; i < h; i++) {
			for(int j = 0; j < w; j++){
				if (isBlueCut[i,j])
					cut++;
				total++;
			}
		}
		return (float)(cut / total);
	}
}
