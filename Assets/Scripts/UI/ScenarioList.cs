using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI
{
	[Serializable]
	public class Scenario
	{
		public string Category;
		public List<string> Scenarios;
	}
	
	public class ScenarioList : MonoBehaviour
	{
		private Dictionary<string, List<string>> _scenariosPerCategory;
		
		private void Awake()
		{
			foreach (Transform child in transform)
			{
				Destroy(child.gameObject);
			}

			_scenariosPerCategory =
				ResourceLoader.GetPersistentJson<List<Scenario>>("Scenarios")
					.ToDictionary(x => x.Category, x => x.Scenarios);
			
			Title categoryPrefab = ResourceLoader.GetResource<Title>("Prefabs/Category");
			ScenarioButton scenarioPrefab = ResourceLoader.GetResource<ScenarioButton>("Prefabs/Scenario");
			
			foreach (string category in _scenariosPerCategory.Keys)
			{
				Title categoryTitle = Instantiate(categoryPrefab, transform, false);

				categoryTitle.Value = category;

				foreach (string scenario in _scenariosPerCategory[category])
				{
					ScenarioButton button = Instantiate(scenarioPrefab, categoryTitle.transform, false);
					button.ScenarioName = scenario;
				}
			}
		}
	}
}
