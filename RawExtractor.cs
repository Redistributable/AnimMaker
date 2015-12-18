using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Text;

namespace a32system.CSProgram.AnimMaker
{
    /// <summary>
    /// RAWファイルを読み込み、フレームの取得などを行います。
    /// </summary>
    class RawExtractor
    {
        // 非公開フィールド
        BinaryReader binReader;
        AnimMode mode;
        Size frameSize;
        int maxFrame; // ←たぶん使わない

        // 公開フィールド・プロパティ
        
        /// <summary>
        /// アニメーションのタイプ（モード）を取得します。
        /// </summary>
        public AnimMode Mode
        {
            get { return this.mode; }
        }

        /// <summary>
        /// アニメーションのフレームサイズを取得します。
        /// </summary>
        public Size FrameSize
        {
            get { return this.frameSize; }
        }


        // 公開イベント
        
        /// <summary>
        /// GetAllFramesメソッドで１フレーム読み込まれるごとに発生します。
        /// </summary>
        public event EventHandler<FrameLoadedEventArgs> FrameLoaded;


        // コンストラクタ

        /// <summary>
        /// 新しいRawExtractor型オブジェクトインスタンスを初期化します。
        /// </summary>
        /// <param name="path">演習のRAW形式で保存されたヘッダ付きアニメーションファイルのパス</param>
        public RawExtractor(string path)
        {
            this.FrameLoaded = delegate { };

            this._initBinReader(path);
            if (!this._getModeFromHeader())
            {
                // ヘッダの読み込みに失敗した
                throw new NotSupportedException("ヘッダの読み込みに失敗しました。");
            }
        }

        /// <summary>
        /// 新しいRawExtractor型オブジェクトインスタンスを初期化します。
        /// </summary>
        /// <param name="path">演習のRAW形式で保存されたアニメーションファイルのパス</param>
        /// <param name="mode">アニメーションのタイプ</param>
        /// <param name="size">フレームのサイズ</param>
        /// <param name="frameCount">読み込む最大のフレーム数 (実際のフレーム数のほうが少ない場合は、そちらが利用されます。)</param>
        public RawExtractor(string path, AnimMode mode, Size size, int frameCount)
        {
            this.FrameLoaded = delegate { };

            this._initBinReader(path);
            this.mode = mode;
            this.frameSize = size;
            this.maxFrame = frameCount;
        }


        // 非公開メソッド

        /// <summary>
        /// ファイルをオープンして、binReaderを初期化します。
        /// </summary>
        /// <param name="path">ファイルのパス</param>
        private void _initBinReader(string path)
        {
            this.binReader = new BinaryReader(
                new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        /// <summary>
        /// binReaderの現在の位置から２バイト読み込み、エンディアン反転をしてからushort値として読み込みます。
        /// </summary>
        /// <returns></returns>
        private ushort _getUShort()
        {
            // ２バイト読み込み
            byte[] buf = this.binReader.ReadBytes(2);
            
            // エンディアン反転
            byte temp = buf[0];
            buf[0] = buf[1];
            buf[1] = temp;

            // ushort値の取得
            BinaryReader br = new BinaryReader(new MemoryStream(buf));
            ushort result = br.ReadUInt16();
            br.Close();

            return result;
        }

        /// <summary>
        /// binReaderの現在の位置からヘッダとして１２バイト読み込み、modeとframeSizeを自動設定します。
        /// </summary>
        /// <return>ヘッダとしての読み込みに失敗した場合は、falseが返ります。</return>
        private bool _getModeFromHeader()
        {
            // カラーモード部まで読み込み
            byte[] headerBuf = this.binReader.ReadBytes(4);

            AnimMode modeResult = AnimMode.WithHeader;
            Size sizeResult;
            int maxFrameResult;

            // 固定フィールド検査
            if (headerBuf[0] != 0xFF || headerBuf[1] != 0xFE)
                // 固定フィールドが不正な値
                return false;

            // カラーモード
            switch (headerBuf[3])
            {
                case 0x01:
                    modeResult = modeResult | AnimMode.GrayScale;
                    break;
                case 0x03:
                    modeResult = modeResult | AnimMode.Color;
                    break;
                case 0x09:
                    modeResult = modeResult | AnimMode.Color_2;
                    break;
                default:
                    // 非対応形式
                    return false;
            }

            // 階調値
            if (this._getUShort() != 0x7F)
                // 非対応
                return false;

            // フレームサイズ
            sizeResult = new Size(this._getUShort(), this._getUShort());

            // フレーム数
            maxFrameResult = this._getUShort();

            // 各値の設定
            this.mode = modeResult;
            this.frameSize = sizeResult;
            this.maxFrame = maxFrameResult;

            //Console.WriteLine("mode = " + modeResult + " / size = " + sizeResult.Width + "x" + sizeResult.Height + " / count = " + maxFrameResult);

            return true;
        }


        // 公開メソッド

        /// <summary>
        /// すべてのフレームを取得します。
        /// </summary>
        /// <returns></returns>
        public List<Image> GetAllFrames()
        {
            if ((this.mode & AnimMode.WithHeader) == AnimMode.WithHeader)
            {
                // ヘッダ付き
                // → 頭12バイトはヘッダのため13バイト目から読み込む
                this.binReader.BaseStream.Position = 12;
            }
            else
            {
                // ヘッダなし
                // → 頭から読み込む
                this.binReader.BaseStream.Position = 0;
            }

            List<Image> result = new List<Image>();
            
            bool exitFlag = false;
            for (int i = 0; i < this.maxFrame && !exitFlag; i++)
            {
                Bitmap bmp = new Bitmap(this.frameSize.Width, this.frameSize.Height);
                
                // カラーモード部のみを抽出
                AnimMode colorMode = this.mode & (AnimMode)15;

                switch (colorMode)
                {
                    case AnimMode.GrayScale:
                        for (int y = 0; y < bmp.Height; y++)
                            for (int x = 0; x < bmp.Width; x++)
                            {
                                if (this.binReader.BaseStream.Length - this.binReader.BaseStream.Position <= 1)
                                {
                                    // 残りバイト数が０バイト
                                    // → 読み込み処理を中止
                                    exitFlag = true;
                                    break;
                                }

                                byte data = this.binReader.ReadByte();
                                if (data > 127)
                                {
                                    // 終端コード
                                    // → 読み込み処理の中止
                                    exitFlag = true;
                                    break;
                                }

                                bmp.SetPixel(x, y, Color.FromArgb(data * 2, data * 2, data * 2));
                            }
                        break;
                    case AnimMode.Color:
                        for (int y = 0; y < bmp.Height; y++)
                        {
                            for (int x = 0; x < bmp.Width; x++)
                            {
                                if (this.binReader.BaseStream.Length - this.binReader.BaseStream.Position <= 4)
                                {
                                    // 残りバイト数が３バイト以下
                                    // → 読み込み処理を中止
                                    exitFlag = true;
                                    break;
                                }

                                byte[] datas = this.binReader.ReadBytes(3);
                                if (datas[0] > 127 || datas[1] > 127 || datas[2] > 127)
                                {
                                    // 終端コード
                                    // → 読み込み処理の中止
                                    exitFlag = true;
                                    break;
                                }

                                bmp.SetPixel(x, y, Color.FromArgb(
                                    datas[0] * 2,
                                    datas[1] * 2,
                                    datas[2] * 2));
                            }

                            if (exitFlag) break;
                        }
                        break;
                    case AnimMode.Color_2:
                        throw new NotImplementedException();
                    default:
                        throw new Exception("カラーモードエラー");
                }

                result.Add(bmp);
                this.FrameLoaded(this, new FrameLoadedEventArgs() { Count = i });
            }

            return result;
        }


        // 内部クラス

        public class FrameLoadedEventArgs : EventArgs
        {
            /// <summary>
            /// 読み込まれたフレームのRAWファイル内での番号を表します。
            /// </summary>
            public int Count { get; set; }
        }
    }
}
