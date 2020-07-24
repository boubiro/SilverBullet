using System.Windows.Controls;
using System.Windows.Documents;

namespace OpenBullet.Views.Main
{
    /// <summary>
    /// Interaction logic for ReleaseNotesPage.xaml
    /// </summary>
    public partial class ReleaseNotesPage : Page
    {
        public ReleaseNotesPage()
        {
            InitializeComponent();
            AppendNote(new[] {
               "Supported drag drop wordlist,proxy,config",
               "Fixed maximum mainwindow",
               "Fixed load loliX config",
               "Added input box for bots",
               "Added Analyze login page (block request)",
               "Updated wordlist tools",
            });
            AppendNote(new[] {
                "Bugs fixed",
                "Supported format loli,loliX,anom",
                "Added find and replace dialog in LoliScript editor",
                "Added OCR",
                "Added set variable in (OCR)",
                "Added image processing (with OpenCv and without OpenCv)",
                "Added mathematical string evaluation (function)",
                "Added date to solar in function block",
                "Added date(solar) to gregorian (function)",
                "Added get remaining day (function)",
                "Added get current year,month,day,hour (function)",
                "Added input to digits,letter,letterOrdigits (function)",
                "Added remove string in function block",
                "Added num to words (en) (function)",
                "Added words to num (en) (function)",
                "Added subwordlist",
                "Added multiple wordlist",
                "Added disable automation in selenium (settings)",
                "Added new icons and updated previous icons",
                "Added generate randomUA android and ios (function)",
                "Added editable custom (key check)",
                "Added show all custom types by hovering the mouse over the custom label",
                "Added refresh in select config (runner)",
                "Added tessdata downloader (tools)",
                "Added supporters (tab)",
                "Supported random num generation up to 18 digits (function)",
                "Updated browser (html view) to CefSharp (chromium-based browser) (stacker)",
                "Updated log in debugger (stacker)",
                "Updated select list (runner)",
            });
        }

        private void AppendNote(string[] notes)
        {
            foreach (var note in notes)
            {
                var bold = new Bold(new Run("• "));
                bold.SetResourceReference(Bold.ForegroundProperty, "ForegroundMain");
                var paragraph = new Paragraph(bold);
                paragraph.SetResourceReference(Paragraph.ForegroundProperty, "ForegroundMain");
                paragraph.Inlines.Add(new Run(note));
                richTextBox.Document.Blocks.Add(paragraph);
            }
            var endPar = new Paragraph(new Bold(new Run("========================")));
            endPar.SetResourceReference(Paragraph.ForegroundProperty, "ForegroundMain");
            richTextBox.Document.Blocks.Add(endPar);
        }
    }
}
