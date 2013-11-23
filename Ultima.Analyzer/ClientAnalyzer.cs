using System;
using System.IO;
using System.Diagnostics;

namespace Ultima.Analyzer
{
	/// <summary>
	/// Provides instance methods for finding UO keys.
	/// </summary>
	public class ClientAnalyzer
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

		private FileInfo _FileInfo;

		/// <summary>
		/// Basic file information.
		/// </summary>
		public FileInfo FileInfo
		{
			get { return _FileInfo; }
		}

		private FileVersionInfo _Version;

		/// <summary>
		/// Provides version information.
		/// </summary>
		public FileVersionInfo Version
		{
			get { return _Version; }
		}

		private int _TimeDateStamp;

		/// <summary>
		/// The low 32 bits of the time stamp of the image. This represents 
		/// the date and time the image was created by the linker.
		/// </summary>
		public int TimeDateStamp
		{
			get { return _TimeDateStamp; }
		}

		private int _ImageBase;

		/// <summary>
		/// The preferred address of the first byte of the image when it is loaded in memory.
		/// </summary>
		public int ImageBase
		{
			get { return _ImageBase; }
		}

		protected SpyInfo _SpyInfo;

		/// <summary>
		/// Gets SpyUO keys.
		/// </summary>
		public SpyInfo SpyInfo
		{
			get { return _SpyInfo; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of UltimaAnalyzer.
		/// </summary>
		/// <param name="filePath">File.</param>
		public ClientAnalyzer( string filePath )
		{
			_FilePath = filePath;
			_Version = FileVersionInfo.GetVersionInfo( filePath );
			_FileInfo = new FileInfo( filePath );

			using ( FileStream stream = new FileStream( filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
			{
				using ( BinaryReader reader = new BinaryReader( stream ) )
				{
					// skip to COFF header
					stream.Seek( 0x3C, SeekOrigin.Begin );
					int peOffset = reader.ReadInt32();
					stream.Seek( peOffset + 8, SeekOrigin.Begin );

					// time date stamp
					_TimeDateStamp = reader.ReadInt32();
					stream.Seek( peOffset + 52, SeekOrigin.Begin );

					// image base
					_ImageBase = reader.ReadInt32();
					stream.Seek( 0, SeekOrigin.Begin );
				}
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Detailed client analysis.
		/// </summary>
		public void Analyze()
		{
			using ( FileStream stream = new FileStream( _FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
			{
				using ( BinaryReader reader = new BinaryReader( stream ) )
				{
					byte[] data = new byte[ stream.Length ];

					if ( stream.Read( data, 0, (int) stream.Length ) != stream.Length )
						throw new FileLoadException();

					Analyze( data );
				}
			}
		}

		/// <summary>
		/// Detailed client analysis.
		/// </summary>
		/// <param name="data">Client data.</param>
		protected virtual void Analyze( byte[] data )
		{
		}

		/// <summary>
		/// Checks if two arrays match.
		/// </summary>
		/// <param name="data">Data to check.</param>
		/// <param name="start">Data length.</param>
		/// <param name="with">Data to check against.</param>
		/// <returns>True if successfull, false otherwise.</returns>
		protected bool CheckArray( byte[] data, int start, byte[] with )
		{
			if ( start > data.Length - with.Length )
				return false;

			for ( int i = start, j = 0; j < with.Length; i++, j++ )
			{
				if ( data[ i ] != with[ j ] && with[ j ] != 0xCC )
					return false;
			}

			return true;
		}
		#endregion
	}
}
