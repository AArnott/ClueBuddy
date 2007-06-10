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
	/// Interaction logic for CompositeClueBrowser.xaml
	/// </summary>
	public partial class CompositeClueBrowser : System.Windows.Controls.UserControl {
		public CompositeClueBrowser() {
			InitializeComponent();
		}

		Game game {
			get { return DataContext as Game; }
		}
		CollectionViewSource cluesDataView {
			get { return Resources["cluesDataView"] as CollectionViewSource; }
		}
		public CompositeClue CurrentClue {
			get { return cluesDataView.View.CurrentItem as CompositeClue; }
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

		void newClueButton_Click(object sender, EventArgs e) {
			game.Clues.Add(new CompositeClue());
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
