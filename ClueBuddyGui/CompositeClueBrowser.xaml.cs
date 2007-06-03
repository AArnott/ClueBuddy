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
		Clue currentClue {
			get { return cluesDataView.View.CurrentItem as Clue; }
		}

		void suggestingPlayer_Changed(object sender, EventArgs e) {
			ComboBox list = (ComboBox)sender;
			StackPanel panel = (StackPanel)list.Parent;
			ListBox lb = (ListBox)panel.FindName("responses");
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
			game.Clues.Remove(currentClue);
		}
	}
}
