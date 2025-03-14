using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;

public static class ResourceLoader
{
	private static readonly Dictionary<string, Object> _resources = new();

	private static Dictionary<string, string> _glossary = new();
	private static Dictionary<string, string> _glossaryLinks = new();

	public static T GetResource<T>(string path, bool force = false) where T : Object
	{
		if (!force && _resources.TryGetValue(path, out Object result)) return (T)result;

		result = Resources.Load<T>(path);
		_resources[path] = result;
		return (T)result;
	}

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
}
