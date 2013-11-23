using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Ultima.Spy
{
	/// <summary>
	/// Describes ultima packet.
	/// </summary>
	public class UltimaPacket
	{
		#region Packet Definitions
		private static UltimaPacketTable _PacketTable = null;

		/// <summary>
		/// Gets packet table.
		/// </summary>
		public static UltimaPacketTable PacketTable
		{
			get { return _PacketTable; }
		}

		private static UltimaPacketDefinition _DefaultDefinition;

		/// <summary>
		/// Gets default packet definition.
		/// </summary>
		public static UltimaPacketDefinition DefaultDefinition
		{
			get { return _DefaultDefinition; }
		}

		/// <summary>
		/// Checks all types in executing assmebly and registers packets.
		/// </summary>
		public static void RegisterPackets()
		{
			// Load tables
			LoadTables( "Data\\PacketTables.xml" );

			// Load packets
			Type[] types = Assembly.GetExecutingAssembly().GetTypes();

			foreach ( Type type in types )
			{
				UltimaPacketAttribute[] attrs = (UltimaPacketAttribute[]) type.GetCustomAttributes( typeof( UltimaPacketAttribute ), false );

				if ( attrs.Length == 1 )
				{
					UltimaPacketAttribute ultimaPacket = attrs[ 0 ];

					if ( ultimaPacket.Ids == null || ultimaPacket.Ids.Length == 0 )
						throw new SpyException( "Packet {0} must have at least one ID", type );

					_PacketTable.RegisterPacket( type, ultimaPacket, 0 );
				}
				else if ( attrs.Length > 1 )
				{
					throw new SpyException( "Class {0} has too many UltimaPacket attributes", type );
				}
			}

			_DefaultDefinition = new UltimaPacketDefinition( typeof( UltimaPacket ), null );
		}

		private static void LoadTables( string filePath )
		{
			_PacketTable = new UltimaPacketTable( "Packet Table" );
			XmlDocument document = new XmlDocument();
			document.Load( filePath );

			XmlElement root = document[ "tables" ];

			if ( root != null )
			{
				foreach ( XmlNode node in root.ChildNodes )
				{
					XmlElement nodeElement = node as XmlElement;

					if ( nodeElement != null && String.Equals( nodeElement.Name, "table", StringComparison.InvariantCultureIgnoreCase ) )
					{
						UltimaPacketTable table = new UltimaPacketTable( null, nodeElement );
						_PacketTable[ table.ID ] = table;
					}
				}
			}
			else
				throw new SpyException( "Empty XML file '{0}'", filePath );
		}

		/// <summary>
		/// Constructs packet based on packet IDs.
		/// </summary>
		/// <param name="data">Data client sent or received.</param>
		/// <param name="fromClient">Determines whehter the client sent or received data.</param>
		/// <param name="time">Date and time packet was received.</param>
		/// <returns>Ultima packet.</returns>
		public static UltimaPacket ConstructPacket( byte[] data, bool fromClient, DateTime time )
		{
			byte id = 0;
			string ids = null;
			UltimaPacketDefinition definition = _PacketTable.GetPacket( data, fromClient, ref id, ref ids );
			UltimaPacket packet = null;

			if ( definition == null )
			{
				definition = _DefaultDefinition;
				packet = new UltimaPacket();
			}
			else
				packet = definition.Constructor();

			packet._Definition = definition;
			packet._Data = data;
			packet._FromClient = fromClient;
			packet._DateTime = time;
			packet._ID = id;
			packet._Ids = ids;

			if ( definition.Attribute != null )
				packet._Name = definition.Attribute.Name;
			else
				packet._Name = "Unknown Packet";

			using ( MemoryStream stream = new MemoryStream( data ) )
			{
				packet.Parse( new BigEndianReader( stream ) );
			}

			return packet;
		}

		/// <summary>
		/// Constructs packet from a binary stream.
		/// </summary>
		/// <param name="reader">Stream to create packet from.</param>
		/// <returns>Ultima packet.</returns>
		public static UltimaPacket ConstructPacket( BinaryReader reader )
		{
			bool fromClient = reader.ReadBoolean();
			long ticks = reader.ReadInt64();
			int length = reader.ReadInt32();
			byte[] data = reader.ReadBytes( length );

			if ( data.Length > 1 && data[ 0 ] != 0x80 && data[ 0 ] != 0x91 )
				return ConstructPacket( data, fromClient, new DateTime( ticks ) );

			return null;
		}

		/// <summary>
		/// Default constructors.
		/// </summary>
		static UltimaPacket()
		{
			RegisterPackets();
		}
		#endregion

		#region Properties
		private UltimaPacketDefinition _Definition;

		/// <summary>
		/// Gets or sets packet defintion.
		/// </summary>
		public UltimaPacketDefinition Definition
		{
			get { return _Definition; }
		}

		private bool _FromClient;

		/// <summary>
		/// Determines whehter the client sent or received data.
		/// </summary>
		[UltimaPacketProperty( "Direction", UltimaPacketPropertyType.Direction )]
		public bool FromClient
		{
			get { return _FromClient; }
		}

		private DateTime _DateTime;

		/// <summary>
		/// Gets or sets packet date and time.
		/// </summary>
		[UltimaPacketProperty( "Time", "{0:H:mm:ss fff}" )]
		public DateTime DateTime
		{
			get { return _DateTime; }
		}

		private byte[] _Data;

		/// <summary>
		/// Data client sent or received.
		/// </summary>
		public byte[] Data
		{
			get { return _Data; }
		}

		private byte _ID;

		/// <summary>
		/// Gets packet ID.
		/// </summary>
		public byte ID
		{
			get { return _ID; }
		}

		private string _Ids;

		/// <summary>
		/// Gets packet ids.
		/// </summary>
		public string Ids
		{
			get { return _Ids; }
		}

		private string _Name;

		/// <summary>
		/// Gets packet name.
		/// </summary>
		public string Name
		{
			get { return _Name; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaPacket.
		/// </summary>
		public UltimaPacket()
		{
		}
		#endregion

		#region Methods
		/// <summary>
		/// Each packet must override this function.
		/// </summary>
		/// <param name="reader">Reader to read from.</param>
		protected virtual void Parse( BigEndianReader reader )
		{
		}

		/// <summary>
		/// Saves binary data to stream.
		/// </summary>
		/// <param name="writer">Stream to write to.</param>
		public void Save( BinaryWriter writer )
		{
			int length = _Data.Length;

			if ( _Data.Length > 0 && ( _Data[ 0 ] == 0x80 || _Data[ 0 ] == 0x91 ) )
				length = 1;

			writer.Write( (bool) _FromClient );
			writer.Write( (long) _DateTime.Ticks );
			writer.Write( (int) length );
			writer.Write( _Data, 0, length );
		}

		/// <summary>
		/// Returns string representaion of this class.
		/// </summary>
		/// <returns>String representaion.</returns>
		public override string ToString()
		{
			string binary = GetBinaryData( 100 );

			if ( _Data.Length > 100 )
				return binary + "...";

			return binary;
		}

		/// <summary>
		/// Gets binary data.
		/// </summary>
		/// <returns></returns>
		public string GetBinaryData()
		{
			return GetBinaryData( _Data.Length );
		}

		private string GetBinaryData( int maxLength )
		{
			int length = Math.Min( maxLength, _Data.Length );
			char[] bytes = new char[ length * 2 ];
			byte b1, b2;

			for ( int i = 0; i < length; i++ )
			{
				b1 = (byte) ( _Data[ i ] >> 4 );
				b2 = (byte) ( _Data[ i ] & 0xF );

				bytes[ i * 2 ] = (char) ( b1 > 9 ? b1 + 0x37 : b1 + 0x30 );
				bytes[ i * 2 + 1 ] = (char) ( b2 > 9 ? b2 + 0x37 : b2 + 0x30 );
			}

			return new string( bytes );
		}
		#endregion
	}
}
