using UnityEngine;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("要加载的场景名称，留空则加载 Build Settings 中的下一个场景")]
    public string nextSceneName = "";
    
    [Tooltip("是否使用场景索引（如果为true，则使用nextSceneIndex）")]
    public bool useSceneIndex = true;
    
    [Tooltip("要加载的场景索引（仅当useSceneIndex为true时生效）")]
    public int nextSceneIndex = 1;

    [Header("Input Settings")]
    [Tooltip("启用前的延迟时间（秒），防止误触")]
    public float inputDelay = 0.5f;

    private float timer = 0f;
    private bool canStart = false;

    void Start()
    {
        timer = 0f;
        canStart = false;
    }

    void Update()
    {
        // 延迟后才能接受输入
        if (!canStart)
        {
            timer += Time.deltaTime;
            if (timer >= inputDelay)
            {
                canStart = true;
            }
            return;
        }

        // 检测鼠标点击或任意键
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.anyKeyDown)
        {
            LoadNextScene();
        }
    }

    private void LoadNextScene()
    {
        if (useSceneIndex)
        {
            // 使用场景索引加载
            SceneManager.LoadScene(nextSceneIndex);
        }
        else if (!string.IsNullOrEmpty(nextSceneName))
        {
            // 使用场景名称加载
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            // 加载 Build Settings 中的下一个场景
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            int nextIndex = currentIndex + 1;
            
            // 确保不超出范围
            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextIndex);
            }
            else
            {
                Debug.LogWarning("[StartManager] 没有下一个场景可加载！");
            }
        }
    }
}
