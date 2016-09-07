using System;
using UnityEngine;
using UnityEngine.UI;

public class LFGEyeball : MonoBehaviour
{
	public Image m_eyeball;

	public Sprite[] m_eyeballSprites;

	public float m_secondsBetweenFrames;

	private static int frameIndex;

	private static float m_secondsUntilNextFrame;

	private void Start()
	{
		LFGEyeball.m_secondsUntilNextFrame = this.m_secondsBetweenFrames;
	}

	private void Update()
	{
		LFGEyeball.m_secondsUntilNextFrame -= Time.get_deltaTime();
		if (LFGEyeball.m_secondsUntilNextFrame < 0f)
		{
			LFGEyeball.m_secondsUntilNextFrame += this.m_secondsBetweenFrames;
			LFGEyeball.frameIndex++;
			if (LFGEyeball.frameIndex >= this.m_eyeballSprites.Length)
			{
				LFGEyeball.frameIndex = 0;
			}
			this.m_eyeball.set_sprite(this.m_eyeballSprites[LFGEyeball.frameIndex]);
		}
	}
}
