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
using System.Security;
using System.Security.Permissions;
using System.Collections.Specialized;
using NerdBank.Algorithms.NodeConstraintSelection;

namespace ClueBuddyConsole {
	class Program {
		int playerColumnWidth {
			get { return Math.Max(game.Players.Max(p => p.Name.Length), "Case File".Length); }
		}
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
		bool solutionFoundAlready;

		public Program() {
			setupGameFileDialog(openGameDialog);
			setupGameFileDialog(saveGameDialog);

			openVarietyDialog.DefaultExt = GameVariety.DefaultFileExtension;
			openVarietyDialog.Filter = string.Format("Clue Varieties (*.{0})|*.{0}|All files (*.*)|*.*", GameVariety.DefaultFileExtension);
			openVarietyDialog.FilterIndex = 0;
			try {
				openVarietyDialog.Title = "Choose which game variety to load";
			} catch (SecurityException) { } // a small convenience we'll leverage when we can
		}

		[STAThread]
		static void Main(string[] args) {
			new Program().main();
		}

		Player choosePlayer(string prompt, bool includeSkip, bool includeInteractivePlayer) {
			return ConsoleHelper.Choose(prompt, includeSkip, p => p.Name,
				game.Players.Where(p => includeInteractivePlayer || p != interactivePlayer).ToArray());
		}

		void main() {
			Console.WriteLine("=======ClueBuddy=======");
			try {
				PermissionSet savingPermissions = new PermissionSet(null);
				savingPermissions.AddPermission(new FileDialogPermission(FileDialogPermissionAccess.Save));
				savingPermissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.SerializationFormatter));
				savingPermissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));
				// briefly demand these permissions to detect whether we'll succeed later
				savingPermissions.Demand();
			} catch (SecurityException) {
				ConsoleHelper.WriteColor(ConsoleColor.Red, "WARNING: insufficient permissions to save games.");
			}
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
					switch (ConsoleHelper.Choose("Main menu:", mainMenu, s => s).Key) {
						case 'L':
							loadGame();
							break;
						case 'N':
							chooseGame();
							if (game == null) break;
							setupPlayers();
							game.Start();
							prepareNewOrLoadedGameState();
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
				} catch (SecurityException ex) {
					Console.Error.WriteLine("Insufficient permissions: {0}.", ex.Demanded);
				} catch (Exception e) {
					if (e is OutOfMemoryException || e is StackOverflowException) throw;
					if (ConsoleHelper.Choose(string.Format("An {0} exception was thrown: {1}.{2}Do you want to try to continue the game?",
						e.GetType().Name, e.Message, Environment.NewLine), false, new string[] { "Yes", "No" }) == 1)
						throw;
				}
			}
		}

		void prepareNewOrLoadedGameState() {
			solutionFoundAlready = false;
			// Hook into the CaseFile changed events so we are notified when the solution
			// has been found.
			game.Clues.CollectionChanged += new NotifyCollectionChangedEventHandler(Clues_CollectionChanged);
			game.BadClueDetected += game_BadClueDetected;
		}

		void game_BadClueDetected(object sender, BadClueEventArgs e) {
			Console.WriteLine("Conflicting clues exist.  Searching for suspect clues...");
			var conflictingClues = e.SuspectClues.ToArray();
			Clue badClue = ConsoleHelper.Choose("Which of these clues are incorrect?", true, clue => clue.ToString(), conflictingClues);
			if (badClue != null) {
				game.Clues.Remove(badClue);
			}

			e.SetHandled();
		}

		bool? saveGame() {
			bool? result = saveGameDialog.ShowDialog();
			if (result.HasValue && result.Value) {
				try {
					// We remove the event handlers hooking to this console app so that
					// we don't serialize an extra instance of it.
					game.Clues.CollectionChanged -= new NotifyCollectionChangedEventHandler(Clues_CollectionChanged);
					game.BadClueDetected -= game_BadClueDetected;

					// Serialize the game state.
					IFormatter formatter = new BinaryFormatter();
					// Save to a memory stream first, to make sure that serialization will complete
					// successfully before we knock out an existing file.
					MemoryStream ms = new MemoryStream();
					formatter.Serialize(ms, game);
					formatter.Serialize(ms, interactivePlayer.Name);
					// Now that we've gotten this far, go ahead and write to the actual file.
					using (Stream s = saveGameDialog.OpenFile()) {
						ms.WriteTo(s);
					}
				} finally {
					// Restore the event handlers.
					game.Clues.CollectionChanged += new NotifyCollectionChangedEventHandler(Clues_CollectionChanged);
					game.BadClueDetected += game_BadClueDetected;
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

					prepareNewOrLoadedGameState();

					// Just in case the intelligence of this program has improved since this game was saved,
					// recalculate everything.
					game.RegenerateConstraints();
				}
				try {
					saveGameDialog.FileName = openGameDialog.FileName;
				} catch (SecurityException) { } // just a convenience that we'll ignore if we can't do it.
			}
			prepareNewOrLoadedGameState();
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
			}
			Console.WriteLine("Starting {0}...", game.VarietyName);
		}

		void Clues_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			// Has the solution been found?
			if (!solutionFoundAlready && game.CaseFile.IsSolved) {
				// we just found the solution
				ConsoleHelper.WriteColor(ConsoleColor.Red, "Solved!  {0}, {1}, {2}", game.CaseFile.Place, game.CaseFile.Suspect, game.CaseFile.Weapon);
				solutionFoundAlready = true;
			}
		}

		void setupPlayers() {
			int players = ConsoleHelper.AskNumber("How many players?");
			Console.WriteLine("In clockwise order...");
			for (int i = 0; i < players; i++) {
				game.Players.Add(new Player(ConsoleHelper.AskString(string.Format("Player {0} name:", i + 1))));
			}
			while (!game.CardAssignmentsAcceptable) {
				for (int i = 0; i < players; i++) {
					game.Players[i].CardsHeldCount = ConsoleHelper.AskNumber(string.Format("How many cards is {0} holding?", game.Players[i].Name));
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
				Card card = ConsoleHelper.Choose("Indicate a card that you hold in your hand:", false, c => c.Name, game.Cards.Where(c2 => !cardsInHand.Contains(c2)).ToArray());
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
						switch (ConsoleHelper.Choose("What do you want to do?", turnMenu, s => s).Key) {
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
				} else if (suggestingPlayer == interactivePlayer) {
					suggestion();
				} else {
					var turnMenu = new Dictionary<char, string>();
					turnMenu.Add('S', "Make a suggestion");
					turnMenu.Add('A', "Make an accusation");
					turnMenu.Add('E', "End turn");
					switch (ConsoleHelper.Choose("What do you want to do?", turnMenu, s => s).Key) {
						case 'S':
							suggestion();
							break;
						case 'A':
							accusation();
							break;
						case 'E':
							return;
					}
				}
			} finally {
				suggestingPlayer = null;
			}
		}

		void spy() {
			// Display a hint to the user about which player might be the most beneficial to spy on.
			// Factors to consider include: 
			// * what % of cards held by a player are still unknown? (odds of benefit)
			// * what % of the unknown cards are also unknown to the case file? (size of benefit assuming any)
			// TODO: an enhancement would be to simulate each of the unknown nodes for this player and measure
			// the size of any cascading effect.
			var stats = from p in game.Players
						where p != interactivePlayer
						let handSize = p.CardsHeldCount
						let knownCardsInHand = game.Nodes.Count(n => n.CardHolder == p && n.IsSelected.HasValue && n.IsSelected.Value)
						let unknownCardsInHand = handSize - knownCardsInHand
						let possiblyRevealingCards = game.Nodes.Count(n => n.CardHolder == p && !n.IsSelected.HasValue)
						let possibleCardsThatMayBeInCaseFile = game.Nodes.Count(n => n.CardHolder == p && !n.IsSelected.HasValue && !game.Nodes.First(cn => cn.CardHolder == game.CaseFile && cn.Card == n.Card).IsSelected.HasValue)
						let oddsOfAnyBenefit = (float)unknownCardsInHand / p.CardsHeldCount
						let oddsOfBenefitBeingSubstantial = possiblyRevealingCards > 0 ? ((float)possibleCardsThatMayBeInCaseFile / possiblyRevealingCards) : 0
						let choiceStrength = oddsOfAnyBenefit * oddsOfBenefitBeingSubstantial
						orderby choiceStrength descending
						select new {
							Name = p.Name,
							OddsOfAnyBenefit = oddsOfAnyBenefit,
							OddsOfBenefitBeingSubstantial = oddsOfBenefitBeingSubstantial,
							ChoiceStrength = choiceStrength
						};

			Console.Write("{0,-" + playerColumnWidth + "} ", "Player");
			Console.Write("{0,20} ", "Odds of any benefit");
			Console.Write("{0,36} ", "Odds of an unknown card being useful");
			Console.Write("{0,20} ", "Strength of choice");
			Console.WriteLine();
			foreach (var stat in stats) {
				Console.Write("{0,-" + playerColumnWidth + "} ", stat.Name);
				Console.Write("{0,20:0}% ", stat.OddsOfAnyBenefit * 100);
				Console.Write("{0,36:0}% ", stat.OddsOfBenefitBeingSubstantial * 100);
				Console.Write("{0,20:0}%", stat.ChoiceStrength * 100);
				Console.WriteLine();
			}

			// Begin spying
			SpyCard clue = new SpyCard();
			clue.Player = choosePlayer("Spy on which player?", true, false);
			if (clue.Player == null) return;
			clue.Card = ConsoleHelper.Choose("Which card did you see?", true, c => c.Name, clue.PossiblySeenCards.ToArray());
			if (clue.Card == null) return;
			game.Clues.Add(clue);
		}

		string getCardSuggestionStrength(Card card) {
			int strength = 0;
			// Consider the card worthwhile if its existence in the Case File is not known.
			if (!game.CaseFile.HasCard(card).HasValue) {
				// Its strength is based on how much we know about it so far, which is based
				// on how many constraints we already have on it.
				int constraintWithCardCount = Enumerable.Count(game.Constraints,
					c => c is ConstraintBase && ((ConstraintBase)c).Nodes.Any(cn => ((Node)cn).Card == card));
				strength = 50 - constraintWithCardCount;
			}
			return string.Format("{0,-20}", card.Name) + string.Format("{0,4}", strength);
		}

		void suggestion() {
			Suspicion suggestion = new Suspicion();
			suggestion.Place = ConsoleHelper.Choose("Where?", true, p => getCardSuggestionStrength(p), game.Places.ToArray());
			if (suggestion.Place == null) return;
			suggestion.Suspect = ConsoleHelper.Choose("Who?", true, s => getCardSuggestionStrength(s), game.Suspects.ToArray());
			if (suggestion.Suspect == null) return;
			suggestion.Weapon = ConsoleHelper.Choose("How?", true, w => getCardSuggestionStrength(w), game.Weapons.ToArray());
			if (suggestion.Weapon == null) return;
			foreach (Player opponent in game.PlayersInOrderAfter(suggestingPlayer)) {
				bool? disproved;
				// Do we already know whether this player could disprove it?
				if ((disproved = opponent.HasAtLeastOneOf(suggestion.Cards)).HasValue) {
					ConsoleHelper.WriteColor(ConsoleHelper.QuestionColor, "{0} {1} disprove {2}.", opponent.Name, disproved.Value ? "CAN" : "CANNOT", suggestion);
				} else {
					// Ask the gamer if the opponent did.
					switch (ConsoleHelper.Choose(string.Format("Could {0} disprove {1}?", opponent.Name, suggestion), false,
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
					IEnumerable<Card> possiblyShownCards = opponent.PossiblyHeldCards.Where(c => suggestion.Cards.Contains(c));
					if (possiblyShownCards.Count() == 1) {
						ConsoleHelper.WriteColor(ConsoleHelper.QuestionColor, "{0} must have shown you {1}.", opponent,
							alabi = possiblyShownCards.First());
					} else {
						alabi = ConsoleHelper.Choose(string.Format("Which card did {0} show you?", opponent),
							true, c => c.Name, possiblyShownCards.ToArray());
					}
				}
				if (disproved.HasValue) {
					if (disproved.Value) {
						game.Clues.Add(new Disproved(opponent, suggestion, alabi));
						if (game.Rules.DisprovalEndsTurn) {
							break;
						}
					} else {
						game.Clues.Add(new CannotDisprove(opponent, suggestion));
					}
				}
			}
		}

		void accusation() {
			Suspicion suggestion = new Suspicion();
			suggestion.Place = ConsoleHelper.Choose("Where?", true, p => getCardSuggestionStrength(p), game.Places.ToArray());
			if (suggestion.Place == null) return;
			suggestion.Suspect = ConsoleHelper.Choose("Who?", true, s => getCardSuggestionStrength(s), game.Suspects.ToArray());
			if (suggestion.Suspect == null) return;
			suggestion.Weapon = ConsoleHelper.Choose("How?", true, w => getCardSuggestionStrength(w), game.Weapons.ToArray());
			if (suggestion.Weapon == null) return;
			switch (ConsoleHelper.Choose(string.Format("Was the accusation correct ({0})?", suggestion), false,
				new string[] { "Yes", "No", "Abort accusation" })) {
				case 0: // Correct
					return; // game over
				case 1: // Incorrect
					// Add a clue that this solution is impossible
					game.Clues.Add(new BadAccusation(suggestion, game.CaseFile));
					break;
				case 2: // Abort
					break;
			}
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
				Console.Write("{0} ", card.Name);
			}
			Console.WriteLine();
		}

		private void printGridRow(ICardHolder player, IEnumerable<Card> cards) {
			string name = (player is Player) ? (player as Player).Name : "Case File";
			Console.Write("{0,-" + playerColumnWidth + "} ", name);
			foreach (Card card in cards) {
				bool? isSelected = player.HasCard(card);
				string value = "?";
				if (isSelected.HasValue) {
					value = isSelected.Value ? "1" : "0";
				}
				Console.Write(ConsoleHelper.CenterString(value, card.Name.Length, card.Name.Length + 1));
			}
			Console.WriteLine();
		}

		void listClues() {
			foreach (Clue c in game.Clues) {
				Console.WriteLine(c);
			}
		}
	}
}
