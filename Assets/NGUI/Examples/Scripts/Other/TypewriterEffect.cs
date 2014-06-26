using UnityEngine;

/// <summary>
/// Trivial script that fills the label's contents gradually, as if someone was typing.
/// </summary>

[RequireComponent(typeof(UILabel))]
[AddComponentMenu("NGUI/Examples/Typewriter Effect")]
public class TypewriterEffect : MonoBehaviour
{
	public int charsPerSecond = 40;

	UILabel mLabel;
	string mText;
	int mOffset = 0;
	float mNextChar = 0f;

	void Update ()
	{
		if (mLabel == null)
		{
			mLabel = GetComponent<UILabel>();
			mLabel.supportEncoding = false;
			mLabel.symbolStyle = NGUIText.SymbolStyle.None;
			mText = mLabel.processedText;
		}

		if (mOffset < mText.Length)
		{
			if (mNextChar <= RealTime.time)
			{
				charsPerSecond = Mathf.Max(1, charsPerSecond);

				// Periods and end-of-line characters should pause for a longer time.
				float delay = 1f / charsPerSecond;
				char c = mText[mOffset];
				if (c == '.' || c == '\n' || c == '!' || c == '?') delay *= 4f;

				mNextChar = RealTime.time + delay;
				mLabel.text = mText.Substring(0, ++mOffset);
			}
		}
		else Destroy(this);
	}
}
