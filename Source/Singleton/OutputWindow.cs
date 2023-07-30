using Community.VisualStudio.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPPUtility
{
    internal class OutputWindow : Singleton<OutputWindow>
    {
        OutputWindowPane pane = null;

        protected override async Task InializeAsync()
        {
            if (pane == null)
                pane = await VS.Windows.CreateOutputWindowPaneAsync("CPP Utility");

            await pane.ClearAsync();
        }

        public Task WriteLineAsync(string line)
        {
            return pane.WriteLineAsync(line);
        }
        public void WriteLine(string line)
        {
            pane.WriteLine(line);
        }
    }
}
