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

            #region 既存のアニメーションから取得

            // コマンドライン引数でファイルが指定されていればそちらを、そうでなければファイル選択ダイアログを表示
            string animPath;// = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "/TestImages/test.gif";
            if (args.Length >= 1 && File.Exists(args[0]))
                animPath = args[0];
            else
            {
                OpenFileDialog ofd = new OpenFileDialog();
                
                ofd.FileName = "";
                ofd.InitialDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                ofd.Filter =
                    "対応するすべての形式(*.gif;*.raw)|*.gif;*.raw|アニメーションGIF(*.gif)|*.gif|ヘッダ付き演習raw形式(*.raw)|*.raw|すべてのファイル(*.*)|*.*";
                ofd.FilterIndex = 1;
                //タイトルを設定する
                ofd.Title = "変換元のアニメーションファイルを選択してください";
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

                animPath = ofd.FileName;
            }

            if (!File.Exists(animPath))
            {
                // ファイルが見つからない
                Console.WriteLine("ファイルが見つかりませんでした。");
                Console.WriteLine(animPath);
                Console.ReadKey();
            }

            string gifName = Path.GetFileName(animPath);


            // １フレーム追加されるごとに実行する処理を定義
            m.ImageList.FrameAdded += (sender, e) =>
            {
                Console.Write("{0} から {1:0000} 番目のフレームを追加しました。", gifName, e.Count + 1);
                Console.SetCursorPosition(0, Console.CursorTop);
            };

            // 読み込み処理
            try
            {
                m.ImageList.Add(animPath);
            }
            catch (Exception ex)
            {
                // 読み込み失敗
                Console.WriteLine("読み込みに失敗しました。");
                Console.WriteLine(animPath);
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }

            Console.WriteLine();

            #endregion


            // 準備完了 :: モード選択
            Console.WriteLine("読み込みが完了しました。");
            Console.WriteLine("出力モードを選択してください。");
            Console.WriteLine("[0]：グレースケール版");
            Console.WriteLine("[1]：カラー版 (各チャンネル独立フレーム)");
            Console.WriteLine("[2]：カラー版 (たぶん正しいほう)");
            Console.WriteLine("[3]：ヘッダ付きグレースケール版");
            Console.WriteLine("[4]：ヘッダ付きカラー版");
            Console.Write("続行するには番号キーを押してください。");
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
                case '3':
                    am = AnimMode.GrayScaleWithHeader;
                    break;
                case '4':
                    am = AnimMode.ColorWithHeader;
                    break;
                default:
                    // その他＝グレー
                    am = AnimMode.GrayScale;
                    break;
            }

            Console.WriteLine();
            
            // サイズ選択
            Console.WriteLine("出力するアニメーションのサイズを選択してください。");
            Console.WriteLine("出力モードを選択してください。");
            Console.WriteLine("[0]：160x120 (演習標準)");
            Console.WriteLine("[1]：80x60");
            Console.WriteLine("[2]：320x240");
            Console.WriteLine("[3]：640x480");
            Console.WriteLine("[4]：800x600");
            Console.Write("続行するには番号キーを押してください。");
            key = Console.ReadKey();

            Size animSize;
            switch (key.KeyChar)
            {
                case '1':
                    animSize = new Size(80, 60);
                    break;
                case '2':
                    animSize = new Size(320, 240);
                    break;
                case '3':
                    animSize = new Size(640, 480);
                    break;
                case '4':
                    animSize = new Size(800, 600);
                    break;
                default:
                    animSize = new Size(160, 120);
                    break;
            }

            m.AnimationSize = animSize;

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
            
            string outputPath = Path.GetDirectoryName(animPath) + "/" + Path.GetFileNameWithoutExtension(animPath) + "_result.raw";
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
