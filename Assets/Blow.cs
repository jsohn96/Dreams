using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Blow : MonoBehaviour {

	private const int FREQUENCY = 48000;    // Wavelength, I think.
	private const int SAMPLECOUNT = 1024;   // Sample Count.
	private const float REFVALUE = 0.1f;    // RMS value for 0 dB.
	private const float THRESHOLD = 0.02f;  // Minimum amplitude to extract pitch (recieve anything)
	private const float ALPHA = 0.05f;      // The alpha for the low pass filter.
	[SerializeField] AudioSource windSound;
	[SerializeField] Transform lightCircle; 
	Vector3 lightScale;
//	AudioSource micInput;
	float time;


	void Awake () {
//		micInput = gameObject.GetComponent<AudioSource> ();
		time = 0f;
	}

	// Use this for initialization
	void Start () {
//		StartMicListener();
		windSound.Play ();
		lightScale = lightCircle.localScale;
	}
	
	// Update is called once per frame
	void Update () {
		
//		if (micInput.isPlaying) {
////			windSound.clip = otherClip;
//			windSound.Play ();
//		} else {
////			windSound.Pause ();
//			windSound.Play ();
//		}
//		windSound.volume = MicInput.MicLoudness* 100f;
//		if (MicInput.MicLoudness * 100f > 0.5f) {
			easeVolume (MicInput.MicLoudness * 100f);
//			Debug.Log (MicInput.MicLoudness * 100f);
//		}
	}


	void easeVolume(float micVolume) {
		if (windSound.volume < 1.0f || windSound.volume > 0f) {
			time += Time.deltaTime;

			if (micVolume > 0.5f) {
				if (time > 0.5f) {
					windSound.volume += 0.02f;
					if (lightScale.x < 1.3f) {
						lightScale.x += 0.015f;
						lightScale.y += 0.015f;
					}
				}
			} else {
				windSound.volume -= 0.01f;
				time = 0f;
				if (lightScale.x > 0.1) {
					lightScale.x -= 0.01f;
					lightScale.y -= 0.01f;
				}
			}
			lightCircle.localScale = lightScale;
		}
	}

	/// Starts the Mic, and plays the audio back in (near) real-time.
//	private void StartMicListener() {
//		micInput.clip = Microphone.Start("Built-in Microphone", true, 999, FREQUENCY);
		// HACK - Forces the function to wait until the microphone has started, before moving onto the play function.
//		while (!(Microphone.GetPosition("Built-in Microphone") > 0)) {
//		} micInput.Play();
//		windSound.Play();
//	}
}
