using System;
using TMPro;
using UnityEngine;

namespace UI
{
	public class Title : MonoBehaviour
	{
		public TMP_Text TextObject;
		public string Value;

		private void Start()
		{
			TextObject.text = Value;
		}
	}
}