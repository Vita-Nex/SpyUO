using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Stop command.
	/// </summary>
	public class UltimaCommand
	{
		public static readonly RoutedUICommand Start = new RoutedUICommand( "Start client to spy", "Start", typeof( UltimaCommand ) );
		public static readonly RoutedUICommand Attach = new RoutedUICommand( "Attach spy to process", "Attach", typeof( UltimaCommand ) );
		public static readonly RoutedUICommand Pause = new RoutedUICommand( "Pauses spy", "Pause", typeof( UltimaCommand ) );
		public static readonly RoutedUICommand Stop = new RoutedUICommand( "Stops spy", "Stop", typeof( UltimaCommand ) );
		public static readonly RoutedUICommand CopyToClipboard = new RoutedUICommand( "Copies all properties to clipboard", "CopyToClipboard", typeof( UltimaCommand ) );
		public static readonly RoutedUICommand OpenInNewWindow = new RoutedUICommand( "Opens packet in new window", "OpenInNewWindow", typeof( UltimaCommand ) );
		public static readonly RoutedUICommand GenerateClass = new RoutedUICommand( "Generates C# Class", "GenerateClass", typeof( UltimaCommand ) );
		public static readonly RoutedUICommand FindRelatives = new RoutedUICommand( "Finds all packets with identical serial", "FindRelatives", typeof( UltimaCommand ) );
	}
}
