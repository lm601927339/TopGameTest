using System;
using UnityEngine;

/// <summary>天气类型；天气只描述规则，视觉预制体由 WeatherController 可选绑定。</summary>
public enum WeatherType{
    Clear = 0,
    Cloudy = 1,
    Tornado = 2,
    MagneticStorm = 3,
    Thunderstorm = 4,
    Blizzard = 5,
    ToxicFog = 6,
    RadiationStorm = 7,
    FireRain = 8
}

/// <summary>可序列化的天气运行时状态，方便时间轴和存档系统传递。</summary>
[Serializable]
public class WeatherState{
    public WeatherType weatherType = WeatherType.Clear;
    public float duration = -1f;
    public Vector2 globalWind = Vector2.zero;
    public float movementStrength = 1f;
    public bool lowerVisibility;

    public WeatherState Clone(){
        return new WeatherState{
            weatherType = weatherType,
            duration = duration,
            globalWind = globalWind,
            movementStrength = movementStrength,
            lowerVisibility = lowerVisibility
        };
    }
}

/// <summary>
/// 天气规则控制器：处理全局风、局部环境体积和持续时间。
/// </summary>
public class WeatherController : MonoBehaviour{
    [SerializeField] private WeatherState currentWeather = new WeatherState();
    public bool applyGlobalWind = true;
    public bool applyLocalVolume = true;
    public GameObject caster;
    public string sourceId = "weather";
    public int priority = 15;
    public float weatherRadius = 8f;
    public int weatherDamagePerTick = 5;
    [Range(0.05f, 1f)]
    [Tooltip("天气开启降低可见度时使用的视野距离倍率。0.55 表示默认 12 米视野缩短为约 6.6 米。")]
    public float loweredVisibilityMultiplier = 0.55f;
    public GameObject weatherVolumePrefab;
    public EnvironmentVolume weatherVolume;

    private GameObject spawnedWeatherVolume;
    private bool ownsWeatherVolume;

    /// <summary>当前天气类型，Clear 表示没有天气。</summary>
    public WeatherType CurrentWeather{ get { return currentWeather == null ? WeatherType.Clear : currentWeather.weatherType; } }
    public WeatherState CurrentState{ get { return currentWeather; } }
    public bool IsWeatherActive{ get { return CurrentWeather != WeatherType.Clear; } }
    public GameObject SpawnedWeatherVolume{ get { return spawnedWeatherVolume; } }
    /// <summary>仅在当前天气明确要求降低可见度时影响全局视野；否则返回 1。</summary>
    public float VisibilityRangeMultiplier{
        get{
            if (!IsWeatherActive || currentWeather == null || !currentWeather.lowerVisibility) return 1f;
            return Mathf.Clamp(loweredVisibilityMultiplier, 0.05f, 1f);
        }
    }

    private void Awake(){
        if (currentWeather == null) currentWeather = new WeatherState();
        if (string.IsNullOrEmpty(sourceId)) sourceId = "weather-" + GetInstanceID();
    }

    private void OnEnable(){
        WeatherVisibilityService.Register(this);
    }

    private void OnDisable(){
        WeatherVisibilityService.Unregister(this);
    }

    private void FixedUpdate(){
        if (!IsWeatherActive){
            RemoveGlobalWind();
            DisableLocalVolume();
            return;
        }

        ConfigureLocalVolume();
        if (currentWeather.duration >= 0f){
            currentWeather.duration -= Time.fixedDeltaTime;
            if (currentWeather.duration <= 0f){
                ClearWeather();
                return;
            }
        }

        if (!applyGlobalWind || currentWeather.globalWind.sqrMagnitude <= 0.0001f) return;
        Vector3 velocity = new Vector3(currentWeather.globalWind.x, currentWeather.globalWind.y, 0f) * currentWeather.movementStrength;
        for (int i = 0; i < ChaState.ActiveCharacters.Count; i++){
            ChaState state = ChaState.ActiveCharacters[i];
            if (state == null || state.dead) continue;
            state.AddOrRefreshMotionInfluence(new MotionInfluence(sourceId, velocity, 0.15f, priority, MotionInfluenceBlendMode.Additive, false));
        }
    }

    /// <summary>设置天气并立即刷新局部环境体积。</summary>
    public void SetWeather(WeatherType weatherType, float duration = -1f, Vector2 globalWind = default(Vector2), float movementStrength = 1f, bool lowerVisibility = false){
        // EditMode 工具可能在生命周期回调前直接设置天气；这里再次登记，保证视野规则立刻生效。
        WeatherVisibilityService.Register(this);
        if (currentWeather == null) currentWeather = new WeatherState();
        currentWeather.weatherType = weatherType;
        currentWeather.duration = duration;
        currentWeather.globalWind = globalWind;
        currentWeather.movementStrength = Mathf.Max(0f, movementStrength);
        currentWeather.lowerVisibility = lowerVisibility;
        if (weatherType == WeatherType.Clear){
            RemoveGlobalWind();
            DisableLocalVolume();
            return;
        }
        ConfigureLocalVolume();
    }

    /// <summary>用一份状态对象切换天气，调用方不会持有控制器内部可变引用。</summary>
    public void SetWeather(WeatherState state){
        if (state == null){
            ClearWeather();
            return;
        }
        WeatherState copy = state.Clone();
        SetWeather(copy.weatherType, copy.duration, copy.globalWind, copy.movementStrength, copy.lowerVisibility);
    }

    /// <summary>清除天气并回收由控制器创建的环境体积。</summary>
    public void ClearWeather(){
        if (currentWeather == null) currentWeather = new WeatherState();
        currentWeather.weatherType = WeatherType.Clear;
        currentWeather.duration = 0f;
        currentWeather.globalWind = Vector2.zero;
        RemoveGlobalWind();
        DisableLocalVolume();
    }

    /// <summary>供特殊天气、调试按钮和测试设置降视野倍率；不要求修改天气预制体。</summary>
    public void ConfigureVisibilityMultiplier(float multiplier){
        loweredVisibilityMultiplier = Mathf.Clamp(multiplier, 0.05f, 1f);
    }

    private EnvironmentVolumeKind GetVolumeKind(){
        switch (CurrentWeather){
            case WeatherType.Tornado: return EnvironmentVolumeKind.Tornado;
            case WeatherType.MagneticStorm: return EnvironmentVolumeKind.MagneticField;
            case WeatherType.Thunderstorm: return EnvironmentVolumeKind.Lightning;
            case WeatherType.Blizzard: return EnvironmentVolumeKind.IceWall;
            case WeatherType.ToxicFog: return EnvironmentVolumeKind.ToxicGas;
            case WeatherType.RadiationStorm: return EnvironmentVolumeKind.Radiation;
            case WeatherType.FireRain: return EnvironmentVolumeKind.FireOil;
            default: return EnvironmentVolumeKind.Cloud;
        }
    }

    /// <summary>创建或复用局部体积；外部绑定的体积不会被销毁。</summary>
    private void ConfigureLocalVolume(){
        if (!applyLocalVolume || !IsWeatherActive) return;
        if (weatherVolume == null && weatherVolumePrefab != null){
            spawnedWeatherVolume = Instantiate(weatherVolumePrefab, transform.position, Quaternion.identity);
            weatherVolume = spawnedWeatherVolume.GetComponent<EnvironmentVolume>();
            if (weatherVolume == null) weatherVolume = spawnedWeatherVolume.AddComponent<EnvironmentVolume>();
            ownsWeatherVolume = true;
        }
        if (weatherVolume == null){
            weatherVolume = GetComponent<EnvironmentVolume>();
            if (weatherVolume == null){
                weatherVolume = gameObject.AddComponent<EnvironmentVolume>();
                ownsWeatherVolume = true;
            }
        }
        if (spawnedWeatherVolume != null) spawnedWeatherVolume.transform.position = transform.position;
        weatherVolume.volumeKind = GetVolumeKind();
        weatherVolume.sourceId = sourceId + "-volume";
        weatherVolume.radius = weatherRadius;
        weatherVolume.duration = -1f;
        weatherVolume.damagePerTick = weatherDamagePerTick;
        weatherVolume.applyDamage = true;
        weatherVolume.affectEnemies = true;
        weatherVolume.affectAllies = true;
        weatherVolume.caster = caster;
        weatherVolume.enabled = true;
        weatherVolume.ApplyNow();
    }

    private void DisableLocalVolume(){
        if (spawnedWeatherVolume != null){
            Destroy(spawnedWeatherVolume);
            spawnedWeatherVolume = null;
            weatherVolume = null;
            ownsWeatherVolume = false;
            return;
        }
        if (weatherVolume != null){
            if (ownsWeatherVolume){
                Destroy(weatherVolume);
                weatherVolume = null;
                ownsWeatherVolume = false;
            }else{
                weatherVolume.enabled = false;
            }
        }
    }

    private void RemoveGlobalWind(){
        for (int i = 0; i < ChaState.ActiveCharacters.Count; i++){
            ChaState state = ChaState.ActiveCharacters[i];
            if (state != null) state.RemoveForceMove(sourceId);
        }
    }

    private void OnDestroy(){
        RemoveGlobalWind();
        DisableLocalVolume();
    }
}
