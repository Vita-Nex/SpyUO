using System;

namespace Ultima.Analyzer
{
	/// <summary>
	/// Enhanced client analyzer.
	/// </summary>
	public class EnhancedClientAnalyzer : ClientAnalyzer
	{
		#region Properties
		private int _FileNameHashFunctionAddress;

		/// <summary>
		/// Gets file name hash function address.
		/// </summary>
		public int FileNameHashFunctionAddress
		{
			get { return _FileNameHashFunctionAddress; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of EnhancedClientAnalyzer and
		/// performs basic analysis.
		/// </summary>
		/// <param name="filePath">Path to .exe to analyze.</param>
		public EnhancedClientAnalyzer( string filePath ) : base( filePath )
		{
			_FileNameHashFunctionAddress = 0;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Detailed client analysis.
		/// </summary>
		/// <param name="data">Client data.</param>
		protected override void Analyze( byte[] data )
		{
			_SpyInfo = null;
			_FileNameHashFunctionAddress = 0;

			int send = 0;
			int recieve = 0;

			for ( int i = 0; i < data.Length; i++ )
			{
				if ( send == 0 && CheckArray( data, i, Statics.EnhancedSendSignature ) )
					send = i;

				if ( recieve == 0 && CheckArray( data, i, Statics.EnhancedRecieveSignature ) )
					recieve = i;

				if ( _FileNameHashFunctionAddress == 0 && CheckArray( data, i, Statics.FileNameSignature ) )
					_FileNameHashFunctionAddress = i + 1;
			}

			if ( recieve != 0 )
				recieve += ImageBase + 0x25;

			if ( send != 0 )
				send += ImageBase;

			_SpyInfo = new SpyInfo( recieve, 3, 5, send, 3, 2 );
		}
		#endregion
	}
}
