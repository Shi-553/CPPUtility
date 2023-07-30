using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace CPPUtility
{

    /// <summary>
    /// CPPUtilityOptions.xaml の相互作用ロジック
    /// </summary>
    public partial class CPPUtilityOptions : UserControl
    {
        public CPPUtilityOptions()
        {
            InitializeComponent();
        }
        internal CPPUtilityOptionPage cppUtilityOptionsPage;

        public void Initialize()
        {
            CPPFunctionCommentSnippetTextbox.Text = CPPUtilityOption.Instance.CPPFunctionCommentSnippet;
            DocumentTopCommentSnippetTextbox.Text = CPPUtilityOption.Instance.DocumentTopCommentSnippet;
            IsUseCreateHeaderFunctionCommentCheckBox.IsChecked = CPPUtilityOption.Instance.IsUseCreateHeaderFunctionComment;
            IsUseGenerateCPPFunctionCommentCheckBox.IsChecked = CPPUtilityOption.Instance.IsUseGenerateCPPFunctionComment;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "<保留中>")]
        private async void CPPFunctionCommentSnippetTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var dialog = new MultipleLineEditDialog();
            await dialog.OpenMultipleLineEditDialogAsync<CPPFunctionCommentLiteralFormatter>(
                CPPFunctionCommentSnippetLabel.Content?.ToString() ?? "",
                CPPUtilityOption.Instance.CPPFunctionCommentSnippet);

            var text = dialog.GetText();

            if (!text.EndsWith("\r\n"))
            {
                text += "\r\n";
            }

            CPPFunctionCommentSnippetTextbox.Text = text;
            CPPUtilityOption.Instance.CPPFunctionCommentSnippet = text;
            CPPUtilityOption.Instance.Save();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "<保留中>")]
        private async void DocumentTopCommentSnippet_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var dialog = new MultipleLineEditDialog();
            await dialog.OpenMultipleLineEditDialogAsync<DocumentLiteralFormatter>(
                DocumentTopCommentSnippetLabel.Content?.ToString() ?? "",
                CPPUtilityOption.Instance.DocumentTopCommentSnippet);

            var text = dialog.GetText();

            if (!text.EndsWith("\r\n"))
            {
                text += "\r\n";
            }

            DocumentTopCommentSnippetTextbox.Text = text;
            CPPUtilityOption.Instance.DocumentTopCommentSnippet = text;
            CPPUtilityOption.Instance.Save();

        }

        private void DocumentTopCommentSnippetTextbox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as TextBox;

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private void IsUseCreateHeaderFunctionCommentCheckBox_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            CPPUtilityOption.Instance.IsUseCreateHeaderFunctionComment = IsUseCreateHeaderFunctionCommentCheckBox.IsChecked ?? true;
            CPPUtilityOption.Instance.Save();
        }

        private void IsUseGenerateCPPFunctionCommentCheckBox_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            CPPUtilityOption.Instance.IsUseGenerateCPPFunctionComment = IsUseGenerateCPPFunctionCommentCheckBox.IsChecked ?? true;
            CPPUtilityOption.Instance.Save();
        }
    }
}
