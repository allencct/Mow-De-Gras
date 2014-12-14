using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour {
	
	private float timeLeft;
	private Color targetColor;
	
	private Color prevColor;
	
	/*void Start () {
		InvokeRepeating ("ChangeColor", 0f, .5f); 
	}
	
	void ChangeColor () {
		//prevColor = renderer.material.GetColor ("_Color");
		//renderer.material.color = Color.Lerp(prevColor, new Color( Random.value, Random.value, Random.value, 1.0f ), Time.time);
		renderer.material.color = new Color( Random.value, Random.value, Random.value, 1.0f );
	}*/
	
	// Update is called once per frame
	void Update () {
		
		transform.Rotate (new Vector3 (15, 30, 45) * Time.deltaTime);
		
		if (timeLeft <= Time.deltaTime) {
			// transition complete
			// assign the target color
			renderer.material.color = targetColor;
			
			// start a new transition
			switch(Random.Range (0,5)){
				case 1:
				targetColor = new Color(255/255f, 102/255f, 102/255f);
					break;
				case 2:
				targetColor = new Color(255/255f, 204/255f, 204/255f);
					break;
				case 3:
				targetColor = new Color(152/255f, 204/255f, 255/255f);
					break;
				case 4:
				targetColor = new Color(255/255f, 178/255f, 102/255f);
					break;
				case 5:
				targetColor = new Color(255/255f, 255/255f, 102/255f);
					break;
			}
			timeLeft = 0.5f;
		}
		else {
			// transition in progress
			// calculate interpolated color
			renderer.material.color = Color.Lerp(renderer.material.color, targetColor, Time.deltaTime / timeLeft);
			
			// update the timer
			timeLeft -= Time.deltaTime;
		}
	}
}
