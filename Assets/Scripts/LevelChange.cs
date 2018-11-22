using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChange : MonoBehaviour {

    public Animator anim;

    private string levelToLoad;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void FadeToLevel(string levelName)
    {
        anim.SetTrigger("ChangeLevel");
        levelToLoad = levelName;
    }

    public void OnFadeComplete()
    {
        SceneManager.LoadScene(levelToLoad);
    }
}
