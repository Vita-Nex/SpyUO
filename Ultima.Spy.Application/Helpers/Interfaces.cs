using System;
using System.IO;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Filter item interface.
	/// </summary>
	public interface IUltimaPacketFilterEntry
	{
		/// <summary>
		/// Gets or sets entry parent.
		/// </summary>
		UltimaPacketFilterTable Parent { get; }

		/// <summary>
		/// Gets or sets visibility.
		/// </summary>
		bool IsVisible { get; set; }

		/// <summary>
		/// Gets or sets status.
		/// </summary>
		bool IsChecked { get; set; }

		/// <summary>
		/// Determines whether this entry is filtered out by search expression.
		/// </summary>
		bool IsFiltered { get; set; }

		/// <summary>
		/// Shows all entries.
		/// </summary>
		void ShowAll();

		/// <summary>
		/// Hides unknown entries.
		/// </summary>
		void HideUnknown();

		/// <summary>
		/// Determines whether this entry is filtered or not.
		/// </summary>
		/// <param name="name">Entry name.</param>
		void Filter( string query );

		/// <summary>
		/// Saves entry to stream.
		/// </summary>
		/// <param name="writer">Writer to write to.</param>
		void Save( BinaryWriter writer );

		/// <summary>
		/// Loads entry from stream.
		/// </summary>
		/// <param name="reeader">Reader to read from.</param>
		void Load( BinaryReader reeader );
	}
}
