using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Interaction logic for UltimaPacketBinaryView.xaml
	/// </summary>
	public partial class UltimaPacketBinaryView : UserControl
	{
		#region Properties
		/// <summary>
		/// Represents Packet property.
		/// </summary>
		public static readonly DependencyProperty PacketProperty = DependencyProperty.Register(
			"Packet", typeof( UltimaPacket ), typeof( UltimaPacketBinaryView ),
			new PropertyMetadata( null, new PropertyChangedCallback( Packet_Changed ) ) );

		/// <summary>
		/// Gets or sets displayed packet.
		/// </summary>
		public UltimaPacket Packet
		{
			get { return GetValue( PacketProperty ) as UltimaPacket; }
			set { SetValue( PacketProperty, value ); }
		}

		private SaveFileDialog _SaveFileDialog;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaPacketBinaryView.
		/// </summary>
		public UltimaPacketBinaryView()
		{
			InitializeComponent();
		}
		#endregion

		#region Methods
		private void UpdatePacket()
		{
			UltimaPacket packet = Packet;

			if ( packet == null )
			{
				Save.Visibility = Visibility.Collapsed;
				Open.Visibility = Visibility.Collapsed;
				Text.Text = String.Empty;
				return;
			}
			else
			{
				Save.Visibility = Visibility.Visible;
				Open.Visibility = Visibility.Visible;
			}

			byte[] data = packet.Data;
			StringBuilder binaryBuilder = new StringBuilder( data.Length * 3 + data.Length / 8 );
			StringBuilder textBuilder = new StringBuilder( data.Length );
			byte b1, b2;

			for ( int i = 0; i < data.Length; i++ )
			{
				b1 = (byte) ( data[ i ] >> 4 );
				b2 = (byte) ( data[ i ] & 0xF );

				binaryBuilder.Append( (char) ( b1 > 9 ? b1 + 0x37 : b1 + 0x30 ) );
				binaryBuilder.Append( (char) ( b2 > 9 ? b2 + 0x37 : b2 + 0x30 ) );
				binaryBuilder.Append( ' ' );

				b1 = data[ i ];

				// Crap
				if ( b1 < 0x20 || b1 == 0xB7 || b1 == 0xFF )
					b1 = (byte) '.';

				textBuilder.Append( (char) b1 );

				if ( ( i + 1 ) % 8 == 0 )
				{
					binaryBuilder.Remove( binaryBuilder.Length - 1, 1 );
					binaryBuilder.AppendLine();
					textBuilder.AppendLine();
				}
			}

			Binary.Text = binaryBuilder.ToString();
			Text.Text = textBuilder.ToString();
		}

		#region Event Handlers
		private static void Packet_Changed( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			UltimaPacketBinaryView view = d as UltimaPacketBinaryView;

			if ( view != null )
				view.UpdatePacket();
		}

		private void Save_Click( object sender, RoutedEventArgs e )
		{
			UltimaPacket packet = Packet;

			if ( packet != null )
			{
				try
				{
					if ( _SaveFileDialog == null )
					{
						_SaveFileDialog = new SaveFileDialog();
						_SaveFileDialog.Filter = "Ultima Packet (*.packet)|*.packet";
						_SaveFileDialog.CheckPathExists = true;
						_SaveFileDialog.Title = "Save Packet";
					}

					if ( _SaveFileDialog.ShowDialog( App.Window ) == true )
					{
						using ( FileStream stream = File.Create( _SaveFileDialog.FileName, packet.Data.Length ) )
						{
							stream.Write( packet.Data, 0, packet.Data.Length );
						}
					}
				}
				catch ( Exception ex )
				{
					App.Window.ShowNotification( NotificationType.Error, ex );
				}
			}
		}

		private void Open_Click( object sender, RoutedEventArgs e )
		{
			UltimaPacket packet = Packet;

			if ( packet != null )
			{
				try
				{
					BinaryWindow window = new BinaryWindow();
					window.Binary.Packet = packet;
					window.Show();
				}
				catch ( Exception ex )
				{
					App.Window.ShowNotification( NotificationType.Error, ex );
				}
			}
		}
		#endregion
		#endregion
	}
}
