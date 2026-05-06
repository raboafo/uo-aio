using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace UOAIO;

public class MainWindow : Window, IComponentConnector
{
	internal TextBox AccountName;

	internal PasswordBox Password;

	internal TextBlock VersionTextBlock;

	private bool _contentLoaded;

	public MainWindow()
	{
		base.WindowStartupLocation = WindowStartupLocation.CenterScreen;
		this.InitializeComponent();
		this.VersionTextBlock.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
		if (!string.IsNullOrEmpty(Settings.Default.Account))
		{
			this.AccountName.Text = Settings.Default.Account;
		}
		if (!string.IsNullOrEmpty(Settings.Default.Password))
		{
			this.Password.Password = Settings.Default.Password;
		}
	}

	private void Window_MouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left)
		{
			base.DragMove();
		}
	}

	private void LoginButton_OnClick(object sender, RoutedEventArgs e)
	{
		Settings.Default.Account = this.AccountName.Text;
		Settings.Default.Password = this.Password.Password;
		Settings.Default.Save();
		Engine.ServerIP = MainWindow.DoGetHostAddresses("play.newrenaissanceuo.com");
		Engine.ServerPort = "2593";
		Engine.Username = this.AccountName.Text;
		Engine.Password = this.Password.Password;
		base.DialogResult = true;
		base.Close();
	}

	private void ExitButton_OnClick(object sender, RoutedEventArgs e)
	{
		base.Close();
	}

	public static string DoGetHostAddresses(string hostname)
	{
		IPAddress[] hostAddresses = Dns.GetHostAddresses(hostname);
		Console.WriteLine("GetHostAddresses({0}) returns:", hostname);
		IPAddress[] array = hostAddresses;
		int num = 0;
		if (num < array.Length)
		{
			IPAddress iPAddress = array[num];
			return iPAddress.ToString();
		}
		return null;
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!this._contentLoaded)
		{
			this._contentLoaded = true;
			Uri resourceLocator = new Uri("/Ultima.Client;component/launchergui/mainwindow.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		switch (connectionId)
		{
		case 1:
			((MainWindow)target).MouseDown += Window_MouseDown;
			break;
		case 2:
			((Button)target).Click += LoginButton_OnClick;
			break;
		case 3:
			((Button)target).Click += ExitButton_OnClick;
			break;
		case 4:
			this.AccountName = (TextBox)target;
			break;
		case 5:
			this.Password = (PasswordBox)target;
			break;
		case 6:
			this.VersionTextBlock = (TextBlock)target;
			break;
		default:
			this._contentLoaded = true;
			break;
		}
	}
}
