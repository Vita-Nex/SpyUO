using System;

namespace Ultima.Package
{
	/// <summary>
	/// Describes spy exception.
	/// </summary>
	public class PackageException : Exception
	{
		#region Constructors
		/// <summary>
		/// Constructs a new instance of PackageException.
		/// </summary>
		/// <param name="format">Message format.</param>
		/// <param name="args">Message arguments.</param>
		public PackageException( string format, params object[] args ) : base( String.Format( format, args ) )
		{
		}
		#endregion
	}
}
