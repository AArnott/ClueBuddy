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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClueBuddy;

namespace ClueBuddyGui {
	/// <summary>
	/// Interaction logic for ClueGrid.xaml
	/// </summary>
	public partial class ClueGrid : System.Windows.Controls.UserControl {
		public ClueGrid() {
			InitializeComponent();
		}

		private Game game;

		public Game Game {
			get { return game; }
			set {
				if (game == value) return;
				game = value;
				if (game != null) {
					initializeGame();
				}
			}
		}

		void initializeGame() {
			clearControls();
			// set game title
			title.DataContext = game;
			title.SetBinding(TextBlock.TextProperty, "VarietyName");
			// fill in the players
			matrix.RowDefinitions.RemoveRange(2, matrix.RowDefinitions.Count - 2);
			foreach (Player player in game.Players) {
				matrix.RowDefinitions.Add(new RowDefinition());
				Label playerLabel = new Label() {
					Style = (Style)Resources["PlayerName"],
				};
				playerLabel.DataContext = player;
				playerLabel.SetBinding(Label.ContentProperty, new Binding("Name"));
				playerLabel.SetValue(Grid.ColumnProperty, 0);
				playerLabel.SetValue(Grid.RowProperty, matrix.RowDefinitions.Count - 1);
				matrix.Children.Add(playerLabel);
			}
			// fill in the cards
			matrix.ColumnDefinitions.RemoveRange(1, matrix.ColumnDefinitions.Count - 1);
			var columnWidth = new GridLength(0.75 / game.Cards.Count(), GridUnitType.Star);

			addCardColumn(columnWidth, game.Suspects.OfType<Card>());
			addCardColumn(columnWidth, game.Weapons.OfType<Card>());
			addCardColumn(columnWidth, game.Places.OfType<Card>());

			suspectsLabel.SetValue(Grid.ColumnSpanProperty, game.Suspects.Count());
			weaponsLabel.SetValue(Grid.ColumnProperty, game.Suspects.Count());
			weaponsLabel.SetValue(Grid.ColumnSpanProperty, game.Weapons.Count());
			placesLabel.SetValue(Grid.ColumnProperty, game.Suspects.Count() + game.Weapons.Count());
			placesLabel.SetValue(Grid.ColumnSpanProperty, game.Places.Count());

			// fill in the envelope row
			matrix.RowDefinitions.Add(new RowDefinition());
			caseFile.SetValue(Grid.RowProperty, game.Players.Count + 2);
			matrix.Children.Add(caseFile);
			int cardIndex = 0;
			foreach (Card c in game.Cards) {
				Node node = game.Nodes.Where(n => n.CardHolder == game.CaseFile && n.Card == c).First();
				Label nodeLabel = new Label();
				nodeLabel.SetValue(Grid.ColumnProperty, 1 + cardIndex);
				nodeLabel.SetValue(Grid.RowProperty, game.Players.Count + 2);
				nodeLabel.DataContext = node;
				nodeLabel.SetBinding(Label.ContentProperty, new Binding("IsSelected"));
				matrix.Children.Add(nodeLabel);
				cardIndex++;
			}
		}

		void addCardColumn(GridLength columnWidth, IEnumerable<Card> cardGroup) {
			foreach (Card c in cardGroup) {
				matrix.ColumnDefinitions.Add(new ColumnDefinition() { Width = columnWidth });
				TextBlock cardBlock = new TextBlock() {
					Style = (Style)Resources["VerticalText"]
				};
				cardBlock.DataContext = c;
				cardBlock.SetBinding(TextBlock.TextProperty, new Binding("Name"));
				cardBlock.SetValue(Grid.ColumnProperty, matrix.ColumnDefinitions.Count - 1);
				cardBlock.SetValue(Grid.RowProperty, 1);
				matrix.Children.Add(cardBlock);

				// Fill in the individual nodes for this card.
				foreach (Player player in game.Players) {
					Node node = game.Nodes.Where(n => n.CardHolder == player && n.Card == c).First();
					Label nodeLabel = new Label();
					nodeLabel.SetValue(Grid.ColumnProperty, matrix.ColumnDefinitions.Count - 1);
					nodeLabel.SetValue(Grid.RowProperty, game.Players.IndexOf(player) + 2);
					nodeLabel.DataContext = node;
					nodeLabel.SetBinding(Label.ContentProperty, new Binding("IsSelected"));
					matrix.Children.Add(nodeLabel);
				}
			}
		}

		void clearControls() {
			Control[] sampleControls = (from ui in matrix.Children.OfType<UIElement>()
										let c = ui as Control
										where c != null && (
											(int)c.GetValue(Grid.RowProperty) > 1 ||
											((int)c.GetValue(Grid.ColumnProperty) > 0 && (int)c.GetValue(Grid.RowProperty) > 0)
										)
										select c).ToArray();
			foreach (Control c in sampleControls)
				matrix.Children.Remove(c);
		}

		public class PlayerClickedEventArgs : EventArgs {
			public PlayerClickedEventArgs(Player player) {
				this.player = player;
			}
			Player player;
			public Player Player { get { return player; } }
		}
		public event EventHandler<PlayerClickedEventArgs> PlayerClicked;
		protected virtual void OnPlayerClicked(Player player) {
			if (player == null) throw new ArgumentNullException("player");
			var playerClicked = PlayerClicked;
			if (playerClicked != null) {
				playerClicked(this, new PlayerClickedEventArgs(player));
			}
		}
		public class CardClickedEventArgs : EventArgs {
			public CardClickedEventArgs(Card card) {
				this.card = card;
			}
			Card card;
			public Card Card { get { return card; } }
		}
		public event EventHandler<CardClickedEventArgs> CardClicked;
		protected virtual void OnCardClicked(Card card) {
			if (card == null) throw new ArgumentNullException("card");
			var cardClicked = CardClicked;
			if (cardClicked != null) {
				cardClicked(this, new CardClickedEventArgs(card));
			}
		}

		void matrix_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			object target = getClickedObject(e.Device.Target);
			Node targetNode = target as Node;
			Card targetCard = target as Card;
			Player targetPlayer = target as Player;
			if (targetNode != null) {
				// TODO:
			} else if (targetCard != null) {
				OnCardClicked(targetCard);
			} else if (targetPlayer != null) {
				OnPlayerClicked(targetPlayer);
			}
		}

		object getClickedObject(IInputElement target) {
			TextBlock tb = target as TextBlock;
			if (tb != null) {
				object dc = tb.DataContext;
				if (dc is Card)
					return dc;
				if (dc is Node)
					return dc;
				if (dc is string)
					return game.Players.Where(p => p.Name.Equals(dc)).First();
			}
			return null;
		}
	}
}
