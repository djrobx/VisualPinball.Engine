using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NLog;
using VisualPinball.Engine.Resources;

namespace VisualPinball.Engine.VPT
{
	public class Texture : Item<TextureData>
	{
		public static readonly Texture BumperBase = new Texture(Resource.BumperBase);
		public static readonly Texture BumperCap = new Texture(Resource.BumperCap);
		public static readonly Texture BumperRing = new Texture(Resource.BumperRing);
		public static readonly Texture BumperSocket = new Texture(Resource.BumperSocket);

		public static readonly Texture[] LocalTextures = {
			BumperBase, BumperCap, BumperRing, BumperSocket
		};

		public int Width => Data.Width;
		public int Height => Data.Width;
		public bool IsHdr => (Data.Path?.ToLower().EndsWith(".hdr") ?? false) || (Data.Path?.ToLower().EndsWith(".exr") ?? false);

		/// <summary>
		/// Data as read from the .vpx file. Note that for bitmaps, it doesn't
		/// contain the header.
		/// </summary>
		/// <see cref="FileContent"/>
		public byte[] Content => GetBinaryData().Bytes;

		/// <summary>
		/// Data as it would written to an image file (incl headers).
		/// </summary>
		public byte[] FileContent => GetBinaryData().FileContent;

		public Texture(BinaryReader reader, string itemName) : base(new TextureData(reader, itemName)) { }

		private Texture(Resource res) : base(new TextureData(res)) { }

		public static void Measure(string filename)
		{
			var img = Decode(filename);
			if (img == null || img.Format != PixelFormats.Bgra32) {
				return;
			}

			var data = GetPixels(img);
			var numPixels = img.PixelWidth * img.PixelHeight;
			var fullPercent = ToPercent(AnalyzeFull(data, img.PixelWidth, img.PixelHeight));
			var fullPercentBetter = ToPercent(AnalyzeFullBetter(data, img.PixelWidth, img.PixelHeight));

			var partial = AnalyzePartial(data, img.PixelWidth, img.PixelHeight, 1000);
			var partialBetter = AnalyzePartialBetter(data, img.PixelWidth, img.PixelHeight, 1000);
			var shaderbytes = AnalyzeShaderbytes(data, img.PixelWidth, img.PixelHeight, 1000);
			var partialPercent = ToPercent(partial);
			var partialPercentBetter = ToPercent(partialBetter);
			var shaderbytesPercent = ToPercent(shaderbytes);

			var iterations = partial.Item1 + partial.Item2 + partial.Item3;
			var iterationsBetter = partialBetter.Item1 + partialBetter.Item2 + partialBetter.Item3;
			var iterationsShaderbytes = shaderbytes.Item1 + shaderbytes.Item2 + shaderbytes.Item3;

			var iterationsPercent = (double) iterations / numPixels * 100;
			var iterationsPercentBetter = (double) iterationsBetter / numPixels * 100;
			var iterationsPercentShaderbytes = (double) iterationsShaderbytes / numPixels * 100;

			Console.WriteLine("Full: " + fullPercent);
			Console.WriteLine("Full2: " + fullPercentBetter);
			Console.WriteLine("part: " + partialPercent);
			Console.WriteLine("part2: " + partialPercentBetter);
		}

		private static Tuple<double, double, double> ToPercent(Tuple<int, int, int> abs)
		{
			var numPixels = (double)(abs.Item1 + abs.Item2 + abs.Item3);
			return new Tuple<double, double, double>(
				System.Math.Round(abs.Item1 / numPixels * 100, 2),
				System.Math.Round(abs.Item2 / numPixels * 100, 2),
				System.Math.Round(abs.Item3 / numPixels * 100, 2)
			);
		}

		private static Tuple<int, int, int> AnalyzeFull(IReadOnlyList<byte> data, int width, int height)
		{
			var opaque = 0;
			var translucent = 0;
			var transparent = 0;
			for (var y = 0; y < height; y++) {
				for (var x = 0; x < width; x++) {
					var a = data[y * 4 * width + x * 4 + 3];
					switch (a) {
						case 0: transparent++; break;
						case 255: opaque++; break;
						default: translucent++; break;
					}
				}
			}
			return new Tuple<int, int, int>(opaque, translucent, transparent);
		}

		private static Tuple<int, int, int> AnalyzePartial(IReadOnlyList<byte> data, int width, int height, int threshold)
		{
			var opaque = 0;
			var translucent = 0;
			var transparent = 0;
			for (var y = 0; y < height; y++) {
				for (var x = 0; x < width; x++) {
					var a = data[y * 4 * width + x * 4 + 3];
					switch (a) {
						case 0: transparent++; break;
						case 255: opaque++; break;
						default: translucent++; break;
					}
					if (translucent + transparent > threshold) {
						return new Tuple<int, int, int>(opaque, translucent, transparent);
					}
				}
			}
			return new Tuple<int, int, int>(opaque, translucent, transparent);
		}

		private static Tuple<int, int, int> AnalyzeFullBetter(IReadOnlyList<byte> data, int width, int height)
		{
			var opaque = 0;
			var translucent = 0;
			var transparent = 0;
			const int d = 10;
			var dx = (int)System.Math.Ceiling((float)width / d);
			var dy = (int)System.Math.Ceiling((float)height / d);
			for (var yy= 0; yy < dy; yy ++) {
				for (var xx = 0; xx < dx; xx++) {
					for (var y = 0; y < height; y += dy) {
						var posY = y + yy;
						if (posY >= height) {
							break;
						}
						for (var x = 0; x < width; x += dx) {
							var posX = x + xx;
							if (posX >= width) {
								break;
							}
							var a = data[posY * 4 * width + posX * 4 + 3];
							switch (a) {
								case 0: transparent++; break;
								case 255: opaque++; break;
								default: translucent++; break;
							}
						}
					}
				}
			}
			return new Tuple<int, int, int>(opaque, translucent, transparent);
		}

		private static Tuple<int, int, int> AnalyzePartialBetter(IReadOnlyList<byte> data, int width, int height, int threshold)
		{
			var opaque = 0;
			var translucent = 0;
			var transparent = 0;
			const int d = 10;
			var dx = (int)System.Math.Ceiling((float)width / d);
			var dy = (int)System.Math.Ceiling((float)height / d);
			for (var yy= 0; yy < dy; yy ++) {
				for (var xx = 0; xx < dx; xx++) {
					for (var y = 0; y < height; y += dy) {
						var posY = y + yy;
						if (posY >= height) {
							break;
						}
						for (var x = 0; x < width; x += dx) {
							var posX = x + xx;
							if (posX >= width) {
								break;
							}
							var a = data[posY * 4 * width + posX * 4 + 3];
							switch (a) {
								case 0: transparent++; break;
								case 255: opaque++; break;
								default: translucent++; break;
							}

							if (translucent + transparent > threshold) {
								return new Tuple<int, int, int>(opaque, translucent, transparent);
							}
						}
					}
				}
			}
			return new Tuple<int, int, int>(opaque, translucent, transparent);
		}

		private static Tuple<int, int, int> AnalyzeShaderbytes(IReadOnlyList<byte> pixels, int width, int height)
		{
			var opaque = 0;
			var translucent = 0;
			var transparent = 0;
			var numPixels = width * height;
			var approximationIndex = 0;
			var approximationStepDistance = (int)((float)numPixels / 10); // this is how maany pixels the aproximationIndex is incremented by
			const float approximationStepDistanceScalar = 0.8f;
			var mustCalculateApproximationIndexStartValue = true;
			var approximationBeatBruteForce = false;

			for (var i = 0; i < numPixels; i++) {

				// bruteforce
				if (pixels[i * 4 + 3] < 254) {
					if (pixels[i * 4 + 3] == 0) {
						transparent++;
					} else {
						translucent++;
					}
					break;
				}

				//approximation
				if (mustCalculateApproximationIndexStartValue) {
					approximationIndex = System.Math.Min(numPixels-1, i + 2);
					mustCalculateApproximationIndexStartValue = false;
				}

				if (pixels[approximationIndex * 4 + 3] < 254) {
					if (pixels[approximationIndex * 4 + 3] == 0) {
						transparent++;
					} else {
						translucent++;
					}
					approximationBeatBruteForce = true;
					break;
				}
				approximationIndex += approximationStepDistance;
				//need to see if the index of approximation is larger than array
				//if so then refine the guessing distance and start again at new aproximationIndex;
				if (approximationIndex >= numPixels) {
					mustCalculateApproximationIndexStartValue = true;
					approximationStepDistance = (int)(approximationStepDistance * approximationStepDistanceScalar);
				}

				opaque++;
			}
			return new Tuple<int, int, int>(opaque, translucent, transparent);
		}

		private static Tuple<int, int, int> AnalyzeShaderbytes(IReadOnlyList<byte> pixels, int width, int height, int threshold)
		{
			var opaque = 0;
			var translucent = 0;
			var transparent = 0;
			var numPixels = width * height;
			var approximationIndex = 0;
			var approximationStepDistance = (int)((float)numPixels / 10); // this is how maany pixels the aproximationIndex is incremented by
			const float approximationStepDistanceScalar = 0.8f;
			var mustCalculateApproximationIndexStartValue = true;
			var approximationBeatBruteForce = false;

			for (var i = 0; i < numPixels; i++) {

				// bruteforce
				if (pixels[i * 4 + 3] < 254) {
					if (pixels[i * 4 + 3] == 0) {
						transparent++;
					} else {
						translucent++;
					}
					if (translucent + transparent > threshold) {
						return new Tuple<int, int, int>(opaque, translucent, transparent);
					}
					continue;
				}

				//approximation
				if (mustCalculateApproximationIndexStartValue) {
					approximationIndex = System.Math.Min(numPixels-1, i + 2);
					mustCalculateApproximationIndexStartValue = false;
				}

				if (pixels[approximationIndex * 4 + 3] < 254) {
					if (pixels[approximationIndex * 4 + 3] == 0) {
						transparent++;
					} else {
						translucent++;
					}
					approximationBeatBruteForce = true;
					if (translucent + transparent > threshold) {
						return new Tuple<int, int, int>(opaque, translucent, transparent);
					}
					continue;
				}
				approximationIndex += approximationStepDistance;
				//need to see if the index of approximation is larger than array
				//if so then refine the guessing distance and start again at new aproximationIndex;
				if (approximationIndex >= numPixels) {
					mustCalculateApproximationIndexStartValue = true;
					approximationStepDistance = (int)(approximationStepDistance * approximationStepDistanceScalar);
				}

				opaque++;
			}
			return new Tuple<int, int, int>(opaque, translucent, transparent);
		}


		private static byte[] GetPixels(BitmapSource img)
		{
			var bytesPerPixel = (img.Format.BitsPerPixel + 7) / 8;
			var stride = (img.PixelWidth * img.Format.BitsPerPixel + 7) / 8;
			var bytes = new byte[bytesPerPixel * img.PixelWidth * img.PixelHeight];
			img.CopyPixels(Int32Rect.Empty, bytes, stride, 0);
			return bytes;
		}

		private static BitmapSource Decode(string filename)
		{
			// if (Data.Binary == null) {
			// 	return null;
			// }
			//using (var stream = new MemoryStream(Data.Binary.Data)) {
			using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read)) {
				var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
				return decoder.Frames[0];
			}
		}

		private IBinaryData GetBinaryData()
		{
			return Data.Binary as IBinaryData ?? Data.Bitmap;
		}
	}
}
