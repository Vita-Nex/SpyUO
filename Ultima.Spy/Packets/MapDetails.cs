using System.Collections.ObjectModel;
using System.IO;

namespace Ultima.Spy.Packets
{
	[UltimaPacket( "Map Details", UltimaPacketDirection.FromServer, 0xF5 )]
	public class MapDetailsPacket : UltimaPacket, IUltimaEntity
	{
		private uint _Serial;

		[UltimaPacketProperty( "Serial", "0x{0:X}" )]
		public uint Serial
		{
			get { return _Serial; }
		}

		private int _CornerImage;

		[UltimaPacketProperty( "Corner Image" )]
		public int CornerImage
		{
			get { return _CornerImage; }
		}

		private int _X1;

		[UltimaPacketProperty]
		public int X1
		{
			get { return _X1; }
		}

		private int _Y1;

		[UltimaPacketProperty]
		public int Y1
		{
			get { return _Y1; }
		}

		private int _X2;

		[UltimaPacketProperty]
		public int X2
		{
			get { return _X2; }
		}

		private int _Y2;

		[UltimaPacketProperty]
		public int Y2
		{
			get { return _Y2; }
		}

		private int _Width;

		[UltimaPacketProperty]
		public int Width
		{
			get { return _Width; }
		}

		private int _Height;

		[UltimaPacketProperty]
		public int Height
		{
			get { return _Height; }
		}

		private int _Map;

		[UltimaPacketProperty]
		public int Map
		{
			get { return _Map; }
		}

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID
			_Serial = reader.ReadUInt32();
			_CornerImage = reader.ReadInt16();
			_X1 = reader.ReadInt16();
			_Y1 = reader.ReadInt16();
			_X2 = reader.ReadInt16();
			_Y2 = reader.ReadInt16();
			_Width = reader.ReadInt16();
			_Height = reader.ReadInt16();
			_Map = reader.ReadInt16();
		}
	}
}
