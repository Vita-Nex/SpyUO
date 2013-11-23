using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ultima.Package
{
	/// <summary>
	/// Describes Truevision TGA file.
	/// </summary>
	public class TGA
	{
		#region Properties
		private const int FooterLength = 26;

		// TRUEVISION-XFILE
		private static readonly byte[] Signature = { 0x54, 0x52, 0x55, 0x45, 0x56, 0x49, 0x53, 0x49, 0x4F, 0x4E, 0x2D, 0x58, 0x46, 0x49, 0x4C, 0x45 };


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
		/// Gets pixel data in ARGB format.
		/// </summary>
		public byte[] PixelData
		{
			get { return _PixelData; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of TGA.
		/// </summary>
		/// <param name="reader">Reader to create from.</param>
		public TGA( BinaryReader reader )
		{
			if ( reader.BaseStream.Length < FooterLength )
				throw new Exception( "Invalid file" );

			// Seek to the signature
			reader.BaseStream.Seek( -FooterLength + 8, SeekOrigin.End );

			// Read signature
			byte[] signature = reader.ReadBytes( Signature.Length );

			// Check if valid TGA 2 footer
			bool hasFooter = true;

			for ( int i = 0; i < Signature.Length && hasFooter; i++ )
			{
				if ( signature[ i ] != Signature[ i ] )
					hasFooter = false;
			}

			// Move to beginning
			long extensionAreaOffset = 0;
			long developerAreaOffset = 0;

			if ( hasFooter )
			{
				reader.BaseStream.Seek( FooterLength, SeekOrigin.End );

				extensionAreaOffset = reader.ReadUInt32();
				developerAreaOffset = reader.ReadUInt32();
			}

			// Read file
			reader.BaseStream.Seek( 0, SeekOrigin.Begin );

			int idLength = reader.ReadByte();
			int colorMapType = reader.ReadByte();
			int imageType = reader.ReadByte();

			// Color map spec
			int colorMapOffset = reader.ReadUInt16();
			int colorMapEntryCount = reader.ReadUInt16();
			int colorMapEntrySize = reader.ReadByte(); // bpp

			if ( colorMapEntrySize == 15 )
				colorMapEntrySize = 16;

			// Image specification
			int originX = reader.ReadUInt16();
			int originY = reader.ReadUInt16();
			int imageWidth = reader.ReadUInt16();
			int imageHeight = reader.ReadUInt16();
			int pixelDepth = reader.ReadByte();

			if ( pixelDepth == 15 )
				pixelDepth = 16;

			byte imageDescriptor = reader.ReadByte();

			int alphaChannelDepth = imageDescriptor & 0xF;
			int direction = ( imageDescriptor >> 4 ) & 0x3;

			// Image ID
			if ( idLength > 0 )
				reader.BaseStream.Seek( idLength, SeekOrigin.Current );

			// Color map data
			byte[] colorMap = null; // ARGB

			if ( colorMapType == 1 )
			{
				if ( pixelDepth != 8 && pixelDepth != 16 )
					throw new Exception( "Invalid pixel depth for mapped image" );

				int colorMapSize = ( colorMapEntrySize * colorMapEntryCount ) / 8;

				if ( colorMapOffset > 0 )
					reader.BaseStream.Seek( ( colorMapOffset * colorMapEntryCount ) / 8, SeekOrigin.Current );

				switch ( colorMapEntrySize )
				{
					case 16:
					{
						colorMap = new byte[ colorMapEntryCount * 4 ];
						int colorMapIndex = 0;

						for ( int i = 0; i < colorMapEntryCount; i++ )
						{
							ushort color = reader.ReadUInt16();

							colorMap[ colorMapIndex++ ] = (byte) ( ( color & 0x1F ) << 3 ); // b
							colorMap[ colorMapIndex++ ] = (byte) ( ( color & 0x3E0 ) >> 2 ); // g
							colorMap[ colorMapIndex++ ] = (byte) ( ( color & 0x7C00 ) >> 7 ); // r
							colorMap[ colorMapIndex++ ] = (byte) ( ( ( color & 0x8000 ) >> 15 ) * 0xFF ); // a
						}

						break;
					}
					case 24:
					{
						colorMap = new byte[ colorMapEntryCount * 4 ];
						int colorMapIndex = 0;

						for ( int i = 0; i < colorMapEntryCount; i++ )
						{
							colorMap[ colorMapIndex++ ] = reader.ReadByte(); // b
							colorMap[ colorMapIndex++ ] = reader.ReadByte(); // g
							colorMap[ colorMapIndex++ ] = reader.ReadByte(); // r
							colorMap[ colorMapIndex++ ] = 0xFF; // a
						}

						break;
					}
					case 32:
					{
						colorMap = reader.ReadBytes( colorMapEntryCount * 4 );
						break;
					}
					default:
					throw new Exception( "Invalid color map entry size" );
				}
			}
			else
			{
				if ( ( imageType == 3 || imageType == 11 ) && pixelDepth != 8 )
					throw new Exception( "Invalid pixel depth for grayscale image" );

				if ( pixelDepth != 16 && pixelDepth != 24 && pixelDepth != 32 )
					throw new Exception( "Invalid pixel depth for true color image" );
			}

			// Image data
			_Width = imageWidth;
			_Height = imageHeight;
			_PixelData = new byte[ _Width * _Height * 4 ];

			int sourceStride = ( ( _Width * pixelDepth + 31 ) & ~31 ) >> 3;
			int sourcePadding = sourceStride - ( ( _Width * pixelDepth ) >> 3 );

			switch ( imageType )
			{
				case 1: // Color-mapped
				{
					if ( colorMapType != 1 )
						throw new Exception( "Image is color-mapped, but no color map found" );

					ReadColorMapped( reader, colorMap, pixelDepth, direction, sourcePadding );
					break;
				}
				case 2: // True-color
				{
					ReadTrueColor( reader, pixelDepth, alphaChannelDepth > 0, direction, sourcePadding );
					break;
				}
				case 3: // Grayscale
				{
					ReadGrayscale( reader, direction, sourcePadding );
					break;
				}
				case 9: // Run-length encoded, color-mapped
				{
					if ( colorMapType != 1 )
						throw new Exception( "Image is color-mapped, but no color map found" );

					ReadEncodedColorMapped( reader, colorMap, pixelDepth, direction );
					break;
				}
				case 10: // Run-length encoded, true-color
				{
					ReadEncodedTrueColor( reader, pixelDepth, alphaChannelDepth > 0, direction );
					break;
				}
				case 11: // Run-length encoded, grayscale
				{
					ReadEncodedGrayscale( reader, direction );
					break;
				}
				default:
					throw new Exception( "Unknown image type" );
			}
		}
		#endregion

		#region Methods
		private void ReadColorMapped( BinaryReader reader, byte[] colorMap, int pixelDepth, int direction, int padding )
		{
			if ( direction != 2 )
			{
				if ( pixelDepth == 8 )
				{
					for ( int y = 0; y < _Height; y++ )
					{
						for ( int x = 0; x < _Width; x++ )
						{
							int pixelDataIndex = GetPixelDataIndex( y, x, direction );
							int colorMapIndex = reader.ReadByte();

							_PixelData[ pixelDataIndex ] = colorMap[ colorMapIndex ];
							_PixelData[ pixelDataIndex++ ] = colorMap[ colorMapIndex++ ];
							_PixelData[ pixelDataIndex++ ] = colorMap[ colorMapIndex++ ];
							_PixelData[ pixelDataIndex++ ] = colorMap[ colorMapIndex++ ];
						}

						reader.ReadBytes( padding );
					}
				}
				else
				{
					for ( int y = 0; y < _Height; y++ )
					{
						for ( int x = 0; x < _Width; x++ )
						{
							int pixelDataIndex = GetPixelDataIndex( y, x, direction );
							int colorMapIndex = reader.ReadUInt16();

							_PixelData[ pixelDataIndex ] = colorMap[ colorMapIndex ];
							_PixelData[ pixelDataIndex++ ] = colorMap[ colorMapIndex++ ];
							_PixelData[ pixelDataIndex++ ] = colorMap[ colorMapIndex++ ];
							_PixelData[ pixelDataIndex++ ] = colorMap[ colorMapIndex++ ];
						}

						reader.ReadBytes( padding );
					}
				}
			}
			else // Common
			{
				int pixelDataIndex = 0;

				if ( pixelDepth == 8 )
				{
					for ( int y = 0; y < _Height; y++ )
					{
						for ( int x = 0; x < _Width; x++ )
						{
							int colorMapIndex = reader.ReadByte();

							_PixelData[ pixelDataIndex ] = colorMap[ colorMapIndex ];
							_PixelData[ pixelDataIndex++ ] = colorMap[ colorMapIndex++ ];
							_PixelData[ pixelDataIndex++ ] = colorMap[ colorMapIndex++ ];
							_PixelData[ pixelDataIndex++ ] = colorMap[ colorMapIndex++ ];
						}

						reader.ReadBytes( padding );
					}
				}
				else
				{
					for ( int y = 0; y < _Height; y++ )
					{
						for ( int x = 0; x < _Width; x++ )
						{
							int colorMapIndex = reader.ReadUInt16();

							_PixelData[ pixelDataIndex ] = colorMap[ colorMapIndex ];
							_PixelData[ pixelDataIndex++ ] = colorMap[ colorMapIndex++ ];
							_PixelData[ pixelDataIndex++ ] = colorMap[ colorMapIndex++ ];
							_PixelData[ pixelDataIndex++ ] = colorMap[ colorMapIndex++ ];
						}

						reader.ReadBytes( padding );
					}
				}
			}
		}

		private void ReadTrueColor( BinaryReader reader, int pixelDepth, bool hasAlpha, int direction, int padding )
		{
			if ( direction != 2 )
			{
				if ( pixelDepth == 16 )
				{
					if ( hasAlpha )
					{
						for ( int y = 0; y < _Height; y++ )
						{
							for ( int x = 0; x < _Width; x++ )
							{
								int pixelDataIndex = GetPixelDataIndex( y, x, direction );
								ushort color = reader.ReadUInt16();

								_PixelData[ pixelDataIndex++ ] = (byte) ( ( color & 0x1F ) << 3 ); // b
								_PixelData[ pixelDataIndex++ ] = (byte) ( ( color & 0x3E0 ) >> 2 ); // g
								_PixelData[ pixelDataIndex++ ] = (byte) ( ( color & 0x7C00 ) >> 7 ); // r
								_PixelData[ pixelDataIndex++ ] = (byte) ( ( ( color & 0x8000 ) >> 15 ) * 0xFF ); // a
							}

							reader.ReadBytes( padding );
						}
					}
					else
					{
						for ( int y = 0; y < _Height; y++ )
						{
							for ( int x = 0; x < _Width; x++ )
							{
								int pixelDataIndex = GetPixelDataIndex( y, x, direction );
								ushort color = reader.ReadUInt16();

								_PixelData[ pixelDataIndex++ ] = (byte) ( ( color & 0x1F ) << 3 ); // b
								_PixelData[ pixelDataIndex++ ] = (byte) ( ( color & 0x3E0 ) >> 2 ); // g
								_PixelData[ pixelDataIndex++ ] = (byte) ( ( color & 0x7C00 ) >> 7 ); // r
								_PixelData[ pixelDataIndex++ ] = 0xFF; // a
							}

							reader.ReadBytes( padding );
						}
					}
				}
				else if ( pixelDepth == 24 )
				{
					for ( int y = 0; y < _Height; y++ )
					{
						for ( int x = 0; x < _Width; x++ )
						{
							int pixelDataIndex = GetPixelDataIndex( y, x, direction );

							_PixelData[ pixelDataIndex++ ] = reader.ReadByte(); // b
							_PixelData[ pixelDataIndex++ ] = reader.ReadByte(); // g
							_PixelData[ pixelDataIndex++ ] = reader.ReadByte(); // r
							_PixelData[ pixelDataIndex++ ] = 0xFF; // a
						}

						reader.ReadBytes( padding );
					}
				}
				else if ( pixelDepth == 32 )
				{
					if ( hasAlpha )
					{
						_PixelData = reader.ReadBytes( _Width * _Height * 30 );
					}
					else
					{
						for ( int y = 0; y < _Height; y++ )
						{
							for ( int x = 0; x < _Width; x++ )
							{
								int pixelDataIndex = GetPixelDataIndex( y, x, direction );

								_PixelData[ pixelDataIndex++ ] = reader.ReadByte(); // b
								_PixelData[ pixelDataIndex++ ] = reader.ReadByte(); // g
								_PixelData[ pixelDataIndex++ ] = reader.ReadByte(); // r
								_PixelData[ pixelDataIndex ] = reader.ReadByte(); // a
								_PixelData[ pixelDataIndex ] = 0xFF; // a
							}

							reader.ReadBytes( padding );
						}
					}
				}
			}
			else // Common
			{
				int pixelDataIndex = 0;

				if ( pixelDepth == 16 )
				{
					if ( hasAlpha )
					{
						for ( int y = 0; y < _Height; y++ )
						{
							for ( int x = 0; x < _Width; x++ )
							{
								ushort color = reader.ReadUInt16();

								_PixelData[ pixelDataIndex++ ] = (byte) ( ( color & 0x1F ) << 3 ); // b
								_PixelData[ pixelDataIndex++ ] = (byte) ( ( color & 0x3E0 ) >> 2 ); // g
								_PixelData[ pixelDataIndex++ ] = (byte) ( ( color & 0x7C00 ) >> 7 ); // r
								_PixelData[ pixelDataIndex++ ] = (byte) ( ( ( color & 0x8000 ) >> 15 ) * 0xFF ); // a
							}

							reader.ReadBytes( padding );
						}
					}
					else
					{
						for ( int y = 0; y < _Height; y++ )
						{
							for ( int x = 0; x < _Width; x++ )
							{
								ushort color = reader.ReadUInt16();

								_PixelData[ pixelDataIndex++ ] = (byte) ( ( color & 0x1F ) << 3 ); // b
								_PixelData[ pixelDataIndex++ ] = (byte) ( ( color & 0x3E0 ) >> 2 ); // g
								_PixelData[ pixelDataIndex++ ] = (byte) ( ( color & 0x7C00 ) >> 7 ); // r
								_PixelData[ pixelDataIndex++ ] = 0xFF; // a
							}

							reader.ReadBytes( padding );
						}
					}
				}
				else if ( pixelDepth == 24 )
				{
					for ( int y = 0; y < _Height; y++ )
					{
						for ( int x = 0; x < _Width; x++ )
						{
							_PixelData[ pixelDataIndex++ ] = reader.ReadByte(); // b
							_PixelData[ pixelDataIndex++ ] = reader.ReadByte(); // g
							_PixelData[ pixelDataIndex++ ] = reader.ReadByte(); // r
							_PixelData[ pixelDataIndex++ ] = 0xFF; // a
						}

						reader.ReadBytes( padding );
					}
				}
				else if ( pixelDepth == 32 )
				{
					if ( hasAlpha )
					{
						_PixelData = reader.ReadBytes( _Width * _Height * 30 );
					}
					else
					{
						for ( int y = 0; y < _Height; y++ )
						{
							for ( int x = 0; x < _Width; x++ )
							{
								_PixelData[ pixelDataIndex++ ] = reader.ReadByte(); // b
								_PixelData[ pixelDataIndex++ ] = reader.ReadByte(); // g
								_PixelData[ pixelDataIndex++ ] = reader.ReadByte(); // r
								_PixelData[ pixelDataIndex++ ] = reader.ReadByte(); // a
							}

							reader.ReadBytes( padding );
						}
					}
				}
			}
		}

		private void ReadGrayscale( BinaryReader reader, int direction, int padding )
		{
			if ( direction != 2 )
			{
				for ( int y = 0; y < _Height; y++ )
				{
					for ( int x = 0; x < _Width; x++ )
					{
						int pixelDataIndex = GetPixelDataIndex( y, x, direction );
						byte color = reader.ReadByte();

						_PixelData[ pixelDataIndex++ ] = color; // b
						_PixelData[ pixelDataIndex++ ] = color; // g
						_PixelData[ pixelDataIndex++ ] = color; // r
						_PixelData[ pixelDataIndex++ ] = 0xFF; // a
					}

					reader.ReadBytes( padding );
				}
			}
			else
			{
				int pixelDataIndex = 0;

				for ( int y = 0; y < _Height; y++ )
				{
					for ( int x = 0; x < _Width; x++ )
					{
						byte color = reader.ReadByte();

						_PixelData[ pixelDataIndex++ ] = color; // b
						_PixelData[ pixelDataIndex++ ] = color; // g
						_PixelData[ pixelDataIndex++ ] = color; // r
						_PixelData[ pixelDataIndex++ ] = 0xFF; // a
					}

					reader.ReadBytes( padding );
				}
			}
		}

		private void ReadEncodedColorMapped( BinaryReader reader, byte[] colorMap, int pixelDepth, int direction )
		{
			int size = _Width * _Height;
			int position = 0;
			int x = 0;
			int y = 0;

			while ( position < size )
			{
				byte repetitionCount = reader.ReadByte();
				int pixelCount = ( repetitionCount & 0x7F ) + 1;

				if ( ( repetitionCount & 0x80 ) > 0 )
				{
					// Run-length
					int colorMapIndex = 0;

					if ( pixelDepth == 8 )
						colorMapIndex = reader.ReadByte();
					else
						colorMapIndex = reader.ReadUInt16();

					for ( int i = 0; i < pixelCount; i++ )
					{
						int pixelDataIndex = GetPixelDataIndex( x, y, direction );

						_PixelData[ pixelDataIndex++ ] = colorMap[ colorMapIndex ];
						_PixelData[ pixelDataIndex++ ] = colorMap[ colorMapIndex + 1 ];
						_PixelData[ pixelDataIndex++ ] = colorMap[ colorMapIndex + 2 ];
						_PixelData[ pixelDataIndex++ ] = colorMap[ colorMapIndex + 3 ];

						// Increase x
						x++;

						if ( x >= _Width )
						{
							x = 0;
							y++;
						}
					}
				}
				else
				{
					// Raw
					for ( int i = 0; i < pixelCount; i++ )
					{
						int pixelDataIndex = GetPixelDataIndex( x, y, direction );
						int colorMapIndex = 0;

						if ( pixelDepth == 8 )
							colorMapIndex = reader.ReadByte();
						else
							colorMapIndex = reader.ReadUInt16();

						_PixelData[ pixelDataIndex++ ] = colorMap[ colorMapIndex ];
						_PixelData[ pixelDataIndex++ ] = colorMap[ colorMapIndex + 1 ];
						_PixelData[ pixelDataIndex++ ] = colorMap[ colorMapIndex + 2 ];
						_PixelData[ pixelDataIndex++ ] = colorMap[ colorMapIndex + 3 ];

						// Increase x
						x++;

						if ( x >= _Width )
						{
							x = 0;
							y++;
						}
					}
				}

				position += pixelCount;
			}
		}

		private void ReadEncodedTrueColor( BinaryReader reader, int pixelDepth, bool hasAlpha, int direction )
		{
			int size = _Width * _Height;
			int position = 0;
			int x = 0;
			int y = 0;
			byte b = 0, g = 0, r = 0, a = 0xFF;

			while ( position < size )
			{
				byte repetitionCount = reader.ReadByte();
				int pixelCount = ( repetitionCount & 0x7F ) + 1;

				if ( ( repetitionCount & 0x80 ) > 0 )
				{
					// Run-length
					if ( pixelDepth == 16 )
					{
						ushort color = reader.ReadUInt16(); 
						
						b = (byte) ( ( color & 0x1F ) << 3 ); // b
						g = (byte) ( ( color & 0x3E0 ) >> 2 ); // g
						r = (byte) ( ( color & 0x7C00 ) >> 7 ); // r

						if ( hasAlpha )
							a = (byte) ( ( ( color & 0x8000 ) >> 15 ) * 0xFF ); // a
					}
					else if ( pixelDepth == 24 )
					{
						b = reader.ReadByte();
						g = reader.ReadByte();
						r = reader.ReadByte();
					}
					else
					{
						b = reader.ReadByte();
						g = reader.ReadByte();
						r = reader.ReadByte();

						if ( hasAlpha )
							a = reader.ReadByte();
						else
							reader.ReadByte();
					}

					for ( int i = 0; i < pixelCount; i++ )
					{
						int pixelDataIndex = GetPixelDataIndex( x, y, direction );

						_PixelData[ pixelDataIndex++ ] = b;
						_PixelData[ pixelDataIndex++ ] = g;
						_PixelData[ pixelDataIndex++ ] = r;
						_PixelData[ pixelDataIndex++ ] = a;

						// Increase x
						x++;

						if ( x >= _Width )
						{
							x = 0;
							y++;
						}
					}
				}
				else
				{
					// Raw
					for ( int i = 0; i < pixelCount; i++ )
					{
						int pixelDataIndex = GetPixelDataIndex( x, y, direction );

						if ( pixelDepth == 16 )
						{
							ushort color = reader.ReadUInt16();

							b = (byte) ( ( color & 0x1F ) << 3 ); // b
							g = (byte) ( ( color & 0x3E0 ) >> 2 ); // g
							r = (byte) ( ( color & 0x7C00 ) >> 7 ); // r

							if ( hasAlpha )
								a = (byte) ( ( ( color & 0x8000 ) >> 15 ) * 0xFF ); // a
						}
						else if ( pixelDepth == 24 )
						{
							b = reader.ReadByte();
							g = reader.ReadByte();
							r = reader.ReadByte();
						}
						else
						{
							b = reader.ReadByte();
							g = reader.ReadByte();
							r = reader.ReadByte();

							if ( hasAlpha )
								a = reader.ReadByte();
							else
								reader.ReadByte();
						}

						_PixelData[ pixelDataIndex++ ] = b;
						_PixelData[ pixelDataIndex++ ] = g;
						_PixelData[ pixelDataIndex++ ] = r;
						_PixelData[ pixelDataIndex++ ] = a;

						// Increase x
						x++;

						if ( x >= _Width )
						{
							x = 0;
							y++;
						}
					}
				}

				position += pixelCount;
			}
		}

		private void ReadEncodedGrayscale( BinaryReader reader, int direction )
		{
			int size = _Width * _Height;
			int position = 0;
			int x = 0;
			int y = 0;

			while ( position < size )
			{
				byte repetitionCount = reader.ReadByte();
				int pixelCount = ( repetitionCount & 0x7F ) + 1;

				if ( ( repetitionCount & 0x80 ) > 0 )
				{
					// Run-length
					byte color = reader.ReadByte();

					for ( int i = 0; i < pixelCount; i++ )
					{
						int pixelDataIndex = GetPixelDataIndex( x, y, direction );

						_PixelData[ pixelDataIndex++ ] = color;
						_PixelData[ pixelDataIndex++ ] = color;
						_PixelData[ pixelDataIndex++ ] = color;
						_PixelData[ pixelDataIndex++ ] = 0xFF;

						// Increase x
						x++;

						if ( x >= _Width )
						{
							x = 0;
							y++;
						}
					}
				}
				else
				{
					// Raw
					for ( int i = 0; i < pixelCount; i++ )
					{
						int pixelDataIndex = GetPixelDataIndex( x, y, direction );
						byte color = reader.ReadByte();

						_PixelData[ pixelDataIndex++ ] = color;
						_PixelData[ pixelDataIndex++ ] = color;
						_PixelData[ pixelDataIndex++ ] = color;
						_PixelData[ pixelDataIndex++ ] = 0xFF;

						// Increase x
						x++;

						if ( x >= _Width )
						{
							x = 0;
							y++;
						}
					}
				}

				position += pixelCount;
			}
		}

		private int GetPixelDataIndex( int y, int x, int direction )
		{
			switch ( direction )
			{
				case 0: return ( _Height - y ) * _Width * 4 + x * 4; // bottom left (reverse y)
				case 1: return ( _Height - y ) * _Width * 4 + ( _Width - x ) * 4; // bottom right (reverse x,y)
				case 2: return y * _Width * 4 + x * 4; // top left (normal)
				case 3: return y * _Width * 4 + ( _Width - x ) * 4; // top right (reverse x)
			}

			throw new Exception( "Invalid direction" );
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
		/// <returns>TGA image.</returns>
		public static TGA FromFile( string filePath )
		{
			using ( FileStream stream = File.Open( filePath, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			{
				return FromStream( stream );
			}
		}

		/// <summary>
		/// Reads TGA from stream.
		/// </summary>
		/// <param name="data">Memory to read from.</param>
		/// <returns>TGA image.</returns>
		public static TGA FromMemory( byte[] data )
		{
			using ( MemoryStream stream = new MemoryStream( data ) )
			{
				return FromStream( stream );
			}
		}

		/// <summary>
		/// Reads TGA from stream.
		/// </summary>
		/// <param name="stream">Stream to read from.</param>
		/// <returns>TGA image.</returns>
		public static TGA FromStream( Stream stream )
		{
			using ( BinaryReader reader = new BinaryReader( stream ) )
			{
				return new TGA( reader );
			}
		}
		#endregion
	}
}
