using System;
using System.Runtime.InteropServices;

namespace Ultima.Package
{
	/// <summary>
	/// Zlib error.
	/// </summary>
	public enum ZLibError
	{
		Okay			= 0,
		StreamEnd		= 1,
		NeedDictionary	= 2,
		FileError		= -1,
		StreamError		= -2,
		DataError		= -3,
		MemoryError		= -4,
		BufferError		= -5,
		VersionError	= -6,
	}

	/// <summary>
	/// Compression library.
	/// </summary>
	public class Zlib
	{
		#region Methods
		[DllImport( "zlibwapi", EntryPoint = "zlibVersion" )]
		private static extern string zlibVersion();

		/// <summary>
		/// Version of the library.
		/// </summary>
		public static string Version
		{
			get
			{
				return zlibVersion(); 
			}
		}

		[DllImport( "zlibwapi", EntryPoint = "uncompress" )]
		private static extern ZLibError uncompress( byte[] dest, ref int destLen, byte[] source, int sourceLen );

		/// <summary>
		/// Decompresses array of bytes.
		/// </summary>
		/// <param name="dest">Destination byte array.</param>
		/// <param name="destLength">Destination length (Sets it).</param>
		/// <param name="source">Source byte array.</param>
		/// <param name="sourceLength">Source length.</param>
		/// <returns>Error code.</returns>
		public static ZLibError Decompress( byte[] dest, ref int destLength, byte[] source, int sourceLength )
		{
			return uncompress( dest, ref destLength, source, sourceLength );
		}
		#endregion
	}
}
