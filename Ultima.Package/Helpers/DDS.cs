using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using MipMaps = System.Collections.Generic.List<byte[]>;
using VolumeMap = System.Collections.Generic.List<byte[]>;

namespace Ultima.Package
{
	/// <summary>
	/// Describes pixel format.
	/// </summary>
	public enum DDSPixelFormat
	{
		// Uncompressed
		A8B8G8R8,
		G16R16,
		A2B10G10R10,
		A1R5G5B5,
		R5G6B5,
		A8,
		A8R8G8B8,
		X8R8G8B8,
		X8B8G8R8,
		A2R10G10B10,
		R8G8B8,
		X1R5G5B5,
		A4R4G4B4,
		X4R4G4B4,
		A8R3G3B2,
		A8L8,
		L16,
		L8,
		A4L4,

		// Compressed
		DXT1,
		DXT2,
		DXT3,
		DXT4,
		DXT5,
		R8G8B8G8,
		G8R8G8B8,
		UYVY,
		YUY2,
		CxV8U8,

		// Other
		Unknown,
		DX10,
	}

	/// <summary>
	/// Describes cube map surface side.
	/// </summary>
	public enum DDSCubeFace : uint
	{
		PositiveX		= 0x0400,
		NegativeX		= 0x0800,
		PositiveY		= 0x1000,
		NegativeY		= 0x2000,
		PositiveZ		= 0x4000,
		NegativeZ		= 0x8000
	}

	/// <summary>
	/// Describes DDS file.
	/// </summary>
	public class DDS
	{
		#region Properties
		private DDSPixelFormat _Format;

		/// <summary>
		/// Gets format used in DDS.
		/// </summary>
		public DDSPixelFormat Format
		{
			get { return _Format; }
		}

		private int _Width;

		/// <summary>
		/// Gets texture width.
		/// </summary>
		public int Width
		{
			get { return _Width; }
		}

		private int _Height;

		/// <summary>
		/// Gets texture height.
		/// </summary>
		public int Height
		{
			get { return _Height; }
		}

		private bool _IsMipMap;

		/// <summary>
		/// Determines whether DDS contains mip map.
		/// </summary>
		public bool IsMipMap
		{
			get { return _IsMipMap; }
		}

		private int _MipMapLevels;

		/// <summary>
		/// Gets mip map level count.
		/// </summary>
		public int MipMapLevels
		{
			get { return _MipMapLevels; }
		}

		private bool _IsCubeMap;

		/// <summary>
		/// Determines whether DDS contains cube map.
		/// </summary>
		public bool IsCubeMap
		{
			get { return _IsCubeMap; }
		}

		private bool _IsVolumeMap;

		/// <summary>
		/// Determines whether DDS contains volume map.
		/// </summary>
		public bool IsVolumeMap
		{
			get { return _IsVolumeMap; }
		}

		private int _VolumeDepth;

		/// <summary>
		/// Gets volume depth.
		/// </summary>
		public int VolumeDepth
		{
			get { return _VolumeDepth; }
		}

		// Just a texture
		private MipMaps _Texture;

		// Cube map textures
		private MipMaps _PositiveX;
		private MipMaps _NegativeX;
		private MipMaps _PositiveY;
		private MipMaps _NegativeY;
		private MipMaps _PositiveZ;
		private MipMaps _NegativeZ;

		// Volume textures
		private List<VolumeMap> _VolumeMaps;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of DDS.
		/// </summary>
		/// <param name="reader">Reader to construct from.</param>
		public DDS( BinaryReader reader )
		{
			// Magic number 'DDS '
			if ( reader.ReadUInt32() != 0x20534444 )
				throw new DDSException( "Invalid DDS magic number" );

			if ( reader.ReadUInt32() != 124 )
				throw new DDSException( "Invalid DDS_HEADER size" );

			uint flags = reader.ReadUInt32();

			_Height = reader.ReadInt32();
			_Width = reader.ReadInt32();

			reader.ReadUInt32();

			_VolumeDepth = reader.ReadInt32();
			_MipMapLevels = reader.ReadInt32();

			// Skip reserved
			reader.BaseStream.Seek( sizeof( uint ) * 11, SeekOrigin.Current );

			// Pixel format
			_Format = ReadPixelFormat( reader );

			uint caps = reader.ReadUInt32();

			if ( ( caps & 0x400000 ) > 0 )
				_IsMipMap = true;

			uint caps2 = reader.ReadUInt32();

			if ( ( caps2 & 0x200 ) > 0 )
				_IsCubeMap = true;

			if ( ( caps2 & 0x200000 ) > 0 )
				_IsVolumeMap = true;

			// Crap
			reader.ReadUInt32();
			reader.ReadUInt32();
			reader.ReadUInt32();

			// Extended DX10
			uint dxgiFormat = 0;
			uint dimension = 0;
			uint cubeFlags = 0;
			uint arraySize = 0;

			if ( _Format == DDSPixelFormat.DX10 )
			{
				dxgiFormat = reader.ReadUInt32();
				dimension = reader.ReadUInt32();
				cubeFlags = reader.ReadUInt32();
				arraySize = reader.ReadUInt32();

				reader.ReadUInt32(); // crap

				throw new NotImplementedException();
			}

			if ( _IsVolumeMap )
			{
				_VolumeMaps = new List<VolumeMap>();

				if ( _IsMipMap )
				{
					int width = _Width;
					int height = _Height;
					int depth = _VolumeDepth;

					for ( int i = 0; i < _MipMapLevels; i++ )
					{
						_VolumeMaps.Add( ReadVolumeMap( reader, width, height, depth ) );

						width = Math.Max( 1, width / 2 );
						height = Math.Max( 1, height / 2 );
						depth = Math.Max( 1, depth / 2 );
					}
				}
				else
					_VolumeMaps.Add( ReadVolumeMap( reader, _Width, _Height, _VolumeDepth ) );
			}
			else if ( _IsCubeMap )
			{
				if ( _IsMipMap )
				{
					if ( ( caps2 & (uint) DDSCubeFace.PositiveX ) > 0 )
						_PositiveX = ReadMipMaps( reader, _MipMapLevels );
					if ( ( caps2 & (uint) DDSCubeFace.NegativeX ) > 0 )
						_NegativeX = ReadMipMaps( reader, _MipMapLevels );
					if ( ( caps2 & (uint) DDSCubeFace.PositiveY ) > 0 )
						_PositiveY = ReadMipMaps( reader, _MipMapLevels );
					if ( ( caps2 & (uint) DDSCubeFace.NegativeY ) > 0 )
						_NegativeY = ReadMipMaps( reader, _MipMapLevels );
					if ( ( caps2 & (uint) DDSCubeFace.PositiveZ ) > 0 )
						_PositiveZ = ReadMipMaps( reader, _MipMapLevels );
					if ( ( caps2 & (uint) DDSCubeFace.NegativeZ ) > 0 )
						_NegativeZ = ReadMipMaps( reader, _MipMapLevels );
				}
				else
				{
					if ( ( caps2 & (uint) DDSCubeFace.PositiveX ) > 0 )
						_PositiveX = ReadTexture( reader );
					if ( ( caps2 & (uint) DDSCubeFace.NegativeX ) > 0 )
						_NegativeX = ReadTexture( reader );
					if ( ( caps2 & (uint) DDSCubeFace.PositiveY ) > 0 )
						_PositiveY = ReadTexture( reader );
					if ( ( caps2 & (uint) DDSCubeFace.NegativeY ) > 0 )
						_NegativeY = ReadTexture( reader );
					if ( ( caps2 & (uint) DDSCubeFace.PositiveZ ) > 0 )
						_PositiveZ = ReadTexture( reader );
					if ( ( caps2 & (uint) DDSCubeFace.NegativeZ ) > 0 )
						_NegativeZ = ReadTexture( reader );
				}
			}
			else
			{
				if ( _IsMipMap )
					_Texture = ReadMipMaps( reader, _MipMapLevels );
				else
					_Texture = ReadTexture( reader );
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Reads DDS from file.
		/// </summary>
		/// <param name="filePath">File path to read from.</param>
		/// <returns>DDS image.</returns>
		public static DDS FromFile( string filePath )
		{
			using ( FileStream stream = File.Open( filePath, FileMode.Open, FileAccess.Read ) )
			{
				return FromStream( stream );
			}
		}

		/// <summary>
		/// Reads DDS from stream.
		/// </summary>
		/// <param name="data">Memory to read from.</param>
		/// <returns>DDS image.</returns>
		public static DDS FromMemory( byte[] data )
		{
			using ( MemoryStream stream = new MemoryStream( data ) )
			{
				return FromStream( stream );
			}
		}

		/// <summary>
		/// Reads DDS from stream.
		/// </summary>
		/// <param name="stream">Stream to read from.</param>
		/// <returns>DDS image.</returns>
		public static DDS FromStream( Stream stream )
		{
			using ( BinaryReader reader = new BinaryReader( stream ) )
			{
				return new DDS( reader );
			}
		}

		private MipMaps ReadTexture( BinaryReader reader )
		{
			MipMaps mipmaps = new MipMaps( 1 );
			mipmaps.Add( Read_All( reader, _Width, _Height ) );
			return mipmaps;
		}

		private MipMaps ReadMipMaps( BinaryReader reader, int mipMapCount )
		{
			MipMaps mipmaps = new MipMaps();
			int width = _Width;
			int height = _Height;

			for ( uint i = 0; i < mipMapCount; i++ )
			{
				mipmaps.Add( Read_All( reader, width, height ) );

				width = Math.Max( 1, width / 2 );
				height = Math.Max( 1, height / 2 );
			}

			return mipmaps;
		}

		private VolumeMap ReadVolumeMap( BinaryReader reader, int width, int height, int depth )
		{
			VolumeMap volumeMap = new VolumeMap( depth );

			for ( int i = 0; i < depth; i++ )
				volumeMap.Add( Read_All( reader, width, height ) );

			return volumeMap;
		}

		private byte[] Read_All( BinaryReader reader, int width, int height )
		{
			switch ( _Format )
			{
				case DDSPixelFormat.A8B8G8R8: return Read_A8B8G8R8( reader, width, height );
				case DDSPixelFormat.X8B8G8R8: return Read_X8B8G8R8( reader, width, height );
				case DDSPixelFormat.A8R8G8B8: return Read_A8R8G8B8( reader, width, height );
				case DDSPixelFormat.X8R8G8B8: return Read_X8R8G8B8( reader, width, height );
				case DDSPixelFormat.R8G8B8: return Read_R8G8B8( reader, width, height );
				case DDSPixelFormat.A1R5G5B5: return Read_A1R5G5B5( reader, width, height );
				case DDSPixelFormat.X1R5G5B5: return Read_X1R5G5B5( reader, width, height );
				case DDSPixelFormat.R5G6B5: return Read_R5G6B5( reader, width, height );
				case DDSPixelFormat.A4R4G4B4: return Read_A4R4G4B4( reader, width, height );
				case DDSPixelFormat.X4R4G4B4: return Read_X4R4G4B4( reader, width, height );
				case DDSPixelFormat.DXT1: return Read_DXT1( reader, width, height );
				case DDSPixelFormat.DXT2:
				case DDSPixelFormat.DXT3: return Read_DXT3( reader, width, height );
				case DDSPixelFormat.DXT4:
				case DDSPixelFormat.DXT5: return Read_DXT5( reader, width, height );
				default: throw new DDSException( "Pixel format {0} not supported", _Format );
			}
		}

		private byte[] Read_A8B8G8R8( BinaryReader reader, int width, int height )
		{
			int toLineWidth = width * 4;
			int sourceLineWidth = ( width * 32 + 7 ) / 8;

			// Decode to ARGB (GDI+) or BGRA (WPF)
			byte[] buffer = new byte[ height * toLineWidth ];
			byte[] sourceBuffer = new byte[ sourceLineWidth ];

			int toIndex = 0;
			int toLineEnd = 0;
			int fromIndex = 0;

			for ( int y = 0; y < height; y++ )
			{
				reader.Read( sourceBuffer, 0, sourceLineWidth );

				toLineEnd += toLineWidth;
				fromIndex = 0;

				for ( ; toIndex < toLineEnd; fromIndex += 4 )
				{
					buffer[ toIndex++ ] = sourceBuffer[ fromIndex + 2 ];
					buffer[ toIndex++ ] = sourceBuffer[ fromIndex + 1 ];
					buffer[ toIndex++ ] = sourceBuffer[ fromIndex ];
					buffer[ toIndex++ ] = sourceBuffer[ fromIndex + 3 ];
				}
			}

			return buffer;
		}

		private byte[] Read_X8B8G8R8( BinaryReader reader, int width, int height )
		{
			int toLineWidth = width * 4;
			int sourceLineWidth = ( width * 32 + 7 ) / 8;

			// Decode to ARGB (GDI+) or BGRA (WPF)
			byte[] buffer = new byte[ height * toLineWidth ];
			byte[] sourceBuffer = new byte[ sourceLineWidth ];

			int toIndex = 0;
			int toLineEnd = 0;
			int fromIndex = 0;

			for ( int y = 0; y < height; y++ )
			{
				reader.Read( sourceBuffer, 0, sourceLineWidth );

				toLineEnd += toLineWidth;
				fromIndex = 0;

				for ( ; toIndex < toLineEnd; fromIndex += 4 )
				{
					buffer[ toIndex++ ] = sourceBuffer[ fromIndex + 2 ];
					buffer[ toIndex++ ] = sourceBuffer[ fromIndex + 1 ];
					buffer[ toIndex++ ] = sourceBuffer[ fromIndex ];
					buffer[ toIndex++ ] = 0xFF;
				}
			}

			return buffer;
		}

		private byte[] Read_A8R8G8B8( BinaryReader reader, int width, int height )
		{
			int toLineWidth = width * 4;
			int sourceLineWidth = ( width * 32 + 7 ) / 8;

			// Decode to ARGB (GDI+) or BGRA (WPF)
			byte[] buffer = new byte[ height * toLineWidth ];
			byte[] sourceBuffer = new byte[ sourceLineWidth ];

			int toIndex = 0;
			int toLineEnd = 0;
			int fromIndex = 0;

			for ( int y = 0; y < height; y++ )
			{
				reader.Read( sourceBuffer, 0, sourceLineWidth );

				toLineEnd += toLineWidth;
				fromIndex = 0;

				for ( ; toIndex < toLineEnd; )
				{
					buffer[ toIndex++ ] = sourceBuffer[ fromIndex++ ];
					buffer[ toIndex++ ] = sourceBuffer[ fromIndex++ ];
					buffer[ toIndex++ ] = sourceBuffer[ fromIndex++ ];
					buffer[ toIndex++ ] = sourceBuffer[ fromIndex++ ];
				}
			}

			return buffer;
		}

		private byte[] Read_X8R8G8B8( BinaryReader reader, int width, int height )
		{
			int toLineWidth = width * 4;
			int sourceLineWidth = ( width * 32 + 7 ) / 8;

			// Decode to ARGB (GDI+) or BGRA (WPF)
			byte[] buffer = new byte[ height * toLineWidth ];
			byte[] sourceBuffer = new byte[ sourceLineWidth ];

			int toIndex = 0;
			int toLineEnd = 0;
			int fromIndex = 0;

			for ( int y = 0; y < height; y++ )
			{
				reader.Read( sourceBuffer, 0, sourceLineWidth );

				toLineEnd += toLineWidth;
				fromIndex = 0;

				for ( ; toIndex < toLineEnd; fromIndex++ )
				{
					buffer[ toIndex++ ] = sourceBuffer[ fromIndex++ ];
					buffer[ toIndex++ ] = sourceBuffer[ fromIndex++ ];
					buffer[ toIndex++ ] = sourceBuffer[ fromIndex++ ];
					buffer[ toIndex++ ] = 0xFF;
				}
			}

			return buffer;
		}

		private byte[] Read_R8G8B8( BinaryReader reader, int width, int height )
		{
			int toLineWidth = width * 4;
			int sourceLineWidth = ( width * 24 + 7 ) / 8;

			// Decode to ARGB (GDI+) or BGRA (WPF)
			byte[] buffer = new byte[ height * toLineWidth ];
			byte[] sourceBuffer = new byte[ sourceLineWidth ];

			int toIndex = 0;
			int toLineEnd = 0;
			int fromIndex = 0;

			for ( int y = 0; y < height; y++ )
			{
				reader.Read( sourceBuffer, 0, sourceLineWidth );

				toLineEnd += toLineWidth;
				fromIndex = 0;

				for ( ; toIndex < toLineEnd; )
				{
					buffer[ toIndex++ ] = sourceBuffer[ fromIndex++ ];
					buffer[ toIndex++ ] = sourceBuffer[ fromIndex++ ];
					buffer[ toIndex++ ] = sourceBuffer[ fromIndex++ ];
					buffer[ toIndex++ ] = 0xFF;
				}
			}

			return buffer;
		}

		private byte[] Read_A1R5G5B5( BinaryReader reader, int width, int height )
		{
			int toLineWidth = width * 4;
			int sourceLineWidth = ( width * 16 + 7 ) / 8;

			// Decode to ARGB (GDI+) or BGRA (WPF)
			byte[] buffer = new byte[ height * toLineWidth ];
			byte[] sourceBuffer = new byte[ sourceLineWidth ];

			int toIndex = 0;
			int toLineEnd = 0;
			int fromIndex = 0;
			short pixel;

			for ( int y = 0; y < height; y++ )
			{
				reader.Read( sourceBuffer, 0, sourceLineWidth );

				toLineEnd += toLineWidth;
				fromIndex = 0;

				for ( ; toIndex < toLineEnd; )
				{
					pixel = (short) ( ( sourceBuffer[ fromIndex++ ] ) | ( sourceBuffer[ fromIndex++ ] << 8 ) );

					buffer[ toIndex++ ] = (byte) ( ( pixel & 0x1F ) << 3 );
					buffer[ toIndex++ ] = (byte) ( ( pixel & 0x3E0 ) >> 2 );
					buffer[ toIndex++ ] = (byte) ( ( pixel & 0x7C00 ) >> 7 );
					buffer[ toIndex++ ] = (byte) ( ( ( pixel & 0x8000 ) >> 15 ) == 1 ? 0xFF : 0x00 );
				}
			}

			return buffer;
		}

		private byte[] Read_X1R5G5B5( BinaryReader reader, int width, int height )
		{
			int toLineWidth = width * 4;
			int sourceLineWidth = ( width * 16 + 7 ) / 8;

			// Decode to ARGB (GDI+) or BGRA (WPF)
			byte[] buffer = new byte[ height * toLineWidth ];
			byte[] sourceBuffer = new byte[ sourceLineWidth ];

			int toIndex = 0;
			int toLineEnd = 0;
			int fromIndex = 0;
			short pixel;

			for ( int y = 0; y < height; y++ )
			{
				reader.Read( sourceBuffer, 0, sourceLineWidth );

				toLineEnd += toLineWidth;
				fromIndex = 0;

				for ( ; toIndex < toLineEnd; )
				{
					pixel = (short) ( ( sourceBuffer[ fromIndex++ ] ) | ( sourceBuffer[ fromIndex++ ] << 8 ) );

					buffer[ toIndex++ ] = (byte) ( ( pixel & 0x1F ) << 3 );
					buffer[ toIndex++ ] = (byte) ( ( pixel & 0x3E0 ) >> 2 );
					buffer[ toIndex++ ] = (byte) ( ( pixel & 0x7C00 ) >> 7 );
					buffer[ toIndex++ ] = 0xFF;
				}
			}

			return buffer;
		}

		private byte[] Read_R5G6B5( BinaryReader reader, int width, int height )
		{
			int toLineWidth = width * 4;
			int sourceLineWidth = ( width * 16 + 7 ) / 8;

			// Decode to ARGB (GDI+) or BGRA (WPF)
			byte[] buffer = new byte[ height * toLineWidth ];
			byte[] sourceBuffer = new byte[ sourceLineWidth ];

			int toIndex = 0;
			int toLineEnd = 0;
			int fromIndex = 0;
			short pixel;

			for ( int y = 0; y < height; y++ )
			{
				reader.Read( sourceBuffer, 0, sourceLineWidth );

				toLineEnd += toLineWidth;
				fromIndex = 0;

				for ( ; toIndex < toLineEnd; )
				{
					pixel = (short) ( ( sourceBuffer[ fromIndex++ ] ) | ( sourceBuffer[ fromIndex++ ] << 8 ) );

					buffer[ toIndex++ ] = (byte) ( ( pixel & 0x1F ) << 3 );
					buffer[ toIndex++ ] = (byte) ( ( pixel & 0x7E0 ) >> 3 );
					buffer[ toIndex++ ] = (byte) ( ( pixel & 0xF800 ) >> 8 );
					buffer[ toIndex++ ] = 0xFF;
				}
			}

			return buffer;
		}

		private byte[] Read_A4R4G4B4( BinaryReader reader, int width, int height )
		{
			int toLineWidth = width * 4;
			int sourceLineWidth = ( width * 16 + 7 ) / 8;

			// Decode to ARGB (GDI+) or BGRA (WPF)
			byte[] buffer = new byte[ height * toLineWidth ];
			byte[] sourceBuffer = new byte[ sourceLineWidth ];

			int toIndex = 0;
			int toLineEnd = 0;
			int fromIndex = 0;
			short pixel;

			for ( int y = 0; y < height; y++ )
			{
				reader.Read( sourceBuffer, 0, sourceLineWidth );

				toLineEnd += toLineWidth;
				fromIndex = 0;

				for ( ; toIndex < toLineEnd; )
				{
					pixel = (short) ( ( sourceBuffer[ fromIndex++ ] ) | ( sourceBuffer[ fromIndex++ ] << 8 ) );

					buffer[ toIndex++ ] = (byte) ( ( pixel & 0xF ) << 3 );
					buffer[ toIndex++ ] = (byte) ( ( pixel & 0xF0 ) >> 0 );
					buffer[ toIndex++ ] = (byte) ( ( pixel & 0xF00 ) >> 4 );
					buffer[ toIndex++ ] = (byte) ( ( pixel & 0xF000 ) >> 8 );
				}
			}

			return buffer;
		}

		private byte[] Read_X4R4G4B4( BinaryReader reader, int width, int height )
		{
			int toLineWidth = width * 4;
			int sourceLineWidth = ( width * 16 + 7 ) / 8;

			// Decode to ARGB (GDI+) or BGRA (WPF)
			byte[] buffer = new byte[ height * toLineWidth ];
			byte[] sourceBuffer = new byte[ sourceLineWidth ];

			int toIndex = 0;
			int toLineEnd = 0;
			int fromIndex = 0;
			short pixel;

			for ( int y = 0; y < height; y++ )
			{
				reader.Read( sourceBuffer, 0, sourceLineWidth );

				toLineEnd += toLineWidth;
				fromIndex = 0;

				for ( ; toIndex < toLineEnd; )
				{
					pixel = (short) ( ( sourceBuffer[ fromIndex++ ] ) | ( sourceBuffer[ fromIndex++ ] << 8 ) );

					buffer[ toIndex++ ] = (byte) ( ( pixel & 0xF ) << 3 );
					buffer[ toIndex++ ] = (byte) ( ( pixel & 0xF0 ) >> 0 );
					buffer[ toIndex++ ] = (byte) ( ( pixel & 0xF00 ) >> 4 );
					buffer[ toIndex++ ] = 0xFF;
				}
			}

			return buffer;
		}

		private byte[] Read_DXT1( BinaryReader reader, int width, int height )
		{
			int toLineWidth = width * 4;
			int toBlockLine = toLineWidth * 4;
			int sourceLineBlockCount = Math.Max( 1, ( ( width + 3 ) / 4 ) );
			int sourceLineWidth = sourceLineBlockCount * 8;

			// Decode to ARGB (GDI+) or BGRA (WPF)
			byte[] buffer = new byte[ height * toLineWidth ];
			byte[] sourceBuffer = new byte[ sourceLineWidth ];
			byte[] pixels = new byte[ 4 * 4 ];

			// Always 0xFF
			pixels[ 3 ] = 0xFF;
			pixels[ 7 ] = 0xFF;
			pixels[ 11 ] = 0xFF;

			int toIndex, toIndexX, x;
			int toBlockY = 0, toBlockX;
			int maxBlockY, maxBlockX;
			int fromIndex, colorIndex;
			ushort color0, color1;
			byte texel;

			for ( int y = 0; y < height; y += 4 )
			{
				reader.Read( sourceBuffer, 0, sourceLineWidth );
				fromIndex = 0;
				toBlockX = 0;
				x = 0;

				for ( int block = 0; block < sourceLineBlockCount; block++ )
				{
					// 2 extremes
					color0 = (ushort) ( ( sourceBuffer[ fromIndex++ ] ) | ( sourceBuffer[ fromIndex++ ] << 8 ) );
					color1 = (ushort) ( ( sourceBuffer[ fromIndex++ ] ) | ( sourceBuffer[ fromIndex++ ] << 8 ) );

					pixels[ 0 ] = (byte) ( ( color0 & 0x1F ) << 3 );
					pixels[ 1 ] = (byte) ( ( color0 & 0x7E0 ) >> 3 );
					pixels[ 2 ] = (byte) ( ( color0 & 0xF800 ) >> 8 );
					pixels[ 4 ] = (byte) ( ( color1 & 0x1F ) << 3 );
					pixels[ 5 ] = (byte) ( ( color1 & 0x7E0 ) >> 3 );
					pixels[ 6 ] = (byte) ( ( color1 & 0xF800 ) >> 8 );

					// Encoding
					if ( color0 > color1 )
					{
						pixels[ 8 ] = (byte) ( ( ( pixels[ 0 ] << 1 ) + pixels[ 4 ] + 1 ) / 3 );
						pixels[ 9 ] = (byte) ( ( ( pixels[ 1 ] << 1 ) + pixels[ 5 ] + 1 ) / 3 );
						pixels[ 10 ] = (byte) ( ( ( pixels[ 2 ] << 1 ) + pixels[ 6 ] + 1 ) / 3 );
						pixels[ 12 ] = (byte) ( ( pixels[ 0 ] + ( pixels[ 4 ] << 1 ) + 1 ) / 3 );
						pixels[ 13 ] = (byte) ( ( pixels[ 1 ] + ( pixels[ 5 ] << 1 ) + 1 ) / 3 );
						pixels[ 14 ] = (byte) ( ( pixels[ 2 ] + ( pixels[ 6 ] << 1 ) + 1 ) / 3 );
						pixels[ 15 ] = 0xFF;
					}
					else
					{
						pixels[ 8 ] = (byte) ( ( pixels[ 0 ] + pixels[ 4 ] ) >> 1 );
						pixels[ 9 ] = (byte) ( ( pixels[ 1 ] + pixels[ 5 ] ) >> 1 );
						pixels[ 10 ] = (byte) ( ( pixels[ 2 ] + pixels[ 6 ] ) >> 1 );
						pixels[ 12 ] = 0x00;
						pixels[ 13 ] = 0x00;
						pixels[ 14 ] = 0x00;
						pixels[ 15 ] = 0x00;
					}

					// 4 texels
					toIndex = toBlockY + toBlockX;
					maxBlockY = Math.Min( height - y, 4 );
					maxBlockX = Math.Min( width - x, 4 );

					for ( int blockY = 0; blockY < maxBlockY; blockY++ )
					{
						texel = sourceBuffer[ fromIndex++ ];
						toIndexX = toIndex;

						for ( int blockX = 0; blockX < maxBlockX; blockX++ )
						{
							colorIndex = ( texel & 0x3 ) << 2;
							buffer[ toIndexX++ ] = pixels[ colorIndex ];
							buffer[ toIndexX++ ] = pixels[ colorIndex + 1 ];
							buffer[ toIndexX++ ] = pixels[ colorIndex + 2 ];
							buffer[ toIndexX++ ] = pixels[ colorIndex + 3 ];
							texel >>= 2;
						}

						toIndex += toLineWidth;
					}

					toBlockX += 16; // 4 pixels * 4 bytes
					x += 4;
				}

				toBlockY += toBlockLine;
			}

			return buffer;
		}

		private byte[] Read_DXT3( BinaryReader reader, int width, int height )
		{
			int toLineWidth = width * 4;
			int toBlockLine = toLineWidth * 4;
			int sourceLineBlockCount = Math.Max( 1, ( ( width + 3 ) / 4 ) );
			int sourceLineWidth = sourceLineBlockCount * 16;

			// Decode to ARGB (GDI+) or BGRA (WPF)
			byte[] buffer = new byte[ height * toLineWidth ];
			byte[] sourceBuffer = new byte[ sourceLineWidth ];
			byte[] pixels = new byte[ 4 * 4 ];

			int toIndex, toIndexX, x;
			int toBlockY = 0, toBlockX;
			int maxBlockY, maxBlockX;
			int fromIndex, colorIndex;
			int alphaBlockIndex;
			ushort color0, color1;
			byte texel;
			uint alpha;

			for ( int y = 0; y < height; y += 4 )
			{
				reader.Read( sourceBuffer, 0, sourceLineWidth );
				fromIndex = 0;
				toBlockX = 0;
				x = 0;

				for ( int block = 0; block < sourceLineBlockCount; block++ )
				{
					// Alpha channel
					alphaBlockIndex = fromIndex;
					fromIndex += 8;

					// 2 extremes
					color0 = (ushort) ( ( sourceBuffer[ fromIndex++ ] ) | ( sourceBuffer[ fromIndex++ ] << 8 ) );
					color1 = (ushort) ( ( sourceBuffer[ fromIndex++ ] ) | ( sourceBuffer[ fromIndex++ ] << 8 ) );

					pixels[ 0 ] = (byte) ( ( color0 & 0x1F ) << 3 );
					pixels[ 1 ] = (byte) ( ( color0 & 0x7E0 ) >> 3 );
					pixels[ 2 ] = (byte) ( ( color0 & 0xF800 ) >> 8 );
					pixels[ 4 ] = (byte) ( ( color1 & 0x1F ) << 3 );
					pixels[ 5 ] = (byte) ( ( color1 & 0x7E0 ) >> 3 );
					pixels[ 6 ] = (byte) ( ( color1 & 0xF800 ) >> 8 );

					// Encoding
					pixels[ 8 ] = (byte) ( ( ( pixels[ 0 ] << 1 ) + pixels[ 4 ] + 1 ) / 3 );
					pixels[ 9 ] = (byte) ( ( ( pixels[ 1 ] << 1 ) + pixels[ 5 ] + 1 ) / 3 );
					pixels[ 10 ] = (byte) ( ( ( pixels[ 2 ] << 1 ) + pixels[ 6 ] + 1 ) / 3 );
					pixels[ 12 ] = (byte) ( ( pixels[ 0 ] + ( pixels[ 4 ] << 1 ) + 1 ) / 3 );
					pixels[ 13 ] = (byte) ( ( pixels[ 1 ] + ( pixels[ 5 ] << 1 ) + 1 ) / 3 );
					pixels[ 14 ] = (byte) ( ( pixels[ 2 ] + ( pixels[ 6 ] << 1 ) + 1 ) / 3 );

					// 4 texels
					toIndex = toBlockY + toBlockX;
					maxBlockY = Math.Min( height - y, 4 );
					maxBlockX = Math.Min( width - x, 4 );

					for ( int blockY = 0; blockY < maxBlockY; blockY++ )
					{
						texel = sourceBuffer[ fromIndex++ ];
						alpha = ( (uint) sourceBuffer[ alphaBlockIndex++ ] << 4 ) | ( (uint) sourceBuffer[ alphaBlockIndex++ ] << 12 );
						toIndexX = toIndex;

						for ( int blockX = 0; blockX < maxBlockX; blockX++ )
						{
							colorIndex = ( texel & 0x3 ) << 2;
							buffer[ toIndexX++ ] = pixels[ colorIndex ];
							buffer[ toIndexX++ ] = pixels[ colorIndex + 1 ];
							buffer[ toIndexX++ ] = pixels[ colorIndex + 2 ];
							buffer[ toIndexX++ ] = (byte) ( alpha & 0xF0 );
							texel >>= 2;
							alpha >>= 4;
						}

						toIndex += toLineWidth;
					}

					toBlockX += 16; // 4 pixels * 4 bytes
					x += 4;
				}

				toBlockY += toBlockLine;
			}

			return buffer;
		}

		private byte[] Read_DXT5( BinaryReader reader, int width, int height )
		{
			int toLineWidth = width * 4;
			int toBlockLine = toLineWidth * 4;
			int sourceLineBlockCount = Math.Max( 1, ( ( width + 3 ) / 4 ) );
			int sourceLineWidth = sourceLineBlockCount * 16;

			// Decode to ARGB (GDI+) or BGRA (WPF)
			byte[] buffer = new byte[ height * toLineWidth ];
			byte[] sourceBuffer = new byte[ sourceLineWidth ];
			byte[] pixels = new byte[ 4 * 4 ];
			byte[] alphas = new byte[ 8 ];

			int toIndex, toIndexX, x;
			int toBlockY = 0, toBlockX;
			int maxBlockY, maxBlockX;
			int fromIndex, colorIndex;
			byte alpha0, alpha1;
			ushort color0, color1;
			byte texel;
			long alpha;

			for ( int y = 0; y < height; y += 4 )
			{
				reader.Read( sourceBuffer, 0, sourceLineWidth );
				fromIndex = 0;
				toBlockX = 0;
				x = 0;

				for ( int block = 0; block < sourceLineBlockCount; block++ )
				{
					// Alpha block
					alphas[ 0 ] = alpha0 = sourceBuffer[ fromIndex++ ];
					alphas[ 1 ] = alpha1 = sourceBuffer[ fromIndex++ ];

					if ( alpha0 > alpha1 )
					{
						alphas[ 2 ] = (byte) ( ( 6 * alpha0 + 1 * alpha1 + 3 ) / 7 );
						alphas[ 3 ] = (byte) ( ( 5 * alpha0 + 2 * alpha1 + 3 ) / 7 );
						alphas[ 4 ] = (byte) ( ( 4 * alpha0 + 3 * alpha1 + 3 ) / 7 );
						alphas[ 5 ] = (byte) ( ( 3 * alpha0 + 4 * alpha1 + 3 ) / 7 );
						alphas[ 6 ] = (byte) ( ( 2 * alpha0 + 5 * alpha1 + 3 ) / 7 );
						alphas[ 7 ] = (byte) ( ( 1 * alpha0 + 6 * alpha1 + 3 ) / 7 );
					}
					else
					{
						alphas[ 2 ] = (byte) ( ( 4 * alpha0 + 1 * alpha1 + 2 ) / 5 );
						alphas[ 3 ] = (byte) ( ( 3 * alpha0 + 2 * alpha1 + 2 ) / 5 );
						alphas[ 4 ] = (byte) ( ( 2 * alpha0 + 3 * alpha1 + 2 ) / 5 );
						alphas[ 5 ] = (byte) ( ( 1 * alpha0 + 4 * alpha1 + 2 ) / 5 );
						alphas[ 6 ] = 0x00;
						alphas[ 7 ] = 0xFF;
					}

					alpha = ( (long) sourceBuffer[ fromIndex++ ] ) | ( (long) sourceBuffer[ fromIndex++ ] << 8 ) | ( (long) sourceBuffer[ fromIndex++ ] << 16 ) |
						( (long) sourceBuffer[ fromIndex++ ] << 24 ) | ( (long) sourceBuffer[ fromIndex++ ] << 32 ) | ( (long) sourceBuffer[ fromIndex++ ] << 40 );

					/*alpha0 = sourceBuffer[ fromIndex++ ];
					alpha1 = sourceBuffer[ fromIndex++ ];

					pixelAlphas[ 0 ] = alphas[ alpha0 & 0x7 ];
					pixelAlphas[ 1 ] = alphas[ ( alpha0 & 0x38 ) >> 3 ];
					pixelAlphas[ 2 ] = alphas[ ( ( alpha0 & 0xC0 ) >> 5 ) | ( alpha1 & 0x1 ) ];
					pixelAlphas[ 3 ] = alphas[ ( alpha1 & 0xE ) >> 1 ];
					pixelAlphas[ 4 ] = alphas[ ( alpha1 & 0x70 ) >> 4 ];

					alpha0 = alpha1;
					alpha1 = sourceBuffer[ fromIndex++ ];

					pixelAlphas[ 5 ] = alphas[ ( ( alpha0 & 0x80 ) >> 5 ) | ( alpha1 & 0x3 ) ];
					pixelAlphas[ 6 ] = alphas[ ( alpha1 & 0x1C ) >> 2 ];
					pixelAlphas[ 7 ] = alphas[ ( alpha1 & 0xE0 ) >> 5 ];

					alpha0 = sourceBuffer[ fromIndex++ ];
					alpha1 = sourceBuffer[ fromIndex++ ];

					pixelAlphas[ 8 ] = alphas[ alpha0 & 0x7 ];
					pixelAlphas[ 9 ] = alphas[ ( alpha0 & 0x38 ) >> 3 ];
					pixelAlphas[ 10 ] = alphas[ ( ( alpha0 & 0xC0 ) >> 5 ) | ( alpha1 & 0x1 ) ];
					pixelAlphas[ 11 ] = alphas[ ( alpha1 & 0xE ) >> 1 ];
					pixelAlphas[ 12 ] = alphas[ ( alpha1 & 0x70 ) >> 4 ];

					alpha0 = alpha1;
					alpha1 = sourceBuffer[ fromIndex++ ];

					pixelAlphas[ 13 ] = alphas[ ( ( alpha0 & 0x80 ) >> 5 ) | ( alpha1 & 0x3 ) ];
					pixelAlphas[ 14 ] = alphas[ ( alpha1 & 0x1C ) >> 2 ];
					pixelAlphas[ 15 ] = alphas[ ( alpha1 & 0xE0 ) >> 5 ];*/


					// Color block
					color0 = (ushort) ( ( sourceBuffer[ fromIndex++ ] ) | ( sourceBuffer[ fromIndex++ ] << 8 ) );
					color1 = (ushort) ( ( sourceBuffer[ fromIndex++ ] ) | ( sourceBuffer[ fromIndex++ ] << 8 ) );

					pixels[ 0 ] = (byte) ( ( color0 & 0x1F ) << 3 );
					pixels[ 1 ] = (byte) ( ( color0 & 0x7E0 ) >> 3 );
					pixels[ 2 ] = (byte) ( ( color0 & 0xF800 ) >> 8 );
					pixels[ 4 ] = (byte) ( ( color1 & 0x1F ) << 3 );
					pixels[ 5 ] = (byte) ( ( color1 & 0x7E0 ) >> 3 );
					pixels[ 6 ] = (byte) ( ( color1 & 0xF800 ) >> 8 );
					pixels[ 8 ] = (byte) ( ( ( pixels[ 0 ] << 1 ) + pixels[ 4 ] + 1 ) / 3 );
					pixels[ 9 ] = (byte) ( ( ( pixels[ 1 ] << 1 ) + pixels[ 5 ] + 1 ) / 3 );
					pixels[ 10 ] = (byte) ( ( ( pixels[ 2 ] << 1 ) + pixels[ 6 ] + 1 ) / 3 );
					pixels[ 12 ] = (byte) ( ( pixels[ 0 ] + ( pixels[ 4 ] << 1 ) + 1 ) / 3 );
					pixels[ 13 ] = (byte) ( ( pixels[ 1 ] + ( pixels[ 5 ] << 1 ) + 1 ) / 3 );
					pixels[ 14 ] = (byte) ( ( pixels[ 2 ] + ( pixels[ 6 ] << 1 ) + 1 ) / 3 );

					// 4 texels
					toIndex = toBlockY + toBlockX;
					maxBlockY = Math.Min( height - y, 4 );
					maxBlockX = Math.Min( width - x, 4 );

					for ( int blockY = 0; blockY < maxBlockY; blockY++ )
					{
						texel = sourceBuffer[ fromIndex++ ];
						toIndexX = toIndex;

						for ( int blockX = 0; blockX < maxBlockX; blockX++ )
						{
							colorIndex = ( texel & 0x3 ) << 2;
							buffer[ toIndexX++ ] = pixels[ colorIndex ];
							buffer[ toIndexX++ ] = pixels[ colorIndex + 1 ];
							buffer[ toIndexX++ ] = pixels[ colorIndex + 2 ];
							buffer[ toIndexX++ ] = alphas[ alpha & 0x7 ];
							alpha >>= 3;
							texel >>= 2;
						}

						// X1
						/*if ( x + 1 < width )
						{
							colorIndex = ( ( texel & 0xC ) >> 2 );
							buffer[ toIndex + 4 ] = pixels[ ( colorIndex << 2 ) ];
							buffer[ toIndex + 5 ] = pixels[ ( colorIndex << 2 ) + 1 ];
							buffer[ toIndex + 6 ] = pixels[ ( colorIndex << 2 ) + 2 ];
							buffer[ toIndex + 7 ] = pixelAlphas[ alpha & 0x7 ];
							alpha >>= 3;

							// X2
							if ( x + 2 < width )
							{
								colorIndex = ( ( texel & 0x30 ) >> 4 );
								buffer[ toIndex + 8 ] = pixels[ ( colorIndex << 2 ) ];
								buffer[ toIndex + 9 ] = pixels[ ( colorIndex << 2 ) + 1 ];
								buffer[ toIndex + 10 ] = pixels[ ( colorIndex << 2 ) + 2 ];
								buffer[ toIndex + 11 ] = pixelAlphas[ alpha & 0x7 ];
								alpha >>= 3;

								// X3
								if ( x + 3 < width )
								{
									colorIndex = ( ( texel & 0xC0 ) >> 6 );
									buffer[ toIndex + 12 ] = pixels[ ( colorIndex << 2 ) ];
									buffer[ toIndex + 13 ] = pixels[ ( colorIndex << 2 ) + 1 ];
									buffer[ toIndex + 14 ] = pixels[ ( colorIndex << 2 ) + 2 ];
									buffer[ toIndex + 15 ] = pixelAlphas[ alpha & 0x7 ];
									alpha >>= 3;
								}
							}
						}*/

						toIndex += toLineWidth;
					}

					toBlockX += 16; // 4 pixels * 4 bytes
					x += 4;
				}

				toBlockY += toBlockLine;
			}

			return buffer;
		}
		
		#region Pixel Format
		private const uint AlphaPixels	= 0x1;
		private const uint Alpha		= 0x2;
		private const uint FourCC		= 0x4;
		private const uint RGB			= 0x40;
		private const uint YUV			= 0x200;
		private const uint Luminance	= 0x20000;

		private static DDSPixelFormat ReadPixelFormat( BinaryReader reader )
		{
			DDSPixelFormat format = DDSPixelFormat.Unknown;

			if ( reader.ReadUInt32() != 32 )
				throw new DDSException( "Invalid DDS_PIXELFORMAT size" );

			uint flags = reader.ReadUInt32();
			uint fourCC = reader.ReadUInt32();
			uint rgbBitCount = reader.ReadUInt32();
			uint redBitMask = reader.ReadUInt32();
			uint greenBitMask = reader.ReadUInt32();
			uint blueBitMask = reader.ReadUInt32();
			uint alphaBitMask = reader.ReadUInt32();

			if ( ( flags & FourCC ) > 0 ) // Compressed
			{
				switch ( fourCC )
				{
					case 0x30315844: format = DDSPixelFormat.DX10; break;
					case 0x31545844: format = DDSPixelFormat.DXT1; break;
					case 0x32545844: format = DDSPixelFormat.DXT2; break;
					case 0x33545844: format = DDSPixelFormat.DXT3; break;
					case 0x34545844: format = DDSPixelFormat.DXT4; break;
					case 0x35545844: format = DDSPixelFormat.DXT5; break;
					case 0x47424752: format = DDSPixelFormat.R8G8B8G8; break;
					case 0x42475247: format = DDSPixelFormat.G8R8G8B8; break;
					case 0x59565955: format = DDSPixelFormat.UYVY; break;
					case 0x32595559: format = DDSPixelFormat.YUY2; break;
					case 117: format = DDSPixelFormat.CxV8U8; break;
					default: format = DDSPixelFormat.Unknown; break;
				}
			}
			else if ( ( flags & Luminance ) > 0 )
			{
				if ( rgbBitCount == 16 )
				{
					if ( redBitMask == 0xFF && alphaBitMask == 0xFF00 )
						format = DDSPixelFormat.A8L8;
					else if ( redBitMask == 0xFFFF )
						format = DDSPixelFormat.L16;
					else
						format = DDSPixelFormat.Unknown;
				}
				else if ( rgbBitCount == 8 )
				{
					if ( redBitMask == 0xF && alphaBitMask == 0xF0 )
						format = DDSPixelFormat.A4L4;
					else if ( redBitMask == 0xFF )
						format = DDSPixelFormat.L8;
					else
						format = DDSPixelFormat.Unknown;
				}
				else
					format = DDSPixelFormat.Unknown;
			}
			else if ( ( flags & RGB ) > 0 && ( flags & AlphaPixels ) > 0 ) // RGBA
			{
				if ( rgbBitCount == 32 )
				{
					if ( redBitMask == 0xFF && greenBitMask == 0xFF00 && blueBitMask == 0xFF0000 && alphaBitMask == 0xFF000000 )
						format = DDSPixelFormat.A8B8G8R8;
					else if ( redBitMask == 0xFF0000 && greenBitMask == 0xFF00 && blueBitMask == 0xFF && alphaBitMask == 0xFF000000 )
						format = DDSPixelFormat.A8R8G8B8;
					else if ( redBitMask == 0xFFF && greenBitMask == 0xFFFF0000 )
						format = DDSPixelFormat.G16R16;
					else if ( redBitMask == 0x3FF && greenBitMask == 0xFFC000 && blueBitMask == 0x3FF00000 )
						format = DDSPixelFormat.A2B10G10R10;
					else if ( redBitMask == 0x3FF00000 && greenBitMask == 0xFFC000 && blueBitMask == 0x3FF )
						format = DDSPixelFormat.A2B10G10R10;
					else
						format = DDSPixelFormat.Unknown;
				}
				else if ( rgbBitCount == 16 )
				{
					if ( redBitMask == 0x7C00 && greenBitMask == 0x3E0 && blueBitMask == 0x1F && alphaBitMask == 0x8000 )
						format = DDSPixelFormat.A1R5G5B5;
					else if ( redBitMask == 0xF00 && greenBitMask == 0xF0 && blueBitMask == 0xF && alphaBitMask == 0xF000 )
						format = DDSPixelFormat.A4R4G4B4;
					else if ( redBitMask == 0xE0 && greenBitMask == 0x1C && blueBitMask == 0x3 && alphaBitMask == 0xFF00 )
						format = DDSPixelFormat.A8R3G3B2;
					else
						format = DDSPixelFormat.Unknown;
				}
				else
					format = DDSPixelFormat.Unknown;
			}
			else if ( ( flags & RGB ) > 0 ) // RGB
			{
				if ( rgbBitCount == 32 )
				{
					if ( redBitMask == 0xFFFF && greenBitMask == 0xFFFF0000 )
						format = DDSPixelFormat.G16R16;
					else if ( redBitMask == 0xFF0000 && greenBitMask == 0xFF00 && blueBitMask == 0xFF )
						format = DDSPixelFormat.X8R8G8B8;
					else if ( redBitMask == 0xFF && greenBitMask == 0xFF00 && blueBitMask == 0xFF0000 )
						format = DDSPixelFormat.X8B8G8R8;
					else if ( redBitMask == 0xFFF && greenBitMask == 0xFFFF0000 )
						format = DDSPixelFormat.G16R16;
					else if ( redBitMask == 0x3FF && greenBitMask == 0xFFC000 && blueBitMask == 0x3FF00000 )
						format = DDSPixelFormat.A2B10G10R10;
					else if ( redBitMask == 0x3FF00000 && greenBitMask == 0xFFC000 && blueBitMask == 0x3FF )
						format = DDSPixelFormat.A2B10G10R10;
					else
						format = DDSPixelFormat.Unknown;
				}
				else if ( rgbBitCount == 24 )
				{
					if ( redBitMask == 0xFF0000 && greenBitMask == 0xFF00 && blueBitMask == 0xFF )
						format = DDSPixelFormat.R8G8B8;
				}
				else if ( rgbBitCount == 16 )
				{
					if ( redBitMask == 0x7C00 && greenBitMask == 0x3E0 && blueBitMask == 0x1F )
						format = DDSPixelFormat.X1R5G5B5;
					else if ( redBitMask == 0xF00 && greenBitMask == 0xF0 && blueBitMask == 0xF )
						format = DDSPixelFormat.X4R4G4B4;
					else if ( redBitMask == 0xF800 && greenBitMask == 0x7E0 && blueBitMask == 0x1F )
						format = DDSPixelFormat.R5G6B5;
					else
						format = DDSPixelFormat.Unknown;
				}
				else
					format = DDSPixelFormat.Unknown;
			}

			return format;
		}
		#endregion

		#region Texture
		/// <summary>
		/// Gets texture.
		/// </summary>
		/// <param name="mipMapLevel">Mip level at which to get texture (0 if 
		/// file does not contain mip maps).</param>
		/// <returns>Pixel data.</returns>
		public byte[] GetTexture( int mipMapLevel = 0 )
		{
			if ( _Texture != null && mipMapLevel >= 0 && mipMapLevel < _Texture.Count )
			{
				return _Texture[ mipMapLevel ];
			}

			return null;
		}

		/// <summary>
		/// Gets texture.
		/// </summary>
		/// <param name="mipMapLevel">Mip level at which to get texture (0 if 
		/// file does not contain mip maps).</param>
		/// <returns>WPF bitmap source.</returns>
		public BitmapSource GetTextureAsBitmapSource( int mipMapLevel = 0 )
		{
			if ( _Texture != null && mipMapLevel >= 0 && mipMapLevel < _Texture.Count )
			{
				byte[] pixels = _Texture[ mipMapLevel ];
				int divisor = (int) Math.Pow( 2, mipMapLevel );
				int width = Math.Max( 1, _Width / divisor );
				int height = Math.Max( 1, _Height / divisor );

				return BitmapSource.Create( width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4 );
			}

			return null;
		}

		/// <summary>
		/// Gets texture.
		/// </summary>
		/// <param name="mipMapLevel">Mip level at which to get texture (0 if 
		/// file does not contain mip maps).</param>
		/// <returns>Bitmap.</returns>
		public Bitmap GetTextureAsBitmap( int mipMapLevel = 0 )
		{
			if ( _Texture != null && mipMapLevel >= 0 && mipMapLevel < _Texture.Count )
			{
				byte[] pixels = _Texture[ mipMapLevel ];
				int divisor = (int) Math.Pow( 2, mipMapLevel );
				int width = Math.Max( 1, _Width / divisor );
				int height = Math.Max( 1, _Height / divisor );

				Bitmap bitmap = new Bitmap( width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb );
				BitmapData data = bitmap.LockBits( new Rectangle( 0, 0, width, height ), ImageLockMode.WriteOnly, bitmap.PixelFormat );
				Marshal.Copy( pixels, 0, data.Scan0, pixels.Length );

				bitmap.UnlockBits( data );
				return bitmap;
			}

			return null;
		}
		#endregion

		#region Cube Map
		/// <summary>
		/// Determines whether file contains certain cube face.
		/// </summary>
		/// <param name="face">Cube face.</param>
		/// <returns>True if contains, false otherwise.</returns>
		public bool HasCubeFace( DDSCubeFace face )
		{
			switch ( face )
			{
				case DDSCubeFace.PositiveX: return _PositiveX != null;
				case DDSCubeFace.NegativeX: return _NegativeX != null;
				case DDSCubeFace.PositiveY: return _PositiveY != null;
				case DDSCubeFace.NegativeY: return _NegativeY != null;
				case DDSCubeFace.PositiveZ: return _PositiveZ != null;
				case DDSCubeFace.NegativeZ: return _NegativeZ != null;
			}

			return false;
		}

		/// <summary>
		/// Gets cube texture.
		/// </summary>
		/// <param name="face">Cube face to get.</param>
		/// <param name="mipMapLevel">Mip level at which to get cube face (0 if 
		/// file does not contain mip maps).</param>
		/// <returns>Pixel data in ARGB format.</returns>
		public byte[] GetCubeTexture( DDSCubeFace face, int mipMapLevel = 0 )
		{
			MipMaps mipMaps = null;

			switch ( face )
			{
				case DDSCubeFace.PositiveX: mipMaps = _PositiveX; break;
				case DDSCubeFace.NegativeX: mipMaps = _NegativeX; break;
				case DDSCubeFace.PositiveY: mipMaps = _PositiveY; break;
				case DDSCubeFace.NegativeY: mipMaps = _NegativeY; break;
				case DDSCubeFace.PositiveZ: mipMaps = _PositiveZ; break;
				case DDSCubeFace.NegativeZ: mipMaps = _NegativeZ; break;
			}

			if ( mipMaps != null && mipMapLevel >= 0 && mipMapLevel < mipMaps.Count )
				return mipMaps[ mipMapLevel ];

			return null;
		}

		/// <summary>
		/// Gets cube texture.
		/// </summary>
		/// <param name="face">Cube face to get.</param>
		/// <param name="mipMapLevel">Mip level at which to get cube face (0 if 
		/// file does not contain mip maps).</param>
		/// <returns>WPF bitmap source.</returns>
		public BitmapSource GetCubeTextureAsBitmapSource( DDSCubeFace face, int mipMapLevel = 0 )
		{
			byte[] pixels = GetCubeTexture( face, mipMapLevel );

			if ( pixels != null )
			{
				int divisor = (int) Math.Pow( 2, mipMapLevel );
				int width = Math.Max( 1, _Width / divisor );
				int height = Math.Max( 1, _Height / divisor );

				return BitmapSource.Create( width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4 );
			}

			return null;
		}

		/// <summary>
		/// Gets cube texture.
		/// </summary>
		/// <param name="face">Cube face to get.</param>
		/// <param name="mipMapLevel">Mip level at which to get cube face (0 if 
		/// file does not contain mip maps).</param>
		/// <returns>Bitmap.</returns>
		public Bitmap GetCubeTextureAsBitmap( DDSCubeFace face, int mipMapLevel = 0 )
		{
			byte[] pixels = GetCubeTexture( face, mipMapLevel );

			if ( pixels != null )
			{
				int divisor = (int) Math.Pow( 2, mipMapLevel );
				int width = Math.Max( 1, _Width / divisor );
				int height = Math.Max( 1, _Height / divisor );

				Bitmap bitmap = new Bitmap( width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb );
				BitmapData data = bitmap.LockBits( new Rectangle( 0, 0, width, height ), ImageLockMode.WriteOnly, bitmap.PixelFormat );
				Marshal.Copy( pixels, 0, data.Scan0, pixels.Length );

				bitmap.UnlockBits( data );
				return bitmap;
			}

			return null;
		}
		#endregion

		#region Volume Map
		/// <summary>
		/// Gets volume texture.
		/// </summary>
		/// <param name="depth">Depth at which to get texture.</param>
		/// <param name="mipMapLevel">Mip level at which to get texture (0 if 
		/// file does not contain mip maps).</param>
		/// <returns>Pixel data in ARGB format.</returns>
		public byte[] GetVolumeTexture( int depth, int mipMapLevel = 0 )
		{
			if ( _VolumeMaps != null && mipMapLevel >= 0 && mipMapLevel < _VolumeMaps.Count )
			{
				VolumeMap volumeMap = _VolumeMaps[ mipMapLevel ];

				if ( depth >= 0 && depth < volumeMap.Count )
					return volumeMap[ depth ];
			}

			return null;
		}

		/// <summary>
		/// Gets volume texture.
		/// </summary>
		/// <param name="depth">Depth at which to get texture.</param>
		/// <param name="mipMapLevel">Mip level at which to get texture (0 if 
		/// file does not contain mip maps).</param>
		/// <returns>WPF bitmap source.</returns>
		public BitmapSource GetVolumeTextureAsBitmapSource( int depth, int mipMapLevel = 0 )
		{
			byte[] pixels = GetVolumeTexture( depth, mipMapLevel );

			if ( pixels != null )
			{
				int divisor = (int) Math.Pow( 2, mipMapLevel );
				int width = Math.Max( 1, _Width / divisor );
				int height = Math.Max( 1, _Height / divisor );

				return BitmapSource.Create( width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4 );
			}

			return null;
		}

		/// <summary>
		/// Gets volume texture.
		/// </summary>
		/// <param name="depth">Depth at which to get texture.</param>
		/// <param name="mipMapLevel">Mip level at which to get texture (0 if 
		/// file does not contain mip maps).</param>
		/// <returns>Bitmap.</returns>
		public Bitmap GetVolumeTextureAsBitmap( int depth, int mipMapLevel = 0 )
		{
			byte[] pixels = GetVolumeTexture( depth, mipMapLevel );

			if ( pixels != null )
			{
				int divisor = (int) Math.Pow( 2, mipMapLevel );
				int width = Math.Max( 1, _Width / divisor );
				int height = Math.Max( 1, _Height / divisor );

				Bitmap bitmap = new Bitmap( width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb );
				BitmapData data = bitmap.LockBits( new Rectangle( 0, 0, width, height ), ImageLockMode.WriteOnly, bitmap.PixelFormat );
				Marshal.Copy( pixels, 0, data.Scan0, pixels.Length );

				bitmap.UnlockBits( data );
				return bitmap;
			}

			return null;
		}
		#endregion
		#endregion
	}

	/// <summary>
	/// Describes DDS exception.
	/// </summary>
	public class DDSException : Exception
	{
		#region Constructors
		/// <summary>
		/// Constructs a new instance of DDSException.
		/// </summary>
		/// <param name="format">Message format.</param>
		/// <param name="args">Message parameters.</param>
		public DDSException( string format, params object[] args )
			: base( String.Format( format, args ) )
		{
		}
		#endregion
	}
}
