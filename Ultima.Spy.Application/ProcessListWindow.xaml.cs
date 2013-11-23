using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using IconImage = System.Drawing.Icon;

namespace Ultima.Spy.Application
{
	/// <summary>
	/// Interaction logic for ProcessListWindow.xaml
	/// </summary>
	public partial class ProcessListWindow : Window
	{
		#region Properties
		private Process _Selected;

		/// <summary>
		/// Gets or sets selected process.
		/// </summary>
		public Process Selected
		{
			get { return _Selected; }
		}

		private List<Process> _ProcessList;
		private BackgroundWorker _Worker;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new instance of ProcessListWindow.
		/// </summary>
		public ProcessListWindow()
		{
			_Worker = new BackgroundWorker();
			_Worker.DoWork += new DoWorkEventHandler( Worker_DoWork );
			_Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler( Worker_RunWorkerCompleted );
			_ProcessList = new List<Process>();

			InitializeComponent();
		}
		#endregion

		#region Methods
		private void RefreshProcessList()
		{
			Header.Text = "Retrieving Process List";

			_Worker.RunWorkerAsync();
		}

		[DllImport( "gdi32.dll", SetLastError = true )]
		private static extern bool DeleteObject( IntPtr hObject );

		private static ImageSource GetProcessIcon( string filePath )
		{
			IconImage icon = IconImage.ExtractAssociatedIcon( filePath );
			Bitmap bitmap = icon.ToBitmap();
			IntPtr hBitmap = bitmap.GetHbitmap();

			ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
				hBitmap,
				IntPtr.Zero,
				Int32Rect.Empty,
				BitmapSizeOptions.FromEmptyOptions() );

			if ( !DeleteObject( hBitmap ) )
			{
				throw new Win32Exception();
			}

			return wpfBitmap;
		}

		#region Event Handlers
		private void Worker_DoWork( object sender, DoWorkEventArgs e )
		{
			Process[] list = Process.GetProcesses();
			List<Process> userList = new List<Process>();

			foreach ( Process process in list )
			{
				try
				{
					if ( ClientSpyStarter.GetClientType( process ) != UltimaClientType.Invalid )
					{
						_Selected = process;
					}

					userList.Add( process );
				}
				catch
				{
				}
			}

			e.Result = userList;
		}

		private void Worker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
		{
			Header.Text = "Process List";

			if ( e.Error != null )
			{
				ErrorWindow.Show( e.Error );
			}
			else
			{
				List.ItemsSource = (List<Process>) e.Result;

				if ( _Selected != null )
				{
					List.SelectedItem = _Selected;
					List.ScrollIntoView( _Selected );
				}
			}
		}

		private void Window_Loaded( object sender, RoutedEventArgs e )
		{
			RefreshProcessList();
		}

		private void RefreshButton_Click( object sender, RoutedEventArgs e )
		{
			RefreshProcessList();
		}

		private void CancelButton_Click( object sender, RoutedEventArgs e )
		{
			DialogResult = false;
		}

		private void AcceptButton_Click( object sender, RoutedEventArgs e )
		{
			if ( _Selected != null )
				DialogResult = true;
			else
				ErrorWindow.Show( "No process selected", "You must select a process to continue" );
		}

		private void ProcessList_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			try
			{
				Process process = List.SelectedItem as Process;

				if ( process != null )
				{
					ImageSource icon = null;
					string filePath = null;
					string name = null;

					try
					{
						name = process.ProcessName;
						filePath = process.MainModule.FileName;
						icon = GetProcessIcon( filePath );
					}
					catch
					{
						// Probably security issue
					}

					ProcessImage.Source = icon;
					ProcessName.Text = process.ProcessName;

					_Selected = process;
				}
			}
			catch ( Exception ex )
			{
				ErrorWindow.Show( ex );
			}
		}
		#endregion
		#endregion
	}
}
