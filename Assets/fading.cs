using UnityEngine;
using System.Collections;

public class fading : MonoBehaviour {
	public Texture2D fadeOutTexture; //the texture that will overlay the screen. Black image or loading graphic
	public float fadeSpeed= 1f; // the fading speed
	
	private int drawDepth = -1000; // the texture's order in the draw hierarchy: a low number means render on top
	private float alpha = 1.0f; // the texture's alpha value between 0 - 1
	private int fadeDir = -1; // the direction to fade: in = -1 or out = 1
	void Start(){
		Cursor.visible = false;
	}
	void OnGUI() {
		//fade out/ in the alpha value using a direction, a speed and TIme.deltattime to conver the operation to seconds
		alpha += fadeDir * fadeSpeed * Time.deltaTime;
		alpha = Mathf.Clamp01 (alpha);
		
		//set the color of our GUI
		GUI.color = new Color (GUI.color.r, GUI.color.g, GUI.color.b, alpha);
		GUI.depth = drawDepth;
		GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), fadeOutTexture);
	}
	
	// sets fadeDir to the direction paramete making the scene fade in if -1 and out if 1
	public float BeginFade(int direction){
		fadeDir = direction;
		return (fadeSpeed);
	}
	
	//OnLevelWasLoaded is called when a level is loaded. takes loaded level index as a parameter
	void OnLevelWasLoaded() {
		//alpha = 1
		BeginFade (-1);
	}
}
//StartCoroutine(ChangeLevel());
//IEnumerator ChangeLevel(){
//	yield return new WaitForSeconds(0.5f);
//	float fadeTime = GameObject.Find ("Main").GetComponent<fading> ().BeginFade (1);
//	yield return new WaitForSeconds(fadeTime);
//	if (Application.loadedLevel != 5) {
//		Application.LoadLevel (Application.loadedLevel + 1);
//	} else {
//		Application.LoadLevel(5);
//	}
//}
