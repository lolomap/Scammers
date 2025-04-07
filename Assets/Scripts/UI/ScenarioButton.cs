using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
	public class ScenarioButton : MonoBehaviour
	{
		public string ScenarioName;

		private Button _button;
		private TMP_Text _title;

		private void Awake()
		{
			_button = GetComponentInChildren<Button>();
			_title = GetComponentInChildren<TMP_Text>();

			if (string.IsNullOrEmpty(ScenarioName))
			{
				_button.interactable = false;
				_title.text = "ЗАБЛОКИРОВАНО";
			}
			else _title.text = ScenarioName;
		}

		public void OnClick()
		{
			GameManager.EventStorage.LoadScenario(ScenarioName);
			
			SceneManager.LoadScene("Scenario");
		}
	}
}
