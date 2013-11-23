using System;

namespace Ultima.Spy
{
	/// <summary>
	/// Describes spy exception.
	/// </summary>
	public class SpyException : Exception
	{
		#region Constructors
		/// <summary>
		/// Constructs a new instance of SpyException.
		/// </summary>
		/// <param name="format">Message format.</param>
		/// <param name="args">Message arguments.</param>
		public SpyException( string format, params object[] args ) : base( String.Format( format, args ) )
		{
		}
		#endregion
	}
}
