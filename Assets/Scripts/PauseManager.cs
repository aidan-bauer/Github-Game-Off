using UnityEngine;

public class PauseManager : MonoBehaviour {

    public KeyCode pauseKey = KeyCode.Escape;
    public GameObject pauseCanvas;

    static bool isPaused;

    public static bool IsPaused
    {
        get
        {
            return isPaused;
        }
    }
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            SetTimeScale(isPaused);
            SetPauseUI(isPaused);
        }
	}

    public void SetTimeScale(bool paused)
    {
        Time.timeScale = paused ? 0f : 1f;
    }

    public void SetPauseUI(bool paused) {
        //set pause canvas visible
        pauseCanvas.SetActive(paused);
    }

    public void Reset()
    {
        isPaused = false;
    }
}
