using System.IO;
using System.Text;

namespace Ultima.Spy.Packets
{
	[UltimaPacket( "Mobile Name", UltimaPacketDirection.FromBoth, 0x98 )]
	public class MobileNamePacket : UltimaPacket, IUltimaEntity
	{
		private uint _Serial;

		[UltimaPacketProperty( "Serial", "0x{0:X}" )]
		public uint Serial
		{
			get { return _Serial; }
		}

		private string _MobileName;

		[UltimaPacketProperty( "Name" )]
		public string MobileName
		{
			get { return _MobileName; }
			set { _MobileName = value; }
		}

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID
			reader.ReadInt16(); // Size

			_Serial = reader.ReadUInt32();
			_MobileName = reader.ReadAsciiString( 30 );
		}
	}
}
