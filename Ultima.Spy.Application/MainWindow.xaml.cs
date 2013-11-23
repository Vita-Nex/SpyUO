using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using Ultima.Package;
using Ultima.Spy.Packets;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region Properties
		private ObservableCollection<Notification> _Notifications;

		/// <summary>
		/// Gets notifications.
		/// </summary>
		public ObservableCollection<Notification> Notifications
		{
			get { return _Notifications; }
		}

		private SpyHelper _SpyHelper;

		/// <summary>
		/// Gets spy helper.
		/// </summary>
		public SpyHelper SpyHelper
		{
			get { return _SpyHelper; }
		}

		private UltimaPacketFilter _Filter;

		/// <summary>
		/// Gets packet filter.
		/// </summary>
		public UltimaPacketFilter Filter
		{
			get { return _Filter; }
		}

		private OpenFileDialog _OpenFileDialog = null;
		private SaveFileDialog _SaveFileDialog = null;

		private double _OldWidth = 250;
		private double _OldHeight;
		private string _DefaultFilter = "Default.filter";
		#endregion

		#region Constructor
		/// <summary>
		/// Constructs a new instance of MainWindow.
		/// </summary>
		public MainWindow()
		{
			_Notifications = new ObservableCollection<Notification>();
			_SpyHelper = new SpyHelper();
			_SpyHelper.PacketsView.Filter += new Predicate<object>( Filter_Displayed );
			_Filter = new UltimaPacketFilter();
			_Filter.Changed += new EventHandler( Filter_Changed );

			InitializeComponent();
		}
		#endregion

		#region Methods
		#region Notifications
		/// <summary>
		/// Shows notification.
		/// </summary>
		/// <param name="type">Type to show.</param>
		/// <param name="ex">Exception to show.</param>
		public void ShowNotification( NotificationType type, Exception ex )
		{
			ShowNotification( new Notification( type, ex ) );
		}

		/// <summary>
		/// Shows notification.
		/// </summary>
		/// <param name="type">Type to show.</param>
		/// <param name="title">Title to show.</param>
		/// <param name="message">Message to show.</param>
		public void ShowNotification( NotificationType type, string title, string message = null )
		{
			ShowNotification( new Notification( type, title, message ) );
		}

		/// <summary>
		/// Shows notification.
		/// </summary>
		/// <param name="notification">Notification to show.</param>
		public void ShowNotification( Notification notification )
		{
			// Make sure its in GUI thread
			Dispatcher.BeginInvoke( new Action<Notification>( AddNotification ), notification );
		}

		private void AddNotification( Notification notification )
		{
			_Notifications.Add( notification );

			NotificationView.Visibility = Visibility.Visible;
			NotificationSplitter.Visibility = Visibility.Visible;
			NotificationRow.Height = new GridLength( _OldHeight );
			NotificationButton.IsChecked = true;
		}

		private void NotificationButton_Checked( object sender, RoutedEventArgs e )
		{
			NotificationView.Visibility = Visibility.Visible;
			NotificationSplitter.Visibility = Visibility.Visible;
			NotificationRow.Height = new GridLength( _OldHeight );
			NotificationButton.IsChecked = true;
		}

		private void NotificationButton_Unchecked( object sender, RoutedEventArgs e )
		{
			_OldHeight = NotificationRow.ActualHeight;
			NotificationView.Visibility = Visibility.Collapsed;
			NotificationSplitter.Visibility = Visibility.Collapsed;
			NotificationRow.Height = new GridLength( 1, GridUnitType.Auto );
			NotificationButton.IsChecked = false;
		}

		private void NotificationView_RowDetailsVisibilityChanged( object sender, DataGridRowDetailsEventArgs e )
		{
			Notification notification = NotificationView.SelectedValue as Notification;

			if ( notification != null && String.IsNullOrEmpty( notification.Message ) )
				e.DetailsElement.Visibility = Visibility.Collapsed;
			else
				e.DetailsElement.Visibility = Visibility.Visible;
		}
		#endregion

		#region Filter
		private void Filter_Changed( object sender, EventArgs e )
		{
			try
			{
				ShowLoading();
				_SpyHelper.PacketsView.Refresh();
				HideLoading();
			}
			catch ( Exception ex )
			{
				ShowNotification( NotificationType.Error, ex );
			}
		}

		private bool Filter_Displayed( object o )
		{
			UltimaPacket packet = o as UltimaPacket;

			if ( packet != null )
			{
				if ( _Filter == null || !_Filter.Active )
					return true;

				try
				{
					return _Filter.IsDisplayed( packet );
				}
				catch ( Exception ex )
				{
					ShowNotification( NotificationType.Error, ex );
				}
			}

			return true;
		}
		#endregion

		#region Loading
		private Storyboard _Loading;
		
		/// <summary>
		/// Shows loading indicator.
		/// </summary>
		public void ShowLoading()
		{
			try
			{
				Loading.Visibility = Visibility.Visible;

				if ( _Loading == null )
					_Loading = Resources[ "MainLoadingAnimation" ] as Storyboard;

				if ( _Loading != null )
					_Loading.Begin();
			}
			catch ( Exception ex )
			{
				ShowNotification( NotificationType.Error, ex );
			}
		}

		/// <summary>
		/// Hides loading indicator.
		/// </summary>
		public void HideLoading()
		{
			try
			{
				if ( _Loading != null )
					_Loading.Stop();

				Loading.Visibility = Visibility.Collapsed;
			}
			catch ( Exception ex )
			{
				ShowNotification( NotificationType.Error, ex );
			}
		}
		#endregion

		#region Assets
		private void InitializeAssets()
		{
			if ( _SpyHelper == null )
				return;

			if ( _SpyHelper.IsEnhancedClient )
			{
				if ( Globals.Instance.EnhancedAssets != null )
					return;
			}
			else
			{
				if ( Globals.Instance.LegacyAssets != null )
					return;
			}

			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += new DoWorkEventHandler( AssertWorker_DoWork );
			worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler( AssetWorker_RunWorkerCompleted );
			worker.RunWorkerAsync( _SpyHelper.Path );
		}

		private void AssertWorker_DoWork( object sender, DoWorkEventArgs e )
		{
			string sourceFolder = Path.GetDirectoryName( (string) e.Argument );

			if ( _SpyHelper.IsEnhancedClient )
				Globals.Instance.InitializeEnhancedAssets( sourceFolder );
			else
				Globals.Instance.InitializeLegacyAssets( sourceFolder );

			e.Result = sourceFolder;
		}

		private void AssetWorker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
		{
			if ( e.Error != null )
			{
				ShowNotification( NotificationType.Error, e.Error );
				return;
			}

			if ( _SpyHelper.IsEnhancedClient )
			{
				if ( Globals.Instance.EnhancedAssets != null )
					ShowNotification( NotificationType.Info, String.Format( "Loaded resources from {0}", e.Result ) );
			}
			else
			{
				if ( Globals.Instance.LegacyAssets != null )
					ShowNotification( NotificationType.Info, String.Format( "Loaded resources from {0}", e.Result ) );
			}
		}
		#endregion

		#region Event Handlers
		private void Window_Loaded( object sender, RoutedEventArgs e )
		{
			_OldHeight = 100;
			_OldWidth = SidePanelColumn.Width.Value;

			BackgroundWorker starterWorker = new BackgroundWorker();
			starterWorker.DoWork += new DoWorkEventHandler( StarterWorker_DoWork );
			starterWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler( StarterWorker_RunWorkerCompleted );
			starterWorker.RunWorkerAsync();

			ShowLoading();
		}

		private void StarterWorker_DoWork( object sender, DoWorkEventArgs e )
		{
			Globals.Instance = new Globals();
		}

		private void StarterWorker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
		{
			HideLoading();

			if ( e.Error != null )
			{
				ShowNotification( NotificationType.Error, e.Error );
				return;
			}

			// Set filter
			_Filter.Initialize();
			FilterView.Filter = _Filter;

			if ( _Filter != null )
			{
				using ( IsolatedStorageFile storage = IsolatedStorageFile.GetStore( IsolatedStorageScope.User | IsolatedStorageScope.Assembly | IsolatedStorageScope.Domain, null, null ) )
				{
					if ( storage.FileExists( _DefaultFilter ) )
					{
						using ( IsolatedStorageFileStream stream = storage.OpenFile( _DefaultFilter, FileMode.Open, FileAccess.Read, FileShare.Read ) )
						{
							if ( stream != null )
								_Filter.Load( stream );
						}
					}
				}
			}

			if ( Globals.Instance.LegacyClientFolder != null )
				ShowNotification( NotificationType.Info, String.Format( "Detected classic client in folder '{0}'", Globals.Instance.LegacyClientFolder ) );

			if ( Globals.Instance.EnhancedClientFolder != null )
				ShowNotification( NotificationType.Info, String.Format( "Detected enhanced client in folder '{0}'", Globals.Instance.EnhancedClientFolder ) );

			if ( Globals.Instance.VlcInstallationFolder != null )
				ShowNotification( NotificationType.Info, String.Format( "Detected VLC player version '{0}'", VlcPlayer.GetVersion( Globals.Instance.VlcInstallationFolder ) ) );
			else
				ShowNotification( NotificationType.Warning, "VLC player not installed. Some features will not be supported." );

			if ( Globals.Instance.ItemDefinitions != null && Globals.Instance.ItemProperties != null )
				LootAnalyzerButton.IsEnabled = true;
		}

		private void NewCommand_Executed( object sender, ExecutedRoutedEventArgs e )
		{
			ShowLoading();

			try
			{
				_SpyHelper.ClearPackets();
			}
			catch ( Exception ex )
			{
				ShowNotification( NotificationType.Error, ex );
			}

			HideLoading();
		}

		private void SaveCommand_Executed( object sender, ExecutedRoutedEventArgs e )
		{
			try
			{
				if ( _SaveFileDialog == null )
				{
					_SaveFileDialog = new SaveFileDialog();
					_SaveFileDialog.Filter = "Binary files (*.bin)|*.bin";
				}

				if ( _SaveFileDialog.ShowDialog( this ) == true )
				{
					ShowLoading();
					_SpyHelper.SaveBinary( _SaveFileDialog.FileName );
				}
			}
			catch ( Exception ex )
			{
				ShowNotification( NotificationType.Error, ex );
			}
			finally
			{
				HideLoading();
			}
		}

		private void OpenCommand_Executed( object sender, ExecutedRoutedEventArgs e )
		{
			try
			{
				if ( _OpenFileDialog == null )
				{
					_OpenFileDialog = new OpenFileDialog();
					_OpenFileDialog.CheckFileExists = true;
				}

				_OpenFileDialog.Filter = "Binary files (*.bin)|*.bin";

				if ( _OpenFileDialog.ShowDialog( this ) == true )
				{
					ShowLoading();

					BackgroundWorker worker = new BackgroundWorker();
					worker.DoWork += new DoWorkEventHandler( LoadPacketWorker_DoWork );
					worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler( LoadPacketWorker_RunWorkerCompleted );
					worker.RunWorkerAsync( _OpenFileDialog.FileName );
				}
			}
			catch ( Exception ex )
			{
				ShowNotification( NotificationType.Error, ex );
			}
		}

		private void StartCommand_Executed( object sender, ExecutedRoutedEventArgs e )
		{
			try
			{
				if ( _SpyHelper.IsPaused )
				{
					ShowLoading();
					_SpyHelper.Resume();
				}
				else
				{
					if ( _OpenFileDialog == null )
					{
						_OpenFileDialog = new OpenFileDialog();
						_OpenFileDialog.CheckFileExists = true;
					}

					_OpenFileDialog.Filter = "Executable files (*.exe)|*.exe";

					if ( _OpenFileDialog.ShowDialog( this ) == true )
					{
						ShowLoading();
						_SpyHelper.Start( _OpenFileDialog.FileName );
						InitializeAssets();
					}
				}
			}
			catch ( Exception ex )
			{
				ShowNotification( NotificationType.Error, ex );
			}
			finally
			{
				HideLoading();
			}
		}

		private void LoadPacketWorker_DoWork( object sender, DoWorkEventArgs e )
		{
			_SpyHelper.LoadBinary( (string) e.Argument );
		}

		private void LoadPacketWorker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
		{
			HideLoading();

			if ( e.Error != null )
			{
				ShowNotification( NotificationType.Error, e.Error );
				return;
			}
		}

		private void AttachCommand_CanExecute( object sender, CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = _SpyHelper.CanStart;
		}

		private void AttachCommand_Executed( object sender, ExecutedRoutedEventArgs e )
		{
			try
			{
				ProcessListWindow window = new ProcessListWindow();

				if ( window.ShowDialog() == true )
				{
					ShowLoading();
					_SpyHelper.Attach( window.Selected );
					InitializeAssets();
				}
			}
			catch ( Exception ex )
			{
				ShowNotification( NotificationType.Error, ex );
			}
			finally
			{
				HideLoading();
			}
		}

		private void PauseCommand_CanExecute( object sender, CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = _SpyHelper.CanPause;
		}

		private void PauseCommand_Executed( object sender, ExecutedRoutedEventArgs e )
		{
			ShowLoading();

			try
			{
				_SpyHelper.Pause();
			}
			catch ( Exception ex )
			{
				ShowNotification( NotificationType.Error, ex );
			}

			HideLoading();
		}

		private void StopCommand_CanExecute( object sender, CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = _SpyHelper.CanStop;
		}

		private void StopCommand_Executed( object sender, ExecutedRoutedEventArgs e )
		{
			ShowLoading();

			try
			{
				_SpyHelper.Stop();
			}
			catch ( Exception ex )
			{
				ShowNotification( NotificationType.Error, ex );
			}

			HideLoading();
		}

		private void CopyToClipboard_CanExecute( object sender, CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = e.Parameter is UltimaPacketValue;
		}

		private void CopyToClipboard_Executed( object sender, ExecutedRoutedEventArgs e )
		{
			try
			{
				UltimaPacketValue value = e.Parameter as UltimaPacketValue;

				if ( value != null )
					Clipboard.SetText( value.ToString() );
			}
			catch ( Exception ex )
			{
				App.Window.ShowNotification( NotificationType.Error, ex );
			}
		}

		private void OpenInNewWindow_CanExecute( object sender, CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = e.Parameter is UltimaPacket;
		}

		private void OpenInNewWindow_Executed( object sender, ExecutedRoutedEventArgs e )
		{
			try
			{
				UltimaPacket packet = e.Parameter as UltimaPacket;

				if ( packet == null )
					return;

				PropertiesWindow properties = new PropertiesWindow();
				properties.Properties.Packet = packet;
				properties.Show();
			}
			catch ( Exception ex )
			{
				App.Window.ShowNotification( NotificationType.Error, ex );
			}
		}

		private void GenerateCommand_CanExecute( object sender, CanExecuteRoutedEventArgs e )
		{
			if ( e.Parameter is GenericGumpPacket ||
				e.Parameter is WorldObjectPacket ||
				e.Parameter is MobileIncommingPacket ||
				e.Parameter is ContainerItem )
				e.CanExecute = true;
			else
				e.CanExecute = false;
		}

		private void GenerateCommand_Executed( object sender, ExecutedRoutedEventArgs e )
		{
			if ( _SaveFileDialog == null )
			{
				_SaveFileDialog = new SaveFileDialog();
				_SaveFileDialog.Filter = "C# Source File (*.cs)|*.cs";
				_SaveFileDialog.CheckPathExists = true;
				_SaveFileDialog.Title = "Save Class";
			}

			if ( _SaveFileDialog.ShowDialog() == true )
			{
				try
				{
					App.Window.ShowLoading();

					if ( e.Parameter is GenericGumpPacket )
					{
						GenericGumpPacket gump = (GenericGumpPacket) e.Parameter;

						using ( FileStream stream = File.Open( _SaveFileDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None ) )
						{
							UltimaGumpGenerator.Generate( stream, gump );
						}
					}
					else if ( e.Parameter is WorldObjectPacket )
					{
						WorldObjectPacket item = (WorldObjectPacket) e.Parameter;
						QueryPropertiesResponsePacket properties = _SpyHelper.FindFirstPacket( item.Serial, typeof( QueryPropertiesResponsePacket ) ) as QueryPropertiesResponsePacket;

						using ( FileStream stream = File.Open( _SaveFileDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None ) )
						{
							UltimaItemGenerator.Generate( stream, item.ObjectID, item.Hue, item.Amount, properties );
						}
					}
					else if ( e.Parameter is ContainerItem )
					{
						ContainerItem item = (ContainerItem) e.Parameter;
						QueryPropertiesResponsePacket properties = _SpyHelper.FindFirstPacket( item.Serial, typeof( QueryPropertiesResponsePacket ) ) as QueryPropertiesResponsePacket;

						using ( FileStream stream = File.Open( _SaveFileDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None ) )
						{
							UltimaItemGenerator.Generate( stream, item.ItemID, item.Hue, item.Amount, properties );
						}
					}
					else if ( e.Parameter is MobileIncommingPacket )
					{
						MobileIncommingPacket mobile = (MobileIncommingPacket) e.Parameter;
						MobileNamePacket name = _SpyHelper.FindFirstPacket( mobile.Serial, typeof( MobileNamePacket ) ) as MobileNamePacket;

						using ( FileStream stream = File.Open( _SaveFileDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None ) )
						{
							UltimaMobileGenerator.Generate( stream, mobile, name );
						}
					}
				}
				catch ( Exception ex )
				{
					ShowNotification( NotificationType.Error, ex );
				}
				finally
				{
					HideLoading();
				}
			}
		}

		private void FindRelatives_CanExecute( object sender, CanExecuteRoutedEventArgs e )
		{
			e.CanExecute = e.Parameter is IUltimaEntity;
		}

		private void FindRelatives_Executed( object sender, ExecutedRoutedEventArgs e )
		{
			ShowLoading();

			try
			{
				if ( e.Parameter is UltimaPacket && e.Parameter is IUltimaEntity )
				{
					IUltimaEntity entity = (IUltimaEntity) e.Parameter;

					RelativesWindow window = new RelativesWindow();
					window.Packet = (UltimaPacket) e.Parameter;
					window.Relatives = _SpyHelper.FindEntities( entity.Serial );
					window.WireEvents( _SpyHelper );
					window.Show();

				}
			}
			catch ( Exception ex )
			{
				ShowNotification( NotificationType.Error, ex );
			}

			HideLoading();
		}

		private void SidePanel_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			if ( SidePanel.SelectedIndex == 0 )
			{
				_OldWidth = SidePanelColumn.ActualWidth;
				SidePanelColumn.Width = new GridLength( 1, GridUnitType.Auto );
			}
			else if ( SidePanelColumn.Width.IsAuto )
				SidePanelColumn.Width = new GridLength( _OldWidth );
		}

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

		private void SubmitBug_Click( object sender, RoutedEventArgs e )
		{
			Process.Start( "http://code.google.com/p/mondains-legacy/issues/entry" );
		}

		private void About_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				AboutWindow window = new AboutWindow();
				window.Show();
			}
			catch ( Exception ex )
			{
				ShowNotification( NotificationType.Error, ex );
			}
		}

		private void NotificationsClear_Click( object sender, RoutedEventArgs e )
		{
			_Notifications.Clear();
		}

		private void ClilocButton_Click( object sender, RoutedEventArgs e )
		{
			ShowLoading();

			try
			{
				ClilocWindow window = new ClilocWindow();

				if ( Globals.Instance.Clilocs != null )
					window.Clilocs = Globals.Instance.Clilocs;

				window.Show();
			}
			catch ( Exception ex )
			{
				ShowNotification( NotificationType.Error, ex );
			}

			HideLoading();
		}

		private void LootAnalyzerButton_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				LootAnalyzerWindow window = new LootAnalyzerWindow();
				window.Show();
			}
			catch ( Exception ex )
			{
				ShowNotification( NotificationType.Error, ex );
			}
		}

		private void Window_Closing( object sender, CancelEventArgs e )
		{
			try
			{
				if ( VlcPlayer.Inititalized )
					VlcPlayer.Uninitialize();

				// Save filter to isolated storage
				if ( _Filter != null )
				{
					using ( IsolatedStorageFile storage = IsolatedStorageFile.GetStore( IsolatedStorageScope.User | IsolatedStorageScope.Assembly | IsolatedStorageScope.Domain, null, null ) )
					{
						using ( IsolatedStorageFileStream stream = storage.CreateFile( _DefaultFilter ) )
						{
							_Filter.Save( stream );
						}
					}
				}
			}
			catch
			{
				// No sense in doing anything
			}
		}
		#endregion
		#endregion
	}
}
