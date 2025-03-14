using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EditorUtilities/GameManager", menuName = "Game/Manager")]
public class GameManager : ScriptableObject
{
	public static GameManager Instance;
	
	// Options
	public GlobalSettings Global;
	public UISettings UI;
	
	//public static FrameRateManager FrameRateManager { get; private set; }
	
	public static EventStorage EventStorage { get; private set; }
	public static PlayerStats PlayerStats { get; private set; }

	private static string _buildNumber = "1";

	public static void OverridePlayerStats(PlayerStats stats)
	{
		PlayerStats = stats;
	}

	public static void OverrideEventStorage(EventStorage eventStorage)
	{
		EventStorage = eventStorage;
	}
	
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void ReloadGame()
	{
		Instance = Resources.Load<GameManager>("EditorUtilities/GameManager");

		_buildNumber = ResourceLoader.GetResource<BuildScriptableObject>("EditorUtilities/Build").BuildNumber;

		//FrameRateManager = new();
		Application.targetFrameRate = Instance.Global.FrameRateLocked;

		PlayerStats = new();
		PlayerStats.Init();
		
		ResourceLoader.ReloadGlossary();
		
		EventStorage = new();
		EventStorage.Load();
	}

	public static void Restart()
	{
		IEnumerable<KeyValuePair<string, float>> saveGlobal = PlayerStats.GetGlobalFlags();
		PlayerStats = new();
		PlayerStats.Init();
		foreach ((string flag, float value) in saveGlobal)
		{
			PlayerStats.SetFlag(flag, value);
		}
		
		EventStorage = new();
		EventStorage.Load();
	}

	public static string GetVersion()
	{
		return $"v{Application.version}.{_buildNumber}";
	}
}

[Serializable]
public struct UISettings
{
	public float AnimationDurationSec;
	public float AnimationScale;
	public float AnimationShakeStrength;
	public float AnimationDelaySec;
	
	public float PopUpDuration;
}

[Serializable]
public struct GlobalSettings
{
	public int FrameRateLocked;
}