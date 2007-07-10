using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Collections.ObjectModel;
using ClueBuddy;
using System.Security;
using System.Windows.Input;
using Microsoft.Win32;

namespace ClueBuddyGui
{
	public class Suspects : ObservableCollection<Suspect> { }

	public partial class StartGameWindow
	{
		public StartGameWindow()
		{
			this.InitializeComponent();

			CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, openVariety));

			DataContext = gameVarieties = discoverAndLoadClueVarieties();
		}

		ObservableCollection<GameVariety> gameVarieties;

		ObservableCollection<GameVariety> discoverAndLoadClueVarieties() {
			var varieties = new ObservableCollection<GameVariety>();
			try {
				foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*." + GameVariety.DefaultFileExtension)) {
					using (Stream s = new FileStream(file, FileMode.Open)) {
						varieties.Add(GameVariety.LoadFrom(s));
					}
				}
			} catch (SecurityException) {
				// Insufficient permissions to query local hard disk.  Pop up Open dialog.
				ApplicationCommands.Open.Execute(null, this);
			}
			return varieties;
		}

		void openVariety(object sender, RoutedEventArgs e) {
			OpenFileDialog openDialog = new OpenFileDialog();
			bool? result = openDialog.ShowDialog(this);
			if (result.HasValue && result.Value) {
				gameVarieties.Add(GameVariety.LoadFrom(openDialog.OpenFile()));
			}
		}
	}
}