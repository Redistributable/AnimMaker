using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

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


            #region 連番のビットマップから読み込み（旧）
            /*
            // アニメーションに使用する連番の画像ファイルが存在するディレクトリ
            string animPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"/TestImages\test_bmp";


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
            */
            #endregion

            #region ＧＩＦアニメーションから取得

            // コマンドライン引数でファイルが指定されていればそちらを、そうでなければファイル選択ダイアログを表示
            string gifPath;// = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "/TestImages/test.gif";
            if (args.Length >= 1 && File.Exists(args[0]))
                gifPath = args[0];
            else
            {
                OpenFileDialog ofd = new OpenFileDialog();
                
                ofd.FileName = "";
                ofd.InitialDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                ofd.Filter =
                    "アニメーションGIF(*.gif)|*.gif|すべてのファイル(*.*)|*.*";
                ofd.FilterIndex = 1;
                //タイトルを設定する
                ofd.Title = "変換元のＧＩＦファイルを選択してください";
                ofd.RestoreDirectory = true;
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;

                //ダイアログを表示する
                if (ofd.ShowDialog() != DialogResult.OK)
                {
                    // GIFファイルが見つからない
                    Console.WriteLine("変換元のファイルを指定してください。");
                    Console.ReadKey();

                    Environment.Exit(0);
                }

                gifPath = ofd.FileName;
            }

            if (!File.Exists(gifPath))
            {
                // GIFファイルが見つからない
                Console.Write("GIFファイルが見つかりませんでした。");
                Console.Write(gifPath);
                Console.ReadKey();
            }

            string gifName = Path.GetFileName(gifPath);


            // １フレーム追加されるごとに実行する処理を定義
            m.ImageList.FrameAdded += (sender, e) =>
            {
                Console.Write("{0} から {1:0000} 番目のフレームを追加しました。", gifName, e.Count + 1);
                Console.SetCursorPosition(0, Console.CursorTop);
            };
            
            // ＧＩＦの読み込み処理
            m.ImageList.Add(gifPath);
            Console.WriteLine();

            #endregion


            // 準備完了 :: モード選択
            Console.WriteLine("読み込みが完了しました。");
            Console.WriteLine("出力モードを選択してください。");
            Console.WriteLine("[ 0] モノクロ版");
            Console.WriteLine("[ 1]：カラー版 (各チャンネル独立フレーム)");
            Console.WriteLine("[ 2]：カラー版 (たぶん正しいほう)");
            Console.WriteLine("[他]：ヘッダ付きカラー版（未実装）");
            Console.Write("続行するには何かキーを押してください。");
            ConsoleKeyInfo key = Console.ReadKey();

            AnimMode am;
            switch (key.KeyChar)
            {
                case '1':
                    // カラーモード (失敗したやつ)
                    am = AnimMode.Color_2;
                    break;
                case '2':
                    // カラーモード
                    am = AnimMode.Color;
                    break;
                default:
                    // その他＝グレー
                    am = AnimMode.GrayScale;
                    break;
            }

            // 実際に書き込む
            Console.WriteLine();
            Console.WriteLine("次のモードで出力を開始します。: " + am.ToString() + ", " + m.AnimationSize.Width + "x" + m.AnimationSize.Height);

            // １フレーム書き込まれるごとに行う処理を定義
            m.FrameSaved += (sender, e) =>
            {
                // 現在の進捗を表示する
                Console.Write("出力しています... {0:0000} / {1:0000}", 1 + e.Count, m.ImageList.Count);
                Console.SetCursorPosition(0, Console.CursorTop);
            };
            
            string outputPath = Path.GetDirectoryName(gifPath) + "/" + Path.GetFileNameWithoutExtension(gifPath) + ".raw";
            FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            SaveResult result = m.SaveToStream(fs, am);
            fs.Close();

            // 終了
            Console.WriteLine();
            Console.WriteLine("{0}フレームのｒａｗファイルが出力されました。", result.Count);
            Console.Write("続行するには何かキーを押してください。");
            Console.ReadKey();

            Environment.Exit(0);
        }
    }
}
