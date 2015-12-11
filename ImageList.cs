using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace a32system.CSProgram.AnimMaker
{
    /// <summary>
    /// Image型のリスト
    /// </summary>
    class ConvertImageList : List<Image>
    {
        // 非公開フィールド

        // 公開フィールド

        // 公開イベント

        /// <summary>
        /// GIFファイルから複数のフレームを取得するAdd(string gifpath)メソッド実行時に、フレームが追加されると発生します。
        /// </summary>
        public event EventHandler<FrameAddedEventArgs> FrameAdded;

        // コンストラクタ

        /// <summary>
        /// 新しいConvertImageList型オブジェクトインスタンスを初期化します。
        /// </summary>
        public ConvertImageList()
            : base()
        {
            this.FrameAdded = delegate { };
        }


        // 非公開メソッド
        
        // 公開メソッド

        /// <summary>
        /// 複数のフレームで構成されるＧＩＦファイルからフレームを取得します。
        /// </summary>
        /// <param name="gifpath">GIFファイルのパス</param>
        /// <returns>追加されたフレーム数</returns>
        public int Add(string gifpath)
        {
            // ファイルの読み込み
            Image target = Image.FromFile(gifpath);
            FrameDimension fd = new FrameDimension(target.FrameDimensionsList[0]);

            // フレーム数の取得
            int frameCount = target.GetFrameCount(fd);

            // 各フレームの取得と追加
            int i;
            for (i = 0; i < frameCount; i++)
            {
                // コピー先の準備
                Image temp = new Bitmap(target.Width, target.Height);
                Graphics g = Graphics.FromImage(temp);

                // 次のフレームへの切り替えとコピー
                target.SelectActiveFrame(fd, i);
                g.DrawImageUnscaled(target, new Point(0, 0));

                // Graphicsの破棄とコピーした画像を追加
                g.Dispose();
                this.Add(temp);
                
                // FrameAddedイベントを発生する
                this.FrameAdded(this, new FrameAddedEventArgs() { Count = i });
            }

            // 実際に追加されたフレーム数を返す
            return i;
        }


        // 内部クラス

        /// <summary>
        /// FrameAddedイベントのイベント引数型。
        /// </summary>
        public class FrameAddedEventArgs : EventArgs
        {
            /// <summary>
            /// GIFファイル内でのフレーム番号を表します。
            /// </summary>
            public int Count { get; set; }
        }
    }
}
