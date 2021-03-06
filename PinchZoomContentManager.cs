using System;
using UnityEngine;
using UnityEngine.UI;

public class PinchZoomContentManager : MonoBehaviour
{
	public float m_zoomSpeed;

	public ScrollRect m_scrollRect;

	public RectTransform m_mapViewRT;

	public RectTransform m_mapViewContentsRT;

	public float m_zoomFactor = 1f;

	public bool m_isPinching;

	public Vector2 m_pinchPivot;

	public float m_zoomInThreshold;

	public float m_zoomOutThreshold;

	public Action ZoomFactorChanged;

	private void Update()
	{
		iTween component = this.m_mapViewContentsRT.GetComponent<iTween>();
		bool flag = component != null;
		if (Input.get_touchCount() == 2)
		{
			MapInfo componentInChildren = base.GetComponentInChildren<MapInfo>();
			if (componentInChildren == null)
			{
				return;
			}
			Touch touch = Input.GetTouch(0);
			Touch touch2 = Input.GetTouch(1);
			if (!this.m_isPinching)
			{
				this.m_isPinching = true;
				this.m_scrollRect.set_enabled(false);
			}
			Vector2 vector = touch.get_position() - touch.get_deltaPosition();
			Vector2 vector2 = touch2.get_position() - touch2.get_deltaPosition();
			float magnitude = (vector - vector2).get_magnitude();
			float magnitude2 = (touch.get_position() - touch2.get_position()).get_magnitude();
			float num = magnitude - magnitude2;
			float num2 = num * -this.m_zoomSpeed;
			Vector2 vector3 = (touch.get_position() + touch2.get_position()) / 2f;
			Ray ray = Camera.get_main().ScreenPointToRay(vector3);
			if (AdventureMapPanel.instance.m_testEnableAutoZoomInOut)
			{
				if (!flag)
				{
					if (num2 > this.m_zoomInThreshold)
					{
						AdventureMapPanel.instance.CenterAndZoom(ray.get_origin(), null, true);
						return;
					}
					if (num2 < -this.m_zoomOutThreshold)
					{
						AdventureMapPanel.instance.CenterAndZoom(ray.get_origin(), null, false);
						return;
					}
				}
				return;
			}
			float num3 = this.m_zoomFactor;
			num3 += num2;
			num3 = Mathf.Clamp(num3, componentInChildren.m_minZoomFactor, componentInChildren.m_maxZoomFactor);
			this.SetZoom(num3, ray.get_origin(), false);
		}
		else
		{
			this.m_isPinching = false;
			this.m_scrollRect.set_enabled(true);
		}
	}

	public void SetZoom(float newZoomFactor, Vector2 zoomCenter, bool bypassLegalPositionEnforcement = false)
	{
		Vector2 vector;
		vector.x = zoomCenter.x - base.get_transform().get_position().x;
		vector.y = zoomCenter.y - base.get_transform().get_position().y;
		Vector2 vector2 = vector * (newZoomFactor / this.m_zoomFactor);
		Vector2 vector3 = vector - vector2;
		base.get_transform().Translate(vector3.x, vector3.y, 0f);
		this.SetZoom(newZoomFactor, bypassLegalPositionEnforcement);
	}

	public void SetZoom(float newZoomFactor, bool bypassLegalPositionEnforcement = false)
	{
		MapInfo componentInChildren = base.GetComponentInChildren<MapInfo>();
		componentInChildren.m_scaleRoot.get_transform().set_localScale(Vector3.get_one() * newZoomFactor * componentInChildren.GetViewRelativeScale());
		RectTransform component = componentInChildren.get_gameObject().GetComponent<RectTransform>();
		this.m_zoomFactor = newZoomFactor;
		float num = this.m_zoomFactor * componentInChildren.GetFillViewSize().x;
		float num2 = this.m_zoomFactor * componentInChildren.GetFillViewSize().y;
		component.set_sizeDelta(new Vector2(num, num2));
		this.m_mapViewContentsRT.set_sizeDelta(component.get_sizeDelta());
		if (!bypassLegalPositionEnforcement)
		{
			Vector3[] array = new Vector3[4];
			this.m_mapViewRT.GetWorldCorners(array);
			float num3 = array[2].x - array[0].x;
			float num4 = array[2].y - array[0].y;
			Vector2 vector;
			vector.x = array[0].x + num3 * 0.5f;
			vector.y = array[0].y + num4 * 0.5f;
			Vector3[] array2 = new Vector3[4];
			this.m_mapViewContentsRT.GetWorldCorners(array2);
			float num5 = array2[2].x - array2[0].x;
			float num6 = array2[2].y - array2[0].y;
			Vector2 vector2;
			vector2.x = array2[0].x + num5 * 0.5f;
			vector2.y = array2[0].y + num6 * 0.5f;
			float num7 = Mathf.Abs(num5 / 2f - num3 / 2f);
			float num8 = Mathf.Abs(num6 / 2f - num4 / 2f);
			Vector3 position = this.m_mapViewContentsRT.get_position();
			position.x = Mathf.Clamp(position.x, vector.x - num7, vector.x + num7);
			position.y = Mathf.Clamp(position.y, vector.y - num8, vector.y + num8);
			this.m_mapViewContentsRT.set_position(position);
		}
		if (this.ZoomFactorChanged != null)
		{
			this.ZoomFactorChanged.Invoke();
		}
	}
}
