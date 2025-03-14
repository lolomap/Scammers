using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	[RequireComponent(typeof(Slider))]
	[RequireComponent(typeof(RectTransform))]
	public class AnimatedBar : MonoBehaviour
	{
		private Slider _slider;
		private RectTransform _rectTransform;
		
		private float _lastValue;

		public float Duration;
		
		private void Awake()
		{
			_slider = GetComponent<Slider>();
			_rectTransform = GetComponent<RectTransform>();
			
			_lastValue = _slider.value;
		}

		public void Set(float value, float animationDelay = 0)
		{
			_slider.value = _lastValue;
			float segDuration = Duration / 3f;

			DOTween.Sequence()
				.AppendInterval(animationDelay)
				.Append(_rectTransform.DOScale(1.2f, segDuration))
				.Append(DOTween.To(() => _slider.value, (x) => _slider.value = x, value, segDuration))
				.Append(_rectTransform.DOScale(1, segDuration));

			_lastValue = value;
		}
	}
}
