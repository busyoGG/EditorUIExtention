using UnityEngine;
using UnityEngine.UIElements;

namespace EditorUIExtension
{
    public class VEStyleUtils
    {
        /// <summary>
        /// 设置 Padding
        /// </summary>
        /// <param name="style"></param>
        /// <param name="num"></param>
        public static void SetPadding(IStyle style,int num)
        {
            style.paddingBottom = num;
            style.paddingTop = num;
            style.paddingLeft = num;
            style.paddingRight = num;
        }
        
        /// <summary>
        /// 设置 Padding
        /// </summary>
        /// <param name="style"></param>
        /// <param name="num"></param>
        public static void SetPadding(IStyle style,int top,int right,int bottom,int left)
        {
            style.paddingBottom = bottom;
            style.paddingTop = top;
            style.paddingLeft = left;
            style.paddingRight = right;
        }
        
        /// <summary>
        /// 设置 Padding
        /// </summary>
        /// <param name="style"></param>
        /// <param name="num"></param>
        public static void SetPadding(IStyle style,int tb,int lr)
        {
            style.paddingBottom = tb;
            style.paddingTop = tb;
            style.paddingLeft = lr;
            style.paddingRight = lr;
        }

        /// <summary>
        /// 设置 Margin
        /// </summary>
        /// <param name="style"></param>
        /// <param name="num"></param>
        public static void SetMargin(IStyle style,int num)
        {
            style.marginBottom = num;
            style.marginTop = num;
            style.marginLeft = num;
            style.marginRight = num;
        }
        
        /// <summary>
        /// 设置 Margin
        /// </summary>
        /// <param name="style"></param>
        /// <param name="num"></param>
        public static void SetMargin(IStyle style,int top,int right,int bottom,int left)
        {
            style.marginBottom = bottom;
            style.marginTop = top;
            style.marginLeft = left;
            style.marginRight = right;
        }
        
        /// <summary>
        /// 设置 Margin
        /// </summary>
        /// <param name="style"></param>
        /// <param name="num"></param>
        public static void SetMargin(IStyle style,int tb,int lr)
        {
            style.marginBottom = tb;
            style.marginTop = tb;
            style.marginLeft = lr;
            style.marginRight = lr;
        }

        /// <summary>
        /// 设置描边粗细
        /// </summary>
        /// <param name="style"></param>
        /// <param name="border"></param>
        public static void SetBorder(IStyle style,int border)
        {
            style.borderBottomWidth = border;
            style.borderLeftWidth = border;
            style.borderRightWidth = border;
            style.borderTopWidth = border;
        }
        
        /// <summary>
        /// 设置描边粗细
        /// </summary>
        /// <param name="style"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <param name="left"></param>
        public static void SetBorder(IStyle style,int top,int right,int bottom,int left)
        {
            style.borderBottomWidth = bottom;
            style.borderLeftWidth = left;
            style.borderRightWidth = right;
            style.borderTopWidth = top;
        }
        
        /// <summary>
        /// 设置描边粗细
        /// </summary>
        /// <param name="style"></param>
        /// <param name="tb"></param>
        /// <param name="lr"></param>
        public static void SetBorder(IStyle style,int tb,int lr)
        {
            style.borderBottomWidth = tb;
            style.borderLeftWidth = lr;
            style.borderRightWidth = lr;
            style.borderTopWidth = tb;
        }

        /// <summary>
        /// 设置描边颜色
        /// </summary>
        /// <param name="style"></param>
        /// <param name="color"></param>
        public static void SetBorderColor(IStyle style,Color color)
        {
            style.borderBottomColor = color;
            style.borderLeftColor = color;
            style.borderRightColor = color;
            style.borderTopColor = color;
        }

        
        /// <summary>
        /// 设置圆角
        /// </summary>
        /// <param name="style"></param>
        /// <param name="radius"></param>
        public static void SetRadius(IStyle style,int radius)
        {
            style.borderBottomLeftRadius = radius;
            style.borderBottomRightRadius = radius;
            style.borderTopLeftRadius = radius;
            style.borderTopRightRadius = radius;
        }

        /// <summary>
        /// 设置圆角
        /// </summary>
        /// <param name="style"></param>
        /// <param name="tr"></param>
        /// <param name="br"></param>
        /// <param name="tl"></param>
        /// <param name="bl"></param>
        public static void SetRadius(IStyle style,int tr,int br,int tl,int bl)
        {
            style.borderBottomLeftRadius = bl;
            style.borderBottomRightRadius = br;
            style.borderTopLeftRadius = tl;
            style.borderTopRightRadius = tr;
        }

        /// <summary>
        /// 设置字体颜色
        /// </summary>
        /// <param name="style"></param>
        /// <param name="fontColor"></param>
        public static void SetFontColor(IStyle style, Color fontColor)
        {
            style.color = fontColor;
        }
    }
}