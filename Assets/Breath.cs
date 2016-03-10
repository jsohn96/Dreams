using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Breath : MonoBehaviour {
	private Color[] palette;
	// the game controller target 
	// you dont need this....
	public GameObject boat;
	public GameObject laughter;
	public GameObject spherePrefab;
	private GameObject currentBubble;
	public GameObject m_camera;
	public GameObject pop;
	private int bubbleCount;
	private const int FREQUENCY = 48000;    // Wavelength, I think.
	private const int SAMPLECOUNT = 1024;   // Sample Count.
	private const float REFVALUE = 0.1f;    // RMS value for 0 dB.
	private const float THRESHOLD = 0.02f;  // Minimum amplitude to extract pitch (recieve anything)
	private const float ALPHA = 0.05f;      // The alpha for the low pass filter (I don't really understand this).
	public GameObject resultDisplay;   // GUIText for displaying results
	public GameObject blowDisplay;     // GUIText for displaying blow or not blow.
	public int recordedLength = 50;    // How many previous frames of sound are analyzed.
	public int requiedBlowTime = 4;    // How long a blow must last to be classified as a blow (and not a sigh for instance).
	public int clamp = 160;            // Used to clamp dB (I don't really understand this either).
	private float rmsValue;            // Volume in RMS
	private float dbValue;             // Volume in DB
	private float pitchValue;          // Pitch - Hz (is this frequency?)
	private int blowingTime;           // How long each blow has lasted
	private float lowPassResults;      // Low Pass Filter result
	private float peakPowerForChannel; //
	private float[] samples;           // Samples
	private float[] spectrum;          // Spectrum
	private List<float> dbValues;      // Used to average recent volume.
	private List<float> pitchValues;   // Used to average recent pitch.
	void Awake(){
		bubbleCount = 0;

//		palette = new Color[8];
//		palette [0] = new Color (165.0f / 255.0f, 208.0f / 255.0f, 174.0f / 255.0f);
//		palette [1] = new Color (239.0f / 255.0f, 97.0f / 255.0f, 83.0f / 255.0f);
//		palette [2] = new Color (247.0f / 255.0f, 168.0f / 255.0f, 181.0f / 255.0f);
//		palette [3] = new Color (226.0f / 255.0f, 220.0f / 255.0f, 207.0f / 255.0f);
//		palette [4] = new Color (248.0f / 255.0f, 187.0f / 255.0f, 17.0f / 255.0f);
//		palette [5] = new Color (205.0f / 255.0f, 173.0f / 255.0f, 163.0f / 255.0f);
//		palette [6] = new Color (175.0f / 255.0f, 215.0f / 255.0f, 220.0f / 255.0f);
//		palette [7] = new Color (14.0f / 255.0f, 118.0f / 255.0f, 135.0f / 255.0f);
		genMeter ();
	}
	public void Start () {
		samples = new float[SAMPLECOUNT];
		spectrum = new float[SAMPLECOUNT];
		dbValues = new List<float>();
		pitchValues = new List<float>();
		StartMicListener();

	}
	public void Update () {
		// If the audio has stopped playing, this will restart the mic play the clip.
		if (!GetComponent<AudioSource>().isPlaying) {
			StartMicListener();
		}
		// Gets volume and pitch values
		AnalyzeSound();
		// Runs a series of algorithms to decide whether a blow is occuring.
		DeriveBlow();
		// Update the meter display.
		if (resultDisplay){
			resultDisplay.GetComponent<GUIText>().text = "RMS: " + rmsValue.ToString("F2") + " (" + dbValue.ToString("F1") + " dB)\n" + "Pitch: " + pitchValue.ToString("F0") + " Hz";
		}
		GameObject curbubble;
		if (GameObject.FindGameObjectWithTag ("Meter") != null) {
			curbubble = GameObject.FindGameObjectWithTag ("Meter");
			if (curbubble.transform.localScale.x >= 19.0f) {
				Debug.Log ("Transition to next scene");
				pop.GetComponent<AudioSource> ().Play ();
				bubbleCount += 1;
				Destroy (curbubble);
				genMeter ();
			}
		}
	}
	/// Starts the Mic, and plays the audio back in (near) real-time.
	private void StartMicListener() {
		GetComponent<AudioSource>().clip = Microphone.Start("Built-in Microphone", true, 999, FREQUENCY);
		// HACK - Forces the function to wait until the microphone has started, before moving onto the play function.
		while (!(Microphone.GetPosition("Built-in Microphone") > 0)) {
		} GetComponent<AudioSource>().Play();
	}
	/// Credits to aldonaletto for the function, http://goo.gl/VGwKt
	/// Analyzes the sound, to get volume and pitch values.
	private void AnalyzeSound() {
		// Get all of our samples from the mic.
		GetComponent<AudioSource>().GetOutputData(samples, 0);
		// Sums squared samples
		float sum = 0;
		for (int i = 0; i < SAMPLECOUNT; i++){
			sum += Mathf.Pow(samples[i], 2);
		}
		// RMS is the square root of the average value of the samples.
		rmsValue = Mathf.Sqrt(sum / SAMPLECOUNT);
		dbValue = 20 * Mathf.Log10(rmsValue / REFVALUE);
		// Clamp it to {clamp} min
		if (dbValue < -clamp) {
			dbValue = -clamp;
		}
		// Gets the sound spectrum.
		GetComponent<AudioSource>().GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
		float maxV = 0;
		int maxN = 0;
		// Find the highest sample.
		for (int i = 0; i < SAMPLECOUNT; i++){
			if (spectrum[i] > maxV  && spectrum[i] > THRESHOLD){
				maxV = spectrum[i];
				maxN = i; // maxN is the index of max
			}
		}
		// Pass the index to a float variable
		float freqN = maxN;
		// Interpolate index using neighbours
		if (maxN > 0 && maxN < SAMPLECOUNT - 1) {
			float dL = spectrum[maxN-1] / spectrum[maxN];
			float dR = spectrum[maxN+1] / spectrum[maxN];
			freqN += 0.5f * (dR * dR - dL * dL);
		}
		// Convert index to frequency
		pitchValue = freqN * 24000 / SAMPLECOUNT;
	}
	private void DeriveBlow() {
		UpdateRecords(dbValue, dbValues);
		UpdateRecords(pitchValue, pitchValues);
		// Find the average pitch in our records (used to decipher against whistles, clicks, etc).
		float sumPitch = 0;
		foreach (float num in pitchValues) {
			sumPitch += num;
		}
		sumPitch /= pitchValues.Count;
		// Run our low pass filter.
		lowPassResults = LowPassFilter(dbValue);
		//Debug.Log ("Lowpass " + lowPassResults);
		// Decides whether this instance of the result could be a blow or not.
		if (lowPassResults > -10 && sumPitch == 0) {
			blowingTime += 1;
		} else {
			blowingTime = 0;
		}
		// Once enough successful blows have occured over the previous frames (requiredBlowTime), the blow is triggered.
		// This example says "blowing", or "not blowing", and also blows up a sphere.
		if (blowingTime > requiedBlowTime) {
			Debug.Log ("Blowing");
			//blowDisplay.GetComponent<GUIText>().text = "Blowing";
			//GameObject.FindGameObjectWithTag("Meter").transform.localScale *= 1.012f
			onBlow ();
			if(blowingTime > requiedBlowTime *2){
//				laughter.GetComponent<RandomLaughter> ().onTickle ();
			}
		} else {
			Debug.Log ("Not Blowing");	
			//blowDisplay.GetComponent<GUIText>().text = "Not blowing";
			if (GameObject.FindGameObjectWithTag ("Meter").transform.localScale.x >= 0.5f) {
				GameObject.FindGameObjectWithTag("Meter").transform.localScale *= 0.998f;
			}
		}
	}
	// Updates a record, by removing the oldest entry and adding the newest value (val).
	private void UpdateRecords(float val, List<float> record) {
		if (record.Count > recordedLength) {
			record.RemoveAt(0);
		}
		record.Add(val);
	}
	/// Gives a result (I don't really understand this yet) based on the peak volume of the record
	/// and the previous low pass results.
	private float LowPassFilter(float peakVolume) {
		return ALPHA * peakVolume + (1.0f - ALPHA) * lowPassResults;
	}
	void onBlow(){
		GameObject.FindGameObjectWithTag("Meter").transform.localScale *= 1.012f;
	}
	void genMeter(){
		GameObject cloneBubble = Instantiate (spherePrefab, transform.position, transform.rotation) as GameObject;
		cloneBubble.tag = "Meter";
		currentBubble = cloneBubble;
//		int colorIdx = bubbleCount % 8;
		Camera cs = m_camera.GetComponent<Camera> ();
		//	cs.backgroundColor = Color.Lerp(cs.backgroundColor, palette[colorIdx], 2.0f);
//		cs.backgroundColor = palette [colorIdx];
//		if (colorIdx < palette.Length - 1) {
//			colorIdx += 1;
//		} else {
//			colorIdx = 0;
//		}
//		currentBubble.GetComponent<Renderer> ().material.color = palette [colorIdx];
	}


}