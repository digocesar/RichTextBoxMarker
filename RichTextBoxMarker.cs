using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinForms.EditorExtensions
{
    public partial class RichTextBoxMarker : RichTextBox
    {
        #region Fields

        private const char SpaceSymbol = '·';
        private const char NewLineSymbol = '¶';

        private bool _replacingText;
        private bool _showSpecialCharacter;

        #endregion


        #region Properties

        [DefaultValue(true)]
        [Browsable(true)]
        [Category("Behavior")]
        [Description("Show markers in text to indicate spaces and new lines.")]
        public bool ShowSpecialCharacter
        {
            get => _showSpecialCharacter;
            set
            {
                if (_showSpecialCharacter == value)
                    return;

                _showSpecialCharacter = value;

                if (_showSpecialCharacter)
                    AddSpecialCharacters();
                else
                    RemoveSpecialCharacters();
            }
        }

        #endregion


        #region Constructors

        public RichTextBoxMarker()
        {
            InitializeComponent();

            InitializeValues();
        }

        public RichTextBoxMarker(IContainer container)
        {
            container.Add(this);

            InitializeComponent();

            InitializeValues();
        }

        private void InitializeValues()
        {
            ShowSpecialCharacter = true;
        }

        #endregion


        #region Methods override

        protected override void OnTextChanged(EventArgs e)
        {
            if (_replacingText)
                return;

            if (ShowSpecialCharacter)
            {
                AddSpecialCharacters();
            }

            base.OnTextChanged(e);
        }

        protected override void OnSelectionChanged(EventArgs e)
        {
            if (_replacingText)
                return;

            if (SelectionStart > 0 && Text[SelectionStart - 1] == NewLineSymbol)
            {
                _replacingText = true;
                SelectionStart--;
                _replacingText = false;
            }

            base.OnSelectionChanged(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (ShowSpecialCharacter && e.Control && e.KeyCode == Keys.C)
            {
                var textWithoutMarkers = SelectedText
                   .Replace(SpaceSymbol, ' ')
                   .Replace(NewLineSymbol.ToString(), "");

                Clipboard.SetText(textWithoutMarkers);
            }

            base.OnKeyUp(e);
        }

        #endregion


        #region Methods

        protected virtual void AddSpecialCharacters()
        {
            var textBeforeCursor = Text.Substring(0, SelectionStart).Split('\n');

            var currentLine = textBeforeCursor.Length;
            var caretPosition = textBeforeCursor.LastOrDefault()?.Length ?? 0;

            var oldTextLines = Text.Split('\n');
            var newText = new StringBuilder(Text.Length);

            for (var x = 0; x < oldTextLines.Length; x++)
            {
                var lineText = oldTextLines[x];
                lineText = lineText.Replace(' ', SpaceSymbol);

                if (x < oldTextLines.Length - 1)
                {
                    if (!lineText.EndsWith(NewLineSymbol.ToString()))
                        lineText += NewLineSymbol;

                    newText.AppendLine(lineText);
                }
                else
                {
                    newText.Append(lineText);
                }
            }


            _replacingText = true;

            Text = newText.ToString();

            var lengthBeforeCurrentLine = Text.Split('\n')
               .Take(currentLine - 1)
               .Sum(x => x.Length + 1);

            SelectionStart = lengthBeforeCurrentLine + caretPosition;

            _replacingText = false;
        }

        protected virtual void RemoveSpecialCharacters()
        {
            var textBeforeCursor = Text.Substring(0, SelectionStart).Split('\n');

            var currentLine = textBeforeCursor.Length;
            var caretPosition = textBeforeCursor.LastOrDefault()?.Length ?? 0;


            _replacingText = true;

            Text = Text
               .Replace(SpaceSymbol, ' ')
               .Replace(NewLineSymbol.ToString(), "");

            var lengthBeforeCurrentLine = Text.Split('\n')
               .Take(currentLine - 1)
               .Sum(x => x.Length + 1);

            SelectionStart = lengthBeforeCurrentLine + caretPosition;

            _replacingText = false;
        }

        #endregion
    }
}