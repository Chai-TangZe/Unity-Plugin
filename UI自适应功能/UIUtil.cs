using UnityEngine;
using UnityEngine.UI;

public class UIUtil
{
    /// <summary>
    /// 自适应Image,不考虑高度,如果图片比父物体要宽,图片将会自适应
    /// </summary>
    /// <param name="mImage">图片</param>
    /// <param name="Size">缩放倍数</param>
    public static void AdaptiveUIImage( Image mImage , float Size =0.7f)
    {
        mImage.SetNativeSize ();
        if (mImage.rectTransform.sizeDelta.x > mImage.rectTransform.parent.GetComponent<RectTransform> ().sizeDelta.x)
        {
            float f = mImage.rectTransform.parent.GetComponent<RectTransform> ().sizeDelta.x / mImage.rectTransform.sizeDelta.x;
            //Debug.Log ("图片宽:" + f + "%");
            mImage.rectTransform.sizeDelta = new Vector2 (mImage.rectTransform.sizeDelta.x * f, mImage.rectTransform.sizeDelta.y * f) * Size;
        }
    }
    /// <summary>
    /// 自适应Text,如果txt数量过多导致显示不出来下一行,可自适应高度
    /// </summary>
    /// <param name="mText">text</param>
    public static void AdaptiveUIText(Text mText )
    {
        mText.rectTransform.sizeDelta = new Vector2 (mText.rectTransform.sizeDelta.x, mText.preferredHeight);
    }
}
