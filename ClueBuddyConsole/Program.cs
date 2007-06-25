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
		OpenFileDialog openVarietyDialog = new OpenFileDialog();
		OpenFileDialog openGameDialog = new OpenFileDialog();
		SaveFileDialog saveGameDialog = new SaveFileDialog();

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
			setupGameFileDialog(openGameDialog);
			setupGameFileDialog(saveGameDialog);

			openVarietyDialog.DefaultExt = GameVariety.DefaultFileExtension;
			openVarietyDialog.Filter = string.Format("Clue Varieties (*.{0})|*.{0}|All files (*.*)|*.*", GameVariety.DefaultFileExtension);
			openVarietyDialog.FilterIndex = 0;
			openVarietyDialog.Title = "Choose which game variety to load";
		}

		[STAThread]
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

		Player choosePlayer(string prompt, bool includeSkip, bool includeInteractivePlayer) {
			return choose(prompt, includeSkip, p => p.Name,
				game.Players.Where(p => includeInteractivePlayer || p != interactivePlayer).ToArray());
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
						mainMenu.Add('C', "List Clues");
					}
					mainMenu.Add('Q', "Quit");
					switch (choose("Main menu:", mainMenu, s => s).Key) {
						case 'L':
							loadGame();
							break;
						case 'N':
							chooseGame();
							if (game == null) break;
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
						case 'C':
							listClues();
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
			bool? result = saveGameDialog.ShowDialog();
			if (result.HasValue && result.Value) {
				IFormatter formatter = new BinaryFormatter();
				using (Stream s = saveGameDialog.OpenFile()) {
					formatter.Serialize(s, game);
					formatter.Serialize(s, interactivePlayer.Name);
				}
			}
			return result;
		}

		bool? loadGame() {
			bool? result = openGameDialog.ShowDialog();
			if (result.HasValue && result.Value) {
				IFormatter formatter = new BinaryFormatter();
				using (Stream s = openGameDialog.OpenFile()) {
					game = (Game)formatter.Deserialize(s);
					interactivePlayer = game.Players.First(p => p.Name.Equals(formatter.Deserialize(s)));
				}
				saveGameDialog.FileName = openGameDialog.FileName;
			}
			return result;
		}

		void setupGameFileDialog(FileDialog dlg) {
			dlg.DefaultExt = "clueBuddy";
			dlg.Filter = "ClueBuddy games (*.clueBuddy)|*.clueBuddy|All Files|*.*";
			dlg.FilterIndex = 0;
		}

		void chooseGame() {
			bool? result = openVarietyDialog.ShowDialog();
			if (!result.HasValue || !result.Value) return;
			using (Stream s = openVarietyDialog.OpenFile()) {
				game = GameVariety.LoadFrom(s).Initialize();
				Console.WriteLine("Starting {0}...", game.VarietyName);
			}
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
			interactivePlayer = choosePlayer("Which player are you?", false, true);
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
			suggestingPlayer = choosePlayer("Whose turn is it?", true, true);
			if (suggestingPlayer == null) return;
			try {
				if (suggestingPlayer == interactivePlayer && game.Rules.HasSpyglass) {
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
			clue.Player = choosePlayer("Spy on which player?", true, false);
			if (clue.Player == null) return;
			clue.Card = choose("Which card did you see?", true, c => c.Name, clue.PossiblySeenCards.ToArray());
			if (clue.Card == null) return;
			game.Clues.Add(clue);
		}

		string getCardSuggestionStrength(Card card) {
			int strength = 0;
			// Consider the card worthwhile if its existence in the Case File is not known.
			if (!game.Nodes.First(n => n.Card == card && n.CardHolder == game.CaseFile).IsSelected.HasValue) {
				// Its strength is based on how much we know about it so far, which is based
				// on how many constraints we already have on it.
				int constraintWithCardCount = Enumerable.Count(game.Constraints,
					c => c is ConstraintBase && ((ConstraintBase)c).Nodes.Any(cn => ((Node)cn).Card == card));
				strength = 50 - constraintWithCardCount;
			}
			return formatString(card.Name, 20, 21, Alignment.Left) +
				formatString(strength.ToString(), 4, 5, Alignment.Right);
		}

		void suggestion() {
			Suspicion suggestion = new Suspicion();
			suggestion.Place = choose("Where?", true, p => getCardSuggestionStrength(p), game.Places.ToArray());
			if (suggestion.Place == null) return;
			suggestion.Suspect = choose("Who?", true, s => getCardSuggestionStrength(s), game.Suspects.ToArray());
			if (suggestion.Suspect == null) return;
			suggestion.Weapon = choose("How?", true, w => getCardSuggestionStrength(w), game.Weapons.ToArray());
			if (suggestion.Weapon == null) return;
			foreach (Player opponent in game.PlayersInOrderAfter(suggestingPlayer)) {
				bool? disproved;
				// Do we already know whether this player could disprove it?
				if ((disproved = canPlayerDisprove(opponent, suggestion)).HasValue) {
					writeColor(questionColor, "{0} {1} disprove {2}.", opponent.Name, disproved.Value ? "CAN" : "CANNOT", suggestion);
				} else {
					// Ask the gamer if the opponent did.
					switch (choose(string.Format("Could {0} disprove {1}?", opponent.Name, suggestion), false,
						new string[] { "Yes", "No", "Skip player", "Abort suggestion" })) {
						case 0:
							disproved = true;
							break;
						case 1:
							disproved = false;
							break;
						case 2:
							continue;
						case 3:
							return;
					}
				}
				Card alabi = null;
				if (suggestingPlayer == interactivePlayer && disproved.HasValue && disproved.Value) {
					IEnumerable<Card> possiblyShownCards = from n in game.Nodes
														   where n.CardHolder == opponent &&
														   suggestion.Cards.Contains(n.Card) &&
														   (!n.IsSelected.HasValue || n.IsSelected.Value)
														   select n.Card;
					if (possiblyShownCards.Count() == 1) {
						writeColor(questionColor, "{0} must have shown you {1}.", opponent,
							alabi = possiblyShownCards.First());
					} else {
						alabi = choose(string.Format("Which card did {0} show you?", opponent),
							true, c => c.Name, possiblyShownCards.ToArray());
					}
				}
				if (disproved.HasValue) {
					if (disproved.Value) {
						game.Clues.Add(new Disproved(opponent, suggestion, alabi));
					} else {
						game.Clues.Add(new CannotDisprove(opponent, suggestion));
					}
				}
			}
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

		void listClues() {
			foreach (Clue c in game.Clues) {
				Console.WriteLine(c);
			}
		}
	}
}
