using System.Collections.ObjectModel;
using System.IO;

namespace Ultima.Spy.Packets
{
	[UltimaPacket( "Damage", UltimaPacketDirection.FromServer, 0xB )]
	public class DamagePacket : UltimaPacket, IUltimaEntity
	{
		private uint _Serial;

		[UltimaPacketProperty( "Serial", "0x{0:X}" )]
		public uint Serial
		{
			get { return _Serial; }
		}

		private int _Damage;

		[UltimaPacketProperty]
		public int Damage
		{
			get { return _Damage; }
		}

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID
			_Serial = reader.ReadUInt32();
			_Damage = reader.ReadInt16();
		}
	}
}
