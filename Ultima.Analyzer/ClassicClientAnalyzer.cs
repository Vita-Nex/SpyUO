using System;

namespace Ultima.Analyzer
{
	/// <summary>
	/// Classic client analyzer.
	/// </summary>
	public class ClassicClientAnalyzer : ClientAnalyzer
	{
		#region Properties
		private int _DebugProtectionAddress1;

		/// <summary>
		/// Gets debug protection address.
		/// </summary>
		public int DebugProtectionAddress1
		{
			get { return _DebugProtectionAddress1; }
		}

		/// <summary>
		/// Determines whether analyzer found debug protection function.
		/// </summary>
		public bool FoundDebugProtectionAddress1
		{
			get { return _DebugProtectionAddress1 != 0; }
		}

		private int _DebugProtectionAddress2;

		/// <summary>
		/// Gets debug protection address.
		/// </summary>
		public int DebugProtectionAddress2
		{
			get { return _DebugProtectionAddress2; }
		}

		/// <summary>
		/// Determines whether analyzer found debug protection function.
		/// </summary>
		public bool FoundDebugProtectionAddress2
		{
			get { return _DebugProtectionAddress2 != 0; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of ClassicClientAnalyzer and
		/// performs basic analysis.
		/// </summary>
		/// <param name="filePath">Path to .exe to analyze.</param>
		public ClassicClientAnalyzer( string filePath ) : base( filePath )
		{
			_DebugProtectionAddress1 = 0;
			_DebugProtectionAddress2 = 0;
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
			_DebugProtectionAddress1 = 0;
			_DebugProtectionAddress2 = 0;

			int send = 0;
			int recieve = 0;

			for ( int i = 0; i < data.Length; i++ )
			{
				if ( send == 0 && CheckArray( data, i, Statics.SendSignature ) )
					send = i;

				if ( recieve == 0 && CheckArray( data, i, Statics.RecieveSignature ) )
					recieve = i;

				if ( _DebugProtectionAddress1 == 0 && CheckArray( data, i, Statics.DebugProtectionSignature1 ) )
					_DebugProtectionAddress1 = i + 17;

				if ( _DebugProtectionAddress2 == 0 && CheckArray( data, i, Statics.DebugProtectionSignature2 ) )
					_DebugProtectionAddress2 = i + 13;
			}

			if ( recieve != 0 )
				recieve += ImageBase;

			if ( send != 0 )
				send += ImageBase;

			if ( _DebugProtectionAddress1 != 0 )
				_DebugProtectionAddress1 += ImageBase;

			if ( _DebugProtectionAddress2 != 0 )
				_DebugProtectionAddress2 += ImageBase;

			_SpyInfo = new SpyInfo( recieve, 7, 6, send, 5, 2 );
		}
		#endregion
	}
}
