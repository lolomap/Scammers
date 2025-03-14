using System;
using TMPro;
using UnityEngine;

namespace UI
{
	public class TaggedValue : MonoBehaviour
	{
		private delegate void UpdateEventHandler(string uiTag, object value);
		private delegate void AnimateEventHandler(string uiTag, UIGenericAnimation.Animation anim);
		private delegate void PreviewEventHandler(string uiTag, object value);
		private delegate void ClearPreviewEventHandler();

		private static event UpdateEventHandler UpdateUI;
		private static event AnimateEventHandler Animate;
		private static event PreviewEventHandler PreviewUI;
		private static event ClearPreviewEventHandler ClearPreviewUI;
		
		public string Tag;

		private UIGenericAnimation _animation;
		
		//private Slider _slider;
		private AnimatedBar _slider;
		private SegmentBar _segmentBar;
		private TMP_Text _text;
		
		private void Awake()
		{
			UpdateUI += OnUpdate;
			PreviewUI += OnPreview;
			ClearPreviewUI += OnClearPreview;
			Animate += OnAnimate;

			_animation = GetComponent<UIGenericAnimation>();
			
			_slider = GetComponent<AnimatedBar>();
			_segmentBar = GetComponent<SegmentBar>();
			_text = GetComponent<TMP_Text>();
		}

		private void OnUpdate(string uiTag, object value)
		{
			if (uiTag != Tag)
				return;
			
			switch (value)
			{
				case int:
					if (_segmentBar != null) _segmentBar.Set(Convert.ToInt32(value));
					if (_text != null) _text.text = value.ToString();
					break;
				
				case decimal:
				case float:
					if (_slider != null)
						_slider.Set(Convert.ToSingle(value),
							uiTag.StartsWith("HIGHLIGHT_") ? GameManager.Instance.UI.AnimationDelaySec : 0);
					if (_segmentBar != null) _segmentBar.Set(Convert.ToInt32(value));
					if (_text != null) _text.text = value.ToString();
					break;
			
				case string:
					if (_text != null) _text.text = value.ToString();
					break;
			}
		}

		private void OnPreview(string uiTag, object value)
		{
			if (uiTag != Tag)
				return;

			if (_segmentBar != null) _segmentBar.Preview(Convert.ToInt32(value));
		}

		private void OnClearPreview()
		{
			if (_segmentBar != null) _segmentBar.ClearPreview();
		}

		private void OnAnimate(string uiTag, UIGenericAnimation.Animation anim)
		{
			if (uiTag != Tag || _animation == null)
				return;

			_animation.Play(anim);
		}

		public static void AnimateAll(string uiTag, UIGenericAnimation.Animation anim)
		{
			Animate?.Invoke(uiTag, anim);
		}

		public static void UpdateAll(string uiTag, object value)
		{
			UpdateUI?.Invoke(uiTag, value);
		}

		public static void PreviewAll(string uiTag, object value)
		{
			PreviewUI?.Invoke(uiTag, value);
		}

		public static void ClearPreviewAll()
		{
			ClearPreviewUI?.Invoke();
		}
	}
}
