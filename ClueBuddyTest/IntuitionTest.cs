using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ClueBuddy;

namespace ClueBuddyTest {
	class DisprovedAnyCards : Clue {
		public DisprovedAnyCards(Player player, params Card[] cards)
			: base(player) {
			this.cards = cards;
		}
		Card[] cards;
		internal override IEnumerable<IConstraint> GetConstraints(IEnumerable<Node> nodes) {
			yield return SelectionCountConstraint.MinSelected(1, from n in nodes
																 where cards.Contains(n.Card) && n.CardHolder == Player
																 select (INode)n);
		}
	}
	class CannotDisproveAnyCards : Clue {
		public CannotDisproveAnyCards(Player player, params Card[] cards)
			: base(player) {
			this.cards = cards;
		}
		Card[] cards;
		internal override IEnumerable<IConstraint> GetConstraints(IEnumerable<Node> nodes) {
			Debug.WriteLine("Generating constraint for " + cards.Length + " cards.");
			IEnumerable<INode> constrainedNodes = from n in nodes
												  where cards.Contains(n.Card) && n.CardHolder == Player
												  select (INode)n;
			Debug.WriteLine("Constraining nodes: " + string.Join(", ", constrainedNodes.Select(n => n.ToString()).ToArray()));
			yield return SelectionCountConstraint.ExactSelected(0, constrainedNodes);
		}
	}

	// These tests were ported from the Ruby implementation
	[TestClass]
	public class IntuitionTest : TestBase {
		Game game;
		Card[] cards;
		Suspect[] suspects;
		Weapon[] weapons;
		Place[] places;

		public override void Setup() {
			base.Setup();
			game = StartPresetGame();
		}

		protected override Game PreparePresetGame() {
			Game g = base.PreparePresetGame();
			cards = g.Cards.ToArray();
			suspects = g.Suspects.ToArray();
			weapons = g.Weapons.ToArray();
			places = g.Places.ToArray();
			return g;
		}

		/// <summary>
		/// Whenever a DOES_HAVE is placed for a card, set DOES_NOT_HAVE for all
		/// other players (and the case_file) for that card.
		/// </summary>
		[TestMethod]
		public void test_does_have_is_exclusive() {
			var card = game.Weapons.First();
			var player = game.Players.First();
			player.see_card(card);
			Debug.WriteLine("Nodes with set values: " + game.Nodes.Where(n => n.IsSelected.HasValue).Count().ToString());
			foreach (Player p in game.Players) {
				bool? cardHeld = game.IsCardHeld(p, card);
				Assert.IsTrue(cardHeld.HasValue);
				Assert.AreEqual(p == player, cardHeld.Value);
			}
			Assert.IsFalse(game.IsCardHeld(game.CaseFile, card).Value);
		}

		/// <summary>
		/// Once the # of unknown cards in hand equals the number of 
		/// unknown cards for that person, mark each of the unknown cards DOES_HAVE.
		/// </summary>
		[TestMethod]
		public void test_last_possible_cards() {
			var player = game.Players.First();
			var hand = player.CardsHeldCount;
			var firstSet = game.Cards.Where((c, i) => i < game.Cards.Count() - (hand + 1));
			var secondSet = game.Cards.Where((c, i) => i == game.Cards.Count() - (hand + 1));
			var thirdSet = game.Cards.Where((c, i) => i > game.Cards.Count() - (hand + 1));
			Debug.Assert(secondSet.Count() == 1);
			Debug.Assert(thirdSet.Count() == hand);

			// eliminate all cards except a handful plus one
			game.Clues.Add(new CannotDisproveAnyCards(player, firstSet.ToArray()));
			Assert.AreEqual(hand + 1, game.Nodes.Where(n => n.CardHolder == player && !n.IsSelected.HasValue).Count());
			// eliminate just one more card (leaving just enough cards that must be held)
			game.Clues.Add(new CannotDisproveAnyCards(player, secondSet.ToArray()));
			Assert.AreEqual(0, game.Nodes.Where(n => n.CardHolder == player && !n.IsSelected.HasValue).Count());
			foreach (Card card in thirdSet)
				Assert.IsTrue(game.IsCardHeld(player, card).Value);
		}

		/// <summary>
		/// When a card is DOES_NOT_HAVE for every player, the case_file must have it.
		/// </summary>
		[TestMethod]
		public void test_no_one_has_card_then_envelope() {
			// Test where no player has a card, the case_file must have it.
			var weapon = weapons[0];
			foreach (Player p in game.Players) {
				Debug.WriteLine("Adding clue to player " + p.Name);
				game.Clues.Add(new CannotDisproveAnyCards(p, weapon));
			}
			Assert.IsTrue(game.CaseFile.has(weapon).Value);
		}

		/// <summary>
		/// When a card is DOES_NOT_HAVE for the case_file and every player except one, 
		/// that one exception player must have it.
		/// </summary>
		[TestMethod]
		public void test_envelope_and_no_one_else_has_card_1() {
			// Test where the case_file is the last changed thing.
			var place = places[0];
			var otherPlace = places[1];
			var player = players[0];
			foreach (Player p in game.Players) {
				if (p != player)
					game.Clues.Add(new CannotDisproveAnyCards(p, place));
			}
			Assert.IsFalse(game.IsCardHeld(player, place).HasValue);
			game.CaseFile.set(otherPlace); // some other place card was somehow ascertained, excluding 'place'
			Assert.IsTrue(game.IsCardHeld(player, place).HasValue);
			Assert.IsTrue(game.IsCardHeld(player, place).Value);
		}

		/// <summary>
		/// When a card is DOES_NOT_HAVE for the case_file and every player except one, 
		/// that one exception player must have it.
		/// </summary>
		[TestMethod]
		public void test_envelope_and_no_one_else_has_card_2() {
			// Do it again, this time ending with a player being the last changed thing.
			var weapon = game.Weapons.First();
			var player = game.Players.First();
			// some other weapon in the CaseFile.
			game.Nodes.Where(n => n.CardHolder == game.CaseFile && n.Card is Weapon && n.Card != weapon).First().IsSelected = true;
			Assert.IsFalse(game.IsCardHeld(player, weapon).HasValue);
			foreach (Player p in game.Players)
				if (p != player)
					game.Clues.Add(new CannotDisproveAnyCards(p, weapon));
			Assert.IsTrue(game.IsCardHeld(player, weapon).Value);
		}

		/// <summary>
		/// When all cards in a category except one are known to not be in the 
		/// case_file (other players hold them), the remaining card in that 
		/// category must be in the case_file.
		/// </summary>
		[TestMethod]
		public void test_last_card_in_category_must_be_in_envelope() {
			var iPlayer = 0;
			// dish out all the places except one among the players
			foreach (Place place in game.Places) {
				if (place == game.Places.Last()) continue;
				game.Clues.Add(new SpyCard(game.Players[iPlayer], place));
				iPlayer = (iPlayer + 1) % game.Players.Count;
			}
			// the last place should be automatically identified as the one
			// in the case_file
			Assert.IsTrue(game.IsCardHeld(game.CaseFile, game.Places.Last()).Value);
		}

		/// <summary>
		/// When all cards in the hand have been identified, set the rest of the
		/// cards to DOES_NOT_HAVE.
		/// </summary>
		[TestMethod]
		public void test_all_cards_known_zeros_out_others() {
			var player = game.Players[0];
			for (int i = 0; i < player.CardsHeldCount; i++)
				player.see_card(game.Cards.Where((c, j) => j == i).First());
			foreach (Card card in game.Cards.Where((c, j) => j >= player.CardsHeldCount)) {
				Assert.IsFalse(game.IsCardHeld(player, card).Value, card.Name + " should have been marked DOES_NOT_HAVE.");
			}
		}

		/// <summary>
		/// Apply greedy algorithm to alabis to find solutions whenever we can,
		/// whenever anything about an intuition hash or its contained arrays change.
		/// Test 1.
		/// </summary>
		[TestMethod]
		public void test_simple_greedy_algorithm_alabi_disproved_last() {
			// we seek to reproduce something that looks similar to:
			// 0 0 A 1 0 B 0 1 BA 0  A  0  B  <-- "BA", index 8, is the card to choose!
			// 0 1 2 3 4 5 6 7 8  9 10 11 12  <-- indexes into cards
			game.Reset();
			game = PreparePresetGame();
			var player = players[0];
			players[1].CardsHeldCount += player.CardsHeldCount - 3;
			player.CardsHeldCount = 3;
			game.Start();
			var orig_unknown_cards = (from n in game.Nodes
									  where n.CardHolder == player && !n.IsSelected.HasValue
									  select n.Card).ToArray();
			player.see_card(cards[3]);
			player.see_card(cards[7]);
			Assert.AreEqual(orig_unknown_cards.Length - 2, (from n in game.Nodes
															where n.CardHolder == player && !n.IsSelected.HasValue
															select n.Card).Count());
			player.disproved(cards[2], cards[8], cards[10]);
			player.disproved(cards[5], cards[8], cards[12]);
			Assert.IsTrue(player.has(cards[8]).Value);
			Assert.IsFalse(player.has(cards[2]).Value);
			Assert.IsFalse(player.has(cards[5]).Value);
			Assert.IsFalse(player.has(cards[10]).Value);
			Assert.IsFalse(player.has(cards[12]).Value);
			Assert.AreEqual(0, (from n in game.Nodes
								where n.CardHolder == player && !n.IsSelected.HasValue
								select n.Card).Count());
		}

		/// <summary>
		/// Apply greedy algorithm to alabis to find solutions whenever we can,
		/// whenever anything about an intuition hash or its contained arrays change.
		/// Test 2.
		/// </summary>
		[TestMethod]
		public void test_simple_greedy_algorithm_alabi_see_card_last() {
			// we seek to reproduce something that looks similar to:
			// 0 0 A 1 0 B 0 1 BA 0  A  0  B  <-- "BA", index 8, is the card to choose!
			// 0 1 2 3 4 5 6 7 8  9 10 11 12  <-- indexes into cards
			game.Reset();
			game = PreparePresetGame();
			var player = players[0];
			players[1].CardsHeldCount += player.CardsHeldCount - 3;
			player.CardsHeldCount = 3;
			game.Start();
			var orig_unknown_cards = (from n in game.Nodes
									  where n.CardHolder == player && !n.IsSelected.HasValue
									  select n.Card).ToArray();
			player.disproved(cards[2], cards[8], cards[10]);
			player.disproved(cards[5], cards[8], cards[12]);
			player.see_card(cards[3], cards[7]);
			Assert.IsTrue(player.has(cards[8]).Value);
			Assert.IsTrue(player.has_not(cards[2]).Value);
			Assert.IsTrue(player.has_not(cards[5]).Value);
			Assert.IsTrue(player.has_not(cards[10]).Value);
			Assert.IsTrue(player.has_not(cards[12]).Value);
			Assert.AreEqual(0, (from n in game.Nodes
								where n.CardHolder == player && !n.IsSelected.HasValue
								select n.Card).Count());
		}

		/// <summary>
		/// When a card is DOES_NOT_HAVE for the CaseFile and every player except one, 
		/// that one exception player must have it.
		/// </summary>
		[TestMethod]
		public void test_another_player_may_have_1() {
			var card = suspects[0];
			var card_in_envelope = suspects[1];
			// Test simple scenario where all (including the CaseFile) except the first player is known to not have a card.
			// CaseFile is set first in this test.  See test_cascade_only_player_left_to_hold_card for CaseFile set last.
			game.CaseFile.set(card_in_envelope);
			foreach (Player p in game.Players) {
				if (p == game.Players[0]) continue; // skip first player
				p.cannot_disprove(card);
			}
			Assert.IsTrue(players[0].has(card).Value);
		}

		/// <summary>
		/// When a card is DOES_NOT_HAVE for the CaseFile and every player except one, 
		/// that one exception player must have it.
		/// </summary>
		[TestMethod]
		public void test_another_player_may_have_2() {
			var card = suspects[0];
			var card_another = suspects[2];
			var card_in_envelope = suspects[1];
			// Test slightly more complicated scenario where one of the players only MAY NOT have the card (alabi group).
			game.CaseFile.set(card_in_envelope);
			for (int i = 0; i < game.Players.Count; i++) {
				var p = game.Players[i];
				switch (i) {
					case 0: continue; // skip first player
					case 1:
						p.disproved(card, card_another); // create alabi group
						break;
					default:
						p.cannot_disprove(card);
						break;
				}
			}
			Assert.IsFalse(players[0].has(card).HasValue);
		}

		/// <summary>
		/// When a card is DOES_NOT_HAVE for the CaseFile and every player except one, 
		/// that one exception player must have it.
		/// </summary>
		[TestMethod]
		public void test_cascade_only_player_left_to_hold_card() {
			var card = suspects[0];
			var card_in_envelope = suspects[1];
			// Test simple scenario where all (including the CaseFile) except the first player is known to not have a card.
			// CaseFile is set last in this test.
			for (int i = 0; i < game.Players.Count; i++) {
				var p = game.Players[i];
				if (i == 0) continue; // skip first player
				p.cannot_disprove(card);
			}
			game.CaseFile.set(card_in_envelope);
			Assert.IsTrue(players[0].has(card).Value);
		}

		/********************************************
		 * Vertical analysis
		 ********************************************/

		/// <summary>
		/// When a suggestion is disproven by three players, 
		/// none of those cards can be in the CaseFile.
		/// </summary>
		[TestMethod]
		public void test_disproven_three_times_then_not_in_CaseFile() {
			//    Verify that this    		   Becomes this        (underscores are not set or checked, allowing for additional behavior)
			// P1: A _ _ A _ _ A _ _		A _ _ A _ _ A _ _
			// P2: A _ _ A _ _ A _ _		A _ _ A _ _ A _ _
			// P3: A _ _ A _ _ A _ _		A _ _ A _ _ A _ _
			// P4: _ _ _ _ _ _ _ _ _		_ _ _ _ _ _ _ _ _
			// CF: _ _ _ _ _ _ _ _ _		0 _ _ 0 _ _ 0 _ _
			var suggestion = new Card[] { suspects[0], weapons[0], places[0] };
			foreach (Player p in players.Take(suggestion.Length)) {
				Debug.WriteLine(p.Name + " disproving suggestion");
				p.disproved(suggestion);
			}

			foreach (Card card in suggestion) {
				Assert.IsTrue(game.CaseFile.has_not(card).Value);
			}
		}

		/// <summary>
		/// When a suggestion is disproven by three players, none of those cards 
		/// can be held by other players.
		/// </summary>
		[TestMethod]
		public void test_disproven_three_times_then_not_held_by_other_players() {
			//    Verify that this    		   Becomes this        (underscores are not set or checked, allowing for additional behavior)
			// P1: A _ _ A _ _ A _ _		A _ _ A _ _ A _ _
			// P2: A _ _ A _ _ A _ _		A _ _ A _ _ A _ _
			// P3: A _ _ A _ _ A _ _		A _ _ A _ _ A _ _
			// P4: _ _ _ _ _ _ _ _ _		0 _ _ 0 _ _ 0 _ _
			// CF: _ _ _ _ _ _ _ _ _		_ _ _ _ _ _ _ _ _
			var suggestion = new Card[] { suspects[0], weapons[0], places[0] };
			foreach (Player p in players.Take(suggestion.Length)) {
				p.disproved(suggestion);
			}

			foreach (Card card in suggestion) {
				foreach (Player p in players.Skip(suggestion.Length)) {
					Assert.IsTrue(p.has_not(card).Value);
				}
			}
		}

		/// <summary>
		/// An alabi group of two cards, shared by another player,
		/// should cancel anyone else's chance of holding the cards.
		/// </summary>
		[TestMethod]
		public void test_two_card_alabi_group_by_two_player_reserves_cards() {
			//    Verify that this    		   Becomes this
			// P1: _ _ _ A _ _ A _ _		_ _ _ A _ _ A _ _
			// P2: _ _ _ A _ _ A _ _		_ _ _ A _ _ A _ _
			// P3: _ _ _ _ _ _ _ _ _		_ _ _ 0 _ _ 0 _ _
			// P4: _ _ _ _ _ _ _ _ _		_ _ _ 0 _ _ 0 _ _
			// CF: _ _ _ _ _ _ _ _ _		_ _ _ 0 _ _ 0 _ _
			var suggestion = new Card[] { weapons[0], places[0] };
			foreach (Player p in players.Take(suggestion.Length)) {
				p.disproved(suggestion);
			}

			foreach (Card card in suggestion) {
				foreach (Player p in players.Skip(suggestion.Length)) {
					Assert.IsTrue(p.has_not(card).Value);
				}
				Assert.IsTrue(game.CaseFile.has_not(card).Value);
			}
		}

		/// <summary>
		/// Alabi group solution possibilities should be explored, and 
		/// where only one resolution possibility exists that can reduce
		/// an alabi group, go ahead and execute it.
		/// </summary>
		[TestMethod]
		public void test_alabi_group_vertical_elimination_intelligence() {
			//    Verify that this    		   Becomes this
			// P1: A _ _ A _ _ A _ _		1 _ _ 0 _ _ 0 _ _
			// P2: _ _ _ A _ _ A _ _		0 _ _ A _ _ A _ _
			// P3: _ _ _ A _ _ A _ _		0 _ _ A _ _ A _ _
			// P4: _ _ _ _ _ _ _ _ _		0 _ _ 0 _ _ 0 _ _
			// CF: _ _ _ _ _ _ _ _ _		0 _ _ 0 _ _ 0 _ _
			var suggestion = new Card[] { suspects[0], weapons[0], places[0] };
			players[0].disproved(suggestion);
			players[1].disproved(suggestion.Skip(1).ToArray());
			players[2].disproved(suggestion.Skip(1).ToArray());

			Assert.IsTrue(players[0].has(suggestion[0]).Value);
			Assert.IsTrue(players[0].has_not(suggestion[1]).Value);
			Assert.IsTrue(players[0].has_not(suggestion[2]).Value);

			foreach (Player p in players.Skip(1).Take(2)) {
				foreach (Card card in suggestion) {
					Assert.IsTrue(p.has_not(card).Value);
				}
				Assert.IsTrue((from c in game.Constraints
							   where c.Nodes.Count() == 2 &&
							   c.Nodes.All(n => ((Node)n).CardHolder == p) &&
							   c.Nodes.Any(n => ((Node)n).Card == suggestion[1]) &&
							   c.Nodes.Any(n => ((Node)n).Card == suggestion[2])
							   select c).Count() == 1);
			}

			foreach (Card card in suggestion) {
				foreach (Player p in players.Skip(suggestion.Length)) {
					Assert.IsTrue(p.has_not(card).Value);
				}
				Assert.IsTrue(game.CaseFile.has_not(card).Value);
			}
		}
	}
}
