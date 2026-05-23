using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
 
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
 
    [SerializeField]
    private MusicLibrary musicLibrary;
    [SerializeField]
    private AudioSource musicSource;

    [System.Serializable]
    public struct SceneTrack
    {
        public string sceneName;
        public string trackName;
    }

    [Header("Scene Music")]
    [SerializeField]
    private SceneTrack[] sceneTracks;
 
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string trackToPlay = null;
        if (sceneTracks != null)
        {
            foreach (var st in sceneTracks)
            {
                if (!string.IsNullOrEmpty(st.sceneName) && st.sceneName == scene.name)
                {
                    trackToPlay = st.trackName;
                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(trackToPlay))
        {
            trackToPlay = scene.name;
        }

        // Only play if the clip exists
        if (musicLibrary != null && musicLibrary.GetClipFromName(trackToPlay) != null)
        {
            PlayMusic(trackToPlay);
        }
    }
 
    public void PlayMusic(string trackName, float fadeDuration = 0.5f)
    {
        if (musicLibrary == null)
        {
            return;
        }

        AudioClip clip = musicLibrary.GetClipFromName(trackName);
        if (clip == null)
        {
            return;
        }

        if (musicSource == null)
        {
            return;
        }

        StartCoroutine(AnimateMusicCrossfade(clip, fadeDuration));
    }
 
    IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float fadeDuration = 0.5f)
    {
        float percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSource.volume = Mathf.Lerp(1f, 0, percent);
            yield return null;
        }
 
        musicSource.clip = nextTrack;
        musicSource.Play();
 
        percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * 1 / fadeDuration;
            musicSource.volume = Mathf.Lerp(0, 1f, percent);
            yield return null;
        }
    }
}