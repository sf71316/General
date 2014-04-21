using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;


namespace General.Security
{
    public abstract class CaptchaBase
    {
        protected string RandText { get; set; }
        protected abstract void Generate(Stream stream);
        /// <summary>
        /// 產生亂數文字
        /// </summary>
        /// <param name="MaxLength"></param>
        /// <returns></returns>
        protected static string GenerateText(int maxLength)
        {
            string[] Code ={ "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
                            "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
                          "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            StringBuilder strRd = new StringBuilder();
            Random rd = new Random(unchecked((int)DateTime.Now.Ticks));
            for (int i = 0; i < 5; i++)       // 亂數產生驗證文字
            {
                strRd.Append(Code[rd.Next(35)]);
            }
            return strRd.ToString();
        }
        protected void GenerateImage(string randText, Stream stream)
        {
            Bitmap Bmp = CreateImage(RandText);
            Bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Gif);
        }
        private static Bitmap CreateImage(string RandText)
        {
            Random rd = new Random(unchecked((int)DateTime.Now.Ticks));
            Bitmap Bmp = new Bitmap(80, 25);  //建立實體圖檔並設定大小
            Graphics Gpi = Graphics.FromImage(Bmp);
            Font Font1 = new Font("Verdana", 14, FontStyle.Italic);
            Pen PenLine = new Pen(Brushes.Red, 1);  //實體化筆刷並設定顏色、大小(畫X,Y軸用)
            Gpi.Clear(Color.White);    //設定背景顏色
            Gpi.DrawLine(PenLine, 0, rd.Next(80), 90, rd.Next(25));
            Gpi.DrawString(RandText, Font1, Brushes.Black, 0, 0);
            for (int i = 0; i <= 25; i++)            //亂數產生霧點，擾亂機器人辨別
            {
                int RandPixelX = rd.Next(0, 80);

                int RandPixelY = rd.Next(0, 25);

                Bmp.SetPixel(RandPixelX, RandPixelY, Color.Blue);

            }
            return Bmp;
        }

    }
}
