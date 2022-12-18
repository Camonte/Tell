using UnityEngine;
using UnityEngine.Events;
using Lean.Common;
using FSA = UnityEngine.Serialization.FormerlySerializedAsAttribute;

namespace Lean.Touch
{
	/// <summary>This component invokes events when a finger touches the screen that satisfies the specified conditions.</summary>
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Finger Spawn")]
	public class LeanFingerSpawn : MonoBehaviour
	{
		[System.Serializable] public class LeanFingerEvent : UnityEvent<LeanFinger> {}

		[System.Flags]
		public enum ButtonTypes
		{
			LeftMouse   = 1 << 0,
			RightMouse  = 1 << 1,
			MiddleMouse = 1 << 2,
			Touch       = 1 << 5
		}

		/// <summary>Ignore fingers with StartedOverGui?</summary>
		public bool IgnoreStartedOverGui { set { ignoreStartedOverGui = value; } get { return ignoreStartedOverGui; } } [FSA("IgnoreStartedOverGui")] [SerializeField] private bool ignoreStartedOverGui = true;

		/// <summary>Which inputs should this component react to?</summary>
		public ButtonTypes RequiredButtons { set { requiredButtons = value; } get { return requiredButtons; } } [SerializeField] private ButtonTypes requiredButtons = (ButtonTypes)~0;

		/// <summary>If the specified object is set and isn't selected, then this component will do nothing.</summary>
		public LeanSelectable RequiredSelectable { set { requiredSelectable = value; } get { return requiredSelectable; } } [FSA("RequiredSelectable")] [SerializeField] private LeanSelectable requiredSelectable;

		/// <summary>This event will be called if the above conditions are met when your finger begins touching the screen.</summary>
		public LeanFingerEvent OnFinger { get { if (onFinger == null) onFinger = new LeanFingerEvent(); return onFinger; } } [FSA("onDown")] [FSA("OnDown")] [SerializeField] private LeanFingerEvent onFinger;

		/// <summary>The method used to find world coordinates from a finger. See LeanScreenDepth documentation for more information.</summary>
		public LeanScreenDepth ScreenDepth = new LeanScreenDepth(LeanScreenDepth.ConversionType.DepthIntercept);

#if UNITY_EDITOR
		protected virtual void Reset()
		{
			requiredSelectable = GetComponentInParent<LeanSelectable>();
		}
#endif

		protected virtual void Awake()
		{
			if (requiredSelectable == null)
			{
				requiredSelectable = GetComponentInParent<LeanSelectable>();
			}
		}

		protected virtual void OnEnable()
		{
			LeanTouch.OnFingerDown += HandleFingerDown;
		}

		protected virtual void OnDisable()
		{
			LeanTouch.OnFingerDown -= HandleFingerDown;
		}

		protected virtual void HandleFingerDown(LeanFinger finger)
		{
			if (ignoreStartedOverGui == true && finger.IsOverGui == true)
			{
				return;
			}

			if (RequiredButtonPressed(finger) == false)
			{
				return;
			}

			if (requiredSelectable != null && requiredSelectable.IsSelected == false)
			{
				return;
			}

			if (onFinger != null)
			{
				onFinger.Invoke(finger);
			}

		}

		private bool RequiredButtonPressed(LeanFinger finger)
		{
			if (finger.Index < 0)
			{
				if (LeanInput.GetMouseExists() == true)
				{
					if ((requiredButtons & ButtonTypes.LeftMouse) != 0 && LeanInput.GetMousePressed(0) == true)
					{
						return true;
					}

					if ((requiredButtons & ButtonTypes.RightMouse) != 0 && LeanInput.GetMousePressed(1) == true)
					{
						return true;
					}

					if ((requiredButtons & ButtonTypes.MiddleMouse) != 0 && LeanInput.GetMousePressed(2) == true)
					{
						return true;
					}
				}
			}
			else if ((requiredButtons & ButtonTypes.Touch) != 0)
			{
				return true;
			}

			return false;
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Touch.Editor
{
	using TARGET = LeanFingerSpawn;

	[UnityEditor.CanEditMultipleObjects]
	[UnityEditor.CustomEditor(typeof(TARGET), true)]
	public class LeanFingerSpawn_Editor : LeanEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("ignoreStartedOverGui", "Ignore fingers with StartedOverGui?");
			Draw("requiredButtons", "Which inputs should this component react to?");
			Draw("requiredSelectable", "If the specified object is set and isn't selected, then this component will do nothing.");

			Separator();

			var showUnusedEvents = DrawFoldout("Show Unused Events", "Show all events?");

			Separator();

			if (Any(tgts, t => t.OnFinger.GetPersistentEventCount() > 0) == true || showUnusedEvents == true)
			{
				Draw("onFinger");
			}
		}
	}
}
#endif