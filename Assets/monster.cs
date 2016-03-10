using UnityEngine;
using System.Collections;

public class monster : MonoBehaviour {

	SpriteRenderer sr;
	Color monsterColor;
	bool dead = false;
	AudioSource deathAudio;
	float angle;
	Rigidbody2D rb;
	bool attacked = false;
	bool born = true;

	// Use this for initialization
	void Start () {
		rb = gameObject.GetComponent <Rigidbody2D> ();
		sr = gameObject.GetComponent<SpriteRenderer> ();
		deathAudio = gameObject.GetComponent<AudioSource>();
		monsterColor = sr.color;
	}

	// Update is called once per frame
	void FixedUpdate () {
		if(attacked){
			monsterColor.a -= 0.05f;
			sr.color = monsterColor;
			if(monsterColor.a < 0.09f){
				Destroy (gameObject);
			}
		}
		else if (dead) {
			monsterColor.a -= 0.05f;
			rb.velocity = new Vector2(0f,0f);
			sr.color = monsterColor;
			if(!deathAudio.isPlaying){
				Destroy (gameObject);
			}
		} else if(born){
			born = false;
//			angle = Mathf.Atan2(transform.position.x, transform.position.y) * Mathf.Rad2Deg;
			rb.velocity = new Vector2(-transform.position.x/4f, -transform.position.y/4f);
		}
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.tag == "Light") {
			dead = true;
			deathAudio.Play ();
		}
	}
		

	void OnCollisionEnter2D(Collision2D other) {
		
		if (other.gameObject.tag == "Child") {
			attacked = true;
		}
	}
}
