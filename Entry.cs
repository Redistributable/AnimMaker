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
            // プログラムの開始地点
            // Javaで言うところのmain関数

            // アニメーション作成器(Maker.csで定義)
            Maker m = new Maker();

            // アニメーションに使用する連番の画像ファイルが存在するディレクトリ
            string animPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\TestImages\test_bmp";


            if (!Directory.Exists(animPath))
            {
                // 画像ファイルのディレクトリが見つからなかった
                Console.WriteLine("画像ディレクトリが見つかりません。");
                Console.ReadKey();
                Environment.Exit(0);
            }

            for (int i = 1; i <= 500; i++)
            {
                // 最大500フレームまで書き込む
                
                // 画像ファイルのパス＝画像ファイルのディレクトリ＋"\\"＋"test_000～～～.bmp"
                string p = animPath + "\\" + String.Format("test_{0:000000}.bmp", i);

                if (!File.Exists(p))
                    break; // 次の番号のファイルが見つからなかった場合は、ループを抜ける

                Console.WriteLine("追加します: " + Path.GetFileName(p));
                m.ImageList.Add(Image.FromFile(p));
            }

            // 準備完了
            Console.Write("続行するには何かキーを押してください。");
            Console.ReadKey();

            // 実際に書き込む
            FileStream fs = new FileStream("test.raw", FileMode.Create, FileAccess.Write, FileShare.None);
            m.SaveToStream(fs);
            fs.Close();
        }
    }
}
