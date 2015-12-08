using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Text;

namespace a32system.CSProgram.AnimMaker
{
    /// <summary>
    /// 保存結果を表します。
    /// </summary>
    struct SaveResult
    {
        /// <summary>
        /// アニメーションのフレーム数を表します。
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// アニメーションのサイズを表します。
        /// </summary>
        public Size Size { get; set; }
    }

    class Maker
    {
        // 非公開フィールド
        private ConvertImageList imageList;
        private Size animationSize;


        // 公開フィールド

        /// <summary>
        /// アニメーションに追加するフレーム(コマ)画像のリストを取得します。
        /// </summary>
        public ConvertImageList ImageList
        {
            get { return this.imageList; }
        }

        /// <summary>
        /// アニメーションのサイズを表します。
        /// </summary>
        public Size AnimationSize
        {
            get { return this.animationSize; }
            set { this.animationSize = value; }
        }


        // コンストラクタ
        
        /// <summary>
        /// 新しいMaker型オブジェクトインスタンスを初期化します。
        /// </summary>
        public Maker()
        {
            // データフィールドの初期化
            this.imageList = new ConvertImageList();
            this.animationSize = new Size(160, 120);
        }


        // 公開メソッド

        /// <summary>
        /// 指定したストリームへアニメーションを出力します。
        /// </summary>
        /// <param name="stream"></param>
        public SaveResult SaveToStream(Stream stream)
        {
            // 書き込み可能かチェック
            if (!stream.CanWrite)
                throw new Exception("保存先には書き込み可能なストリームを設定してください。");

            // 書き込み
            foreach (Image img in this.imageList)
            {
                // img = 現在のフレーム

                // 書き込み用の画像を生成
                // 無理やりアニメーションのサイズにリサイズ
                Bitmap canv = new Bitmap(this.animationSize.Width, this.animationSize.Height);
                Graphics g = Graphics.FromImage(canv);
                g.DrawImage(img, new Rectangle(0, 0, canv.Width, canv.Height));
                g.Dispose();

                // ストリームへの書き込み
                int x, y;
                for (y = 0; y < canv.Height; y++)
                    for (x = 0; x < canv.Width; x++)
                        // 画像のx, yピクセル部分の色情報取得 → 輝度取得（0 ～ 1） → 0～127のbyte値に直して書き込み
                        stream.WriteByte((byte)(canv.GetPixel(x, y).GetBrightness() * 127));

                /*
                 * 書き込む値の値域について
                 * 
                 * rawファイルの調色は128階調。これは演習の資料にも記述してあった模様。
                 * 終端符号が負の値になるということと128階調であることから推察するに、rawファイルには8ビットの符号付き整数で書き込まれている模様。
                 * 読み込んだ際にintへキャストしてから値を使用していることがクライアントのソースコードからも分かる。
                 * 
                 */
            }

            // 終端符号の書き込み
            // → 先頭ビットが負の符号を示すような値を書き込む。
            // → …… 200ぐらい？（適当）
            stream.WriteByte((byte) 200);

            // SaveResultの初期化
            // SaveResult構造体は、このメソッドの戻り値専用の型。書き込まれたフレーム数とアニメーションの縦横サイズを格納する。
            SaveResult result = new SaveResult()
            {
                Count = this.imageList.Count,
                Size = this.animationSize,
            };
            
            // SaveResultを返す
            return result;
        }
    }
}
