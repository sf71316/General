using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace General
{
    public static class ColorHelper
    {
        /// <summary>
        /// Color To ARGB
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ToArgb(Color color)
        {
            return string.Format("argb({0},{1},{2},{3})",
                         color.A,color.R,color.G,color.B);
        }
        /// <summary>
        /// Color To 十六進制
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ToHex(Color color)
        {

            return ColorTranslator.ToHtml(color);
            
        }
    }
}
