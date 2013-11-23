using System.IO;

namespace Ultima.Spy.Packets
{
	[UltimaPacket( "Death Animation", UltimaPacketDirection.FromServer, 0xAF )]
	public class DeathAnimationPacket : UltimaPacket, IUltimaEntity
	{
		private uint _Victim;

		[UltimaPacketProperty( "Victim", "0x{0:X}" )]
		public uint Serial
		{
			get { return _Victim; }
		}

		private uint _Corpse;

		[UltimaPacketProperty( "Corpse", "0x{0:X}" )]
		public uint Corpse
		{
			get { return _Corpse; }
		}

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID

			_Victim = reader.ReadUInt32();
			_Corpse = reader.ReadUInt32();
		}
	}
}
