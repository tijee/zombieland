using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
	public static SoundManager instance = null;

	public AudioSource enemyAudioSource;
	public float lowPitchRange = .8f;
	public float highPitchRange = 1.2f;

	void Awake ()
	{
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}
		DontDestroyOnLoad (gameObject);
	}

	// Randomly plays one of the given audio clips on the enemy source. If a sound is already playing, do nothing.
	public void PlayEnemySound (params AudioClip[] clips)
	{
		if (!enemyAudioSource.isPlaying) {
			int randomIndex = Random.Range (0, clips.Length);
			float randomPitch = Random.Range(lowPitchRange, highPitchRange);
			enemyAudioSource.pitch = randomPitch;
			enemyAudioSource.clip = clips [randomIndex];
			enemyAudioSource.Play ();
		}
	}
}
