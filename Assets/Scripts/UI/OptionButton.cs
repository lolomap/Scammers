using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class OptionButton : MonoBehaviour
	{
		private TMP_Text _title;
		
		public delegate void OnSelectEventHandler(Option option);
		public static event OnSelectEventHandler Select;

		private Option _data;
		public Option Data
		{
			get => _data;
			set
			{
				_data = value;
				_title.text = value.Title;
			}
		}

		private void Awake()
		{
			_title = GetComponentInChildren<TMP_Text>();
		}

		public void OnClick()
		{
			Select?.Invoke(Data);
		}
	}
}