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
		Game game;

		public Main() {
			InitializeComponent();

			game = Game.GreatDetective;
			game.Players.Add(new Player("Andrew"));
			game.Players.Add(new Player("Cheryl"));
			game.Players.Add(new Player("Jeff"));
			game.Players.Add(new Player("Julia"));
			game.Players.Add(new Player("Anthony"));
			game.Players.Add(new Player("Brandee"));
			this.clueMatrix.DataContext = game.Players;
			this.sidePanel.DataContext = game;
			game.AssignApproximatePlayerHandSizes();
			game.Start();
			this.clueMatrix.Game = game;

			game.Clues.Add(new CompositeClue());
		}

		void newGameButton_Click(object sender, EventArgs e) {
			StartGameWindow startGameWindow = new StartGameWindow();
			startGameWindow.Owner = this;
			bool? result = startGameWindow.ShowDialog();
			if (result.HasValue && result.Value) {

				MessageBox.Show("TODO: Start new game.");
			}
		}

	}
}
