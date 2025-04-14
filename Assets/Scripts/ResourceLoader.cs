using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

public static class ResourceLoader
{
	private static readonly Dictionary<string, Object> _resources = new();

	/*private static Dictionary<string, string> _glossary = new();
	private static Dictionary<string, string> _glossaryLinks = new();*/

	public static T GetResource<T>(string path, bool force = false) where T : Object
	{
		if (!force && _resources.TryGetValue(path, out Object result)) return (T)result;

		result = Resources.Load<T>(path);
		_resources[path] = result;
		return (T)result;
	}

	/// <summary>
	/// Loads JSON resource from persistent storage or save it in storage from build Resources.
	/// </summary>
	/// <param name="path">Path to the resource, same for Resources and PersistentDataPath</param>
	/// <returns>Deserialized JSON resource as T</returns>
	public static T GetPersistentJson<T>(string path)
	{
		string filePath = Application.persistentDataPath + "/" + path + ".json";
		string rawData;
		T result = default;

		bool loadedFromStorage = false;
		if (File.Exists(filePath))
		{
			rawData = File.ReadAllText(filePath);
			try
			{
				result = JsonConvert.DeserializeObject<T>(rawData);
				loadedFromStorage = true;
			}
			catch
			{
				Debug.LogWarning($"Failed to load persistent file: {filePath}");
			}
		}
		
		if (!loadedFromStorage) // If file is failed to load from storage, load it from resources and save in storage
		{
			TextAsset asset = GetResource<TextAsset>(path);
			if (asset != null)
			{
				rawData = asset.text;
				Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
				File.WriteAllText(filePath, rawData);
				result = JsonConvert.DeserializeObject<T>(rawData);
			}
			else return result;
		}

		return result;
	}

	/// <summary>
	/// Write data to persistent storage.
	/// </summary>
	/// <param name="path">If object can be loaded from Resources, path should be the same</param>
	/// <param name="data">Target object</param>
	public static void SavePersistent<T>(string path, T data)
	{
		string filePath = Application.persistentDataPath + "/" + path + ".json";
		File.WriteAllText(filePath, JsonConvert.SerializeObject(data));
	}

	/*
	public static Dictionary<string, string> GetAdIds()
	{
		return JsonConvert.DeserializeObject<Dictionary<string, string>>(Resources.Load<TextAsset>("AdIds").text);
	}

	public static void ReloadGlossary()
	{
		_glossary =
			JsonConvert.DeserializeObject<Dictionary<string, string>>(Resources.Load<TextAsset>("Glossary").text);
		_glossaryLinks =
			JsonConvert.DeserializeObject<Dictionary<string, string>>(
				Resources.Load<TextAsset>("GlossaryLinks").text
			);
	}

	public static string GetGlossaryText(string id)
	{
		return _glossary.GetValueOrDefault(id);
	}

	public static void AddGlossaryLinks(List<GameEvent> events)
	{
		foreach (GameEvent gameEvent in events)
		{
			foreach ((string regexp, string style) in _glossaryLinks)
			{
				Regex regex = new(regexp, RegexOptions.IgnoreCase);
					
				MatchCollection matches = regex.Matches(gameEvent.Description);
				foreach (Match match in matches)
				{
					gameEvent.Description = gameEvent.Description.Replace(
						match.Value,
						$"<style=\"{style}\">{match.Value}</style>"
					);
				}
			}

			if (gameEvent.TLDR is {Count: > 0})
				AddGlossaryLinks(gameEvent.TLDR);
		}
	}
	
	*/
}
