using System;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using IronPython.Runtime;

namespace RuriLib.LS
{
    /// Implements AvalonEdit ICompletionData interface to provide the entries in the
    /// completion drop down.
    public class LoliScriptCompletionData : ICompletionData
    {
        public LoliScriptCompletionData(string text,string description)
        {
            Text = text;
            Description = description;
        }

        public System.Windows.Media.ImageSource Image
        {
            get { return null; }
        }

        public string Text { get; private set; }

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content
        {
            get { return Text + " (" + Description + ")"; }
        }

        public object Description { get; private set; }

        public double Priority { get; }

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }

        public static List<string> DataList = new List<string>()
        {
            "HEADER","FILTER","OCR","PARSE",
            "REQUEST"
        };
    }
}
