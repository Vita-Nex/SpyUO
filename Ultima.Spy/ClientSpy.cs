using System;

namespace Ultima.Spy
{
	/// <summary>
	/// Client spy.
	/// </summary>
	public class ClientSpy : BaseSpy
	{
		#region Properties
		/// <summary>
		/// Client send info.
		/// </summary>
		protected AddressAndRegisters _SendKeys;

		/// <summary>
		/// Client receive info.
		/// </summary>
		protected AddressAndRegisters _ReceiveKeys;
		#endregion

		#region Events
		/// <summary>
		/// Occurs when client sends/receives packet.
		/// </summary>
		public event Action<byte[],bool> OnPacket;

		/// <summary>
		/// Occurs when spy stops.
		/// </summary>
		public event Action<SpyStoppedArgs> OnStopped;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of ClassicPacketSpy.
		/// </summary>
		/// <param name="sendKeys">Client send info.</param>
		/// <param name="receiveKeys">Client receive info.</param>
		public ClientSpy( AddressAndRegisters sendKeys, AddressAndRegisters receiveKeys )
		{
			_SendKeys = sendKeys;
			_ReceiveKeys = receiveKeys;
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
		}
		

		/// <summary>
		/// Occurs when spy stopps.
		/// </summary>
		/// <param name="args">Stop arguments.</param>
		protected override void OnStop( SpyStoppedArgs args )
		{
			if ( OnStopped != null )
				OnStopped( args );
		}

		/// <summary>
		/// Triggers OnPacket event.
		/// </summary>
		/// <param name="data">Packet data.</param>
		/// <param name="send">Determines wheter client sent or received data.</param>
		protected void Packet( byte[] data, bool send )
		{
			if ( OnPacket != null && ( !send || data[ 0 ] != 0x80 ) )
				OnPacket( data, send );
		}
		#endregion
	}
}
