using UnityEngine;
using System.Collections;

public class dontDestroy : MonoBehaviour {

	static bool musicStart = false;
	AudioSource aSource;

	void Awake() {
		aSource = gameObject.GetComponent<AudioSource> ();
		if(!musicStart){
			aSource.Play ();
			musicStart = true;
		}
		
		DontDestroyOnLoad(this.gameObject);
	}
}
