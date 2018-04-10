using System;
using System.Collections.Generic;
using System.Text;

namespace ConverterLib
{
	public class InputParameters
	{
		private string _inputPath;
		private string _outputPath;
		private int _frameWidth;
		private int _frameHeight;
		private int _frequency;
		private string _outputContainer;
		private string _outputVideoCodec;
		private string _outputAudioCodec;
		private string _aspect;
		private bool _owerwrite;
		private int _minQuantizer;
		private int _maxQuantizer;
		private string _bitrate;
		private int _bufferSize;
		private string _audioBitrate;
        private string _picFormat;

		public string InputPath { get { return _inputPath; } set { _inputPath = value; } }
		public string OutputPath { get { return _outputPath; } set { _outputPath = value; } }
		public int FrameWidth { get { return _frameWidth; } set { _frameWidth = value; } }
		public int FrameHeight { get { return _frameHeight; } set { _frameHeight = value; } }
		public int Frequency { get { return _frequency; } set { _frequency = value; } }
		public string OutputContainer { get { return _outputContainer; } set { _outputContainer = value; } }
		public string OutputVideoCodec { get { return _outputVideoCodec; } set { _outputVideoCodec = value; } }
		public string OutputAudioCodec { get { return _outputAudioCodec; } set { _outputAudioCodec = value; } }
		public string Aspect { get { return _aspect; } set { _aspect = value; } }
		public bool Owerwrite { get { return _owerwrite; } set { _owerwrite = value; } }
		public int MinQuantizer { get { return _minQuantizer; } set { _minQuantizer = value; } }
		public int MaxQuantizer { get { return _maxQuantizer; } set { _maxQuantizer = value; } }
		public string Bitrate { get { return _bitrate; } set { _bitrate = value; } }
		public int BufferSize { get { return _bufferSize; } set { _bufferSize = value; } }
		public string AudioBitrate { get { return _audioBitrate; } set { _audioBitrate = value; } }
        public string PicFormat { get { return _picFormat; } set { _picFormat = value; } }
	}
}
