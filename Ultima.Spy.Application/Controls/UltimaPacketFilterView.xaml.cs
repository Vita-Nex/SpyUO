using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Interaction logic for UltimaPacketFilterView.xaml
	/// </summary>
	public partial class UltimaPacketFilterView : UserControl
	{
		#region Propeties
		/// <summary>
		/// Represents Filter property.
		/// </summary>
		public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
			"Filter", typeof( UltimaPacketFilter ), typeof( UltimaPacketFilterView ), new PropertyMetadata( null ) );

		/// <summary>
		/// Gets or sets filter represented by this control.
		/// </summary>
		public UltimaPacketFilter Filter
		{
			get { return GetValue( FilterProperty ) as UltimaPacketFilter; }
			set { SetValue( FilterProperty, value ); }
		}

		private SaveFileDialog _SaveFileDialog;
		private OpenFileDialog _OpenFileDialog;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of UltimaPacketFilterView.
		/// </summary>
		public UltimaPacketFilterView()
		{
			InitializeComponent();
		}
		#endregion

		#region Methods
		private void OpenButton_Click( object sender, RoutedEventArgs e )
		{
			UltimaPacketFilter filter = Filter;

			if ( filter != null )
			{
				try
				{
					if ( _OpenFileDialog == null )
					{
						_OpenFileDialog = new OpenFileDialog();
						_OpenFileDialog.Filter = "SpyUO filter (*.filter)|*.filter";
						_OpenFileDialog.CheckPathExists = true;
						_OpenFileDialog.FileName = "Filter.filter";
						_OpenFileDialog.Title = "Open Filter";
					}

					if ( _OpenFileDialog.ShowDialog( App.Current.MainWindow ) == true )
						filter.Load( _OpenFileDialog.FileName );
				}
				catch ( Exception ex )
				{
					Trace.WriteLine( ex );
				}
			}
		}

		private void SaveButton_Click( object sender, RoutedEventArgs e )
		{
			UltimaPacketFilter filter = Filter;

			if ( filter != null )
			{
				try
				{
					if ( _SaveFileDialog == null )
					{
						_SaveFileDialog = new SaveFileDialog();
						_SaveFileDialog.Filter = "SpyUO filter (*.filter)|*.filter";
						_SaveFileDialog.CheckPathExists = true;
						_SaveFileDialog.Title = "Save Filter";
					}

					if ( _SaveFileDialog.ShowDialog( App.Current.MainWindow ) == true )
						filter.Save( _SaveFileDialog.FileName );
				}
				catch ( Exception ex )
				{
					Trace.WriteLine( ex );
				}
			}
		}

		private void EditButton_Click( object sender, RoutedEventArgs e )
		{
			try
			{
				FrameworkElement element = sender as FrameworkElement;

				if ( element == null )
					return;

				UltimaPacketFilterEntry entry = element.Tag as UltimaPacketFilterEntry;

				if ( entry == null )
					return;

				FilterPropertiesEditor editor = new FilterPropertiesEditor();
				editor.Entry = entry;
				editor.ShowDialog();
			}
			catch ( Exception ex )
			{
				Trace.WriteLine( ex );
			}
		}
		#endregion
	}
}
