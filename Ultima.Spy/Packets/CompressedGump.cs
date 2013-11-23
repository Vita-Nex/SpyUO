using System;
using System.Collections.Generic;
using System.Text;

namespace Ultima.Spy.Packets
{
	[UltimaPacket( "Compressed Gump", UltimaPacketDirection.FromServer, 0xDD )]
	public class CompressedGump : GenericGumpPacket
	{
		protected override void Parse( BigEndianReader reader )
		{
			reader.ReadByte(); // ID
			reader.ReadInt16(); // Size

			_Serial = reader.ReadUInt32();
			_GumpID = reader.ReadInt32();
			_X = reader.ReadInt32();
			_Y = reader.ReadInt32();

			int compressedEntriesLength = reader.ReadInt32() - 4;
			int decompressedEntriesLength = reader.ReadInt32();

			byte[] compressedEntries = reader.ReadBytes( compressedEntriesLength );
			byte[] decompressedEntries = new byte[ decompressedEntriesLength ];

			Ultima.Package.Zlib.Decompress( decompressedEntries, ref decompressedEntriesLength, compressedEntries, compressedEntriesLength );
			string entries = Encoding.ASCII.GetString( decompressedEntries );

			int lineCount = reader.ReadInt32();
			int compressedStringsLength = reader.ReadInt32() - 4;
			int decompressedStringsLength = reader.ReadInt32();

			byte[] compressedStrings = reader.ReadBytes( compressedStringsLength );
			byte[] decompressedStrings = new byte[ decompressedStringsLength ];

			Ultima.Package.Zlib.Decompress( decompressedStrings, ref decompressedStringsLength, compressedStrings, compressedStringsLength );
			string strings = Encoding.ASCII.GetString( decompressedStrings );
			
			_Text = new List<string>();

			int start = 0;

			for ( int i = 0; i < strings.Length; i++ )
			{
				if ( strings[ i ] == 0 )
				{
					if ( i - start > 0 ) 
						_Text.Add( strings.Substring( start, i - start ) );
					else
						_Text.Add( String.Empty );

					start = i + 1;
				}
			}

			Parse( entries );
		}
	}
}
