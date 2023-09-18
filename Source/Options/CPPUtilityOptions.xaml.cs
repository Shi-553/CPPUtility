using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
            DataContext = CPPUtilityOption.Instance;
            InitializeComponent();
        }
        internal CPPUtilityOptionPage cppUtilityOptionsPage;

        public void Initialize()
        {
            Focus();
        }
        public void OnClosed()
        {
            var bindingList = new List<BindingExpressionBase>();
            DependencyObjectHelper.GetBindingsRecursive(this, bindingList);

            foreach (var b in bindingList)
            {
                b.UpdateTarget();
            }
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


        public void UpdateBinding(DependencyObject parent)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is ContentControl contentControl)
                {
                    BindingExpression bindingExpr = BindingOperations.GetBindingExpression(contentControl, ContentControl.ContentProperty);
                    bindingExpr?.UpdateTarget();  // refreshes the ItemsSource
                }
            }
        }

        private T GetInstance<T>(ComboBox comboBox) where T : class
        {
            var variable = StaticClass.GetSubclasses<T>().ElementAtOrDefault(comboBox.SelectedIndex);

            if (variable != null)
            {
                return Activator.CreateInstance(variable.GetType()) as T;
            }
            return null;
        }

        private void VariableSelectorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;

            var instance = GetInstance<VariableSelectorBase>(comboBox);


            if (instance != null && comboBox.DataContext is VariableFormatInfo variableFormatInfo)
            {
                if (variableFormatInfo.Selector.GetType() == instance.GetType())
                {
                    return;
                }
                variableFormatInfo.Selector = instance;

                UpdateBinding(comboBox.Parent);
            }
        }
        private void VariableFormatterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;

            var instance = GetInstance<VariableFormatterBase>(comboBox);


            if (instance != null && comboBox.DataContext is VariableFormatInfo variableFormatInfo)
            {
                if (variableFormatInfo.Formatter.GetType() == instance.GetType())
                {
                    return;
                }
                variableFormatInfo.Formatter = instance;

                UpdateBinding(comboBox.Parent);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;

            if (comboBox.DataContext is EnumInputInfoBase enumInput)
            {
                enumInput.SetValue(comboBox.SelectedValue as string);
            }
        }

        private void AddVariableFormatInfoButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CPPUtilityOption.Instance.VariableFormatInfos.Add(new VariableFormatInfo()
            {
                Selector = new TypeRegexMatchVariableSelector(),
                Formatter = new PrefixVariableFormatter()
            });
        }

        private void RemoveVariableFormatInfoButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button.DataContext is VariableFormatInfo variableFormatInfo)
            {
                CPPUtilityOption.Instance.VariableFormatInfos.Remove(variableFormatInfo);
            }
        }

        void MoveVariableFormatInfo(int oldIndex,int newIndex)
        {
            var infos = CPPUtilityOption.Instance.VariableFormatInfos;
            if (oldIndex < 0 || infos.Count <= oldIndex)
                return;
            if (newIndex < 0 || infos.Count <= newIndex)
                return;

            CPPUtilityOption.Instance.VariableFormatInfos.Move(oldIndex, newIndex);
        }

        private void UpVariableFormatInfoButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button.DataContext is VariableFormatInfo variableFormatInfo)
            {
                var index = CPPUtilityOption.Instance.VariableFormatInfos.IndexOf(variableFormatInfo);
                MoveVariableFormatInfo(index, index - 1);
            }
        }

        private void DownVariableFormatInfoButton_Click(object sender, RoutedEventArgs e)
        {

            var button = sender as Button;

            if (button.DataContext is VariableFormatInfo variableFormatInfo)
            {
                var index = CPPUtilityOption.Instance.VariableFormatInfos.IndexOf(variableFormatInfo);
                MoveVariableFormatInfo(index, index + 1);
            }
        }
    }
}
