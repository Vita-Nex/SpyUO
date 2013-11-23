using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ultima.Package
{
	/// <summary>
	/// Describes ultima art.
	/// </summary>
	public class UltimaLegacyArt
	{
		#region Properties
		private int _Width;

		/// <summary>
		/// Gets image width.
		/// </summary>
		public int Width
		{
			get { return _Width; }
		}

		private int _Height;

		/// <summary>
		/// Gets image height.
		/// </summary>
		public int Height
		{
			get { return _Height; }
		}

		private byte[] _PixelData;

		/// <summary>
		/// Gets pixel data in ARGB.
		/// </summary>
		public byte[] PixelData
		{
			get { return _PixelData; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaLegacyArt.
		/// </summary>
		/// <param name="reader">Reader to construct from.</param>
		/// <param name="land"></param>
		public UltimaLegacyArt( BinaryReader reader, bool land )
		{
			if ( land )
				ReadLand( reader );
			else
				ReadStatic( reader );
		}
		#endregion

		#region Methods
		private void ReadStatic( BinaryReader reader )
		{
			reader.ReadInt32();
			_Width = reader.ReadUInt16();
			_Height = reader.ReadUInt16();

			// Lookups
			int[] lookups = new int[ _Height ];
			int start = 8 + _Height * 2;

			for ( int y = 0; y < _Height; y++ )
				lookups[ y ] = start + reader.ReadUInt16() * 2;

			// Pixel data
			_PixelData = new byte[ _Width * _Height * 4 ];
			int pixelDataIndex = 0;

			for ( int y = 0; y < _Height; y++ )
			{
				reader.BaseStream.Seek( lookups[ y ], SeekOrigin.Begin );

				// Read line start/length sort of RLEish
				int offset;
				int length;
				pixelDataIndex = y * _Width * 4;

				do
				{
					offset = reader.ReadUInt16();
					length = reader.ReadUInt16();
					pixelDataIndex += offset * 4;

					for ( int x = 0; x < length; x++ )
					{
						int pixel = reader.ReadUInt16() ^ 0x8000;

						_PixelData[ pixelDataIndex++ ] = (byte) ( ( pixel & 0x1F ) << 3 ); // b
						_PixelData[ pixelDataIndex++ ] = (byte) ( ( pixel & 0x3E0 ) >> 2 ); // g
						_PixelData[ pixelDataIndex++ ] = (byte) ( ( pixel & 0x7C00 ) >> 7 ); // r
						_PixelData[ pixelDataIndex++ ] = (byte) ( ( ( pixel & 0x8000 ) >> 15 ) * 0xFF ); // a
					}
				}
				while ( offset + length > 0 );
			}
		}

		private void ReadLand( BinaryReader reader )
		{
			int pixelDepth = 4;
			int half = 22;

			_Width = 44;
			_Height = 44;
			_PixelData = new byte[ _Width * _Height * pixelDepth ];

			for ( int y = 0; y < half; y++ )
			{
				int pixelDataIndex = y * _Width * pixelDepth + ( half - y - 1 ) * pixelDepth;

				for ( int x = half - y - 1; x < half + y + 1; x++ )
				{
					int pixel = reader.ReadUInt16() | 0x8000;

					_PixelData[ pixelDataIndex++ ] = (byte) ( ( pixel & 0x1F ) << 3 ); // b
					_PixelData[ pixelDataIndex++ ] = (byte) ( ( pixel & 0x3E0 ) >> 2 ); // g
					_PixelData[ pixelDataIndex++ ] = (byte) ( ( pixel & 0x7C00 ) >> 7 ); // r
					_PixelData[ pixelDataIndex++ ] = (byte) ( ( ( pixel & 0x8000 ) >> 15 ) * 0xFF ); // a
				}
			}

			for ( int y = half; y < _Height; y++ )
			{
				int pixelDataIndex = y * _Width * pixelDepth + ( y - half ) * pixelDepth;

				for ( int x = y - half; x < _Width - ( y - half ); x++ )
				{
					int pixel = reader.ReadUInt16() | 0x8000;

					_PixelData[ pixelDataIndex++ ] = (byte) ( ( pixel & 0x1F ) << 3 ); // b
					_PixelData[ pixelDataIndex++ ] = (byte) ( ( pixel & 0x3E0 ) >> 2 ); // g
					_PixelData[ pixelDataIndex++ ] = (byte) ( ( pixel & 0x7C00 ) >> 7 ); // r
					_PixelData[ pixelDataIndex++ ] = (byte) ( ( ( pixel & 0x8000 ) >> 15 ) * 0xFF ); // a
				}
			}
		}

		/// <summary>
		/// Gets image as bitmap source.
		/// </summary>
		/// <returns>WPF bitmap source.</returns>
		public BitmapSource GetImageAsBitmapSource()
		{
			return BitmapSource.Create( _Width, _Height, 96, 96, PixelFormats.Bgra32, null, _PixelData, _Width * 4 );
		}

		/// <summary>
		/// Gets image as bitmap.
		/// </summary>
		/// <returns>Bitmap.</returns>
		public Bitmap GetImageAsBitmap()
		{
			Bitmap bitmap = new Bitmap( _Width, _Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb );
			BitmapData data = bitmap.LockBits( new Rectangle( 0, 0, _Width, _Height ), ImageLockMode.WriteOnly, bitmap.PixelFormat );
			Marshal.Copy( _PixelData, 0, data.Scan0, _PixelData.Length );

			bitmap.UnlockBits( data );
			return bitmap;
		}

		/// <summary>
		/// Reads TGA from file.
		/// </summary>
		/// <param name="filePath">File path to read from.</param>
		/// <param name="land">Determines whether to load land tile.</param>
		/// <returns>Art image.</returns>
		public static UltimaLegacyArt FromFile( string filePath, bool land )
		{
			using ( FileStream stream = File.Open( filePath, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			{
				return FromStream( stream, land );
			}
		}

		/// <summary>
		/// Reads TGA from stream.
		/// </summary>
		/// <param name="data">Memory to read from.</param>
		/// <param name="land">Determines whether to load land tile.</param>
		/// <returns>Art image.</returns>
		public static UltimaLegacyArt FromMemory( byte[] data, bool land )
		{
			using ( MemoryStream stream = new MemoryStream( data ) )
			{
				return FromStream( stream, land );
			}
		}

		/// <summary>
		/// Reads TGA from stream.
		/// </summary>
		/// <param name="stream">Stream to read from.</param>
		/// <param name="land">Determines whether to load land tile.</param>
		/// <returns>TGA image.</returns>
		public static UltimaLegacyArt FromStream( Stream stream, bool land )
		{
			using ( BinaryReader reader = new BinaryReader( stream ) )
			{
				return new UltimaLegacyArt( reader, land );
			}
		}
		#endregion
	}
}
