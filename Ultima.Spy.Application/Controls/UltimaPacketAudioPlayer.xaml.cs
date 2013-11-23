using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Ultima.Package;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Interaction logic for UltimaPacketAudioPlayer.xaml
	/// </summary>
	public partial class UltimaPacketAudioPlayer : UserControl
	{
		#region Properties
		/// <summary>
		/// Represents File property.
		/// </summary>
		public static readonly DependencyProperty FileProperty = DependencyProperty.Register(
			"File", typeof( AudioFile ), typeof( UltimaPacketAudioPlayer ), new PropertyMetadata( null ) );

		/// <summary>
		/// Gets or sets audio file.
		/// </summary>
		public AudioFile File
		{
			get { return GetValue( FileProperty ) as AudioFile; }
			set { SetValue( FileProperty, value ); }
		}

		private Image _PlayImage;
		private Image _PauseImage;
		private bool _Suppress;
		#endregion

		#region Constrctors
		/// <summary>
		/// Constructs a new instance of UltimaPacketAudioPlayer.
		/// </summary>
		public UltimaPacketAudioPlayer()
		{
			InitializeComponent();
		}
		#endregion

		#region Methods
		#region Event Handlers
		private void PlayPause_Click( object sender, RoutedEventArgs e )
		{
			VlcPlayer player = Globals.Instance.VlcPlayer;

			if ( player == null || File == null )
				return;

			try
			{
				if ( player.IsPlaying )
				{
					player.Pause();
				}
				else
				{
					if ( !player.IsMediaLoaded )
					{
						player.LoadFromMemory( File.Data );
					}

					player.Play();
				}
			}
			catch ( Exception ex )
			{
				App.Window.ShowNotification( NotificationType.Error, ex );
			}
		}

		private void Slider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			VlcPlayer player = Globals.Instance.VlcPlayer;

			if ( player == null || File == null )
				return;

			try
			{
				if ( !_Suppress && player.IsMediaLoaded )
					player.Time = (int) Slider.Value;

				_Suppress = false;
			}
			catch ( Exception ex )
			{
				App.Window.ShowNotification( NotificationType.Error, ex );
			}
		}

		private void Save_Click( object sender, RoutedEventArgs e )
		{
			if ( File == null )
				return;

			try
			{
				SaveFileDialog dialog = new SaveFileDialog();
				dialog.Filter = "All Files|*.*";
				dialog.CheckPathExists = true;
				dialog.Title = "Save File";
				dialog.FileName = File.Name;

				if ( dialog.ShowDialog() == true )
				{
					using ( FileStream stream = System.IO.File.OpenWrite( dialog.FileName ) )
					{
						stream.Write( File.Data, 0, File.Data.Length );
					}
				}
			}
			catch ( Exception ex )
			{
				App.Window.ShowNotification( NotificationType.Error, ex );
			}
		}

		private void Player_Stopped()
		{
			PlayPause.Content = _PauseImage;
		}

		private void Player_TimeChanged()
		{
			Dispatcher.BeginInvoke( new Action( Player_TimeChangedSafe ) );
		}

		private void Player_TimeChangedSafe()
		{
			VlcPlayer player = Globals.Instance.VlcPlayer;

			if ( player == null )
				return;

			_Suppress = true;
			Slider.Value = player.Time;
			Slider.Minimum = 0;
			Slider.Maximum = player.Length;
		}

		private void UserControl_Loaded( object sender, RoutedEventArgs e )
		{
			_PlayImage = Resources[ "PlayerPlayImage" ] as Image;
			_PauseImage = Resources[ "PlayerPauseImage" ] as Image;
			PlayPause.Content = _PlayImage;

			VlcPlayer player = Globals.Instance.VlcPlayer;

			if ( player == null )
				return;

			player.TimeChanged += new Action( Player_TimeChanged );
			player.Stopped += new Action( Player_Stopped );
		}

		private void UserControl_Unloaded( object sender, RoutedEventArgs e )
		{
			VlcPlayer player = Globals.Instance.VlcPlayer;

			if ( player == null )
				return;

			player.TimeChanged -= new Action( Player_TimeChanged );
			player.Stopped -= new Action( Player_Stopped );
		}
		#endregion
		#endregion
	}
}
