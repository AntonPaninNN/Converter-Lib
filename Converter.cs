using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace ConverterLib
{
	public class Converter
	{
		private int _progress;
		private bool _error;
		private Process _ffmpegProcess;

        private const bool NOT_USE_OLD_VERSION = false;
		private const string FULL_DURATION_PATTERN = @"(\d{2}):(\d{2}):(\d{2}.\d{2})";
		private const string CURRENT_DURATION_PATTERN = @"time=(\d{1,}).(\d{2})";

		public int Progress { get { return _progress; } }
		public bool Error { get { return _error; } }

		private string createArgString(InputParameters iParam)
		{
			try
			{
				if (string.IsNullOrEmpty(iParam.InputPath) || string.IsNullOrEmpty(iParam.OutputPath))
					return null;

				StringBuilder args = new StringBuilder(string.Format("-i \"{0}\"", iParam.InputPath));

				if (!string.IsNullOrEmpty(iParam.OutputVideoCodec))
					args.Append(string.Format(" -vcodec {0}", iParam.OutputVideoCodec));

				if (!string.IsNullOrEmpty(iParam.OutputAudioCodec))
					args.Append(string.Format(" -acodec {0}", iParam.OutputAudioCodec));

				if (iParam.Frequency > 0)
					args.Append(string.Format(" -ar {0}", iParam.Frequency));

				if (!string.IsNullOrEmpty(iParam.OutputContainer))
					args.Append(string.Format(" -f {0}", iParam.OutputContainer));

				if (iParam.FrameHeight > 0 && iParam.FrameWidth > 0)
					args.Append(string.Format(" -s {0}x{1}", iParam.FrameWidth, iParam.FrameHeight));

                if (!string.IsNullOrEmpty(iParam.PicFormat))
                    args.Append(string.Format(" -s {0}", iParam.PicFormat));

				if (!string.IsNullOrEmpty(iParam.Aspect))
					args.Append(string.Format(" -aspect {0}", iParam.Aspect));

				if (iParam.MinQuantizer > 0)
					args.Append(string.Format(" -qmin {0}", iParam.MinQuantizer));

				if (iParam.MaxQuantizer > 0)
					args.Append(string.Format(" -qmax {0}", iParam.MaxQuantizer));

				if (!string.IsNullOrEmpty(iParam.Bitrate))
					args.Append(string.Format(" -b {0}", iParam.Bitrate));

				if (!string.IsNullOrEmpty(iParam.AudioBitrate))
					args.Append(string.Format(" -ab {0}", iParam.AudioBitrate));

				if (iParam.BufferSize > 0)
					args.Append(string.Format(" -bufsize {0}", iParam.BufferSize));

				if (iParam.Owerwrite)
					args.Append(" -y");

				/* Final output path */
				args.Append(string.Format(" \"{0}\"", iParam.OutputPath));

				return args.ToString();
			}
			catch
			{
				return null;
			}
		}

		private int extractDuration(string line, bool fullDuration)
		{
			string pattern = string.Empty;

			if (fullDuration)
				pattern = FULL_DURATION_PATTERN;
			else
				pattern = CURRENT_DURATION_PATTERN;

			try
			{
				Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
				MatchCollection matches = rgx.Matches(line);

				if (fullDuration)
				{
					if (matches.Count == 1)
					{
						if (matches[0].Groups.Count == 4)
							return int.Parse(matches[0].Groups[1].Value) * 3600 +
								int.Parse(matches[0].Groups[2].Value) * 60 + 
								int.Parse(matches[0].Groups[3].Value.Split('.')[0]);
						else
							return -1;
					}
					else
						return -1;
				}
				else
				{
					if (matches.Count == 1)
					{
						if (matches[0].Groups.Count == 3)
							return int.Parse(matches[0].Groups[1].Value);
						else
							return -1;
					}
					else
						return -1;
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				return -1;
			}
		}

		public void convert(InputParameters iParam)
		{
			_error = false;
			ThreadPool.QueueUserWorkItem(new WaitCallback(convertInThread), iParam);
		}

		private void convertInThread(object target)
		{
			if (target == null)
			{
				_error = true;
				return;
			}

			try
			{
				InputParameters iParam = target as InputParameters;

				_ffmpegProcess = new Process();
				int fullDuration = 0;
				int currentDuration = 0;
				string args = string.Empty;
				string ffmpegPath = string.Empty;
				bool fullDurationExist = false;
				bool currentDurationExist = false;

				if (string.IsNullOrEmpty(args = createArgString(iParam)))
				{
					_error = true;
					return;
				}

				if (string.IsNullOrEmpty(ffmpegPath = Path.Combine(Path.GetDirectoryName(
						 Assembly.GetExecutingAssembly().Location), "ffmpeg.exe")))
				{
					_error = true;
					return;
				}

				_ffmpegProcess.StartInfo.FileName = ffmpegPath;
				_ffmpegProcess.StartInfo.Arguments = args;
				_ffmpegProcess.EnableRaisingEvents = false;
				_ffmpegProcess.StartInfo.UseShellExecute = false;
				_ffmpegProcess.StartInfo.CreateNoWindow = true;
				_ffmpegProcess.StartInfo.RedirectStandardOutput = true;
				_ffmpegProcess.StartInfo.RedirectStandardError = true;
				_ffmpegProcess.Start();

				StreamReader sReader = _ffmpegProcess.StandardError;
				do
				{
					string line = sReader.ReadLine();

					if (line.Contains("Duration: "))
					{
						fullDurationExist = true;
						if ((fullDuration = extractDuration(line, true)) < 0)
						{
							_error = true;
							return;
						}
					}
					else if (line.Contains("time="))
					{
						currentDurationExist = true;
						if ((currentDuration = extractDuration(line, false)) < 0) /* TRUE - big ffmpeg, FALSE - little ffmpeg */
						{
							_error = true;
							return;
						}
						else
							_progress = (int)((double)(currentDuration * 100) / (double)fullDuration);
					}
				} while (!sReader.EndOfStream);

				sReader.Close();
				_ffmpegProcess.WaitForExit();
				_ffmpegProcess.Close();

				if (!fullDurationExist || !currentDurationExist)
				{
					_error = true;
					return;
				}

				_progress = -2;
			}
			catch
			{
				_error = true;
			}
		}

		public void KillFFmpegProcess()
		{
			if (_ffmpegProcess != null)
			{
				try
				{
					_ffmpegProcess.Kill();
				}
				catch
				{ }
			}
		}
	}
}
