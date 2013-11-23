using System;

namespace Ultima.Spy
{
	/// <summary>
	/// Processor registers.
	/// </summary>
	public enum Register : int
	{
		Eax = 1,
		Ebx = 2,
		Ecx = 3,
		Edx = 4,
		Esi = 5,
		Edi = 6,
		Ebp = 7,
		Esp = 8
	}

	/// <summary>
	/// Describes client address and register.
	/// </summary>
	public class AddressAndRegisters
	{
		#region Properties
		private uint _Address;

		/// <summary>
		/// Gets or sets client address.
		/// </summary>
		public uint Address
		{
			get { return _Address; }
			set { _Address = value; }
		}

		private Register _DataAddressRegister;

		/// <summary>
		/// Gets or sets data address register.
		/// </summary>
		public Register DataAddressRegister
		{
			get { return _DataAddressRegister; }
			set { _DataAddressRegister = value; }
		}

		private Register _DataLengthRegister;

		/// <summary>
		/// Gets or sets data length register.
		/// </summary>
		public Register DataLengthRegister
		{
			get { return _DataLengthRegister; }
			set { _DataLengthRegister = value; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of AddressAndRegisters.
		/// </summary>
		/// <param name="address">Client address.</param>
		/// <param name="dataAddressRegister">Data address register.</param>
		/// <param name="dataLengthRegister">Data length register.</param>
		public AddressAndRegisters( int address, Register dataAddressRegister, Register dataLengthRegister )
		{
			_Address = (uint) address;
			_DataAddressRegister = dataAddressRegister;
			_DataLengthRegister = dataLengthRegister;
		}

		/// <summary>
		/// Constructs a new instance of AddressAndRegisters.
		/// </summary>
		/// <param name="address">Client address.</param>
		/// <param name="dataAddressRegister">Data address register.</param>
		/// <param name="dataLengthRegister">Data length register.</param>
		public AddressAndRegisters( int address, int dataAddressRegister, int dataLengthRegister )
		{
			_Address = (uint) address;
			_DataAddressRegister = (Register) dataAddressRegister;
			_DataLengthRegister = (Register) dataLengthRegister;
		}
		#endregion
	}
}