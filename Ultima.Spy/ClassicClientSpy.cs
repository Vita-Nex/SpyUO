using System;

namespace Ultima.Spy
{
	/// <summary>
	/// Classic client spy.
	/// </summary>
	public class ClassicClientSpy : ClientSpy
	{
		#region Properties
		private static readonly byte[] _DebugProtectionReplacement1 = new byte[]
		{
			0x31, 0xC0,		// XOR EAX, EAX
		};

		private static readonly byte[] _DebugProtectionReplacement2 = new byte[]
		{
			0x83, 0xF9, 0x00,	// CMP ECX, 0
			0x90,				// NOP
		};

		private uint _DebugProtectionAddress1;
		private uint _DebugProtectionAddress2;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of ClassicPacketSpy.
		/// </summary>
		/// <param name="debugProtectionAddress1">Debug protection address 1.</param>
		/// <param name="debugProtectionAddress2">Debug protection address 2.</param>
		public ClassicClientSpy( AddressAndRegisters sendInfo, AddressAndRegisters receiveInfo, uint debugProtectionAddress1, uint debugProtectionAddress2 ) : base( sendInfo, receiveInfo )
		{
			_DebugProtectionAddress1 = debugProtectionAddress1;
			_DebugProtectionAddress2 = debugProtectionAddress2;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Occurs when breakpoints have to be initialized.
		/// </summary>
		protected override void InitBreakpoints()
		{
			AddBreakpoint( _SendKeys.Address );
			AddBreakpoint( _ReceiveKeys.Address );

			if ( _DebugProtectionAddress1 > 0 )
				ReplaceCode( _DebugProtectionAddress1, _DebugProtectionReplacement1 );

			if ( _DebugProtectionAddress2 > 0 )
				ReplaceCode( _DebugProtectionAddress2, _DebugProtectionReplacement2 );
		}

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
				byte[] data = ReadProcessMemory( dataAddress, dataLength );

				if ( data != null )
					Packet( data, send );
			}
		}
		#endregion
	}
}
