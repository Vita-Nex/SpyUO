using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Interaction logic for RelativesWindow.xaml
	/// </summary>
	public partial class RelativesWindow : Window
	{
		#region Properties
		/// <summary>
		/// Represents Packet property.
		/// </summary>
		public static readonly DependencyProperty PacketProperty = DependencyProperty.Register(
			"Packet", typeof( UltimaPacket ), typeof( RelativesWindow ), new PropertyMetadata( null ) );

		/// <summary>
		/// Gets or sets packet.
		/// </summary>
		public UltimaPacket Packet
		{
			get { return GetValue( PacketProperty ) as UltimaPacket; }
			set { SetValue( PacketProperty, value ); }
		}

		/// <summary>
		/// Represents Relatives property.
		/// </summary>
		public static readonly DependencyProperty RelativesProperty = DependencyProperty.Register(
			"Relatives", typeof( ObservableCollection<UltimaPacket> ), typeof( RelativesWindow ), new PropertyMetadata( null ) );

		/// <summary>
		/// Gets or sets relatives.
		/// </summary>
		public ObservableCollection<UltimaPacket> Relatives
		{
			get { return GetValue( RelativesProperty ) as ObservableCollection<UltimaPacket>; }
			set { SetValue( RelativesProperty, value ); }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of RelativesWindow.
		/// </summary>
		public RelativesWindow()
		{
			InitializeComponent();
		}
		#endregion

		#region Methods
		/// <summary>
		/// Wires events for realtime relative display.
		/// </summary>
		/// <param name="spyHelper">Spy helper.</param>
		public void WireEvents( SpyHelper spyHelper )
		{
			spyHelper.OnPacket += new Action<UltimaPacket>( SpyHelper_OnPacket );
		}

		#region Event Handlers
		private void PacketsView_MouseDoubleClick( object sender, MouseButtonEventArgs e )
		{
			UltimaPacket packet = PacketsView.SelectedValue as UltimaPacket;

			if ( packet != null )
			{
				PropertiesWindow properties = new PropertiesWindow();
				properties.Properties.Packet = packet;
				properties.Show();
			}
		}

		private void SpyHelper_OnPacket( UltimaPacket relative )
		{
			if ( Packet == null )
				return;

			IUltimaEntity packet = Packet as IUltimaEntity;
			IUltimaEntity entity = relative as IUltimaEntity;

			if ( packet != null && entity != null && packet.Serial == entity.Serial )
				Relatives.Add( relative );
		}
		#endregion
		#endregion
	}
}
