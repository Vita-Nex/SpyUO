using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System;

namespace Ultima.Spy.Packets
{
	[UltimaPacket( "Query Properties Request", UltimaPacketDirection.FromClient, 0xBF, 0x10 )]
	public class QueryPropertiesRequestPacket : UltimaPacket, IUltimaEntity
	{
		private uint _Serial;

		[UltimaPacketProperty( "Serial", "0x{0:X}" )]
		public uint Serial
		{
			get { return _Serial; }
		}

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID
			reader.ReadInt16(); // Size
			reader.ReadByte(); // Command

			_Serial = reader.ReadUInt32();
		}
	}

	[UltimaPacket( "Batch Query Properties Request", UltimaPacketDirection.FromClient, 0xD6 )]
	public class BatchQueryPropertiesRequestPacket : UltimaPacket
	{
		private List<QueryPropertiesItem> _Items;

		[UltimaPacketProperty]
		public List<QueryPropertiesItem> Items
		{
			get { return _Items; }
		}

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID
			reader.ReadInt16(); // Size

			_Items = new List<QueryPropertiesItem>();
			uint serial = 0;
			int maxCount = ( Data.Length - 3 ) / 4;
			int count = 0;

			while ( count++ < maxCount && ( serial = reader.ReadUInt32() ) != 0 )
				_Items.Add( new QueryPropertiesItem( this, serial ) );
		}
	}

	public class QueryPropertiesItem
	{
		private UltimaPacket _Parent;

		public UltimaPacket  Parent
		{
			get { return _Parent; }
		}

		private uint _Serial;

		[UltimaPacketProperty( "Serial", "0x{0:X}" )]
		public uint Serial
		{
			get { return _Serial; }
		}

		public QueryPropertiesItem( UltimaPacket parent, uint serial )
		{
			_Parent = parent;
			_Serial = serial;
		}

		public override string ToString()
		{
			return String.Format( "0x{0:X}", _Serial );
		}
	}
}
