using System;
using System.Text;

namespace Ultima.Spy
{
	/// <summary>
	/// Ultima packet direction.
	/// </summary>
	public enum UltimaPacketDirection
	{
		FromClient,
		FromServer,
		FromBoth,
	}

	/// <summary>
	/// Ultima packet attribute.
	/// </summary>
	[AttributeUsage( AttributeTargets.Class )]
	public class UltimaPacketAttribute : Attribute
	{
		#region Properties
		private string _Name;

		/// <summary>
		/// Gets packet name.
		/// </summary>
		public string Name
		{
			get { return _Name; }
		}

		private UltimaPacketDirection _Direction;

		/// <summary>
		/// Gets packet direction.
		/// </summary>
		public UltimaPacketDirection Direction
		{
			get { return _Direction; }
		}

		private byte[] _Ids;

		/// <summary>
		/// Gets packet IDs.
		/// </summary>
		public byte[] Ids
		{
			get { return _Ids; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaPacketAttribute.
		/// </summary>
		/// <param name="name">Packet name.</param>
		/// <param name="direction">Packet direction.</param>
		/// <param name="ids">Packet IDs.</param>
		public UltimaPacketAttribute( string name, UltimaPacketDirection direction, params byte[] ids )
		{
			_Name = name;
			_Direction = direction;
			_Ids = ids;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Builds string which represents this packet.
		/// </summary>
		/// <returns>Returns packet ids and name.</returns>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();

			foreach ( byte id in _Ids )
				builder.Append( id.ToString( "X" ) + " " );

			builder.Remove( builder.Length - 1, 1 );

			return String.Format( "{0} - {1}", builder, _Name );
		}
		#endregion
	}
}
