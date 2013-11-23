using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

using DebugEventException = Ultima.Spy.NativeMethods.DEBUG_EVENT_EXCEPTION;
using ProcessContext = Ultima.Spy.NativeMethods.CONTEXT;
using ProcessContext64 = Ultima.Spy.NativeMethods.WOW64_CONTEXT;
using ProcessInfo = Ultima.Spy.NativeMethods.PROCESS_INFORMATION;
using StartupInfo = Ultima.Spy.NativeMethods.STARTUPINFO;

namespace Ultima.Spy
{
	/// <summary>
	/// Stop type.
	/// </summary>
	public enum SpyStopType
	{
		/// <summary>
		/// Error occured.
		/// </summary>
		Error,

		/// <summary>
		/// Stopped via Stop function.
		/// </summary>
		Stopped,

		/// <summary>
		/// Client manually closed.
		/// </summary>
		Closed,
	}

	/// <summary>
	/// Packet stopped arguments.
	/// </summary>
	public class SpyStoppedArgs
	{
		#region Properties
		private SpyStopType _StopType;

		/// <summary>
		/// Gets stop type.
		/// </summary>
		public SpyStopType StopType
		{
			get { return _StopType; }
		}

		private Exception _Error;

		/// <summary>
		/// Gets exception that might have occured.
		/// </summary>
		public Exception Error
		{
			get { return _Error; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Cosntructs a new insance of PacketStoppedArgs.
		/// </summary>
		/// <param name="stopType">Stop type.</param>
		/// <param name="error">Exception that might have occured.</param>
		public SpyStoppedArgs( SpyStopType stopType, Exception error = null )
		{
			_StopType = stopType;
			_Error = error;
		}
		#endregion
	}

	/// <summary>
	/// Describes methods for spying on processes.
	/// </summary>
	public class BaseSpy : IDisposable
	{
		#region Properties
		private static readonly NativeMethods.DesiredAccessThread OpenThreadFlags = 
			NativeMethods.DesiredAccessThread.THREAD_GET_CONTEXT | 
			NativeMethods.DesiredAccessThread.THREAD_SET_CONTEXT | 
			NativeMethods.DesiredAccessThread.THREAD_QUERY_INFORMATION;
		private static readonly byte BreakpointCode = 0xCC;

		private Process _Process;
		private IntPtr _ProcessHandle;
		private DebugEventException _EventBuffer;
		private ProcessContext _Context;
		private ProcessContext64 _Context64;
		private Dictionary<uint, byte> _Breakpoints;
		private Dictionary<uint, byte[]> _ReplacedCode;

		private bool _SafeToStop;
		private ManualResetEvent _StopToken;
		private SpyStopType _StopType;

		private bool SafeToStop
		{
			get { lock ( this ) return _SafeToStop; }
			set { lock ( this ) _SafeToStop = value; }
		}

		/// <summary>
		/// Determines if process has terminated.
		/// </summary>
		private bool ProcessTerminated
		{
			get { return _Process != null && _Process.HasExited; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Cosntructs a new instance of BaseSpy.
		/// </summary>
		public BaseSpy()
		{
			_Process = null;
			_ProcessHandle = IntPtr.Zero;
			_EventBuffer = new DebugEventException();
			_Context = new ProcessContext();
			_Context.ContextFlags = NativeMethods.ContextFlags.CONTEXT_CONTROL | NativeMethods.ContextFlags.CONTEXT_INTEGER;
			_Context64 = new ProcessContext64();
			_Context64.ContextFlags = NativeMethods.ContextFlags.CONTEXT_CONTROL | NativeMethods.ContextFlags.CONTEXT_INTEGER;
			_StopToken = new ManualResetEvent( true );

			_Breakpoints = new Dictionary<uint, byte>();
			_ReplacedCode = new Dictionary<uint, byte[]>();
		}
		#endregion

		#region Methods
		/// <summary>
		/// Starts packet listener.
		/// </summary>
		/// <param name="process">Process to spy on.</param>
		public void AttachAsync( Process process )
		{
			if ( _Process != null )
				throw new SpyException( "Spy already started. Please stop before starting again." );

			ThreadStart starter = new ThreadStart( delegate()
			{
				Attach( process );
			} );

			new Thread( starter ).Start();
		}

		/// <summary>
		/// Stops spying.
		/// </summary>
		public void Stop()
		{
			_StopType = SpyStopType.Stopped;
			Dispose();
		}

		/// <summary>
		/// Performs application-defined tasks associated with 
		/// freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if ( !SafeToStop )
			{
				SafeToStop = true;

				_StopToken.WaitOne();
				_StopToken.Close();
			}
		}

		private void Attach( Process process )
		{
			Exception exception = null;

			try
			{
				_StopType = SpyStopType.Closed;
				_Process = process;
				_ProcessHandle = NativeMethods.OpenProcess( NativeMethods.DesiredAccessProcess.PROCESS_ALL_ACCESS, false, (uint) _Process.Id );

				if ( _ProcessHandle == IntPtr.Zero )
					throw new SpyException( "Error opening process. ErrorCode={0}", NativeMethods.GetLastError() );

				if ( !NativeMethods.DebugActiveProcess( (uint) _Process.Id ) )
					throw new SpyException( "Error starting debugger. ErrorCode={0}", NativeMethods.GetLastError() );

				InitBreakpoints();
				MainLoop();
			}
			catch ( Exception ex )
			{
				exception = ex;
				_StopType = SpyStopType.Error;
			}
			finally
			{
				try
				{
					if ( SafeToStop )
						SafeStop();
				}
				catch ( Exception ex )
				{
					exception = ex;
					_StopType = SpyStopType.Error;
				}
			}

			OnStop( new SpyStoppedArgs( _StopType, exception ) );
		}

		private void MainLoop()
		{
			_StopToken.Reset();
			SafeToStop = false;

			while ( !SafeToStop && !ProcessTerminated )
			{
				if ( NativeMethods.WaitForDebugEvent( ref _EventBuffer, 1000 ) )
				{
					if ( _EventBuffer.dwDebugEventCode == NativeMethods.DebugEventCode.EXCEPTION_DEBUG_EVENT )
					{
						uint address = (uint) _EventBuffer.u.Exception.ExceptionRecord.ExceptionAddress.ToInt32();

						if ( _Breakpoints.ContainsKey( address ) )
							Breakpoint( _EventBuffer.dwThreadId, address );
					}

					if ( !NativeMethods.ContinueDebugEvent( (uint) _Process.Id, _EventBuffer.dwThreadId, NativeMethods.ContinueStatus.DBG_CONTINUE ) )
						throw new SpyException( "Error continuing debug event. ErrorCode={0}", NativeMethods.GetLastError() );
				}
			}
		}

		/// <summary>
		/// Stops spying on client.
		/// </summary>
		private void SafeStop()
		{
			RemoveBreakpoints();

			if ( !NativeMethods.DebugActiveProcessStop( (uint) _Process.Id ) )
				throw new SpyException( "Error stopping debugger. ErroCode={0}", NativeMethods.GetLastError() );

			if ( !NativeMethods.CloseHandle( _ProcessHandle ) )
				throw new SpyException( "Error closing handle. ErrorCode={0}", NativeMethods.GetLastError() );

			_Process = null;
			_ProcessHandle = IntPtr.Zero;

			_StopToken.Set();
		}

		/// <summary>
		/// Occurs when spy stopps.
		/// </summary>
		/// <param name="args">Stop arguments.</param>
		protected virtual void OnStop( SpyStoppedArgs args )
		{
		}

		#region Breakpoints
		/// <summary>
		/// Occurs when breakpoints have to be initialized.
		/// </summary>
		protected virtual void InitBreakpoints()
		{
		}

		/// <summary>
		/// Replaces one byte in process memory with breakpints code.
		/// </summary>
		/// <param name="address">Memory breakpoints.</param>
		protected void AddBreakpoint( uint address )
		{
			if ( !_Breakpoints.ContainsKey( address ) )
			{
				byte code = ReadProcessMemory( address );

				WriteProcessMemory( address, BreakpointCode );

				_Breakpoints.Add( address, code );
			}
		}

		/// <summary>
		/// Replaces code on specific address.
		/// </summary>
		/// <param name="address">Address to replace.</param>
		/// <param name="code">Date to replace with.</param>
		protected void ReplaceCode( uint address, byte[] code )
		{
			if ( _ReplacedCode.ContainsKey( address ) )
				throw new SpyException( "Already replaced code. Address={0:X}", address );

			byte[] oldCode = ReadProcessMemory( address, (uint) code.Length );

			_ReplacedCode.Add( address, oldCode );
			WriteProcessMemory( address, code );
		}

		/// <summary>
		/// Occurs when client reaches breakpoint.
		/// </summary>
		/// <param name="address">Breakpoint address.</param>
		protected virtual void OnBreakpoint( uint address )
		{
		}

		private void RemoveBreakpoints()
		{
			foreach ( KeyValuePair<uint, byte> kvp in _Breakpoints )
				WriteProcessMemory( kvp.Key, kvp.Value );

			_Breakpoints.Clear();

			foreach ( KeyValuePair<uint, byte[]> kvp in _ReplacedCode )
				WriteProcessMemory( kvp.Key, kvp.Value );

			_ReplacedCode.Clear();
		}

		private void Breakpoint( uint threadID, uint address )
		{
			IntPtr threadHandle = NativeMethods.OpenThread( OpenThreadFlags, false, threadID );
			byte code = _Breakpoints[ address ];

			if ( threadHandle == null )
				throw new SpyException( "Error opening thread. ErrorCode={0}", NativeMethods.GetLastError() );

			GetThreadContext( threadHandle );

			// Notify childs
			OnBreakpoint( address );

			// Step through
			WriteProcessMemory( address, code );

			if ( SystemInfo.IsX64 )
			{
				_Context64.Eip--;
				_Context64.EFlags |= 0x100; // Single step
			}
			else
			{
				_Context.Eip--;
				_Context.EFlags |= 0x100; // Single step
			}

			SetThreadContext( threadHandle );

			if ( !NativeMethods.ContinueDebugEvent( (uint) _Process.Id, _EventBuffer.dwThreadId, NativeMethods.ContinueStatus.DBG_CONTINUE ) )
				throw new SpyException( "Error continuing debug event. ErrorCode={0}", NativeMethods.GetLastError() );

			if ( !NativeMethods.WaitForDebugEvent( ref _EventBuffer, 2500 ) )
				throw new SpyException( "Error waiting for debug event. ErrorCode={0}", NativeMethods.GetLastError() );

			WriteProcessMemory( address, BreakpointCode );

			GetThreadContext( threadHandle );

			if ( SystemInfo.IsX64 )
				_Context64.EFlags &= ~0x100u; //  End single step
			else
				_Context.EFlags &= ~0x100u; // End single step

			SetThreadContext( threadHandle );

			if ( !NativeMethods.CloseHandle( threadHandle ) )
				throw new SpyException( "Error closing handle. ErrorCode={0}", NativeMethods.GetLastError() );
		}
		#endregion

		#region Context
		/// <summary>
		/// Gets value of specific register.
		/// </summary>
		/// <param name="register">Register value.</param>
		/// <returns>Register value.</returns>
		protected uint GetContextRegister( Register register )
		{
			if ( SystemInfo.IsX64 )
			{
				switch ( register )
				{
					case Register.Eax: return _Context64.Eax;
					case Register.Ebp: return _Context64.Ebp;
					case Register.Ebx: return _Context64.Ebx;
					case Register.Ecx: return _Context64.Ecx;
					case Register.Edi: return _Context64.Edi;
					case Register.Edx: return _Context64.Edx;
					case Register.Esi: return _Context64.Esi;
					case Register.Esp: return _Context64.Esp;
				}
			}
			else
			{
				switch ( register )
				{
					case Register.Eax: return _Context.Eax;
					case Register.Ebp: return _Context.Ebp;
					case Register.Ebx: return _Context.Ebx;
					case Register.Ecx: return _Context.Ecx;
					case Register.Edi: return _Context.Edi;
					case Register.Edx: return _Context.Edx;
					case Register.Esi: return _Context.Esi;
					case Register.Esp: return _Context.Esp;
				}
			}

			return 0;
		}

		private void GetThreadContext( IntPtr hThread )
		{
			if ( SystemInfo.IsX64 )
			{
				if ( !NativeMethods.Wow64GetThreadContext( hThread, ref _Context64 ) )
					throw new SpyException( "Error getting thread context (WOW46). Error={0}", NativeMethods.GetLastError() );
			}
			else
			{
				if ( !NativeMethods.GetThreadContext( hThread, ref _Context ) )
					throw new SpyException( "Error getting thread context. Error={0}", NativeMethods.GetLastError() );
			}
		}

		private void SetThreadContext( IntPtr hThread )
		{
			if ( SystemInfo.IsX64 )
			{
				if ( !NativeMethods.Wow64SetThreadContext( hThread, ref _Context64 ) )
					throw new SpyException( "Error setting thread context (WOW46). Error={0}", NativeMethods.GetLastError() );
			}
			else
			{
				if ( !NativeMethods.SetThreadContext( hThread, ref _Context ) )
					throw new SpyException( "Error setting thread context. Error={0}", NativeMethods.GetLastError() );
			}
		}
		#endregion

		#region Memory
		/// <summary>
		/// Reads one byte data from process memory.
		/// </summary>
		/// <param name="address">Data address.</param>
		/// <returns>Buffer containing data.</returns>
		protected byte ReadProcessMemory( uint address )
		{
			return ReadProcessMemory( address, 1 )[ 0 ];
		}

		/// <summary>
		/// Reads data from process memory.
		/// </summary>
		/// <param name="address">Data address.</param>
		/// <param name="length">The number of bytes to be read.</param>
		/// <returns>Buffer containing data.</returns>
		protected byte[] ReadProcessMemory( uint address, uint length )
		{
			byte[] buffer = new byte[ length ];
			uint read;

			if ( !NativeMethods.ReadProcessMemory( _ProcessHandle,new IntPtr( address ), buffer, length, out read ) )
				throw new SpyException( "Error reading process memory. ErrorCode={0}", NativeMethods.GetLastError() );

			if ( read != length )
				throw new SpyException( "Invalid read length. Desired={0} Read={1}", length, read );

			return buffer;
		}

		/// <summary>
		/// Writes byte to process memory.
		/// </summary>
		/// <param name="address">Memory address.</param>
		/// <param name="data">Date to write.</param>
		protected void WriteProcessMemory( uint address, byte data )
		{
			WriteProcessMemory( address, new byte[] { data } );
		}

		/// <summary>
		/// Writes data to process memory.
		/// </summary>
		/// <param name="address">Memory address.</param>
		/// <param name="data">Date to write.</param>
		protected void WriteProcessMemory( uint address, byte[] data )
		{
			IntPtr addressPtr = new IntPtr( address );
			uint length = (uint) data.Length;
			uint written;

			if ( !NativeMethods.WriteProcessMemory( _ProcessHandle, addressPtr, data, length, out written ) )
				throw new SpyException( "Error writing process memory. ErrorCode={0}", NativeMethods.GetLastError() );

			if ( written != length )
				throw new SpyException( "Invalid write length. Desired={0} Written={1}", length, written );

			if ( !NativeMethods.FlushInstructionCache( _ProcessHandle, addressPtr, length ) )
				throw new SpyException( "Error flushing instruction cache. ErrorCode={0}", NativeMethods.GetLastError() );
		}
		#endregion
		#endregion
	}
}
