using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ultima.Package
{
	/// <summary>
	/// Describes animation.
	/// </summary>
	public class UltimaAnimation
	{
		#region Properties
		private int _Version;

		/// <summary>
		/// Gets animation frame version.
		/// </summary>
		public int Version
		{
			get { return _Version; }
		}

		private int _AnimationID;

		/// <summary>
		/// Gets animation ID.
		/// </summary>
		public int AnimationID
		{
			get { return _AnimationID; }
		}

		private int _StartX;

		/// <summary>
		/// Gets start X coordinate, relative to tile center.
		/// </summary>
		public int StartX
		{
			get { return _StartX; }
		}

		private int _StartY;

		/// <summary>
		/// Gets start Y coordinate, relative to tile center.
		/// </summary>
		public int StartY
		{
			get { return _StartY; }
		}

		private int _EndX;

		/// <summary>
		/// Gets end X coordinate, relative to tile center.
		/// </summary>
		public int EndX
		{
			get { return _EndX; }
		}

		private int _EndY;

		/// <summary>
		/// Gets end Y coordinate, relative to tile center.
		/// </summary>
		public int EndY
		{
			get { return _EndY; }
		}

		private List<UltimaAnimationFrame> _Frames;

		/// <summary>
		/// Gets frames.
		/// </summary>
		public List<UltimaAnimationFrame> Frames
		{
			get { return _Frames; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaAnimation.
		/// </summary>
		/// <param name="reader">Reader to construct from.</param>
		/// <param name="legacy">Determines animation frames are saved in legacy format.</param>
		public UltimaAnimation( BinaryReader reader, bool legacy )
		{
			byte[] id = reader.ReadBytes( 4 );

			if ( id[ 0 ] != 'A' || id[ 1 ] != 'M' || id[ 2 ] != 'O' || id[ 3 ] != 'U' )
				throw new FormatException( "Not animation frame file" );

			_Version = reader.ReadInt32();
			int length = reader.ReadInt32();
			_AnimationID = reader.ReadInt32();
			_StartX = reader.ReadInt16();
			_StartY = reader.ReadInt16();
			_EndX = reader.ReadInt16();
			_EndY = reader.ReadInt16();

			int paletteSize = reader.ReadInt32();
			int paletteAddress = reader.ReadInt32();

			int frameCount = reader.ReadInt32();
			int frameTableAddress = reader.ReadInt32();

			// Read palette (ARGB)
			uint[] palette = new uint[ paletteSize ];

			for ( int i = 0; i < palette.Length; i++ )
				palette[ i ] = reader.ReadUInt32() ^ 0xFF000000;

			// Read frame table
			_Frames = new List<UltimaAnimationFrame>( frameCount );

			for ( int i = 0; i < frameCount; i++ )
				_Frames.Add( new UltimaAnimationFrame( reader ) );

			// Read pixel data
			if ( legacy )
			{
				for ( int i = 0; i < frameCount; i++ )
					_Frames[ i ].ReadLegacyPixelData( reader, palette, frameTableAddress );
			}
			else
			{
				for ( int i = 0; i < frameCount; i++ )
					_Frames[ i ].ReadPixelData( reader, palette, frameTableAddress );
			}
		}
		#endregion

		#region Mehtods
		/// <summary>
		/// Gets image as bitmap source.
		/// </summary>
		/// <param name="frameIndex">Frame index.</param>
		/// <returns>WPF bitmap source.</returns>
		public BitmapSource GetImageAsBitmapSource( int frameIndex = 0 )
		{
			if ( frameIndex < 0 && frameIndex > _Frames.Count )
				return null;

			UltimaAnimationFrame frame = _Frames[ frameIndex ];
			return BitmapSource.Create( frame.Width, frame.Height, 96, 96, PixelFormats.Bgra32, null, frame.PixelData, frame.Width * 4 );
		}

		/// <summary>
		/// Gets image as bitmap.
		/// </summary>
		/// <param name="frameIndex">Frame index.</param>
		/// <returns>Bitmap.</returns>
		public Bitmap GetImageAsBitmap( int frameIndex = 0 )
		{
			if ( frameIndex < 0 && frameIndex > _Frames.Count )
				return null;

			UltimaAnimationFrame frame = _Frames[ frameIndex ];
			Bitmap bitmap = new Bitmap( frame.Width, frame.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb );
			BitmapData data = bitmap.LockBits( new Rectangle( 0, 0, frame.Width, frame.Height ), ImageLockMode.WriteOnly, bitmap.PixelFormat );
			Marshal.Copy( frame.PixelData, 0, data.Scan0, frame.PixelData.Length );

			bitmap.UnlockBits( data );
			return bitmap;
		}

		/// <summary>
		/// Constructs animation from from file.
		/// </summary>
		/// <param name="filePath">Path to the file.</param>
		/// <param name="legacy">Determines animation frames are saved in legacy format.</param>
		/// <returns>Animation .</returns>
		public static UltimaAnimation FromFile( string filePath, bool legacy = false )
		{
			using ( FileStream stream = File.Open( filePath, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			{
				return FromStream( stream, legacy );
			}
		}

		/// <summary>
		/// Constructs animation from stream.
		/// </summary>
		/// <param name="data">Memory to read from.</param>
		/// <param name="legacy">Determines animation frames are saved in legacy format.</param>
		/// <returns>Art image.</returns>
		public static UltimaAnimation FromMemory( byte[] data, bool legacy = false )
		{
			using ( MemoryStream stream = new MemoryStream( data ) )
			{
				return FromStream( stream, legacy );
			}
		}

		/// <summary>
		/// Constructs animation frame from stream.
		/// </summary>
		/// <param name="stream">Stream to construct from.</param>
		/// <param name="legacy">Determines animation frames are saved in legacy format.</param>
		/// <returns>Animation.</returns>
		public static UltimaAnimation FromStream( Stream stream, bool legacy = false )
		{
			using ( BinaryReader reader = new BinaryReader( stream ) )
			{
				return new UltimaAnimation( reader, legacy );
			}
		}
		#endregion
	}

	/// <summary>
	/// Describes animation frame.
	/// </summary>
	public class UltimaAnimationFrame
	{
		#region Properties
		private const int DoubleXor = ( 0x200 << 22 ) | ( 0x200 << 12 );

		private int _MoveID;

		/// <summary>
		/// Gets move ID.
		/// </summary>
		public int MoveID
		{
			get { return _MoveID; }
		}

		private int _FrameNumber;

		/// <summary>
		/// Gets frame number.
		/// </summary>
		public int FrameNumber
		{
			get { return _FrameNumber; }
		}

		private int _StartX;

		/// <summary>
		/// Gets start X coordinate, relative to tile center.
		/// </summary>
		public int StartX
		{
			get { return _StartX; }
		}

		private int _StartY;

		/// <summary>
		/// Gets start Y coordinate, relative to tile center.
		/// </summary>
		public int StartY
		{
			get { return _StartY; }
		}

		private int _Width;

		/// <summary>
		/// Gets frame width;
		/// </summary>
		public int Width
		{
			get { return _Width; }
		}

		private int _Height;

		/// <summary>
		/// Gets frame height.
		/// </summary>
		public int Height
		{
			get { return _Height; }
		}

		private byte[] _PixelData;

		/// <summary>
		/// Gets pixel data.
		/// </summary>
		public byte[] PixelData
		{
			get { return _PixelData; }
		}

		private int _FrameOffset;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaAnimationFrame.
		/// </summary>
		/// <param name="reader">Reader to construct from.</param>
		public UltimaAnimationFrame( BinaryReader reader )
		{
			_MoveID = reader.ReadInt16();
			_FrameNumber = reader.ReadInt16();
			_StartX = reader.ReadInt16();
			_StartY = reader.ReadInt16();
			_Width = reader.ReadInt16() - _StartX;
			_Height = reader.ReadInt16() - _StartY;
			_FrameOffset = reader.ReadInt32();
			_PixelData = new byte[ Width * _Height * 4 ];
		}

		/// <summary>
		/// Reads legacy pixel data.
		/// </summary>
		/// <param name="reader">Reader to construct from.</param>
		/// <param name="palette">Color palette.</param>
		/// <param name="frameTableStart">Frame table start.</param>
		public void ReadLegacyPixelData( BinaryReader reader, uint[] palette, int frameTableStart )
		{
			// Weird stuff

			/*reader.BaseStream.Seek( frameTableStart + _FrameOffset, SeekOrigin.Begin );

			int header = 0;

			while ( ( header = reader.ReadInt32() ) != 0x7FFF7FFF )
			{
				int y = ( header >> 12 ) & 0x3FF;
				int x = ( header >> 22 ) & 0x3FF;
				int length = header & 0xFFF;
				int index = y * _Width * 4 + x * 4;

				for ( int i = 0; i < length; i++ )
				{
					uint pixel = palette[ reader.ReadByte() ];

					_PixelData[ index++ ] = (byte) ( ( pixel & 0xFF ) >> 0 );
					_PixelData[ index++ ] = (byte) ( ( pixel & 0xFF00 ) >> 8 );
					_PixelData[ index++ ] = (byte) ( ( pixel & 0xFF0000 ) >> 16 );
					_PixelData[ index++ ] = (byte) ( ( pixel & 0xFF000000 ) >> 24 );
				}
			}*/
		}

		/// <summary>
		/// Reads pixel data.
		/// </summary>
		/// <param name="reader">Reader to construct from.</param>
		/// <param name="palette">Color palette.</param>
		/// <param name="frameTableStart">Frame table start.</param>
		public void ReadPixelData( BinaryReader reader, uint[] palette, int frameTableStart )
		{
			reader.BaseStream.Seek( frameTableStart + _FrameOffset, SeekOrigin.Begin );

			int index = 0;
			int size = _Width * _Height * 4;

			while ( index < size )
			{
				byte first = reader.ReadByte();

				if ( ( first & 0x80 ) == 0 )
				{
					index += first * 4;
				}
				else
				{
					first = (byte) ( first & 0x7F );
					byte factor = reader.ReadByte();

					if ( ( factor & 0xF0 ) > 0 )
					{
						int colorIndex = reader.ReadByte();
						uint color = BlendColors( 0x000000000, palette[ colorIndex ], factor >> 4 );

						_PixelData[ index++ ] = (byte) ( ( color & 0xFF ) >> 0 );
						_PixelData[ index++ ] = (byte) ( ( color & 0xFF00 ) >> 8 );
						_PixelData[ index++ ] = (byte) ( ( color & 0xFF0000 ) >> 16 );
						_PixelData[ index++ ] = (byte) ( ( color & 0xFF000000 ) >> 24 );
					}

					while ( first-- > 0 )
					{
						int colorIndex = reader.ReadByte();
						uint color = palette[ colorIndex ];

						_PixelData[ index++ ] = (byte) ( ( color & 0xFF ) >> 0 );
						_PixelData[ index++ ] = (byte) ( ( color & 0xFF00 ) >> 8 );
						_PixelData[ index++ ] = (byte) ( ( color & 0xFF0000 ) >> 16 );
						_PixelData[ index++ ] = (byte) ( ( color & 0xFF000000 ) >> 24 );
					}

					if ( ( factor & 0xF ) > 0 )
					{
						int colorIndex = reader.ReadByte();
						uint color = BlendColors( 0x00000000, palette[ colorIndex ], factor & 0xF );

						_PixelData[ index++ ] = (byte) ( ( color & 0xFF ) >> 0 );
						_PixelData[ index++ ] = (byte) ( ( color & 0xFF00 ) >> 8 );
						_PixelData[ index++ ] = (byte) ( ( color & 0xFF0000 ) >> 16 );
						_PixelData[ index++ ] = (byte) ( ( color & 0xFF000000 ) >> 24 );
					}
				}
			}
		}

		private uint BlendColors( uint background, uint color, int factor )
		{
			uint b = background;
			uint c = color;

			uint ac = (uint) ( ( ( c >> 4 ) & 0xFFF00FF0 ) * factor );
			uint ab = (uint) ( ( ( b >> 4 ) & 0xFFF00FF0 ) * ( 0x10 - factor ) );
			uint ar = ac + ab;

			uint bc = (uint) ( ( c & 0xFF00FF ) * factor );
			uint bb = (uint) ( ( b & 0xFF00FF ) * ( 0x10 - factor ) );
			uint br = ar ^ ( ( bc + bb ) >> 4 );

			uint cc = (uint) ( ( ( c >> 4 ) & 0xFF00FF0 ) * factor );
			uint cb = (uint) ( ( ( b >> 4 ) & 0xFF00FF0 ) * ( 0x10 - factor ) );
			uint cr = cc + cb;

			return (uint) ( cr ^ br & 0xFF00FF );
		}
		#endregion
	}

	/// <summary>
	/// Describes animation descriptor.
	/// </summary>
	public class UltimaAnimationDescriptor
	{
		#region Properties
		private int _BodyID;

		/// <summary>
		/// Gets body ID.
		/// </summary>
		public int BodyID
		{
			get { return _BodyID; }
		}

		private string _Portrait;

		/// <summary>
		/// Gets portrait filename.
		/// </summary>
		public string Portrait
		{
			get { return _Portrait; }
		}

		private string _Name;

		/// <summary>
		/// Gets body name.
		/// </summary>
		public string Name
		{
			get { return _Name; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaAnimationDescriptor.
		/// </summary>
		/// <param name="bodyID">Body ID.</param>
		/// <param name="portrait">Portrait file name.</param>
		/// <param name="name">Body name.</param>
		public UltimaAnimationDescriptor( int bodyID, string portrait, string name )
		{
			_BodyID = bodyID;
			_Portrait = portrait;
			_Name = name;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Constructs animation descriptors from stream.
		/// </summary>
		/// <param name="stream">Steam to construct from.</param>
		/// <returns>Animation descriptors.</returns>
		public static Dictionary<int, UltimaAnimationDescriptor> FromStream( Stream stream )
		{
			Dictionary<int, UltimaAnimationDescriptor> descriptors = new Dictionary<int, UltimaAnimationDescriptor>();

			using ( TextReader reader = new StreamReader( stream ) )
			{
				string line;
				char[] separators = new char[] { ',' };

				while ( ( line = reader.ReadLine() ) != null )
				{
					string[] parts = line.Split( separators, StringSplitOptions.RemoveEmptyEntries );

					if ( parts.Length == 3 )
					{
						int bodyID = 0;

						if ( Int32.TryParse( parts[ 0 ], out bodyID ) && !descriptors.ContainsKey( bodyID ) )
						{
							descriptors.Add( bodyID, new UltimaAnimationDescriptor( bodyID, parts[ 1 ], parts[ 2 ] ) );
						}
					}
				}
			}

			return descriptors;
		}
		#endregion
	}
}
