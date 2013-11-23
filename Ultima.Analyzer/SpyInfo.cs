using System;

namespace Ultima.Analyzer
{
	/// <summary>
	/// Provides information about SpyUO keys.
	/// </summary>
	public class SpyInfo
	{
		#region Methods
		/// <summary>
		/// Determines wheter Receive key was found.
		/// </summary>
		public bool FoundReceive
		{
			get { return _ReceiveAddress != 0; }
		}

		private int _ReceiveAddress;

		/// <summary>
		/// Receive key.
		/// </summary>
		public int ReceiveAddress
		{
			get { return _ReceiveAddress; }
		}

		private int _ReceiveAddressRegister;

		/// <summary>
		/// Register which contains Received packet address.
		/// </summary>
		public int ReceiveAddressRegister
		{
			get { return _ReceiveAddressRegister; }
		}

		private int _ReceiveLengthRegister;

		/// <summary>
		/// Register which contains Received packet length.
		/// </summary>
		public int ReceiveLengthRegister
		{
			get { return _ReceiveLengthRegister; }
		}

		/// <summary>
		/// Determines wheter send key was found.
		/// </summary>
		public bool FoundSend
		{
			get { return _SendAddress != 0; }
		}

		private int _SendAddress;

		/// <summary>
		/// Send key.
		/// </summary>
		public int SendAddress
		{
			get { return _SendAddress; }
		}

		private int _SendAddressRegister;

		/// <summary>
		/// Register which contains sent packet address.
		/// </summary>
		public int SendAddressRegister
		{
			get { return _SendAddressRegister; }
		}

		private int _SendLengthRegister;

		/// <summary>
		/// Register which contains sent packet length.
		/// </summary>
		public int SendLengthRegister
		{
			get { return _SendLengthRegister; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of SpyInfo.
		/// </summary>
		/// <param name="receiveAddr">Receive address.</param>
		/// <param name="receiveAddrReg">Register which contains Received packet address.</param>
		/// <param name="receiveLenReg">Register which contains Received packet length.</param>
		/// <param name="sendAddr">Send address.</param>
		/// <param name="sendAddrReg">Register which contains sent packet address.</param>
		/// <param name="sendLenReg">Register which contains sent packet length.</param>
		public SpyInfo( int receiveAddr, int receiveAddrReg, int receiveLenReg, int sendAddr, int sendAddrReg, int sendLenReg )
		{
			_ReceiveAddress = receiveAddr;
			_ReceiveAddressRegister = receiveAddrReg;
			_ReceiveLengthRegister = receiveLenReg;
			_SendAddress = sendAddr;
			_SendAddressRegister = sendAddrReg;
			_SendLengthRegister = sendLenReg;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Returns the key as a string.
		/// </summary>
		/// <returns>A string representing the key.</returns>
		public override string ToString()
		{
			return String.Format( "{0:X} {1} {2} {3:X} {4} {5}", _SendAddress, _SendAddressRegister, _SendLengthRegister, _ReceiveAddress, _ReceiveAddressRegister, _ReceiveLengthRegister );
		}
		#endregion
	}
}
