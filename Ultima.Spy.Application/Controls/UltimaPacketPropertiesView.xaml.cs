using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ultima.Spy.Packets;
using Microsoft.Win32;
using System.IO;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Interaction logic for UltimaPacketPropertiesView.xaml
	/// </summary>
	public partial class UltimaPacketPropertiesView : UserControl
	{
		#region Properties
		/// <summary>
		/// Represents Packet property.
		/// </summary>
		public static readonly DependencyProperty PacketProperty = DependencyProperty.Register(
			"Packet", typeof( UltimaPacket ), typeof( UltimaPacketPropertiesView ), 
			new PropertyMetadata( null, new PropertyChangedCallback( Packet_Changed ) ) );

		/// <summary>
		/// Gets or sets displayed packet.
		/// </summary>
		public UltimaPacket Packet
		{
			get { return GetValue( PacketProperty ) as UltimaPacket; }
			set { SetValue( PacketProperty, value ); }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaPacketPropertiesView.
		/// </summary>
		public UltimaPacketPropertiesView()
		{
			InitializeComponent();
		}
		#endregion

		#region Methods
		private void UpdatePacket()
		{
			UltimaPacket packet = Packet;

			if ( packet is GenericGumpPacket || 
				packet is WorldObjectPacket || 
				packet is MobileIncommingPacket )
				Generate.Visibility = Visibility.Visible;
			else
				Generate.Visibility = Visibility.Collapsed;

			if ( packet is IUltimaEntity )
				FindRelatives.Visibility = Visibility.Visible;
			else
				FindRelatives.Visibility = Visibility.Collapsed;

			if ( packet != null )
			{
				CopyToClipboard.Visibility = Visibility.Visible;
				OpenInNewWindow.Visibility = Visibility.Visible;
			}
			else
			{
				CopyToClipboard.Visibility = Visibility.Collapsed;
				OpenInNewWindow.Visibility = Visibility.Collapsed;
			}
		}

		#region Event Handlers
		private static void Packet_Changed( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			UltimaPacketPropertiesView view = d as UltimaPacketPropertiesView;

			if ( view != null )
				view.UpdatePacket();
		}
		#endregion
		#endregion
	}
}
