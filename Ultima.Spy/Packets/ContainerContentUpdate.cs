using System.Collections.ObjectModel;
using System.IO;

namespace Ultima.Spy.Packets
{
	[UltimaPacket( "Container Content Update", UltimaPacketDirection.FromServer, 0x25 )]
	public class ContainerContentUpdatePacket : UltimaPacket, IUltimaEntity
	{
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

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID
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
	}
}
