using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using ClueBuddy;

namespace ClueBuddyGui {
	/// <summary>
	/// Interaction logic for Main.xaml
	/// </summary>

	public partial class Main : Window {
		public Main() {
			InitializeComponent();

			Game game = Game.GreatDetective;
			game.Players.AddRange(new Player[] { new Player("DB1"), new Player("DB2") });
			this.matrix.DataContext = game.Players;
			this.sidePanel.DataContext = game;

			//foreach (var p in game.Players) {
			//    Label lbl = new Label() { Content = p.Name };
			//    matrix.Children.Add(lbl);
			//}
		}

		void newGameButton_Click(object sender, EventArgs e) {
			StartGameWindow startGameWindow = new StartGameWindow();
			bool? result = startGameWindow.ShowDialog();
			if (result.HasValue && result.Value) {

				MessageBox.Show("TODO: Start new game.");
			}
		}
	}
}
