namespace ClueBuddyGui {
	using System;
	using System.ComponentModel;
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

	/// <summary>
	/// Interaction logic for CompositeClueBrowser.xaml
	/// </summary>
	public partial class CompositeClueBrowser : System.Windows.Controls.UserControl {
		/// <summary>
		/// Initializes a new instance of the <see cref="CompositeClueBrowser"/> class.
		/// </summary>
		public CompositeClueBrowser() {
			InitializeComponent();
		}

		protected override void OnInitialized(EventArgs e) {
			base.OnInitialized(e);
		}

		private bool templateChangeHooked;

		void cluesDataView_CurrentChanged(object sender, EventArgs e) {
			cc.ContentTemplate = (DataTemplate) Resources[cluesDataView.View.CurrentItem is CompositeClue ? "CompositeClueTemplate" : "SpyClueTemplate"];
		}

		Game game {
			get { return DataContext as Game; }
		}

		private CollectionViewSource cluesDataView {
			get { return Resources["cluesDataView"] as CollectionViewSource; }
		}

		public Clue CurrentClue {
			get { return cluesDataView.View.CurrentItem as Clue; }
		}

		private void possiblyHeldCardsDataView_Filter(object sender, FilterEventArgs e) {
			SpyCard clue = CurrentClue as SpyCard;
			if (clue != null && clue.Player != null) {
				Player spiedPlayer = clue.Player;
				Card possibleCard = (Card)e.Item;
				Node node = game.Nodes.First(n => n.CardHolder == spiedPlayer && n.Card == possibleCard);
				e.Accepted = !node.IsSelected.HasValue || node.IsSelected.Value;
			}
		}

		void spiedPlayer_Changed(object sender, EventArgs e) {
			ComboBox list = (ComboBox)sender;
			Panel panel = (Panel)list.Parent;
			ItemsControl lb = (ItemsControl)panel.FindName("spiedCard");
			lb.Items.Refresh();
		}

		void suggestingPlayer_Changed(object sender, EventArgs e) {
			ComboBox list = (ComboBox)sender;
			Panel panel = (Panel)list.Parent;
			ItemsControl lb = (ItemsControl)panel.FindName("responses");
			lb.Items.Refresh();
		}

		void previousClueButton_Click(object sender, EventArgs e) {
			if (!cluesDataView.View.MoveCurrentToPrevious())
				cluesDataView.View.MoveCurrentToFirst();
		}

		void nextClueButton_Click(object sender, EventArgs e) {
			if (!cluesDataView.View.MoveCurrentToNext())
				cluesDataView.View.MoveCurrentToLast();
		}

		void newCompositeClueButton_Click(object sender, EventArgs e) {
			game.Clues.Add(new CompositeClue());
			cluesDataView.View.MoveCurrentToLast();
		}

		void newSpyClueButton_Click(object sender, EventArgs e) {
			// <HACK>
			if (!templateChangeHooked) {
				templateChangeHooked = true;
				cluesDataView.View.CurrentChanged += new EventHandler(cluesDataView_CurrentChanged);
			}
			// </HACK>
			game.Clues.Add(new SpyCard());
			cluesDataView.View.MoveCurrentToLast();
		}

		void deleteClueButton_Click(object sender, EventArgs e) {
			game.Clues.Remove(CurrentClue);
		}

		void refreshConstraintsButton_Click(object sender, EventArgs e) {
			Cursor oldCursor = Cursor;
			Cursor = Cursors.Wait;
			try {
				game.RegenerateConstraints();
			} finally {
				Cursor = oldCursor;
			}
		}
	}
}
