using System;
using System.Collections.Generic;
using System.IO;

namespace Ultima.Package
{
	/// <summary>
	/// Describes ultima package file (.uop).
	/// </summary>
	public class UltimaPackage : IDisposable
	{
		#region Properties
		private string _FilePath;

		/// <summary>
		/// Gets file path.
		/// </summary>
		public string FilePath
		{
			get { return _FilePath; }
		}

		private int _Version;

		/// <summary>
		/// Gets version.
		/// </summary>
		public int Version
		{
			get { return _Version; }
		}

		private Dictionary<ulong, UltimaPackageFile> _Files;

		/// <summary>
		/// Gets files by file name hash.
		/// </summary>
		public Dictionary<ulong, UltimaPackageFile> Files
		{
			get { return _Files; }
		}

		private FileStream _Stream;
		private BinaryReader _Reader;
		#endregion

		#region Constructors
		/// <summary>
		/// Cosntructs a new instance of UltimaPackage.
		/// </summary>
		/// <param name="filePath">Path to the .uop file.</param>
		public UltimaPackage( string filePath )
		{
			_FilePath = filePath;
			_Files = new Dictionary<ulong, UltimaPackageFile>();
			_Stream = File.Open( filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
			_Reader = new BinaryReader( _Stream );

			byte[] id = _Reader.ReadBytes( 4 );

			if ( id[ 0 ] != 'M' || id[ 1 ] != 'Y' || id[ 2 ] != 'P' || id[ 3 ] != 0 )
				throw new FormatException( "Not a Mythic Package file!" );

			_Version = _Reader.ReadInt32();
			_Reader.ReadInt32(); // Constant number

			long nextBlockAddress = _Reader.ReadInt64();
			long blockSize = _Reader.ReadInt32(); 

			// Unknown
			_Reader.ReadInt32();

			// Move to first block header
			do
			{
				_Stream.Seek( nextBlockAddress, SeekOrigin.Begin );
				int fileCount = _Reader.ReadInt32();
				nextBlockAddress = _Reader.ReadInt64();

				for ( int i = 0; i < blockSize; i++ )
				{
					if ( i < fileCount )
					{
						// Actual file
						UltimaPackageFile file = new UltimaPackageFile( this, _Reader );

						if ( file.FileAddress == 0 )
							continue;

						if ( !_Files.ContainsKey( file.FileNameHash ) )
							_Files.Add( file.FileNameHash, file );
					}
					else
					{
						// Blank
						_Stream.Seek( UltimaPackageFile.FileHeaderSize, SeekOrigin.Current );
					}
				}
			}
			while ( nextBlockAddress > 0 );
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~UltimaPackage()
		{
			Dispose( true );
		}
		#endregion

		#region Methods
		/// <summary>
		/// Gets file data.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <returns>File data.</returns>
		public byte[] GetFile( string fileName )
		{
			return GetFile( HashFileName( fileName ) );
		}

		/// <summary>
		/// Gets file data.
		/// </summary>
		/// <param name="fileNameHash">File hash.</param>
		/// <returns>File data.</returns>
		public byte[] GetFile( ulong fileNameHash )
		{
			UltimaPackageFile file = null;

			if ( !_Files.TryGetValue( fileNameHash, out file ) )
				return null;

			_Stream.Seek( file.FileAddress, SeekOrigin.Begin );

			byte[] compressed = new byte[ file.CompressedSize ];

			if ( _Reader.Read( compressed, 0, file.CompressedSize ) != file.CompressedSize )
				throw new FileLoadException( "Error reading file" );

			if ( file.Compression == FileCompression.Zlib )
			{
				int decompressedSize = file.DecompressedSize;
				byte[] decompressed = new byte[ decompressedSize ];

				ZLibError error = Zlib.Decompress( decompressed, ref decompressedSize, compressed, compressed.Length );

				if ( decompressedSize != file.DecompressedSize )
					throw new PackageException( "Error decompressing vile. Decompressed length missmatch. Defined={0} Actual={1}", file.DecompressedSize, decompressedSize );

				if ( error == ZLibError.Okay )
					return decompressed;

				throw new PackageException( "Error decompressing file. Error={0}", error );
			}

			return compressed;
		}

		/// <summary>
		/// Computes KR hash of the <paramref name="s"/>.
		/// </summary>
		/// <param name="s">String to hash.</param>
		/// <returns>Hashed string.</returns>
		public static ulong HashFileName( string s )
		{
			uint eax, ecx, edx, ebx, esi, edi;

			eax = ecx = edx = ebx = esi = edi = 0;
			ebx = edi = esi = (uint) s.Length + 0xDEADBEEF;

			int i = 0;

			for ( i = 0; i + 12 < s.Length; i += 12 )
			{
				edi = (uint) ( ( s[ i + 7 ] << 24 ) | ( s[ i + 6 ] << 16 ) | ( s[ i + 5 ] << 8 ) | s[ i + 4 ] ) + edi;
				esi = (uint) ( ( s[ i + 11 ] << 24 ) | ( s[ i + 10 ] << 16 ) | ( s[ i + 9 ] << 8 ) | s[ i + 8 ] ) + esi;
				edx = (uint) ( ( s[ i + 3 ] << 24 ) | ( s[ i + 2 ] << 16 ) | ( s[ i + 1 ] << 8 ) | s[ i ] ) - esi;

				edx = ( edx + ebx ) ^ ( esi >> 28 ) ^ ( esi << 4 );
				esi += edi;
				edi = ( edi - edx ) ^ ( edx >> 26 ) ^ ( edx << 6 );
				edx += esi;
				esi = ( esi - edi ) ^ ( edi >> 24 ) ^ ( edi << 8 );
				edi += edx;
				ebx = ( edx - esi ) ^ ( esi >> 16 ) ^ ( esi << 16 );
				esi += edi;
				edi = ( edi - ebx ) ^ ( ebx >> 13 ) ^ ( ebx << 19 );
				ebx += esi;
				esi = ( esi - edi ) ^ ( edi >> 28 ) ^ ( edi << 4 );
				edi += ebx;
			}

			if ( s.Length - i > 0 )
			{
				switch ( s.Length - i )
				{
					case 12:
						esi += (uint) s[ i + 11 ] << 24;
						goto case 11;
					case 11:
						esi += (uint) s[ i + 10 ] << 16;
						goto case 10;
					case 10:
						esi += (uint) s[ i + 9 ] << 8;
						goto case 9;
					case 9:
						esi += (uint) s[ i + 8 ];
						goto case 8;
					case 8:
						edi += (uint) s[ i + 7 ] << 24;
						goto case 7;
					case 7:
						edi += (uint) s[ i + 6 ] << 16;
						goto case 6;
					case 6:
						edi += (uint) s[ i + 5 ] << 8;
						goto case 5;
					case 5:
						edi += (uint) s[ i + 4 ];
						goto case 4;
					case 4:
						ebx += (uint) s[ i + 3 ] << 24;
						goto case 3;
					case 3:
						ebx += (uint) s[ i + 2 ] << 16;
						goto case 2;
					case 2:
						ebx += (uint) s[ i + 1 ] << 8;
						goto case 1;
					case 1:
						ebx += (uint) s[ i ];
						break;
				}

				esi = ( esi ^ edi ) - ( ( edi >> 18 ) ^ ( edi << 14 ) );
				ecx = ( esi ^ ebx ) - ( ( esi >> 21 ) ^ ( esi << 11 ) );
				edi = ( edi ^ ecx ) - ( ( ecx >> 7 ) ^ ( ecx << 25 ) );
				esi = ( esi ^ edi ) - ( ( edi >> 16 ) ^ ( edi << 16 ) );
				edx = ( esi ^ ecx ) - ( ( esi >> 28 ) ^ ( esi << 4 ) );
				edi = ( edi ^ edx ) - ( ( edx >> 18 ) ^ ( edx << 14 ) );
				eax = ( esi ^ edi ) - ( ( edi >> 8 ) ^ ( edi << 24 ) );

				return ( (ulong) edi << 32 ) | eax;
			}

			return ( (ulong) esi << 32 ) | eax;
		}

		/// <summary>
		/// Closes file and deletes resources.
		/// </summary>
		public void Dispose()
		{
			Dispose( false );
			GC.SuppressFinalize( this );
		}

		private void Dispose( bool onlyUnmanaged )
		{
			if ( !onlyUnmanaged )
			{
				if ( _Files != null )
				{
					_Files.Clear();
				}

				if ( _Reader != null )
				{
					_Reader.Dispose();
					_Reader = null;
				}

				if ( _Stream != null )
				{
					_Stream.Dispose();
					_Stream = null;
				}
			}
		}
		#endregion
	}
}
