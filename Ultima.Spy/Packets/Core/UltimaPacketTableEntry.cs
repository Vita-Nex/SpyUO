
namespace Ultima.Spy
{
	/// <summary>
	/// Describes ultima packet entry.
	/// </summary>
	public class UltimaPacketTableEntry
	{
		#region Properties
		private UltimaPacketDefinition _FromClient;

		/// <summary>
		/// Gets definition for packets comming from client.
		/// </summary>
		public UltimaPacketDefinition FromClient
		{
			get { return _FromClient; }
			set { _FromClient = value; }
		}

		private UltimaPacketDefinition _FromServer;

		/// <summary>
		/// Gets definition for packet comming from server.
		/// </summary>
		public UltimaPacketDefinition FromServer
		{
			get { return _FromServer; }
			set { _FromServer = value; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaPacketTableEntry.
		/// </summary>
		public UltimaPacketTableEntry()
		{
		}
		#endregion
	}
}
