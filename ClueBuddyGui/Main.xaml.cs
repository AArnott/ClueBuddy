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
using Microsoft.Win32;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ClueBuddyGui {
	/// <summary>
	/// Interaction logic for Main.xaml
	/// </summary>

	public partial class Main : Window {
		Game game;

		public Main() {
			InitializeComponent();

			using (Stream s = new FileStream("Master Detective.clueVariety", FileMode.Open)) {
				game = GameVariety.LoadFrom(s).Initialize();
			}
			game.Players.Add(new Player("Andrew"));
			game.Players.Add(new Player("Cheryl"));
			game.Players.Add(new Player("Jeff"));
			game.Players.Add(new Player("Julia"));
			game.Players.Add(new Player("Anthony"));
			game.Players.Add(new Player("Brandee"));
			game.AssignApproximatePlayerHandSizes();
			game.AutoConstraintRegeneration = false;
			game.Start();
			this.clueMatrix.DataContext = game;
			this.clueMatrix.Game = game;
			this.clueMatrix.PlayerClicked += new EventHandler<ClueGrid.PlayerClickedEventArgs>((sender, e) => {
				this.sidePanel.CurrentClue.Player = e.Player;
			});
			this.clueMatrix.CardClicked += new EventHandler<ClueGrid.CardClickedEventArgs>((sender, e) => {
				if (e.Card is Weapon) {
					this.sidePanel.CurrentClue.Suspicion.Weapon = e.Card as Weapon;
				} else if (e.Card is Suspect) {
					this.sidePanel.CurrentClue.Suspicion.Suspect = e.Card as Suspect;
				} else if (e.Card is Place) {
					this.sidePanel.CurrentClue.Suspicion.Place = e.Card as Place;
				}
			});
			this.sidePanel.DataContext = game;

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
