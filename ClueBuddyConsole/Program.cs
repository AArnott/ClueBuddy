using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using ClueBuddy;
using System.Diagnostics;

namespace ClueBuddyConsole {
	class Program {
		int cardColumnWidth = 10;
		int playerColumnWidth = 10;
		static ConsoleColor questionColor = ConsoleColor.Yellow;
		OpenFileDialog openDialog = new OpenFileDialog();
		SaveFileDialog saveDialog = new SaveFileDialog();

		Game game;
		/// <summary>
		/// The player who is using this program.
		/// </summary>
		Player interactivePlayer;
		/// <summary>
		/// The player whose turn it is.
		/// </summary>
		Player suggestingPlayer;

		public Program() {
			setupFileDialog(openDialog);
			setupFileDialog(saveDialog);
		}

		static void Main(string[] args) {
			new Program().main();
		}

		static T choose<T>(string prompt, bool includeSkip, Func<T, string> toString, params T[] options) {
			var dict = new Dictionary<char, T>();
			for (int i = 0; i < options.Length; i++) {
				dict.Add((char)('A' + i), options[i]);
			}
			if (includeSkip) {
				dict.Add((char)('A' + options.Length), default(T));
			}
			return choose(prompt, dict, c => (c == null) ? "Skip" : toString(c)).Value;
		}

		static KeyValuePair<char, T> choose<T>(string prompt, Dictionary<char, T> options, Func<T, string> toString) {
			writeColor(questionColor, prompt);
			foreach (KeyValuePair<char, T> pair in options) {
				Debug.Assert(pair.Key == pair.Key.ToString().ToUpper()[0]);
				Console.WriteLine("{0}. {1}", pair.Key.ToString().ToUpper(), toString(pair.Value));
			}
			Console.Write("Selection: ");
			char keyPressed = ' ';
			while (!options.ContainsKey(keyPressed))
				keyPressed = Console.ReadKey(true).KeyChar.ToString().ToUpper()[0];
			Console.WriteLine(keyPressed);
			return new KeyValuePair<char, T>(keyPressed, options[keyPressed]);
		}

		private static void writeColor(ConsoleColor color, string prompt, params object[] args) {
			ConsoleColor oldColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(prompt, args);
			Console.ForegroundColor = oldColor;
		}

		static int choose(string prompt, bool includeSkip, string[] options) {
			string result = choose<string>(prompt, includeSkip, s => s, options);
			int indexOfSelection = Array.IndexOf(options, result);
			return indexOfSelection;
		}

		Player choosePlayer(string prompt, bool includeSkip) {
			return choose(prompt, includeSkip, p => p.Name, game.Players.ToArray());
		}

		static string askString(string prompt) {
			Console.Write(prompt + " ");
			return Console.ReadLine();
		}

		static int askNumber(string prompt) {
			Console.Write(prompt + " ");
			int result;
			while (!int.TryParse(Console.ReadLine(), out result)) {
				Console.Error.WriteLine("Invalid input.");
				Console.Write(prompt + " ");
			}
			return result;
		}
		void main() {
			Console.WriteLine("=======ClueBuddy=======");
			while (true) {
				try {
					var mainMenu = new Dictionary<char, string>();
					mainMenu.Add('L', "Load game");
					mainMenu.Add('N', "New game");
					if (game != null) {
						mainMenu.Add('S', "Save game");
						mainMenu.Add('T', "Play a Turn");
						mainMenu.Add('G', "See Grid");
					}
					mainMenu.Add('Q', "Quit");
					switch (choose("Main menu:", mainMenu, s => s).Key) {
						case 'L':
							loadGame();
							break;
						case 'N':
							chooseGame();
							setupPlayers();
							game.Start();
							learnOwnHand();
							break;
						case 'S':
							saveGame();
							break;
						case 'T':
							takeTurn();
							break;
						case 'G':
							printGrid();
							break;
						case 'Q':
							return;
					}
				} catch (Exception e) {
					if (e is OutOfMemoryException || e is StackOverflowException) throw;
					if (choose(string.Format("An {0} exception was thrown: {1}.{2}Do you want to try to continue the game?",
						e.GetType().Name, e.Message, Environment.NewLine), false, new string[] { "Yes", "No" }) == 1)
						throw;
				}
			}
		}

		bool? saveGame() {
			bool? result = saveDialog.ShowDialog();
			if (result.HasValue && result.Value) {
				IFormatter formatter = new BinaryFormatter();
				using (Stream s = saveDialog.OpenFile()) {
					formatter.Serialize(s, game);
				}
			}
			return result;
		}

		bool? loadGame() {
			bool? result = openDialog.ShowDialog();
			if (result.HasValue && result.Value) {
				IFormatter formatter = new BinaryFormatter();
				using (Stream s = openDialog.OpenFile()) {
					game = (Game)formatter.Deserialize(s);
				}
			}
			return result;
		}

		void setupFileDialog(FileDialog dlg) {
			dlg.DefaultExt = "clueBuddy";
			dlg.Filter = "ClueBuddy games (*.clueBuddy)|*.clueBuddy|All Files|*.*";
			dlg.FilterIndex = 0;
		}

		void chooseGame() {
			game = choose("Which game variety are you starting?", false, v => v.VarietyName, Game.Varieties.ToArray());
		}

		void setupPlayers() {
			int players = askNumber("How many players?");
			for (int i = 0; i < players; i++) {
				game.Players.Add(new Player(askString(string.Format("Player {0} name:", i + 1))));
			}
			while (!game.CardAssignmentsAcceptable) {
				for (int i = 0; i < players; i++) {
					game.Players[i].CardsHeldCount = askNumber(string.Format("How many cards is {0} holding?", game.Players[i].Name));
				}
				if (!game.CardAssignmentsAcceptable) {
					Console.Error.WriteLine("ERROR: Those cards do not add up to {0}.", game.Cards.Count() - 3);
				}
			}
			interactivePlayer = choosePlayer("Which player are you?", false);
		}

		void learnOwnHand() {
			List<Card> cardsInHand = new List<Card>(interactivePlayer.CardsHeldCount);
			for (int i = 0; i < interactivePlayer.CardsHeldCount; i++) {
				Card card = choose("Indicate a card that you hold in your hand:", false, c => c.Name, game.Cards.Where(c2 => !cardsInHand.Contains(c2)).ToArray());
				cardsInHand.Add(card);
			}
			foreach (Card card in cardsInHand) {
				game.Clues.Add(new SpyCard(interactivePlayer, card));
			}
		}

		void takeTurn() {
			suggestingPlayer = choosePlayer("Whose turn is it?", true);
			if (suggestingPlayer == null) return;
			try {
				if (suggestingPlayer == interactivePlayer) {
					while (true) {
						var turnMenu = new Dictionary<char, string>();
						turnMenu.Add('S', "Make a suggestion");
						turnMenu.Add('L', "Look at someone else's card");
						turnMenu.Add('E', "End turn");
						switch (choose("What do you want to do?", turnMenu, s => s).Key) {
							case 'S':
								suggestion();
								break;
							case 'L':
								spy();
								break;
							case 'E':
								return;
						}
					}
				} else {
					suggestion();
				}
			} finally {
				suggestingPlayer = null;
			}
		}

		void spy() {
			SpyCard clue = new SpyCard();
			clue.Player = choosePlayer("Spy on which player?", true);
			if (clue.Player == null) return;
			clue.Card = choose("Which card did you see?", true, c => c.Name, clue.PossiblySeenCards.ToArray());
			if (clue.Card == null) return;
			game.Clues.Add(clue);
		}

		void suggestion() {
			CompositeClue cc = new CompositeClue();
			cc.Player = suggestingPlayer;
			cc.Suspicion.Place = choose("Where?", true, p => p.Name, game.Places.ToArray());
			if (cc.Suspicion.Place == null) return;
			cc.Suspicion.Suspect = choose("Who?", true, s => s.Name, game.Suspects.ToArray());
			if (cc.Suspicion.Suspect == null) return;
			cc.Suspicion.Weapon = choose("How?", true, w => w.Name, game.Weapons.ToArray());
			if (cc.Suspicion.Weapon == null) return;
			foreach (Player opponent in game.PlayersInOrderAfter(suggestingPlayer)) {
				// Do we already know whether this player could disprove it?
				bool? opponentCanDisprove = canPlayerDisprove(opponent, cc.Suspicion);
				if (opponentCanDisprove.HasValue) {
					writeColor(questionColor, "{0} {1} disprove {2}.", opponent.Name,
						opponentCanDisprove.Value ? "CAN" : "CANNOT", cc.Suspicion);
				} else {
					// Ask the gamer if the opponent did.
					switch (choose(string.Format("Could {0} disprove {1}?", opponent.Name, cc.Suspicion), false, new string[] { "Yes", "No", "Abort suggestion" })) {
						case 0:
							cc.Responses[opponent].Disproved = true;
							break;
						case 1:
							cc.Responses[opponent].Disproved = false;
							break;
						case 2:
							return;
					}
				}
			}
			game.Clues.Add(cc);
		}

		bool? canPlayerDisprove(Player opponent, Suspicion suspicion) {
			IEnumerable<Node> relevantNodes = game.Nodes.Where(n => n.CardHolder == opponent && suspicion.Cards.Contains(n.Card));
			// If the player is known to not have any of the cards...
			if (relevantNodes.All(n => n.IsSelected.HasValue && !n.IsSelected.Value))
				return false; // ...then he cannot possibly disprove the suggestion.
			// If the player is known to have at least one of the cards...
			if (relevantNodes.Any(n => n.IsSelected.HasValue && n.IsSelected.Value))
				return true; // ...then he can disprove the suggestion.
			// Otherwise, we don't know for sure.
			return null;
		}

		void printGrid() {
			printGridHeader(game.Suspects.OfType<Card>());
			printGridBody(game.Suspects.OfType<Card>());
			printGridHeader(game.Weapons.OfType<Card>());
			printGridBody(game.Weapons.OfType<Card>());
			printGridHeader(game.Places.OfType<Card>());
			printGridBody(game.Places.OfType<Card>());
		}

		private void printGridBody(IEnumerable<Card> cards) {
			foreach (Player player in game.Players) {
				printGridRow(player, cards);
			}
			printGridRow(game.CaseFile, cards);
		}

		private void printGridHeader(IEnumerable<Card> cards) {
			// Print header row with card names
			Console.Write(new string(' ', playerColumnWidth + 1));
			foreach (Card card in cards) {
				Console.Write(formatString(card.Name, cardColumnWidth, cardColumnWidth + 1, Alignment.Left));
			}
			Console.WriteLine();
		}

		private void printGridRow(ICardHolder player, IEnumerable<Card> cards) {
			string name = (player is Player) ? (player as Player).Name : "Case File";
			Console.Write(formatString(name, playerColumnWidth, playerColumnWidth + 1, Alignment.Left));
			foreach (Card card in cards) {
				Node n = game.Nodes.First(node => node.CardHolder == player && node.Card == card);
				string value = "?";
				if (n.IsSelected.HasValue) {
					value = n.IsSelected.Value ? "1" : "0";
				}
				Console.Write(formatString(value, cardColumnWidth, cardColumnWidth + 1, Alignment.Center));
			}
			Console.WriteLine();
		}

		enum Alignment {
			Left,
			Center,
			Right
		}

		string formatString(string value, int maxLength, int spacing, Alignment alignment) {
			Debug.Assert(maxLength <= spacing);
			int actualCharactersFromValueToDisplay = Math.Min(maxLength, value.Length);
			StringBuilder result = new StringBuilder(spacing);

			int prefixCharacters, suffixCharacters;
			switch (alignment) {
				case Alignment.Left:
					prefixCharacters = 0;
					suffixCharacters = spacing - actualCharactersFromValueToDisplay;
					break;
				case Alignment.Center:
					prefixCharacters = (spacing - actualCharactersFromValueToDisplay) / 2;
					suffixCharacters = (spacing - actualCharactersFromValueToDisplay) - prefixCharacters;
					break;
				case Alignment.Right:
					prefixCharacters = spacing - actualCharactersFromValueToDisplay;
					suffixCharacters = 0;
					break;
				default:
					throw new ApplicationException();
			}

			result.Append(' ', prefixCharacters);
			result.Append(value, 0, actualCharactersFromValueToDisplay);
			result.Append(' ', suffixCharacters);
			return result.ToString();
		}
	}
}
