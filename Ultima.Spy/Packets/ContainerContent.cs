using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Collections.Generic;

namespace Ultima.Spy.Packets
{
	[UltimaPacket( "Container Content", UltimaPacketDirection.FromServer, 0x3C )]
	public class ContainerContentPacket : UltimaPacket, IUltimaEntity
	{
		private uint _Serial;

		[UltimaPacketProperty( "Serial", "0x{0:X}" )]
		public uint Serial
		{
			get { return _Serial; }
		}

		private List<ContainerItem> _Items;

		[UltimaPacketProperty]
		public List<ContainerItem> Items
		{
			get { return _Items; }
		}

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID
			reader.ReadInt16(); // Size

			int count = reader.ReadInt16();
			_Items = new List<ContainerItem>();

			for ( int i = 0; i < count; i++ )
			{
				_Items.Add( new ContainerItem( this, reader ) );
				_Serial = _Items[ 0 ].ParentSerial;
			}
		}
	}

	public class ContainerItem
	{
		private UltimaPacket _Parent;

		public UltimaPacket Parent
		{
			get { return _Parent; }
		}

		private uint _Serial;

		[UltimaPacketProperty( "Serial", "0x{0:X}" )]
		public uint Serial
		{
			get { return _Serial; }
		}

		private int _ItemID;

		[UltimaPacketProperty( "Item ID", UltimaPacketPropertyType.Texture )]
		public int ItemID
		{
			get { return _ItemID; }
		}

		private byte _ItemIDOffset;

		[UltimaPacketProperty( "Item ID OFfset" )]
		public byte ItemIDOffset
		{
			get { return _ItemIDOffset; }
		}

		private int _Amount;

		[UltimaPacketProperty]
		public int Amount
		{
			get { return _Amount; }
		}

		private int _X;

		[UltimaPacketProperty]
		public int X
		{
			get { return _X; }
		}

		private int _Y;

		[UltimaPacketProperty]
		public int Y
		{
			get { return _Y; }
		}

		private byte _GridLocation;

		[UltimaPacketProperty( "Grid Location" )]
		public byte GridLocation
		{
			get { return _GridLocation; }
		}

		private uint _ParentSerial;

		[UltimaPacketProperty( "Parent Serial", "0x{0:X}" )]
		public uint ParentSerial
		{
			get { return _ParentSerial; }
		}

		private int _Hue;

		[UltimaPacketProperty]
		public int Hue
		{
			get { return _Hue; }
		}

		public ContainerItem( UltimaPacket parent, BigEndianReader reader )
		{
			_Parent = parent;
			_Serial = reader.ReadUInt32();
			_ItemID = reader.ReadInt16();
			_ItemIDOffset = reader.ReadByte();
			_Amount = reader.ReadInt16();
			_X = reader.ReadInt16();
			_Y = reader.ReadInt16();
			_GridLocation = reader.ReadByte();
			_ParentSerial = reader.ReadUInt32();
			_Hue = reader.ReadInt16();
		}

		public override string ToString()
		{
			return String.Format( "ItemID: {0}", _ItemID );
		}
	}
}
