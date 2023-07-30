using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace CPPUtility
{
    public partial class MultipleLineEditDialog : Window
    {
        string text;
        bool isOK = false;

        public MultipleLineEditDialog()
        {
            InitializeComponent();

            dataGrid.Loaded += SetMinWidths;
        }
        public void SetMinWidths(object source, EventArgs e)
        {
            foreach (var column in dataGrid.Columns)
            {
                column.MinWidth = column.ActualWidth;
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            }
        }

        internal async Task<bool> OpenMultipleLineEditDialogAsync<T>(string title, string text) where T : BasicLiteralFormatter, new()
        {
            this.text = text;
            TextBox.Text = text;

            TitleLabel.Content = title;
            Title = title;

            dataGrid.ItemsSource = SingletonHelper<T>.Instance.GetLiteralSamplesData();


            isOK = false;
            await this.ShowDialogAsync();
            return isOK;
        }

        public string GetText()
        {
            return text;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            text = TextBox.Text;
            isOK = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ScrollViewer_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
                e.Handled = true;
            }
        }
    }
}
