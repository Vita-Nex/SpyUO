using System;
using System.Runtime.InteropServices;

namespace Ultima.Spy
{
	/// <summary>
	/// Interop methods.
	/// </summary>
	public sealed class NativeMethods
	{
		public enum PROCESS_ARCHITECTURE : ushort
		{
			PROCESSOR_ARCHITECTURE_AMD64	= 9,
			PROCESSOR_ARCHITECTURE_IA64		= 6,
			PROCESSOR_ARCHITECTURE_INTEL	= 0,
			PROCESSOR_ARCHITECTURE_UNKNOWN	= 0xffff
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct SYSTEM_INFO
		{
			public ushort ProcessorArchitecture;
			public ushort Reserved;
			public uint PageSize;
			public IntPtr MinimumApplicationAddress;
			public IntPtr MaximumApplicationAddress;
			public UIntPtr ActiveProcessorMask;
			public uint NumberOfProcessors;
			public uint ProcessorType;
			public uint AllocationGranularity;
			public ushort ProcessorLevel;
			public ushort ProcessorRevision;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct PROCESS_INFORMATION
		{
			public IntPtr hProcess;
			public IntPtr hThread;
			public uint dwProcessId;
			public uint dwThreadId;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct STARTUPINFO
		{
			public uint cb;
			[MarshalAs( UnmanagedType.LPTStr )]
			public string lpReserved;
			[MarshalAs( UnmanagedType.LPTStr )]
			public string lpDesktop;
			[MarshalAs( UnmanagedType.LPTStr )]
			public string lpTitle;
			public uint dwX;
			public uint dwY;
			public uint dwXSize;
			public uint dwYSize;
			public uint dwXCountChars;
			public uint dwYCountChars;
			public uint dwFillAttribute;
			public uint dwFlags;
			public ushort wShowWindow;
			public ushort cbReserved2;
			public IntPtr lpReserved2;
			public IntPtr hStdInput;
			public IntPtr hStdOutput;
			public IntPtr hStdError;
		}

		[Flags]
		public enum CreationFlag : uint
		{
			DEBUG_PROCESS						= 0x00000001,
			DEBUG_ONLY_THIS_PROCESS				= 0x00000002,

			CREATE_SUSPENDED					= 0x00000004,

			DETACHED_PROCESS					= 0x00000008,

			CREATE_NEW_CONSOLE					= 0x00000010,

			NORMAL_PRIORITY_CLASS				= 0x00000020,
			IDLE_PRIORITY_CLASS					= 0x00000040,
			HIGH_PRIORITY_CLASS					= 0x00000080,
			REALTIME_PRIORITY_CLASS				= 0x00000100,

			CREATE_NEW_PROCESS_GROUP			= 0x00000200,
			CREATE_UNICODE_ENVIRONMENT			= 0x00000400,

			CREATE_SEPARATE_WOW_VDM				= 0x00000800,
			CREATE_SHARED_WOW_VDM				= 0x00001000,
			CREATE_FORCEDOS						= 0x00002000,

			BELOW_NORMAL_PRIORITY_CLASS			= 0x00004000,
			ABOVE_NORMAL_PRIORITY_CLASS			= 0x00008000,
			STACK_SIZE_PARAM_IS_A_RESERVATION	= 0x00010000,

			CREATE_BREAKAWAY_FROM_JOB			= 0x01000000,
			CREATE_PRESERVE_CODE_AUTHZ_LEVEL	= 0x02000000,

			CREATE_DEFAULT_ERROR_MODE			= 0x04000000,
			CREATE_NO_WINDOW					= 0x08000000,

			PROFILE_USER						= 0x10000000,
			PROFILE_KERNEL						= 0x20000000,
			PROFILE_SERVER						= 0x40000000,

			CREATE_IGNORE_SYSTEM_DEFAULT		= 0x80000000
		}

		[DllImport( "Kernel32" )]
		public static extern bool CreateProcess
			(
			[MarshalAs( UnmanagedType.LPStr )] string lpApplicationName,
			[MarshalAs( UnmanagedType.LPStr )] string lpCommandLine,
			IntPtr lpProcessAttributes,
			IntPtr lpThreadAttributes,
			bool bInheritHandles,
			CreationFlag dwCreationFlags,
			IntPtr lpEnvironment,
			[MarshalAs( UnmanagedType.LPStr )] string lpCurrentDirectory,
			ref STARTUPINFO lpStartupInfo,
			out PROCESS_INFORMATION lpProcessInformation
			);

		[DllImport( "Kernel32" )]
		public static extern bool DebugActiveProcess( uint dwProcessId );

		[DllImport( "Kernel32" )]
		public static extern bool DebugActiveProcessStop( uint dwProcessId );

		[DllImport( "Kernel32" )]
		public static extern bool OpenProcessToken( IntPtr ProcessHandle, uint DesiredAccess, out IntPtr tokenHandle );

		[DllImport( "Kernel32" )]
		public static extern IntPtr GetCurrentProcess();

		public const UInt32 STANDARD_RIGHTS_REQUIRED = 0x000F0000;
		public const UInt32 STANDARD_RIGHTS_READ = 0x00020000;
		public const UInt32 TOKEN_ASSIGN_PRIMARY = 0x0001;
		public const UInt32 TOKEN_DUPLICATE = 0x0002;
		public const UInt32 TOKEN_IMPERSONATE = 0x0004;
		public const UInt32 TOKEN_QUERY = 0x0008;
		public const UInt32 TOKEN_QUERY_SOURCE = 0x0010;
		public const UInt32 TOKEN_ADJUST_PRIVILEGES = 0x0020;
		public const UInt32 TOKEN_ADJUST_GROUPS = 0x0040;
		public const UInt32 TOKEN_ADJUST_DEFAULT = 0x0080;
		public const UInt32 TOKEN_ADJUST_SESSIONID = 0x0100;
		public const UInt32 TOKEN_READ = ( STANDARD_RIGHTS_READ | TOKEN_QUERY );
		public const UInt32 TOKEN_ALL_ACCESS = ( STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY |
			TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE |
			TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT |
			TOKEN_ADJUST_SESSIONID );

		public enum DesiredAccessProcess : uint
		{
			PROCESS_TERMINATE			= 0x0001,
			PROCESS_CREATE_THREAD		= 0x0002,
			PROCESS_VM_OPERATION		= 0x0008,
			PROCESS_VM_READ				= 0x0010,
			PROCESS_VM_WRITE			= 0x0020,
			PROCESS_DUP_HANDLE			= 0x0040,
			PROCESS_CREATE_PROCESS		= 0x0080,
			PROCESS_SET_QUOTA			= 0x0100,
			PROCESS_SET_INFORMATION		= 0x0200,
			PROCESS_QUERY_INFORMATION	= 0x0400,
			SYNCHRONIZE					= 0x00100000,
			PROCESS_ALL_ACCESS			= SYNCHRONIZE | 0xF0FFF
		}

		[DllImport( "Kernel32" )]
		public static extern IntPtr OpenProcess( DesiredAccessProcess dwDesiredAccess, bool bInheritHandle, uint dwProcessId );

		[Flags]
		public enum DesiredAccessThread : uint
		{
			SYNCHRONIZE					= 0x00100000,
			THREAD_TERMINATE			= 0x0001,
			THREAD_SUSPEND_RESUME		= 0x0002,
			THREAD_GET_CONTEXT			= 0x0008,
			THREAD_SET_CONTEXT			= 0x0010,
			THREAD_SET_INFORMATION		= 0x0020,
			THREAD_QUERY_INFORMATION	= 0x0040,
			THREAD_SET_THREAD_TOKEN		= 0x0080,
			THREAD_IMPERSONATE			= 0x0100,
			THREAD_DIRECT_IMPERSONATION = 0x0200,
			THREAD_ALL_ACCESS			= SYNCHRONIZE | 0xF03FF
		}

		[DllImport( "Kernel32" )]
		public static extern IntPtr OpenThread( DesiredAccessThread dwDesiredAccess, bool bInheritHandle, uint dwThreadId );

		[DllImport( "Kernel32" )]
		public static extern bool CloseHandle( IntPtr hObject );

		public enum DebugEventCode : uint
		{
			EXCEPTION_DEBUG_EVENT			= 1,
			CREATE_THREAD_DEBUG_EVENT		= 2,
			CREATE_PROCESS_DEBUG_EVENT		= 3,
			EXIT_THREAD_DEBUG_EVENT			= 4,
			EXIT_PROCESS_DEBUG_EVENT		= 5,
			LOAD_DLL_DEBUG_EVENT			= 6,
			UNLOAD_DLL_DEBUG_EVENT			= 7,
			OUTPUT_DEBUG_STRING_EVENT		= 8,
			RIP_EVENT						= 9
		}

		public const int EXCEPTION_MAXIMUM_PARAMETERS = 15;

		[StructLayout( LayoutKind.Sequential )]
		public struct EXCEPTION_RECORD
		{
			public uint ExceptionCode;
			public uint ExceptionFlags;
			public IntPtr ExceptionRecord;
			public IntPtr ExceptionAddress;
			public uint NumberParameters;
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = EXCEPTION_MAXIMUM_PARAMETERS )]
			public uint[] ExceptionInformation;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct EXCEPTION_DEBUG_INFO
		{
			public EXCEPTION_RECORD ExceptionRecord;
			public uint dwFirstChance;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct DEBUG_EVENT_EXCEPTION
		{
			public DebugEventCode dwDebugEventCode;
			public uint dwProcessId;
			public uint dwThreadId;

			[StructLayout( LayoutKind.Explicit )]
			public struct UnionException
			{
				[FieldOffset( 0 )]
				public EXCEPTION_DEBUG_INFO Exception;
			}
			public UnionException u;
		}

		[DllImport( "Kernel32" )]
		public static extern bool WaitForDebugEvent( ref DEBUG_EVENT_EXCEPTION lpDebugEvent, uint dwMilliseconds );

		[Flags]
		public enum ContinueStatus : uint
		{
			DBG_CONTINUE = 0x00010002,
			DBG_EXCEPTION_NOT_HANDLED = 0x80010001
		}

		[DllImport( "Kernel32" )]
		public static extern bool ContinueDebugEvent( uint dwProcessId, uint dwThreadId, ContinueStatus dwContinueStatus );

		[DllImport( "Kernel32" )]
		public static extern bool ReadProcessMemory( IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out uint lpNumberOfBytesRead );

		[DllImport( "Kernel32" )]
		public static extern bool WriteProcessMemory( IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out uint lpNumberOfBytesWritten );

		[DllImport( "Kernel32" )]
		public static extern bool FlushInstructionCache( IntPtr hProcess, IntPtr lpBaseAddress, uint dwSize );

		[Flags]
		public enum ContextFlags : uint
		{
			CONTEXT_i386 = 0x00010000,
			CONTEXT_i486 = 0x00010000,

			CONTEXT_CONTROL = CONTEXT_i386 | 0x00000001,
			CONTEXT_INTEGER = CONTEXT_i386 | 0x00000002,
			CONTEXT_SEGMENTS = CONTEXT_i386 | 0x00000004,
			CONTEXT_FLOATING_POINT = CONTEXT_i386 | 0x00000008,
			CONTEXT_DEBUG_REGISTERS = CONTEXT_i386 | 0x00000010,
			CONTEXT_EXTENDED_REGISTERS = CONTEXT_i386 | 0x00000020,

			CONTEXT_FULL = CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_SEGMENTS
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct FLOATING_SAVE_AREA
		{
			public uint ControlWord;
			public uint StatusWord;
			public uint TagWord;
			public uint ErrorOffset;
			public uint ErrorSelector;
			public uint DataOffset;
			public uint DataSelector;
			[MarshalAs( UnmanagedType.ByValArray, SizeConst = 80 )]
			public byte[] RegisterArea;
			public uint Cr0NpxState;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct CONTEXT
		{
			public ContextFlags ContextFlags;

			public uint Dr0;
			public uint Dr1;
			public uint Dr2;
			public uint Dr3;
			public uint Dr6;
			public uint Dr7;

			public FLOATING_SAVE_AREA FloatSave;

			public uint SegGs;
			public uint SegFs;
			public uint SegEs;
			public uint SegDs;

			public uint Edi;
			public uint Esi;
			public uint Ebx;
			public uint Edx;
			public uint Ecx;
			public uint Eax;

			public uint Ebp;
			public uint Eip;
			public uint SegCs;
			public uint EFlags;
			public uint Esp;
			public uint SegSs;

			[MarshalAs( UnmanagedType.ByValArray, SizeConst = 512 )]
			public byte[] ExtendedRegisters;
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct WOW64_CONTEXT
		{
			public ContextFlags ContextFlags;

			public uint Dr0;
			public uint Dr1;
			public uint Dr2;
			public uint Dr3;
			public uint Dr6;
			public uint Dr7;

			public FLOATING_SAVE_AREA FloatSave;

			public uint SegGs;
			public uint SegFs;
			public uint SegEs;
			public uint SegDs;

			public uint Edi;
			public uint Esi;
			public uint Ebx;
			public uint Edx;
			public uint Ecx;
			public uint Eax;

			public uint Ebp;
			public uint Eip;
			public uint SegCs;
			public uint EFlags;
			public uint Esp;
			public uint SegSs;

			[MarshalAs( UnmanagedType.ByValArray, SizeConst = 512 )]
			public byte[] ExtendedRegisters;
		}

		[DllImport( "Kernel32" )]
		public static extern bool GetThreadContext( IntPtr hThread, ref CONTEXT lpContext );

		[DllImport( "Kernel32" )]
		public static extern bool Wow64GetThreadContext( IntPtr hThread, ref WOW64_CONTEXT lpContext );

		[DllImport( "Kernel32" )]
		public static extern bool SetThreadContext( IntPtr hThread, ref CONTEXT lpContext );

		[DllImport( "Kernel32" )]
		public static extern bool Wow64SetThreadContext( IntPtr hThread, ref WOW64_CONTEXT lpContext );

		[DllImport( "Kernel32" )]
		public static extern int GetLastError();

		[DllImport( "kernel32" )]
		public static extern void GetNativeSystemInfo( ref SYSTEM_INFO systemInfo );
	}
}