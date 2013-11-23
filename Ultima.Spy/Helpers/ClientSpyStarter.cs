using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

using Ultima.Analyzer;

using ProcessInfo = Ultima.Spy.NativeMethods.PROCESS_INFORMATION;
using StartupInfo = Ultima.Spy.NativeMethods.STARTUPINFO;

namespace Ultima.Spy
{
	/// <summary>
	/// Describes client type.
	/// </summary>
	public enum UltimaClientType
	{
		/// <summary>
		/// Invalid.
		/// </summary>
		Invalid,

		/// <summary>
		/// Classic.
		/// </summary>
		Classic,

		/// <summary>
		/// Enhanced.
		/// </summary>
		Enhanced,
	}

	/// <summary>
	/// Provides methods for starting client spy.
	/// </summary>
	public static class ClientSpyStarter
	{
		#region Properties
		private const string RegistryKey = "SOFTWARE\\SpyUO\\";
		private const string ClassicClientProductName = "Ultima Online";
		private const string EnhancedClientProductName = "Ultima Online: Stygian Abyss";
		private const string ClassicClientProcessName = "client";
		private const string EnhancedClientProcessName = "UOSA";

		private const string DebugProtection1 = "DebugProtection1";
		private const string DebugProtection2 = "DebugProtection2";
		private const string TimeDateStamp = "TimeDateStamp";
		private const string SendAddress = "SendAddress";
		private const string SendAddressRegister = "SendAddressRegister";
		private const string SendLengthRegister = "SendLengthRegister";
		private const string ReceiveAddress = "ReceiveAddress";
		private const string ReceiveAddressRegister = "ReceiveAddressRegister";
		private const string ReceiveLengthRegister = "ReceiveLengthRegister";
		#endregion

		#region Methods
		/// <summary>
		/// Determines client type.
		/// </summary>
		/// <param name="process">Process to determine.</param>
		/// <returns>Client type.</returns>
		public static UltimaClientType GetClientType( Process process )
		{
			string fileName = process.ProcessName;

			if ( String.Equals( fileName, ClassicClientProcessName, StringComparison.InvariantCultureIgnoreCase ) )
				return UltimaClientType.Classic;
			else if ( String.Equals( fileName, EnhancedClientProcessName, StringComparison.InvariantCultureIgnoreCase ) )
				return UltimaClientType.Enhanced;

			return UltimaClientType.Invalid;
		}

		/// <summary>
		/// Starts a process and initializes client spy.
		/// </summary>
		/// <param name="filePath">File to attach to.</param>
		/// <param name="process">Reference to started process.</param>
		/// <returns>Client spy.</returns>
		public static ClientSpy Initialize( string filePath, out Process process )
		{
			ProcessStartInfo info = new ProcessStartInfo( filePath );
			info.WorkingDirectory = Path.GetDirectoryName( filePath );

			process = Process.Start( info );

			return Initialize( process );
		}

		/// <summary>
		/// Initializes client spy.
		/// </summary>
		/// <param name="process">Process to attach to.</param>
		/// <returns>Client spy.</returns>
		public static ClientSpy Initialize( Process process )
		{
			UltimaClientType clientType = GetClientType( process );

			if ( clientType == UltimaClientType.Enhanced )
				return InitializeEnhancedClientSpy( process );
			else
				return InitializeClassicClientSpy( process );
		}

		private static ClientSpy InitializeClassicClientSpy( Process process )
		{
			ClassicClientAnalyzer analyzer = new ClassicClientAnalyzer( process.MainModule.FileName );

			// Keys
			int debugProtection1 = 0;
			int debugProtection2 = 0;
			int timeDateStamp;
			AddressAndRegisters sendKeys;
			AddressAndRegisters receiveKeys;

			// Detailed analysis if keys not found or version mismatch
			RegistryKey key = Registry.LocalMachine.OpenSubKey( RegistryKey, true );

			try
			{
				if ( key == null || !TryGetKeys( key, out timeDateStamp, out sendKeys, out receiveKeys, "Classic" ) || analyzer.TimeDateStamp != timeDateStamp )
				{
					analyzer.Analyze();

					if ( !analyzer.FoundDebugProtectionAddress1 || !analyzer.FoundDebugProtectionAddress2 )
						throw new SpyException( "Cannot find debug protection" );
					else if ( analyzer.SpyInfo == null )
						throw new SpyException( "Error finding classic client keys" );
					else if ( !analyzer.SpyInfo.FoundSend && !analyzer.SpyInfo.FoundReceive )
						throw new SpyException( "Cannot find send and receive classic client keys" );
					else if ( !analyzer.SpyInfo.FoundSend )
						throw new SpyException( "Cannot find send classic client key" );
					else if ( !analyzer.SpyInfo.FoundReceive )
						throw new SpyException( "Cannot find receive classic client key" );

					timeDateStamp = analyzer.TimeDateStamp;
					sendKeys = new AddressAndRegisters( (int) analyzer.SpyInfo.SendAddress, analyzer.SpyInfo.SendAddressRegister, analyzer.SpyInfo.SendLengthRegister );
					receiveKeys = new AddressAndRegisters( (int) analyzer.SpyInfo.ReceiveAddress, analyzer.SpyInfo.ReceiveAddressRegister, analyzer.SpyInfo.ReceiveLengthRegister );
					debugProtection1 = analyzer.DebugProtectionAddress1;
					debugProtection2 = analyzer.DebugProtectionAddress2;

					// Save
					if ( key == null )
						key = Registry.LocalMachine.CreateSubKey( RegistryKey, RegistryKeyPermissionCheck.ReadWriteSubTree );

					key.SetValue( DebugProtection1, debugProtection1 );
					key.SetValue( DebugProtection2, debugProtection2 );
					SaveKeys( key, timeDateStamp, sendKeys, receiveKeys, "Classic" );
				}
				else if ( key != null )
				{
					int? debugProtectionValue = key.GetValue( DebugProtection1 ) as int?;

					if ( debugProtectionValue != null )
						debugProtection1 = (int) debugProtectionValue;

					debugProtectionValue = key.GetValue( DebugProtection2 ) as int?;

					if ( debugProtectionValue is int )
						debugProtection2 = (int) debugProtectionValue;

					if ( debugProtection1 == 0 || debugProtection2 == 0 )
						throw new SpyException( "Cannot find debug protection" );
				}
			}
			finally
			{
				if ( key != null )
					key.Close();
			}

			// Client
			return new ClassicClientSpy( sendKeys, receiveKeys, (uint) debugProtection1, (uint) debugProtection2 );
		}

		private static ClientSpy InitializeEnhancedClientSpy( Process process )
		{
			EnhancedClientAnalyzer analyzer = new EnhancedClientAnalyzer( process.MainModule.FileName );

			// Keys
			int timeDateStamp;
			AddressAndRegisters sendKeys;
			AddressAndRegisters receiveKeys;

			// Detailed analysis if keys not found or version mismatch
			RegistryKey key = Registry.LocalMachine.OpenSubKey( RegistryKey, true );

			try
			{
				if ( key == null || !TryGetKeys( key, out timeDateStamp, out sendKeys, out receiveKeys, "Enhanced" ) || analyzer.TimeDateStamp != timeDateStamp )
				{
					analyzer.Analyze();

					if ( analyzer.SpyInfo == null )
						throw new SpyException( "Error finding enhanced client keys" );
					else if ( !analyzer.SpyInfo.FoundSend && !analyzer.SpyInfo.FoundReceive )
						throw new SpyException( "Cannot find send and receive enhanced client keys" );
					else if ( !analyzer.SpyInfo.FoundSend  )
						throw new SpyException( "Cannot find send enhanced client key" );
					else if ( !analyzer.SpyInfo.FoundReceive )
						throw new SpyException( "Cannot find receive enhanced client key" );

					timeDateStamp = analyzer.TimeDateStamp;
					sendKeys = new AddressAndRegisters( (int) analyzer.SpyInfo.SendAddress, analyzer.SpyInfo.SendAddressRegister, analyzer.SpyInfo.SendLengthRegister );
					receiveKeys = new AddressAndRegisters( (int) analyzer.SpyInfo.ReceiveAddress, analyzer.SpyInfo.ReceiveAddressRegister, analyzer.SpyInfo.ReceiveLengthRegister );

					// Save
					if ( key == null )
						key = Registry.LocalMachine.CreateSubKey( RegistryKey, RegistryKeyPermissionCheck.ReadWriteSubTree );

					SaveKeys( key, timeDateStamp, sendKeys, receiveKeys, "Enhanced" );
				}
			}
			finally
			{
				if ( key != null )
					key.Close();
			}

			// Client
			return new EnhancedClientSpy( sendKeys, receiveKeys );
		}

		private static bool TryGetKeys( RegistryKey key, out int timeDateStamp, out AddressAndRegisters sendKeys, out AddressAndRegisters receiveKeys, string type )
		{
			timeDateStamp = 0;
			sendKeys = null;
			receiveKeys = null;
			
			if ( key != null )
			{
				int? timeDateStampValue = key.GetValue( type + TimeDateStamp ) as int?;

				if ( timeDateStampValue != null )
					timeDateStamp = (int) timeDateStampValue;

				int? sendAddress = key.GetValue( type + SendAddress ) as int?;
				int? sendAddressRegister = key.GetValue( type + SendAddressRegister ) as int?;
				int? sendLengthRegister = key.GetValue( type + SendLengthRegister ) as int?;

				if ( sendAddress != null && sendAddress != 0 &&
					sendAddressRegister != null && sendAddressRegister != 0 &&
					sendLengthRegister != null && sendLengthRegister != 0 )
					sendKeys = new AddressAndRegisters( (int) sendAddress, (Register) sendAddressRegister, (Register) sendLengthRegister );

				int? receiveAddress = key.GetValue( type + ReceiveAddress ) as int?;
				int? receiveAddressRegister = key.GetValue( type + ReceiveAddressRegister ) as int?;
				int? receiveLengthRegister = key.GetValue( type + ReceiveLengthRegister ) as int?;

				if ( receiveAddress != null && receiveAddress != 0 &&
					receiveLengthRegister != null && receiveLengthRegister != 0 &&
					receiveLengthRegister != null && receiveLengthRegister != 0 )
					receiveKeys = new AddressAndRegisters( (int) receiveAddress, (Register) receiveAddressRegister, (Register) receiveLengthRegister );

				return timeDateStamp != 0 && sendKeys != null && receiveKeys != null;
			}

			return false;
		}

		private static void SaveKeys( RegistryKey key, int timeDateStamp, AddressAndRegisters sendKeys, AddressAndRegisters receiveKeys, string type )
		{
			if ( key != null )
			{
				key.SetValue( type + TimeDateStamp, timeDateStamp, RegistryValueKind.DWord );

				key.SetValue( type + SendAddress, sendKeys.Address, RegistryValueKind.DWord );
				key.SetValue( type + SendAddressRegister, sendKeys.DataAddressRegister, RegistryValueKind.DWord );
				key.SetValue( type + SendLengthRegister, sendKeys.DataLengthRegister, RegistryValueKind.DWord );

				key.SetValue( type + ReceiveAddress, receiveKeys.Address, RegistryValueKind.DWord );
				key.SetValue( type + ReceiveAddressRegister, receiveKeys.DataAddressRegister, RegistryValueKind.DWord );
				key.SetValue( type + ReceiveLengthRegister, receiveKeys.DataLengthRegister, RegistryValueKind.DWord );
			}
		}
		#endregion
	}
}
