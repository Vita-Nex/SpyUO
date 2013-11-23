using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace Ultima.Spy.Packets
{
	[UltimaPacket( "Unicode Message", UltimaPacketDirection.FromServer, 0xAE )]
	public class UnicodeMessagePacket : UltimaPacket, IUltimaEntity
	{
		private uint _Serial;

		[UltimaPacketProperty( "Serial", "0x{0:X}" )]
		public uint Serial
		{
			get { return _Serial; }
		}

		private int _Graphics;

		[UltimaPacketProperty( "Graphics", "0x{0:X}" )]
		public int Graphics
		{
			get { return _Graphics; }
		}

		private MessageType _Type;

		[UltimaPacketProperty]
		public MessageType Type
		{
			get { return _Type; }
		}

		private int _Hue;

		[UltimaPacketProperty]
		public int Hue
		{
			get { return _Hue; }
		}

		private int _Font;

		[UltimaPacketProperty]
		public int Font
		{
			get { return _Font; }
		}

		private string _Language;

		[UltimaPacketProperty]
		public string Language
		{
			get { return _Language; }
		}

		private string _EntityName;

		[UltimaPacketProperty( "Name" )]
		public string EntityName
		{
			get { return _EntityName; }
		}

		private string _Message;

		[UltimaPacketProperty]
		public string Message
		{
			get { return _Message; }
		}

		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID
			reader.ReadInt16();
			_Serial = reader.ReadUInt32();
			_Graphics = reader.ReadInt16();
			_Type = (MessageType) reader.ReadByte();
			_Hue = reader.ReadInt16();
			_Font = reader.ReadInt16();
			_Language = reader.ReadAsciiString( 4 );
			_EntityName = reader.ReadAsciiString( 30 );
			_Message = reader.ReadUnicodeString( ( Data.Length - 44 ) / 2 );
		}
	}
}
