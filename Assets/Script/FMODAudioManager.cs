using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// FMOD Audio Manager for GGJ Masking Project
/// 根据 GGJ Masking Fmod Project API.docx 文档创建
/// 
/// 使用方法：
/// 1. 将此脚本挂载到场景中的一个空 GameObject 上
/// 2. 在需要播放音效的地方调用对应的方法
/// </summary>
public class FMODAudioManager : MonoBehaviour
{
    public static FMODAudioManager Instance { get; private set; }

    #region Event Paths
    private const string BGM_EVENT = "event:/BGM";
    private const string LEVEL_SOUND_DESIGN_EVENT = "event:/Level Sound Design";
    #endregion

    #region Parameter Names
    // BGM 参数
    private const string PARAM_PREPARE = "Prepare";

    // Level Sound Design 参数
    private const string PARAM_COLLISION = "Collision";
    private const string PARAM_EJECTION = "Ejection";
    private const string PARAM_VF = "VF";
    private const string PARAM_STATEMENT = "Statement";
    #endregion

    #region Event Instances
    private EventInstance bgmInstance;
    private EventInstance levelSoundInstance;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 初始化并播放 BGM
        InitializeBGM();
        PlayBGM();
    }

    private void OnDestroy()
    {
        StopAndReleaseBGM();
        StopAndReleaseLevelSound();
    }
    #endregion

    #region BGM 控制
    /// <summary>
    /// 初始化 BGM
    /// </summary>
    public void InitializeBGM()
    {
        if (!bgmInstance.isValid())
        {
            bgmInstance = RuntimeManager.CreateInstance(BGM_EVENT);
        }
    }

    /// <summary>
    /// 播放 BGM
    /// </summary>
    public void PlayBGM()
    {
        if (!bgmInstance.isValid())
        {
            InitializeBGM();
        }
        bgmInstance.start();
    }

    /// <summary>
    /// 停止并释放 BGM
    /// </summary>
    public void StopAndReleaseBGM()
    {
        if (bgmInstance.isValid())
        {
            bgmInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            bgmInstance.release();
        }
    }

    /// <summary>
    /// 设置 BGM Prepare 参数
    /// Close(0) = 高切效果，小球未发射或死亡回到起点
    /// Open(1) = 全频段，发射瞬间
    /// </summary>
    public void SetBGMPrepare(bool isOpen)
    {
        if (bgmInstance.isValid())
        {
            bgmInstance.setParameterByName(PARAM_PREPARE, isOpen ? 1f : 0f);
        }
    }

    /// <summary>
    /// BGM 切换到 Open 状态（发射时调用）
    /// </summary>
    public void BGMOpen()
    {
        SetBGMPrepare(true);
    }

    /// <summary>
    /// BGM 切换到 Close 状态（准备阶段或死亡回到起点）
    /// </summary>
    public void BGMClose()
    {
        SetBGMPrepare(false);
    }
    #endregion

    #region Level Sound Design 控制
    /// <summary>
    /// 初始化 Level Sound Design 事件
    /// </summary>
    private void InitializeLevelSound()
    {
        if (!levelSoundInstance.isValid())
        {
            levelSoundInstance = RuntimeManager.CreateInstance(LEVEL_SOUND_DESIGN_EVENT);
        }
    }

    /// <summary>
    /// 停止并释放 Level Sound
    /// </summary>
    private void StopAndReleaseLevelSound()
    {
        if (levelSoundInstance.isValid())
        {
            levelSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            levelSoundInstance.release();
        }
    }

    /// <summary>
    /// 播放一次性音效（One-Shot）
    /// 设置参数后自动播放并重置
    /// </summary>
    private void PlayOneShotLevelSound(string paramName, float value)
    {
        // 创建新实例用于 One-Shot
        EventInstance instance = RuntimeManager.CreateInstance(LEVEL_SOUND_DESIGN_EVENT);
        instance.setParameterByName(paramName, value);
        instance.start();
        instance.release(); // 播放完成后自动释放
    }

    // ==================== 蓄力与发射 ====================
    
    /// <summary>
    /// 蓄力开始时调用
    /// Ejection = isCharging(1)
    /// </summary>
    public void PlayEjectionCharging()
    {
        PlayOneShotLevelSound(PARAM_EJECTION, 1f);
        Debug.Log("[FMOD] 播放蓄力音效");
    }

    /// <summary>
    /// 发射瞬间调用
    /// Ejection = isReleased(2)
    /// 同时将 BGM 切换到 Open
    /// </summary>
    public void PlayEjectionReleased()
    {
        PlayOneShotLevelSound(PARAM_EJECTION, 2f);
        BGMOpen(); // 发射时 BGM 切换到全频段
        Debug.Log("[FMOD] 播放发射音效");
    }

    // ==================== 碰撞 ====================
    
    /// <summary>
    /// 碰撞边框时调用
    /// Collision = Hit(1)
    /// 会随机播放 hit_1/hit_2/hit_3 之一
    /// </summary>
    public void PlayCollisionHit()
    {
        PlayOneShotLevelSound(PARAM_COLLISION, 1f);
        Debug.Log("[FMOD] 播放碰撞音效");
    }

    // ==================== 状态切换（鼠标按键） ====================
    
    /// <summary>
    /// 鼠标左键 - 重力开关时调用
    /// Statement = Gravity ONOFF(1)
    /// </summary>
    public void PlayGravityToggle()
    {
        PlayOneShotLevelSound(PARAM_STATEMENT, 1f);
        Debug.Log("[FMOD] 播放重力切换音效");
    }

    /// <summary>
    /// 鼠标右键 - 碰撞开关时调用
    /// Statement = Collision ONOFF(2)
    /// </summary>
    public void PlayCollisionToggle()
    {
        PlayOneShotLevelSound(PARAM_STATEMENT, 2f);
        Debug.Log("[FMOD] 播放碰撞切换音效");
    }

    // ==================== 胜利与失败 ====================
    
    /// <summary>
    /// 胜利时调用
    /// VF = Victory(1)
    /// </summary>
    public void PlayVictory()
    {
        PlayOneShotLevelSound(PARAM_VF, 1f);
        Debug.Log("[FMOD] 播放胜利音效");
    }

    /// <summary>
    /// 失败时调用
    /// VF = Fail(2)
    /// 同时将 BGM 切换到 Close
    /// </summary>
    public void PlayFail()
    {
        PlayOneShotLevelSound(PARAM_VF, 2f);
        BGMClose(); // 失败时 BGM 切换回高切效果
        Debug.Log("[FMOD] 播放失败音效");
    }

    // ==================== 重生 ====================
    
    /// <summary>
    /// 死亡/重生回到起点时调用
    /// 将 BGM 切换回 Close 状态
    /// </summary>
    public void OnPlayerRespawn()
    {
        BGMClose();
        Debug.Log("[FMOD] 玩家重生，BGM 切换到准备状态");
    }
    #endregion
}
