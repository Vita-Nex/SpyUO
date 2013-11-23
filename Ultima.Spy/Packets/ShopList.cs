using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ultima.Spy.Packets
{
	[UltimaPacket( "Shop List", UltimaPacketDirection.FromServer, 0x74 )]
	public class ShopListPacket : UltimaPacket, IUltimaEntity
	{
		private uint _Serial;

		[UltimaPacketProperty( "Vendor Serial", "0x{0:X}" )]
		public uint Serial
		{
			get { return _Serial; }
		}

		private List<ShopListItem> _Items;

		[UltimaPacketProperty]
		public List<ShopListItem> Items
		{
			get { return _Items; }
		}

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID
			reader.ReadInt16(); // Size

			_Serial = reader.ReadUInt32();

			int itemCount = reader.ReadByte();
			_Items = new List<ShopListItem>();

			for ( int i = 0; i < itemCount; i++ )
				_Items.Add( new ShopListItem( reader ) );
		}
	}

	public class ShopListItem
	{
		private int _Price;

		[UltimaPacketProperty]
		public int Price
		{
			get { return _Price; }
		}

		private string _Name;

		[UltimaPacketProperty]
		public string Name
		{
			get { return _Name; }
		}

		public ShopListItem( BigEndianReader reader )
		{
			_Price = reader.ReadInt32();
			_Name = reader.ReadAsciiString();
		}

		public override string ToString()
		{
			return String.Format( "{0} - {1}", _Name, _Price );
		}
	}
}
