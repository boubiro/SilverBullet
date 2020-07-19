// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Windows.Controls;
using System.Windows.Documents;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Search;

namespace OpenBullet.Editor.Search
{
    /// <summary>
    /// Provides search functionality for AvalonEdit. It is displayed in the top-right corner of the TextArea.
    /// </summary>
    public class SearchTextEditor
    {
        TextArea textArea;
        SearchInputHandler handler;
        TextDocument currentDocument;
        SearchResultBackgroundRenderer renderer;
        TextBox searchTextBox;
        ISearchStrategy strategy;


        public int Count => renderer == null ? 0 : renderer.CurrentResults.Count;

        /// <summary>
        /// Gets/sets the search pattern.
        /// </summary>
        public string SearchPattern
        {
            get; set;
        }

        public void UpdateSearch()
        {
            // only reset as long as there are results
            // if no results are found, the "no matches found" message should not flicker.
            // if results are found by the next run, the message will be hidden inside DoSearch ...
            strategy = SearchStrategyFactory.Create(SearchPattern ?? "", true, false, SearchMode.Normal);
            OnSearchOptionsChanged(new SearchOptionsChangedEventArgs(SearchPattern, true, false, false));
            DoSearch(true);
        }

        /// <summary>
        /// Creates a new SearchPanel.
        /// </summary>
        SearchTextEditor()
        {
        }

        /// <summary>
        /// Creates a SearchPanel and installs it to the TextEditor's TextArea.
        /// </summary>
        /// <remarks>This is a convenience wrapper.</remarks>
        public static SearchTextEditor Install(TextEditor editor)
        {
            if (editor == null)
                throw new ArgumentNullException("editor");
            return Install(editor.TextArea);
        }

        /// <summary>
        /// Creates a SearchPanel and installs it to the TextArea.
        /// </summary>
        public static SearchTextEditor Install(TextArea textArea)
        {
            if (textArea == null)
                throw new ArgumentNullException("textArea");
            SearchTextEditor searchTextEditor = new SearchTextEditor();
            searchTextEditor.AttachInternal(textArea);
            return searchTextEditor;
        }

        void AttachInternal(TextArea textArea)
        {
            this.textArea = textArea;

            renderer = new SearchResultBackgroundRenderer();
            currentDocument = textArea.Document;
            if (currentDocument != null)
                currentDocument.TextChanged += textArea_Document_TextChanged;
            textArea.DocumentChanged += textArea_DocumentChanged;
        }

        void textArea_DocumentChanged(object sender, EventArgs e)
        {
            if (currentDocument != null)
                currentDocument.TextChanged -= textArea_Document_TextChanged;
            currentDocument = textArea.Document;
            if (currentDocument != null)
            {
                currentDocument.TextChanged += textArea_Document_TextChanged;
                DoSearch(false);
            }
        }

        void textArea_Document_TextChanged(object sender, EventArgs e)
        {
            DoSearch(false);
        }

        /// <summary>
        /// Reactivates the SearchPanel by setting the focus on the search box and selecting all text.
        /// </summary>
        public void Reactivate()
        {
            if (searchTextBox == null)
                return;
            searchTextBox.Focus();
            searchTextBox.SelectAll();
        }

        /// <summary>
        /// Moves to the next occurrence in the file.
        /// </summary>
        public void FindNext()
        {
            SearchResult result = renderer.CurrentResults.FindFirstSegmentWithStartAfter(textArea.Caret.Offset + 1);
            if (result == null)
                result = renderer.CurrentResults.FirstSegment;
            if (result != null)
            {
                SelectResult(result);
            }
        }

        /// <summary>
        /// Moves to the previous occurrence in the file.
        /// </summary>
        public void FindPrevious()
        {
            SearchResult result = renderer.CurrentResults.FindFirstSegmentWithStartAfter(textArea.Caret.Offset);
            if (result != null)
                result = renderer.CurrentResults.GetPreviousSegment(result);
            if (result == null)
                result = renderer.CurrentResults.LastSegment;
            if (result != null)
            {
                SelectResult(result);
            }
        }

        public void DoSearch(bool changeSelection)
        {
            renderer.CurrentResults.Clear();

            if (!string.IsNullOrEmpty(SearchPattern))
            {
                int offset = textArea.Caret.Offset;
                if (changeSelection)
                {
                    textArea.ClearSelection();
                }
                // We cast from ISearchResult to SearchResult; this is safe because we always use the built-in strategy
                foreach (SearchResult result in strategy.FindAll(textArea.Document, 0, textArea.Document.TextLength))
                {
                    if (changeSelection && result.StartOffset >= offset)
                    {
                        SelectResult(result);
                        changeSelection = false;
                    }
                    renderer.CurrentResults.Add(result);
                }
            }
            textArea.TextView.InvalidateLayer(KnownLayer.Selection);
        }

        void SelectResult(SearchResult result)
        {
            textArea.Caret.Offset = result.StartOffset;
            textArea.Selection = Selection.Create(textArea, result.StartOffset, result.EndOffset);
            textArea.Caret.BringCaretToView();
            // show caret even if the editor does not have the Keyboard Focus
            textArea.Caret.Show();
        }

        /// <summary>
        /// Fired when SearchOptions are changed inside the SearchPanel.
        /// </summary>
        public event EventHandler<SearchOptionsChangedEventArgs> SearchOptionsChanged;

        /// <summary>
        /// Raises the <see cref="SearchTextEditor.SearchOptionsChanged" /> event.
        /// </summary>
        protected virtual void OnSearchOptionsChanged(SearchOptionsChangedEventArgs e)
        {
            SearchOptionsChanged?.Invoke(this, e);
        }
    }

    /// <summary>
    /// EventArgs for <see cref="SearchTextEditor.SearchOptionsChanged"/> event.
    /// </summary>
    public class SearchOptionsChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the search pattern.
        /// </summary>
        public string SearchPattern { get; private set; }

        /// <summary>
        /// Gets whether the search pattern should be interpreted case-sensitive.
        /// </summary>
        public bool MatchCase { get; private set; }

        /// <summary>
        /// Gets whether the search pattern should be interpreted as regular expression.
        /// </summary>
        public bool UseRegex { get; private set; }

        /// <summary>
        /// Gets whether the search pattern should only match whole words.
        /// </summary>
        public bool WholeWords { get; private set; }

        /// <summary>
        /// Creates a new SearchOptionsChangedEventArgs instance.
        /// </summary>
        public SearchOptionsChangedEventArgs(string searchPattern, bool matchCase, bool useRegex, bool wholeWords)
        {
            this.SearchPattern = searchPattern;
            this.MatchCase = matchCase;
            this.UseRegex = useRegex;
            this.WholeWords = wholeWords;
        }
    }

    class SearchPanelAdorner : Adorner
    {
        SearchTextEditor panel;

        public SearchPanelAdorner(TextArea textArea, SearchTextEditor panel)
            : base(textArea)
        {
            this.panel = panel;
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }
    }
}
