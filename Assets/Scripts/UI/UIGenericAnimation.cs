using System;
using DG.Tweening;
using UnityEngine;

namespace UI
{
	public class UIGenericAnimation : MonoBehaviour
	{
		private RectTransform _rectTransform;
		// You should not play some animations while last is not done bc of relative values
		private bool _isBusy;

		public enum Animation
		{
			ButtonScale,
			ButtonShake
		}

		private void Awake()
		{
			_rectTransform = GetComponent<RectTransform>();
		}

		public void Play(Animation anim)
		{
			switch (anim)
			{
				case Animation.ButtonScale:
					ButtonScale();
					break;
				case Animation.ButtonShake:
					ButtonShake();
					break;
			}
		}
		
		public void ButtonScale()
		{
			DOTween.Sequence()
				.Append(_rectTransform.DOScale(GameManager.Instance.UI.AnimationScale,
					GameManager.Instance.UI.AnimationDurationSec))
				.Append(_rectTransform.DOScale(1f, GameManager.Instance.UI.AnimationDurationSec));
		}

		public void ButtonShake()
		{
			if (_isBusy) return;

			_isBusy = true;
			DOTween.Sequence()
				.Append(_rectTransform.DOShakePosition(GameManager.Instance.UI.AnimationDurationSec,
                        					GameManager.Instance.UI.AnimationShakeStrength))
				.AppendCallback(() => { _isBusy = false; });
		}
	}
}
