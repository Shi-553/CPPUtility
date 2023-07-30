using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CPPUtility
{
    public class CustomDataGrid : DataGrid
    {
        protected override void OnExecutedCopy(ExecutedRoutedEventArgs args)
        {
            base.OnExecutedCopy(args);

            string text = Clipboard.GetText();
            if (!string.IsNullOrEmpty(text))
            {
                Clipboard.SetText(text.TrimEnd('\r', '\n'));
            }
        }
    }
}
