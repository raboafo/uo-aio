using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;

namespace UOAIO;

public partial class CharacterSelect : Window
{
	private readonly List<CharacterInfo> _characters;

	public CharacterSelect(List<CharacterInfo> characters)
	{
		WindowStartupLocation = WindowStartupLocation.CenterScreen;
		ResizeMode = ResizeMode.NoResize;
		_characters = characters ?? new List<CharacterInfo>();
		InitializeComponent();
		Characters.ItemsSource = _characters;
		if (_characters.Count > 0)
		{
			Characters.SelectedIndex = 0;
		}
	}

	private void LoginCharacter(object sender, RoutedEventArgs e)
	{
		if (Characters.SelectedItem is not CharacterInfo characterInfo)
		{
			return;
		}

		IPAddress ipAddress = IPAddress.Parse("1.1.1.1");
		int address = BitConverter.ToInt32(ipAddress.GetAddressBytes(), 0);
		Network.Send(new PCharacterSelect(characterInfo.Name, characterInfo.Index, address));
		Close();
	}
}
