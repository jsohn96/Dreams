using UnityEngine;
using System.Collections;

public class start : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown (KeyCode.Space)){
			GameObject.Find ("gameController").GetComponent<fading>().BeginFade(1);
			StartCoroutine(givenUp());
		}
	}

	IEnumerator givenUp(){
		yield return new WaitForSeconds(1f);
		Application.LoadLevel (Application.loadedLevel + 1);
	}
}
