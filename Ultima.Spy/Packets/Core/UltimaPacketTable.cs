using System;
using System.Globalization;
using System.Xml;

namespace Ultima.Spy
{
	/// <summary>
	/// Describes packet table;
	/// </summary>
	public class UltimaPacketTable
	{
		#region Properties
		private byte _ID;

		/// <summary>
		/// Gets packet ID.
		/// </summary>
		public byte ID
		{
			get { return _ID; }
		}

		private string _Name;

		/// <summary>
		/// Gets table name.
		/// </summary>
		public string Name
		{
			get { return _Name; }
		}

		private bool _Word;

		/// <summary>
		/// Determines whether ID type if word (true) or byte (false).
		/// </summary>
		public bool Word
		{
			get { return _Word; }
		}

		private int _Offset;

		/// <summary>
		/// Gets id field offset in bytes.
		/// </summary>
		public int Offset
		{
			get { return _Offset; }
		}

		private object[] _Table;

		/// <summary>
		/// Gets table length.
		/// </summary>
		public int Length
		{
			get { return _Table.Length; }
		}

		/// <summary>
		/// Gets or sets table items.
		/// </summary>
		/// <param name="i">Index to get/set item.</param>
		/// <returns>Item at specific index.</returns>
		public object this[ int i ]
		{
			get { return _Table[ i ]; }
			set { _Table[ i ] = value; }
		}

		private string _Ids;

		/// <summary>
		/// Gets list of IDs for this table.
		/// </summary>
		public string Ids
		{
			get { return _Ids; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of PacketTable.
		/// </summary>
		/// <param name="parentIds">Parent IDs.</param>
		/// <param name="e">Xml element to construct from.</param>
		public UltimaPacketTable( string parentIds, XmlElement e )
		{
			// Parse
			string id = e.GetAttribute( "id" );

			if ( String.IsNullOrWhiteSpace( id ) )
				throw new SpyException( "Missing attribute 'id' in element 'table'" );

			if ( id.StartsWith( "0x" ) )
			{
				id = id.Substring( 2, id.Length - 2 );

				if ( !Byte.TryParse( id, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _ID ) )
					throw new SpyException( "Attribute 'id' in element 'table' must be byte" );
			}
			else
			{
				if ( !Byte.TryParse( id, NumberStyles.Integer, CultureInfo.InvariantCulture, out _ID ) )
					throw new SpyException( "Attribute 'id' in element 'table' must be byte" );
			}

			_Name = e.GetAttribute( "name" );
			_Table = new object[ Byte.MaxValue + 1 ];

			if ( String.IsNullOrWhiteSpace( _Name ) )
				throw new SpyException( "Missing attribute 'name' in element 'table'" );

			string type = e.GetAttribute( "type" );

			if ( !String.IsNullOrWhiteSpace( type ) )
			{
				type = type.ToLower();

				if ( String.Equals( type, "byte" ) )
					_Word = false;
				else if ( String.Equals( type, "word" ) )
					_Word = true;
				else
					throw new SpyException( "Attribute 'type' in element 'table' must be either 'byte' or 'word'" );
			}
			else
				_Word = false;

			string offset = e.GetAttribute( "offset" );

			if ( !String.IsNullOrWhiteSpace( offset ) )
			{
				if ( !Int32.TryParse( offset, out _Offset ) )
					throw new SpyException( "Attribute 'offset' in element 'table' must be integer" );
			}
			else
				_Offset = 0;

			// Construct Ids
			if ( String.IsNullOrEmpty( parentIds ) )
				_Ids = String.Format( "{0:X2}", _ID );
			else
				_Ids = String.Format( "{0}.{1:X2}", parentIds, _ID );

			// Parse children
			foreach ( XmlNode node in e.ChildNodes )
			{
				XmlElement nodeElement = node as XmlElement;

				if ( nodeElement != null && String.Equals( nodeElement.Name, "table", StringComparison.InvariantCultureIgnoreCase ) )
				{
					UltimaPacketTable table = new UltimaPacketTable( _Ids, nodeElement );
					_Table[ table.ID ] = table;
				}
			}
		}

		/// <summary>
		/// Constructs a new instance of PacketTable.
		/// </summary>
		/// <param name="name">Gets table name.</param>
		/// <param name="word">Determine ID field type (true - word, false - byte ).</param>
		/// <param name="offset">Determine ID field offset in bytes.</param>
		public UltimaPacketTable( string name, bool word = false, int offset = 0 )
		{
			_Name = name;
			_Word = word;
			_Offset = offset;
			_Ids = null;
			_Table = new object[ Byte.MaxValue + 1 ];
		}
		#endregion

		#region Methods
		/// <summary>
		/// Gets packet info based on packet header.
		/// </summary>
		/// <param name="data">Packet data.</param>
		/// <param name="fromClient">Packet direction.</param>
		/// <param name="id">Last packet id.</param>
		/// <param name="ids">All packet ids.</param>
		/// <returns>Packet definition if exists, null otherwise.</returns>
		public UltimaPacketDefinition GetPacket( byte[] data, bool fromClient, ref byte id, ref string ids )
		{
			UltimaPacketTable table = this;
			int offset = 0;

			while ( table != null )
			{
				id = data[ offset ];
				object item = table[ id ];

				if ( item != null )
				{
					UltimaPacketTableEntry entry = item as UltimaPacketTableEntry;

					if ( entry != null && (
						( fromClient && entry.FromClient != null ) ||
						( !fromClient && entry.FromServer != null ) ) )
					{
						// Found packet definition
						if ( _Ids == null )
							ids = id.ToString( "X2" );
						else
							ids = _Ids + "." + id.ToString( "X2" );

						if ( fromClient )
							return entry.FromClient;
						else
							return entry.FromServer;
					}
					else if ( entry == null )
					{
						// Need to look in mordor
						offset += ( table.Word ? 2 : 1 );
						table = (UltimaPacketTable) item;
						offset += table.Offset;
					}
					else
						break;
				}
				else
				{
					// Unknown packet
					if ( _Ids == null )
						ids = id.ToString( "X2" );
					else
						ids = _Ids + "." + id.ToString( "X2" );

					return null;
				}
			}

			// Panic at the disco
			return null;
		}

		/// <summary>
		/// Tries to register packet by UltimaPacketAttribute.
		/// </summary>
		/// <param name="packetType">Type to register.</param>
		/// <param name="packetIds">Packet IDs.</param>
		/// <param name="index">Current packet ID index.</param>
		public void RegisterPacket( Type packetType, UltimaPacketAttribute packet, int index )
		{
			byte id = packet.Ids[ index ];
			object item = _Table[ id ];

			if ( packet.Ids.Length - 1 == index )
			{
				UltimaPacketTableEntry entry = item as UltimaPacketTableEntry;

				if ( item is UltimaPacketTable )
					throw new SpyException( "Packet '{0}' is missing one or more IDs", packetType );

				if ( entry == null )
					entry = new UltimaPacketTableEntry();

				if ( packet.Direction == UltimaPacketDirection.FromClient )
				{
					if ( entry.FromClient != null )
						throw new SpyException( "Packet from client with ID '{0}' already exists", id );

					entry.FromClient = new UltimaPacketDefinition( packetType, packet );
				}
				else if ( packet.Direction == UltimaPacketDirection.FromServer )
				{
					if ( entry.FromServer != null )
						throw new SpyException( "Packet from server with ID '{0}' already exists", id );

					entry.FromServer = new UltimaPacketDefinition( packetType, packet );
				}
				else if ( packet.Direction == UltimaPacketDirection.FromBoth )
				{
					if ( entry.FromClient != null )
						throw new SpyException( "Packet from client with ID '{0}' already exists", id );
					else if ( entry.FromServer != null )
						throw new SpyException( "Packet from server with ID '{0}' already exists", id );

					entry.FromClient = new UltimaPacketDefinition( packetType, packet );
					entry.FromServer = entry.FromClient;
				}

				_Table[ id ] = entry;
			}
			else if ( item is UltimaPacketTable )
			{
				UltimaPacketTable table = (UltimaPacketTable) item;
				table.RegisterPacket( packetType, packet, index + 1 );
			}
			else
				throw new SpyException( "Table for Packet '{0}' not defined", packetType );
		}
		#endregion
	}
}
