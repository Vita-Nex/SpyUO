using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace Ultima.Spy.Packets
{
	[UltimaPacket( "Query Properties Response", UltimaPacketDirection.FromServer, 0xD6 )]
	public class QueryPropertiesResponsePacket : UltimaPacket, IUltimaEntity
	{
		private uint _Serial;

		[UltimaPacketProperty( "Serial", "0x{0:X}" )]
		public uint Serial
		{
			get { return _Serial; }
		}

		private uint _Hash;

		[UltimaPacketProperty( "Hash", "0x{0:X}" )]
		public uint Hash
		{
			get { return _Hash; }
		}

		private List<QueryPropertiesProperty> _Properties;

		[UltimaPacketProperty]
		public List<QueryPropertiesProperty> Properties
		{
			get { return _Properties; }
		}

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID
			reader.ReadInt16(); // Size
			reader.ReadInt16();
			_Serial = reader.ReadUInt32();
			reader.ReadInt16();
			_Hash = reader.ReadUInt32();

			_Properties = new List<QueryPropertiesProperty>();
			int cliloc;

			while ( ( cliloc = reader.ReadInt32() ) != 0 )
				_Properties.Add( new QueryPropertiesProperty( cliloc, reader ) );
		}
	}

	public class QueryPropertiesProperty
	{
		private int _Cliloc;

		[UltimaPacketProperty( UltimaPacketPropertyType.Cliloc )]
		public int Cliloc
		{
			get { return _Cliloc; }
		}

		private string _Arguments;

		[UltimaPacketProperty]
		public string Arguments
		{
			get { return _Arguments; }
		}

		public QueryPropertiesProperty( int cliloc, BigEndianReader reader )
		{
			_Cliloc = cliloc;

			if ( _Cliloc > 0 )
				_Arguments = reader.ReadUnicodeString();
		}

		public override string ToString()
		{
			return _Cliloc.ToString();
		}
	}
}
