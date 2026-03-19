using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit;

namespace BioSphereIDE
{
    public class SyntaxColorizer : DocumentColorizingTransformer
    {
        private List<Token> _tokens = new List<Token>();

        public void UpdateTokens(List<Token> tokens)
        {
            _tokens = tokens;
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            if (_tokens == null || _tokens.Count == 0 || this.CurrentContext.Document == null) return;

            int lineStartOffset = line.Offset;
            int lineEndOffset = lineStartOffset + line.Length;

            foreach (var token in _tokens)
            {
                if (token.Type == TokenType.EOF) continue;
                
                try 
                {
                    int startOffset = this.CurrentContext.Document.GetOffset(token.Line, token.Column);
                    int endOffset = startOffset + token.Lexeme.Length;

                    if (endOffset > lineStartOffset && startOffset < lineEndOffset)
                    {
                        int s = Math.Max(startOffset, lineStartOffset);
                        int e = Math.Min(endOffset, lineEndOffset);
                        if (e > s)
                        {
                            ChangeLinePart(s, e, element => {
                                element.TextRunProperties.SetForegroundBrush(GetBrushForType(token.Type));
                            });
                        }
                    }
                }
                catch { /* Ignore invalid offsets */ }
            }
        }
        
        private SolidColorBrush GetBrushForType(TokenType type)
        {
            return type switch
            {
                TokenType.PALABRA_RESERVADA => new SolidColorBrush(System.Windows.Media.Color.FromRgb(86, 156, 214)),
                TokenType.NUMERO => new SolidColorBrush(System.Windows.Media.Color.FromRgb(181, 206, 168)),
                TokenType.CADENA => new SolidColorBrush(System.Windows.Media.Color.FromRgb(214, 157, 133)),
                TokenType.OPERADOR => new SolidColorBrush(System.Windows.Media.Color.FromRgb(180, 180, 180)),
                TokenType.ERROR_LEXICO => System.Windows.Media.Brushes.Red,
                _ => new SolidColorBrush(System.Windows.Media.Color.FromRgb(212, 212, 212))
            };
        }
    }

    public class CommentColorizer : DocumentColorizingTransformer
    {
        protected override void ColorizeLine(DocumentLine line)
        {
            string text = CurrentContext.Document.GetText(line);
            int index = text.IndexOf("//");
            if (index >= 0)
            {
                ChangeLinePart(line.Offset + index, line.Offset + line.Length, element => {
                    element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(System.Windows.Media.Color.FromRgb(87, 166, 74)));
                });
            }
        }
    }

    public class ErrorSquiggleRenderer : IBackgroundRenderer
    {
        private TextEditor _editor;
        private List<ErrorInfo> _errors = new List<ErrorInfo>();
        private System.Windows.Media.Pen _errorPen;

        public ErrorSquiggleRenderer(TextEditor editor)
        {
            _editor = editor;
            
            // Red dashed pen instead of complex wavy path for compatibility and simplicity
            _errorPen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.Red, 1.5) { DashStyle = System.Windows.Media.DashStyles.Dash };
            _errorPen.Freeze();
        }

        public void UpdateErrors(List<ErrorInfo> errors)
        {
            _errors = errors;
        }

        public KnownLayer Layer => KnownLayer.Selection;

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (_errors == null || _errors.Count == 0 || _editor.Document == null) return;

            foreach (var error in _errors)
            {
                try
                {
                    int startOffset = _editor.Document.GetOffset(error.Line, error.Column);
                    int length = Math.Max(1, error.Length);
                    
                    if (startOffset + length > _editor.Document.TextLength) 
                    {
                        length = _editor.Document.TextLength - startOffset;
                    }
                    if (length <= 0) continue;

                    var segment = new TextSegment { StartOffset = startOffset, Length = length };
                    foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment))
                    {
                        drawingContext.DrawLine(_errorPen, 
                            new System.Windows.Point(rect.BottomLeft.X, rect.BottomLeft.Y + 2), 
                            new System.Windows.Point(rect.BottomRight.X, rect.BottomRight.Y + 2));
                    }
                }
                catch { /* Ignore invalid offsets */ }
            }
        }
    }
}
