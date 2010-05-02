namespace ClueBuddyGui {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Runtime.Serialization;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Text;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Navigation;
	using System.Windows.Shapes;

	using ClueBuddy;

	using Microsoft.Win32;

	/// <summary>
	/// Interaction logic for Main.xaml
	/// </summary>
	public partial class Main : Window {
		private Game game;

		public Main() {
			InitializeComponent();

			this.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, newGame));
			this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, openGame));
			this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, saveGame));

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
				CompositeClue cc = this.sidePanel.CurrentClue as CompositeClue;
				if (cc != null) {
					if (e.Card is Weapon) {
						cc.Suspicion.Weapon = e.Card as Weapon;
					} else if (e.Card is Suspect) {
						cc.Suspicion.Suspect = e.Card as Suspect;
					} else if (e.Card is Place) {
						cc.Suspicion.Place = e.Card as Place;
					}
				}
			});
			this.sidePanel.DataContext = game;

			game.Clues.Add(new CompositeClue());
		}

		void newGame(object sender, ExecutedRoutedEventArgs e) {
			StartGameWindow startGameWindow = new StartGameWindow();
			startGameWindow.Owner = this;
			bool? result = startGameWindow.ShowDialog();
			if (result.HasValue && result.Value) {

				MessageBox.Show("TODO: Start new game.");
			}
			e.Handled = true;
		}

		void openGame(object sender, ExecutedRoutedEventArgs e) {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.DefaultExt = "ClueBuddy";
			bool? result = dlg.ShowDialog(this);
			if (result.HasValue && result.Value) {

			}
			e.Handled = true;
		}

		void saveGame(object sender, ExecutedRoutedEventArgs e) {
			SaveFileDialog dlg = new SaveFileDialog ();
			dlg.DefaultExt = "ClueBuddy";
			bool? result = dlg.ShowDialog(this);
			if (result.HasValue && result.Value) {
				IFormatter formatter = new BinaryFormatter();
				using (Stream s = dlg.OpenFile()) {
					formatter.Serialize(s, game);
				}
			}
			e.Handled = true;
		}
	}
}
