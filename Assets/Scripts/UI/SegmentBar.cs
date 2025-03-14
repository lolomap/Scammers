using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	[ExecuteInEditMode]
	public class SegmentBar : MonoBehaviour
	{
		// Max value for MaxValue
		private const int MaxMax = 10;
		
		[Range(0, MaxMax)]
		public int MaxValue;
		public Sprite Segment;
		// Don't use outside Unity Inspector. Use Set method instead.
		[Range(0, MaxMax)]
		public int Value;

		private int _preMax;
		private int _preValue;

		private readonly Image[] _segments = new Image[MaxMax];

		public void Set(int value)
		{
			Value = Math.Clamp(value, 0, MaxValue);

			for (int i = 0; i < MaxValue; i++)
			{
				_segments[i].color = i < Value ? Color.white : Color.clear;
			}
		}

		public void Preview(int value)
		{
			Material blinkMat = ResourceLoader.GetResource<Material>("Materials/SimpleBlink");
			
			if (Value - value >= 0)
			{
				for (int i = Value - 1; i > value - 1 && i > 0; i--)
				{
					_segments[i].material = blinkMat;
				}
			}
			else
			{
				for (int i = Value; i < value && i < MaxValue; i++)
				{
					_segments[i].material = blinkMat;
					_segments[i].color = Color.white;
				}
			}
			
		}

		public void ClearPreview()
		{
			foreach (Image segment in _segments)
			{
				if (segment != null)
					segment.material = null;
			}
			Set(Value);
		}

		private void Awake()
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				_segments[i] = transform.GetChild(i).gameObject.GetComponent<Image>();
			}
		}

#if UNITY_EDITOR
		private void Update()
		{
			if (_preMax != MaxValue)
			{
				CreateSegments();
				_preMax = MaxValue;
			}

			if (_preValue != Value)
			{
				Set(Value);
				_preValue = Value;
			}
		}

		private void CreateSegments()
		{
			// Destroy all childrens
			for (int i = 0; i < MaxMax; i++)
			{
				_segments[i] = null;
			}
			for (int i = transform.childCount - 1; i >= 0; i--)
			{
				DestroyImmediate(transform.GetChild(i).gameObject);
			}
			
			// Create all segments in bar up to MaxValue
			GameObject segment = new()
			{
				name = "Segment"
			};
			Image segImage = segment.AddComponent<Image>();
			segImage.sprite = Segment;
			segImage.preserveAspect = true;

			for (int i = 0; i < MaxValue; i++)
			{
				GameObject instance = Instantiate(segment, transform);
				_segments[i] = instance.GetComponent<Image>();
			}
			
			DestroyImmediate(segment);
			
		}
#endif
	}
}
