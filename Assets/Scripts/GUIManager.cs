using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour {
	
	public GUIText instructions, redScore, blueScore, gameOverText, timeText, powerUp, grassLoad, mowingOpponentText;
	
	// this seems pretty dodgy, although I guess if you know there is just one ... ugh
	private static GUIManager instance;

	private float mowingOppTime;
	private bool isMowingOpp;

	void Start() {
		// perhaps should check here to make sure only one?
		instance = this;
		instructions.enabled = true;
		isMowingOpp = false;
		GameEventManager.GameStart += GameStart;
		GameEventManager.GameOver += GameOver;
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Return)) {
		    if (GameManager.gameStarted)
				GameEventManager.TriggerGameOver();
			else
				GameEventManager.TriggerGameStart();
		}
		if(isMowingOpp && Time.time - mowingOppTime > .7){
			isMowingOpp = false;
			instance.mowingOpponentText.text = "";
		}
	}

	
	IEnumerator FadeInstructions() {
		for (float f = 5f; f >= 0; f -= 0.05f) {
			Color c = instructions.color;
			c.a = f/5f;
			instructions.color = c;
			yield return new WaitForSeconds(.01f);
		}
		instructions.enabled = false;
	}
	
	private void GameStart() {
		instructions.enabled = false;
		gameOverText.enabled = false;
		//redScore.enabled = true;
		//blueScore.enabled = true;
		timeText.enabled = true;
		//StartCoroutine(FadeInstructions());
		//SetRedScore(GameManager.redScore);
		//SetBlueScore(GameManager.blueScore);
	}

	public void GameOver() {
		redScore.enabled = false;
		blueScore.enabled = false;
		timeText.enabled = false;
		gameOverText.enabled = true;
		instructions.enabled = true;
		if (GameManager.redPercent > GameManager.bluePercent){
			instance.gameOverText.color = Color.red;
			instance.gameOverText.text = "Red is the winnner!\nPress ENTER to Restart";
		}
		else if (GameManager.bluePercent > GameManager.redPercent){
			instance.gameOverText.color = Color.blue;
			instance.gameOverText.text = "Blue is the winnner!\nPress ENTER to Restart";
		}
		else
			instance.gameOverText.text = "It's a tie!\nPress ENTER to Restart";
	}

	public static void SetRedScore(int score){
		instance.redScore.text = "Red: " + score.ToString();
	}

	public static void SetBlueScore(int score){
		instance.blueScore.text = "Blue: " + score.ToString();
	}

	public static void setTime(float timeLeft) {
		instance.timeText.text = timeLeft.ToString();
	}
	public static void powerUpDisplay(string display){
		instance.powerUp.enabled = true;
		instance.powerUp.text = display;
	}
	public static void grassLoadDisplay(string display){
		instance.grassLoad.enabled = true;
		instance.grassLoad.text = display;
	}
	public static void mowingOpp(Color c){
		instance.isMowingOpp = true;
		instance.mowingOppTime = Time.time;
		instance.mowingOpponentText.color = c;
		instance.mowingOpponentText.text = "You are mowing\nyour opponents grass!";
	}
}