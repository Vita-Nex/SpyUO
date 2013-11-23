using System;
using System.Runtime.InteropServices;

namespace Ultima.Spy
{
	/// <summary>
	/// Contains information about the current computer system.
	/// </summary>
	public static class SystemInfo
	{
		#region Properties
		private static NativeMethods.SYSTEM_INFO _SystemInfo = new NativeMethods.SYSTEM_INFO();
		private static bool _Initialized = false;

		#region IsX64
		/// <summary>
		/// Determines whether system runs on 64 bit OS.
		/// </summary>
		public static bool IsX64
		{
			get
			{
				if ( !_Initialized )
				{
					NativeMethods.GetNativeSystemInfo( ref _SystemInfo );
					_Initialized = true;
				}

				return _SystemInfo.ProcessorArchitecture == (ushort) NativeMethods.PROCESS_ARCHITECTURE.PROCESSOR_ARCHITECTURE_AMD64;
			}
		}
		#endregion

		#region IsX32
		/// <summary>
		/// Determines whether system runs on 32 bit OS.
		/// </summary>
		public static bool IsX32
		{
			get
			{
				if ( !_Initialized )
				{
					NativeMethods.GetNativeSystemInfo( ref _SystemInfo );
					_Initialized = true;
				}

				return _SystemInfo.ProcessorArchitecture == (ushort) NativeMethods.PROCESS_ARCHITECTURE.PROCESSOR_ARCHITECTURE_INTEL;
			}
		}
		#endregion
		#endregion
	}
}
