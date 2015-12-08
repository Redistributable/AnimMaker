using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace a32system.CSProgram.AnimMaker
{
    /// <summary>
    /// メインクラス
    /// </summary>
    public static class MainClass
    {
        /// <summary>
        /// メインエントリポイント
        /// </summary>
        /// <param name="args">コマンドライン引数</param>
        [STAThread]
        public static void Main(string[] args)
        {
            Maker m = new Maker();
            string animPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\TestImages\test_bmp";

            if (!Directory.Exists(animPath))
            {
                Console.WriteLine("画像ディレクトリが見つかりません。");
                Console.ReadKey();
                Environment.Exit(0);
            }

            for (int i = 1; i <= 500; i++)
            {
                string p = animPath + "\\" + String.Format("test_{0:000000}.bmp", i);
                if (!File.Exists(p))
                    break;
                Console.WriteLine("追加します: " + Path.GetFileName(p));
                m.ImageList.Add(Image.FromFile(p));
            }

            Console.Write("続行するには何かキーを押してください。");
            Console.ReadKey();

            FileStream fs = new FileStream("test.raw", FileMode.Create, FileAccess.Write, FileShare.None);
            m.SaveToStream(fs);
            fs.Close();
        }
    }
}
