using System.IO;

namespace Ultima.Package
{
	public enum FileCompression
	{
		/// <summary>
		/// No compression
		/// </summary>
		None,

		/// <summary>
		/// ZLIB compression.
		/// </summary>
		Zlib,
	}

	public class UltimaPackageFile
	{
		#region Properties
		/// <summary>
		/// File header size.
		/// </summary>
		public const int FileHeaderSize = 34;


		private UltimaPackage _Package;

		/// <summary>
		/// Gets package that owns this file.
		/// </summary>
		public UltimaPackage Package
		{
			get { return _Package; }
		}

		private long _FileAddress;

		/// <summary>
		/// Gets or sets file address.
		/// </summary>
		public long FileAddress
		{
			get { return _FileAddress; }
		}

		private FileCompression _Compression;

		/// <summary>
		/// Gets or sets compression type.
		/// </summary>
		public FileCompression Compression
		{
			get { return _Compression; }
		}

		private int _CompressedSize;

		/// <summary>
		/// Gets compressed size.
		/// </summary>
		public int CompressedSize
		{
			get { return _CompressedSize; }
		}

		private int _DecompressedSize;

		/// <summary>
		/// Gets decompressed size.
		/// </summary>
		public int DecompressedSize
		{
			get { return _DecompressedSize; }
		}

		private ulong _FileNameHash;

		/// <summary>
		/// Gets file name hash.
		/// </summary>
		public ulong FileNameHash
		{
			get { return _FileNameHash; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaPackageFile.
		/// </summary>
		/// <param name="package">Pacakge that contains this file.</param>
		/// <param name="reader">Reader to read from.</param>
		public UltimaPackageFile( UltimaPackage package, BinaryReader reader )
		{
			_Package = package;
			_FileAddress = reader.ReadInt64();
			_FileAddress += reader.ReadInt32();
			_CompressedSize = reader.ReadInt32();
			_DecompressedSize = reader.ReadInt32();
			_FileNameHash = reader.ReadUInt64();

			reader.ReadInt32(); // Header hash

			switch ( reader.ReadInt16() )
			{
				case 0: _Compression = FileCompression.None; break;
				case 1: _Compression = FileCompression.Zlib; break;
			}
		}
		#endregion
	}
}
