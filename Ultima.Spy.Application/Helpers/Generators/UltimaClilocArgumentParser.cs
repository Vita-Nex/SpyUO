using System;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Descrbies ultima cliloc argument parser.
	/// </summary>
	public class UltimaClilocArgumentParser
	{
		#region Properties
		private static readonly char[] Separators = { '\t' };

		private string[] _Arguments;

		/// <summary>
		/// Gets or sets cliloc arguments.
		/// </summary>
		public string[] Arguments
		{
			get { return _Arguments; }
			set { _Arguments = value; }
		}

		/// <summary>
		/// Gets or sets argument at specific index.
		/// </summary>
		/// <param name="index">Index to get or set.</param>
		/// <returns>Argument.</returns>
		public string this[ int index ]
		{
			get
			{
				if ( _Arguments != null && index < _Arguments.Length )
					return _Arguments[ index ];

				return null;
			}
			set
			{
				if ( _Arguments != null && index < _Arguments.Length )
					_Arguments[ index ] = value;
			}
		}

		/// <summary>
		/// Gets arguments length.
		/// </summary>
		public int Length
		{
			get
			{
				if ( _Arguments != null )
					return _Arguments.Length;

				return 0;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaClilocArgumentParser.
		/// </summary>
		/// <param name="arguments">Cliloc arguments.</param>
		public UltimaClilocArgumentParser( string arguments )
		{
			if ( arguments != null )
				_Arguments = arguments.Split( Separators );
		}
		#endregion

		#region Methods
		/// <summary>
		/// Tries to get parse argument as integer.
		/// </summary>
		/// <param name="index">Argument index.</param>
		/// <returns>True if integer, false otherwise.</returns>
		public bool TryGetInteger( int index, out int integer )
		{
			integer = 0;

			if ( index >= _Arguments.Length )
				return false;

			if ( Int32.TryParse( _Arguments[ index ], out integer ) )
				return true;

			return false;
		}

		/// <summary>
		/// Gets argument as integer.
		/// </summary>
		/// <param name="index">Argument index.</param>
		/// <returns>Argument as integer.</returns>
		public int GetCliloc( int index )
		{
			if ( index >= _Arguments.Length )
				return 0;

			int integer = 0;
			string v = _Arguments[ index ];

			if ( !v.StartsWith( "#" ) )
				return 0;

			if ( Int32.TryParse( v.Substring( 1, v.Length - 1 ), out integer ) )
				return integer;

			return 0;
		}
		#endregion
	}
}
