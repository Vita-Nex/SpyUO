using System;
using System.Runtime.InteropServices;

namespace Ultima.Package
{
	/// <summary>
	/// Contains information about the current computer system. This includes the architecture
	/// and type of the processor, the number of processors in the system, the page size, and
	/// other such information.
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
	public struct SYSTEM_INFO
	{
		/// <summary>
		/// The processor architecture of the installed operating system. This member can be
		/// one of the following values.
		/// </summary>
		public ushort ProcessorArchitecture;

		/// <summary>
		/// This member is reserved for future use.
		/// </summary>
		public ushort Reserved;

		/// <summary>
		/// The page size and the granularity of page protection and commitment. This is the
		/// page size used by the VirtualAlloc function.
		/// </summary>
		public uint PageSize;

		/// <summary>
		/// A pointer to the lowest memory address accessible to applications and dynamic-link
		/// libraries (DLLs).
		/// </summary>
		public IntPtr MinimumApplicationAddress;

		/// <summary>
		/// A pointer to the highest memory address accessible to applications and DLLs.
		/// </summary>
		public IntPtr MaximumApplicationAddress;

		/// <summary>
		/// A mask representing the set of processors configured into the system. Bit 0
		/// is processor 0; bit 31 is processor 31.
		/// </summary>
		public UIntPtr ActiveProcessorMask;

		/// <summary>
		/// The number of physical processors in the system.
		/// </summary>
		public uint NumberOfProcessors;

		/// <summary>
		/// An obsolete member that is retained for compatibility.
		/// </summary>
		public uint ProcessorType;

		/// <summary>
		/// The granularity for the starting address at which virtual memory can be allocated.
		/// </summary>
		public uint AllocationGranularity;

		/// <summary>
		/// The architecture-dependent processor level.
		/// </summary>
		public ushort ProcessorLevel;

		/// <summary>
		/// The architecture-dependent processor revision.
		/// </summary>
		public ushort ProcessorRevision;
	}

	/// <summary>
	/// Contains information about the current computer system.
	/// </summary>
	public class SystemInfo
	{
		#region Properties
		[DllImport( "kernel32.dll" )]
		private static extern void GetNativeSystemInfo( ref SYSTEM_INFO systemInfo );

		private static SYSTEM_INFO _SystemInfo = new SYSTEM_INFO();
		private static bool _Initialized = false;

		private const ushort PROCESSOR_ARCHITECTURE_AMD64 = 9;
		private const ushort PROCESSOR_ARCHITECTURE_IA64 = 6;
		private const ushort PROCESSOR_ARCHITECTURE_INTEL = 0;

		/// <summary>
		/// Determines whether system runs on 64 bit OS.
		/// </summary>
		public static bool IsX64
		{
			get
			{
				if ( !_Initialized )
				{
					GetNativeSystemInfo( ref _SystemInfo );
					_Initialized = true;
				}

				return _SystemInfo.ProcessorArchitecture == PROCESSOR_ARCHITECTURE_AMD64;
			}
		}

		/// <summary>
		/// Determines whether system runs on 32 bit OS.
		/// </summary>
		public static bool IsX32
		{
			get
			{
				if ( !_Initialized )
				{
					GetNativeSystemInfo( ref _SystemInfo );
					_Initialized = true;
				}

				return _SystemInfo.ProcessorArchitecture == PROCESSOR_ARCHITECTURE_INTEL;
			}
		}
		#endregion
	}
}
