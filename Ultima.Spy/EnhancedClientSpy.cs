using System;
using System.IO;

namespace Ultima.Spy
{
	/// <summary>
	/// Enhanced client spy.
	/// </summary>
	public class EnhancedClientSpy : ClientSpy
	{
		#region Constructors
		/// <summary>
		/// Constructs a new instance of EnhancedClientSpy.
		/// </summary>
		public EnhancedClientSpy( AddressAndRegisters sendInfo, AddressAndRegisters receiveInfo ) : base( sendInfo, receiveInfo )
		{
		}
		#endregion

		#region Methods
		/// <summary>
		/// Occurs when client reaches breakpoint.
		/// </summary>
		/// <param name="address">Breakpoint address.</param>
		protected override void OnBreakpoint( uint address )
		{
			AddressAndRegisters ar;
			bool send = false;

			if ( address == _SendKeys.Address )
			{
				ar = _SendKeys;
				send = true;
			}
			else if ( address == _ReceiveKeys.Address )
			{
				ar = _ReceiveKeys;
				send = false;
			}
			else
				return;

			uint dataAddress = GetContextRegister( ar.DataAddressRegister );
			uint dataLength = GetContextRegister( ar.DataLengthRegister ) & 0xFFFF;

			if ( dataLength > 0 )
			{
				byte[] data = ReadProcessMemory( dataAddress + 4, 8 );

				using ( MemoryStream stream = new MemoryStream( data ) )
				{
					using ( BinaryReader reader = new BinaryReader( stream ) )
					{
						uint start = reader.ReadUInt32();
						uint length = reader.ReadUInt32() - start;

						data = ReadProcessMemory( start, length );

						if ( data != null )
							Packet( data, send );
					}
				}
			}
		}
		#endregion
	}
}
