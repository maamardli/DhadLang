using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dhad.Core
{
    /// <summary>
    /// Represents a token identified by the Lexer.
    /// Contains the type, the original text (lexeme), any literal value,
    /// and the line number where it appeared.
    /// </summary>
    public class Token
    {
        public TokenType Type { get; }
        public string Lexeme { get; }
        public object? Literal { get; } // Use nullable object for literals
        public int Line { get; }

        public Token(TokenType type, string lexeme, object? literal, int line)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = line;
        }

        public override string ToString()
        {
            string literalStr = Literal == null ? "null" : Literal.ToString() ?? "null";
            if (Literal is string) // Ensure strings are quoted in output
            {
                literalStr = $"\"{literalStr}\"";
            }
            return $"[{Line}] {Type}: '{Lexeme}' ({literalStr})";
        }
    }
}
