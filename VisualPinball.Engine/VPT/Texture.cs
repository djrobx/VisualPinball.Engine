using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
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

		public TextureStats GetStats(int threshold)
		{
			if (Data.Path == null || !Data.Path.ToLower().EndsWith(".png")) {
				return null;
			}
			var img = Decode();
			if (img == null) {
				return null;
			}
			var data = MemoryMarshal.AsBytes(img.GetPixelSpan()).ToArray();
			return Analyze(data, img.Width, img.Height, threshold);
		}

		/// <summary>
		/// Retrieves metrics on how many pixels are opaque (no alpha),
		/// translucent (some alpha), and transparent (100% alpha).
		/// </summary>
		/// <param name="data"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="threshold"></param>
		/// <returns></returns>
		private static TextureStats Analyze(IReadOnlyList<byte> data, int width, int height, int threshold)
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
								return new TextureStats(opaque, translucent, transparent);
							}
						}
					}
				}
			}
			return new TextureStats(opaque, translucent, transparent);
		}

		private Image<Rgba32> Decode()
		{
			if (Data.Binary == null) {
				return null;
			}

			using (var stream = new MemoryStream(Data.Binary.Data)) {
				try {
					return Image.Load<Rgba32>(stream, new PngDecoder());

				} catch (Exception) {
					return null;
				}
			}
		}

		private IBinaryData GetBinaryData()
		{
			return Data.Binary as IBinaryData ?? Data.Bitmap;
		}
	}

	public class TextureStats
	{
		public float Translucent => (float) _numTranslucentPixels / _numTotalPixels;
		public float Transparent => (float) _numTransparentPixels / _numTotalPixels;

		public bool IsOpaque => _numTranslucentPixels == 0 && _numTranslucentPixels == 0;

		private readonly int _numOpaquePixels;
		private readonly int _numTranslucentPixels;
		private readonly int _numTransparentPixels;
		private readonly int _numTotalPixels;

		public TextureStats(int numOpaquePixels, int numTranslucentPixels, int numTransparentPixels)
		{
			_numOpaquePixels = numOpaquePixels;
			_numTranslucentPixels = numTranslucentPixels;
			_numTransparentPixels = numTransparentPixels;
			_numTotalPixels = numOpaquePixels + numTranslucentPixels + numTransparentPixels;
		}
	}
}
