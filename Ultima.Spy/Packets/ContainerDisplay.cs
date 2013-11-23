using System.Collections.ObjectModel;
using System.IO;

namespace Ultima.Spy.Packets
{
	public enum ContainerType
	{
		Vendor,
		ContainerOrSpellbook,
	}

	[UltimaPacket( "Container Display", UltimaPacketDirection.FromServer, 0x24 )]
	public class ContainerDisplayPacket : UltimaPacket, IUltimaEntity
	{
		private uint _Serial;

		[UltimaPacketProperty( "Serial", "0x{0:X}" )]
		public uint Serial
		{
			get { return _Serial; }
		}

		private int _GumpID;

		[UltimaPacketProperty( "Gump ID", "0x{0:X}" )]
		public int GumpID
		{
			get { return _GumpID; }
		}

		private ContainerType _ContainerType;

		[UltimaPacketProperty( "Container Type", "{0:D} - {0}" )]
		public ContainerType ContainerType
		{
			get { return _ContainerType; }
		}

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID
			_Serial = reader.ReadUInt32();
			_GumpID = reader.ReadInt16();

			int type = reader.ReadInt16();

			if ( type == 0 )
				_ContainerType = ContainerType.Vendor;
			else
				_ContainerType = ContainerType.ContainerOrSpellbook;
		}
	}
}
