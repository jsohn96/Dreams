using UnityEngine;
using System.Collections;

public class monsterSpawn : MonoBehaviour {

	[SerializeField] float minRange = 144f;
	[SerializeField] float maxRange = 196f;
	float distance;
	[SerializeField] Transform monsterPrefab;
	int maxLives = 5;
	int lives;
	float timer;
	float spawnAverage = 2f;
	float spawnSpeed = 3f;
	int instantiateCount = 0;
	SpriteRenderer sr;
	Vector2 tempPoint;
	AudioSource aSource;


	// Use this for initialization
	void Start () {
		lives = maxLives;
		aSource = gameObject.GetComponent <AudioSource> ();
		sr = gameObject.GetComponent <SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (lives == 0) {
			GameObject.Find ("gameController").GetComponent<fading>().BeginFade(1);
			StartCoroutine(givenUp());
		} else {
			timer += Time.deltaTime;
			if (timer > spawnSpeed && instantiateCount > 2) {
				timer = 0f;
				drawMonster ();
				Instantiate (monsterPrefab, drawMonster (), new Quaternion (0f, 0f, 0f, 0f));
				instantiateCount++;
				spawnSpeed = Random.Range (spawnAverage - 2f, spawnAverage + 2f);
			} else if (instantiateCount < 3) {
				instantiateCount++;
			}
			if (!aSource.isPlaying) {
				sr.color = new Color (1f, 1f, 1f, (float)lives/maxLives);
				Debug.Log ((float)lives / maxLives);
			}
		}
	}

	Vector2 drawMonster(){
		float x = Random.Range (-12.0F, 12.0F);
		float y = Random.Range (-12.0F, 12.0F);
		tempPoint.Set (x, y);
		distance = tempPoint.sqrMagnitude;
		if(distance<minRange || distance > maxRange){
			drawMonster ();
		}
			return tempPoint;
	}


	void OnCollisionEnter2D(Collision2D other) {
		if (other.gameObject.tag == "Monster") {
			aSource.Play ();
			sr.color = new Color(1f,0f,0f);
			lives--;
		}
	}


	IEnumerator givenUp(){
		yield return new WaitForSeconds(5f);
		Application.LoadLevel (0);
	}
}
