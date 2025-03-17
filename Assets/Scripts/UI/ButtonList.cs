using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class ButtonList : MonoBehaviour
	{
		private List<GameObject> _elements;
		
		private List<Option> _options;
		public List<Option> Options
		{
			get => _options;
			set
			{
				_options = value;

				foreach (GameObject element in _elements)
				{
					Destroy(element);
				}
				
				_elements.Clear();
				
				OptionButton prefab = ResourceLoader.GetResource<OptionButton>("Prefabs/OptionButton");
				foreach (Option option in value)
				{
					OptionButton obj = Instantiate(prefab, transform, false);
					obj.Data = option;

					_elements.Add(obj.gameObject);
				}
			}
		}

		private void Awake()
		{
			_elements = new();
		}
	}
}