using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using Ultima.Spy.Packets;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Interaction logic for LootAnalyzerWindow.xaml
	/// </summary>
	public partial class LootAnalyzerWindow : Window
	{
		#region Properties
		/// <summary>
		/// Represents LootAnalyzer property.
		/// </summary>
		public static readonly DependencyProperty LootAnalyzerProperty = DependencyProperty.Register(
			"LootAnalyzer", typeof( UltimaLootAnalyzer ), typeof( LootAnalyzerWindow ), 
			new PropertyMetadata( null, new PropertyChangedCallback( LootAnalyzer_Changed ) ) );

		/// <summary>
		/// Gets or sets entry bound to this thing.
		/// </summary>
		public UltimaLootAnalyzer LootAnalyzer
		{
			get { return GetValue( LootAnalyzerProperty ) as UltimaLootAnalyzer; }
			set { SetValue( LootAnalyzerProperty, value ); }
		}

		private BackgroundWorker _MobileWorker;
		private BackgroundWorker _LootWorker;
		private SaveFileDialog _SaveFileDialog;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of LootAnalyzerWindow.
		/// </summary>
		public LootAnalyzerWindow()
		{
			_MobileWorker = new BackgroundWorker();
			_MobileWorker.DoWork += new DoWorkEventHandler( MobileWorker_DoWork );
			_MobileWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler( MobileWorker_RunWorkerCompleted );
			_LootWorker = new BackgroundWorker();
			_LootWorker.DoWork += new DoWorkEventHandler( LootWorker_DoWork );
			_LootWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler( LootWorker_RunWorkerCompleted );

			InitializeComponent();
		}
		#endregion

		#region Methods
		#region Loading
		private Storyboard _Loading;

		/// <summary>
		/// Shows loading indicator.
		/// </summary>
		/// <param name="loadingText">Loading text.</param>
		public void ShowLoading( string loadingText )
		{
			try
			{
				Loading.Visibility = Visibility.Visible;
				LoadingText.Visibility = Visibility.Visible;

				if ( _Loading == null )
					_Loading = Resources[ "LootLoadingAnimation" ] as Storyboard;

				if ( _Loading != null )
					_Loading.Begin();

				LoadingText.Text = loadingText;
			}
			catch ( Exception ex )
			{
				ErrorWindow.Show( ex );
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
				LoadingText.Visibility = Visibility.Collapsed;
			}
			catch ( Exception ex )
			{
				ErrorWindow.Show( ex );
			}
		}
		#endregion

		#region Event Handlers
		private void UpdateLootAnalyzer()
		{
			if ( LootAnalyzer != null )
			{
				List<UltimaItemCounter> items = new List<UltimaItemCounter>();

				foreach ( KeyValuePair<string, UltimaItemCounter> kvp in LootAnalyzer.DefaultGroup.Items )
				{
					items.Add( kvp.Value );
				}

				UncategorizedItems.ItemsSource = items;
				Results.Text = LootAnalyzer.BuildReport();
			}
		}

		private void UpdateMobiles()
		{
			if ( App.Window.SpyHelper == null || _MobileWorker.IsBusy || _LootWorker.IsBusy )
				return;

			ShowLoading( "Loading mobiles" );
			_MobileWorker.RunWorkerAsync( App.Window.SpyHelper.Packets );
		}

		private object GetArgument( object argument, int index )
		{
			object[] arguments = (object[]) argument;

			return arguments[ index ];
		}

		private void Window_Loaded( object sender, RoutedEventArgs e )
		{
			UpdateMobiles();
		}

		private void RefreshMobiles_Click( object sender, RoutedEventArgs e )
		{
			UpdateMobiles();
		}

		private void MobileWorker_DoWork( object sender, DoWorkEventArgs e )
		{
			ObservableCollection<UltimaPacket> packets = (ObservableCollection<UltimaPacket>) e.Argument;

			e.Result = UltimaLootAnalyzer.GetAvailableMobiles( packets );
		}

		private void MobileWorker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
		{
			HideLoading();

			if ( e.Error != null )
			{
				ErrorWindow.Show( e.Error );
				return;
			}

			Mobiles.ItemsSource = (List<string>) e.Result;
		}

		private void AnalyzeMobiles_Click( object sender, RoutedEventArgs e )
		{
			if ( App.Window.SpyHelper == null || _MobileWorker.IsBusy || _LootWorker.IsBusy )
				return;

			ShowLoading( "Analyzing mobiles" );

			List<string> selected = new List<string>();

			foreach ( var o in Mobiles.SelectedItems )
			{
				if ( o is string )
					selected.Add( (string) o );
			}

			_LootWorker.RunWorkerAsync( new object[] { App.Window.SpyHelper.Packets, selected });
		}

		private void ExportButton_Click( object sender, RoutedEventArgs e )
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
					ShowLoading( "Exporting to C# file" );

					Button button = (Button) sender;
					uint serial = (uint) button.Tag;

					ContainerItem item = App.Window.SpyHelper.FindContainerItem( serial );
					QueryPropertiesResponsePacket properties = App.Window.SpyHelper.FindFirstPacket( serial, typeof( QueryPropertiesResponsePacket ) ) as QueryPropertiesResponsePacket;

					if ( item != null )
					{
						using ( FileStream stream = File.Open( _SaveFileDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None ) )
						{
							UltimaItemGenerator.Generate( stream, item.ItemID, item.Hue, item.Amount, properties );
						}
					}
				}
				catch ( Exception ex )
				{
					ErrorWindow.Show( ex );
				}
				finally
				{
					HideLoading();
				}
			}
		}

		private void LootWorker_DoWork( object sender, DoWorkEventArgs e )
		{
			ObservableCollection<UltimaPacket> packets = (ObservableCollection<UltimaPacket>) GetArgument( e.Argument, 0 );
			List<string> selected = (List<string>) GetArgument( e.Argument, 1 );

			e.Result = new UltimaLootAnalyzer( packets, selected );
		}

		private void LootWorker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
		{
			HideLoading();

			if ( e.Error != null )
			{
				ErrorWindow.Show( e.Error );
				return;
			}

			LootAnalyzer = (UltimaLootAnalyzer) e.Result;
		}

		private static void LootAnalyzer_Changed( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			LootAnalyzerWindow window = d as LootAnalyzerWindow;

			if ( window != null )
				window.UpdateLootAnalyzer();
		}
		#endregion
		#endregion
	}
}
