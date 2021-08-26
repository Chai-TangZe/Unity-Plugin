using UnityEngine;
using UnityEngine.UI;
namespace TFramwork
{
    public enum AnchorPresets
    {
        TopLeft,
        TopCenter,
        TopRight,

        MiddleLeft,
        MiddleCenter,
        MiddleRight,

        BottomLeft,
        BottonCenter,
        BottomRight,
        BottomStretch,

        VertStretchLeft,
        VertStretchRight,
        VertStretchCenter,

        HorStretchTop,
        HorStretchMiddle,
        HorStretchBottom,

        StretchAll
    }
    public enum PivotPresets
    {
        TopLeft,
        TopCenter,
        TopRight,

        MiddleLeft,
        MiddleCenter,
        MiddleRight,

        BottomLeft,
        BottomCenter,
        BottomRight,
    }
    public enum UILocation
    {
        左,
        中,
        右
    }
    public static class UIUtil
    {
        /// <summary>
        /// 自适应Image,不考虑高度,如果图片比父物体要宽,图片将会自适应
        /// </summary>
        /// <param name="mImage">图片</param>
        /// <param name="Size">缩放倍数默认0.9%</param>
        public static void AdaptiveUIImage( this Image mImage, float Size = 0.9f )
        {
            if (mImage.type != Image.Type.Simple)
                return;
            mImage.SetNativeSize ();
            if (mImage.rectTransform.sizeDelta.x > mImage.rectTransform.parent.GetComponent<RectTransform> ().sizeDelta.x)
            {
                float f = mImage.rectTransform.parent.GetComponent<RectTransform> ().sizeDelta.x / mImage.rectTransform.sizeDelta.x;
                //Debug.Log ("图片宽:" + f + "%");
                mImage.rectTransform.sizeDelta = new Vector2 (mImage.rectTransform.sizeDelta.x * f, mImage.rectTransform.sizeDelta.y * f) * Size;
            }
        }
        /// <summary>
        /// 自适应Text,不考虑宽度(宽度可自定义),如果txt数量过多导致显示不出来下一行,可自适应高度
        /// </summary>
        /// <param name="mText">text</param>
        public static void AdaptiveUIText( this Text mText )
        {
            mText.rectTransform.sizeDelta = new Vector2 (mText.rectTransform.sizeDelta.x, mText.preferredHeight);
        }

        /// <summary>
        /// UI自适应(子物体锚点不允许为Stretch模式)
        /// </summary>
        /// <param name="mUIParent">父物体</param>
        /// <param name="mUILocation">自适应位置</param>
        /// <param name="mGap">间隔距离</param>
        /// <param name="mAlterUIParent">父物体是否跟随子物体调整大小</param>
        public static void AdaptiveUI( this RectTransform mUIParent, UILocation mUILocation = UILocation.中, float mGap = 10f, bool mAlterUIParent = true )
        {

            #region 锚点固定左上角
            foreach (RectTransform item in mUIParent)
            {
                item.SetAnchor (AnchorPresets.TopLeft);
            }
            #endregion

            RectTransform UIItem = null;
            foreach (Transform item in mUIParent.transform)
            {
                #region 自适应Text与Image
                Image image = item.GetComponent<Image> ();
                Text text = item.GetComponent<Text> ();
                if (image)
                    AdaptiveUIImage (image);
                if (text)
                    AdaptiveUIText (text);
                #endregion

                if (UIItem == null)
                {
                    UIItem = item.GetComponent<RectTransform> ();
                    UIItem.anchoredPosition = new Vector2 (UIItem.anchoredPosition.x, ( UIItem.sizeDelta.y / -2 ));
                }
                else
                {
                    float itemx = item.GetComponent<RectTransform> ().anchoredPosition.x;
                    float itemy = UIItem.anchoredPosition.y - ( item.GetComponent<RectTransform> ().sizeDelta.y / 2 ) - ( UIItem.sizeDelta.y / 2 );
                    item.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (itemx, itemy);
                    UIItem = item.GetComponent<RectTransform> ();
                }
                SetUILocation (mUIParent, item.GetComponent<RectTransform> (), mUILocation, mGap);
            }
            //设置父物体的长度
            if (mAlterUIParent)
            {
                float mUIParent_Y = ( UIItem.anchoredPosition.y * -1 ) + ( UIItem.sizeDelta.y / 2 );
                mUIParent.sizeDelta = new Vector2 (mUIParent.sizeDelta.x, mUIParent_Y + mGap);
            }
        }

        static void SetUILocation( RectTransform mUIParent, RectTransform mTaggetUI, UILocation mUILocation, float mGap )
        {
            float mTaggetUI_X = 0;
            float mTaggetUI_Y = mTaggetUI.anchoredPosition.y - mGap;
            switch (mUILocation)
            {
                case UILocation.左:
                    mTaggetUI_X = mTaggetUI.sizeDelta.x / 2 + mGap;
                    break;
                case UILocation.中:
                    mTaggetUI_X = mUIParent.sizeDelta.x / 2;
                    break;
                case UILocation.右:
                    mTaggetUI_X = mUIParent.sizeDelta.x - ( mTaggetUI.sizeDelta.x / 2 ) - mGap;
                    break;
            }
            mTaggetUI.anchoredPosition = new Vector2 (mTaggetUI_X, mTaggetUI_Y);
        }

        public static void SetAnchor( this RectTransform source, AnchorPresets allign, int offsetX = 0, int offsetY = 0 )
        {
            source.anchoredPosition = new Vector3 (offsetX, offsetY, 0);

            switch (allign)
            {
                case ( AnchorPresets.TopLeft ):
                    {
                        source.anchorMin = new Vector2 (0, 1);
                        source.anchorMax = new Vector2 (0, 1);
                        break;
                    }
                case ( AnchorPresets.TopCenter ):
                    {
                        source.anchorMin = new Vector2 (0.5f, 1);
                        source.anchorMax = new Vector2 (0.5f, 1);
                        break;
                    }
                case ( AnchorPresets.TopRight ):
                    {
                        source.anchorMin = new Vector2 (1, 1);
                        source.anchorMax = new Vector2 (1, 1);
                        break;
                    }

                case ( AnchorPresets.MiddleLeft ):
                    {
                        source.anchorMin = new Vector2 (0, 0.5f);
                        source.anchorMax = new Vector2 (0, 0.5f);
                        break;
                    }
                case ( AnchorPresets.MiddleCenter ):
                    {
                        source.anchorMin = new Vector2 (0.5f, 0.5f);
                        source.anchorMax = new Vector2 (0.5f, 0.5f);
                        break;
                    }
                case ( AnchorPresets.MiddleRight ):
                    {
                        source.anchorMin = new Vector2 (1, 0.5f);
                        source.anchorMax = new Vector2 (1, 0.5f);
                        break;
                    }

                case ( AnchorPresets.BottomLeft ):
                    {
                        source.anchorMin = new Vector2 (0, 0);
                        source.anchorMax = new Vector2 (0, 0);
                        break;
                    }
                case ( AnchorPresets.BottonCenter ):
                    {
                        source.anchorMin = new Vector2 (0.5f, 0);
                        source.anchorMax = new Vector2 (0.5f, 0);
                        break;
                    }
                case ( AnchorPresets.BottomRight ):
                    {
                        source.anchorMin = new Vector2 (1, 0);
                        source.anchorMax = new Vector2 (1, 0);
                        break;
                    }

                case ( AnchorPresets.HorStretchTop ):
                    {
                        source.anchorMin = new Vector2 (0, 1);
                        source.anchorMax = new Vector2 (1, 1);
                        break;
                    }
                case ( AnchorPresets.HorStretchMiddle ):
                    {
                        source.anchorMin = new Vector2 (0, 0.5f);
                        source.anchorMax = new Vector2 (1, 0.5f);
                        break;
                    }
                case ( AnchorPresets.HorStretchBottom ):
                    {
                        source.anchorMin = new Vector2 (0, 0);
                        source.anchorMax = new Vector2 (1, 0);
                        break;
                    }

                case ( AnchorPresets.VertStretchLeft ):
                    {
                        source.anchorMin = new Vector2 (0, 0);
                        source.anchorMax = new Vector2 (0, 1);
                        break;
                    }
                case ( AnchorPresets.VertStretchCenter ):
                    {
                        source.anchorMin = new Vector2 (0.5f, 0);
                        source.anchorMax = new Vector2 (0.5f, 1);
                        break;
                    }
                case ( AnchorPresets.VertStretchRight ):
                    {
                        source.anchorMin = new Vector2 (1, 0);
                        source.anchorMax = new Vector2 (1, 1);
                        break;
                    }

                case ( AnchorPresets.StretchAll ):
                    {
                        source.anchorMin = new Vector2 (0, 0);
                        source.anchorMax = new Vector2 (1, 1);
                        break;
                    }
            }
        }

        public static void SetPivot( this RectTransform source, PivotPresets preset )
        {

            switch (preset)
            {
                case ( PivotPresets.TopLeft ):
                    {
                        source.pivot = new Vector2 (0, 1);
                        break;
                    }
                case ( PivotPresets.TopCenter ):
                    {
                        source.pivot = new Vector2 (0.5f, 1);
                        break;
                    }
                case ( PivotPresets.TopRight ):
                    {
                        source.pivot = new Vector2 (1, 1);
                        break;
                    }

                case ( PivotPresets.MiddleLeft ):
                    {
                        source.pivot = new Vector2 (0, 0.5f);
                        break;
                    }
                case ( PivotPresets.MiddleCenter ):
                    {
                        source.pivot = new Vector2 (0.5f, 0.5f);
                        break;
                    }
                case ( PivotPresets.MiddleRight ):
                    {
                        source.pivot = new Vector2 (1, 0.5f);
                        break;
                    }

                case ( PivotPresets.BottomLeft ):
                    {
                        source.pivot = new Vector2 (0, 0);
                        break;
                    }
                case ( PivotPresets.BottomCenter ):
                    {
                        source.pivot = new Vector2 (0.5f, 0);
                        break;
                    }
                case ( PivotPresets.BottomRight ):
                    {
                        source.pivot = new Vector2 (1, 0);
                        break;
                    }
            }
        }
    }
}