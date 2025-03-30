using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization; // Required for CultureInfo.InvariantCulture

namespace Dhad.Core
{
    /// <summary>
    /// Scans Dhad source code and produces a list of Tokens.
    /// </summary>
    public class Lexer
    {
        private readonly string _source;
        private readonly List<Token> _tokens = new List<Token>();
        private int _start = 0;     // Index of the first character in the lexeme being scanned
        private int _current = 0;   // Index of the character currently being considered
        private int _line = 1;      // Current line number for error reporting

        // Dictionary to hold keywords for efficient lookup
        private static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>()
        {
            {"أدخل", TokenType.Keyword_Adkhel},
            {"مصفوفة", TokenType.Keyword_Masfoofa},
            {"ثابت", TokenType.Keyword_Thabit},
            {"متغير", TokenType.Keyword_Mutaghayyer},
            {"أظهر", TokenType.Keyword_Azher},
            {"إذا", TokenType.Keyword_Itha},
            {"كرر", TokenType.Keyword_Karrer}, // Note: Used for both FOR and WHILE end
            {"بينما", TokenType.Keyword_Baynama},
            {"إلى", TokenType.Keyword_Ila},
            {"من", TokenType.Keyword_Min},
            //{"نهاية", TokenType.Keyword_Nihaya}, // Too ambiguous, use specific endings
            {"نهاية إذا", TokenType.Keyword_Nihaya_Itha}, // Needs special handling if multi-word
            {"نهاية الدالة", TokenType.Keyword_Nihaya_Dalla}, // Needs special handling
            {"دالة", TokenType.Keyword_Dalla},
            {"بخطوة", TokenType.Keyword_Bekhotwa},
            {"وإلا", TokenType.Keyword_WaIlla},
            {"فإنه", TokenType.Keyword_FaInnaho},
            {"سطر", TokenType.Keyword_Sater},
            {"القلم", TokenType.Keyword_AlQalam},
            {"نافذة", TokenType.Keyword_Nafitha},
            {"ارسم", TokenType.Keyword_Irsem},
            {"أرجع", TokenType.Keyword_Arjea},
            {"و", TokenType.Keyword_Wa},
            {"أو", TokenType.Keyword_Aw},
            {"نفي", TokenType.Keyword_Nafi},
            {"صح", TokenType.Keyword_Sah},
            {"خطأ", TokenType.Keyword_Khata},
            {"نقطة", TokenType.Keyword_Noqta},
            {"خط", TokenType.Keyword_Khat},
            {"دائرة", TokenType.Keyword_Daera},
            {"مستطيل", TokenType.Keyword_Mostateel},
            {"نص", TokenType.Keyword_Nas}, // Can be shape or general text
            {"لونه", TokenType.Keyword_Lawnaho},
            {"عرضه", TokenType.Keyword_Ardoho},
            {"طولها", TokenType.Keyword_Tooloha},
            {"مسح", TokenType.Keyword_Masah},
            {"جا", TokenType.Keyword_Ja},
            {"جتا", TokenType.Keyword_Jata},
            {"ظا", TokenType.Keyword_Za},
            {"مطلق", TokenType.Keyword_Motlaq},
            {"صحيح", TokenType.Keyword_Sahih},
            {"قرب", TokenType.Keyword_Qarrab},
            {"جذر", TokenType.Keyword_Jathr},
            {"ناتج", TokenType.Keyword_Natej},
            {"عشوائي", TokenType.Keyword_Ashwaey},
            {"ط", TokenType.Keyword_Taa},
            {"طول_النص", TokenType.Keyword_ToolAlnas}, // Using underscore for multi-word builtins
            {"الحرف", TokenType.Keyword_Alharf},
            {"جزء_النص", TokenType.Keyword_JuzAlnas},
            {"الوقت_و_التاريخ", TokenType.Keyword_Alwaqt_W_Altareekh},
            {"التاريخ", TokenType.Keyword_Altareekh},
            {"الوقت", TokenType.Keyword_Alwaqt}
        };


        public Lexer(string source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        /// <summary>
        /// Scans the entire source code and returns a list of tokens.
        /// </summary>
        public List<Token> ScanTokens()
        {
            _tokens.Clear(); // Ensure list is empty if called multiple times
            _start = 0;
            _current = 0;
            _line = 1;

            while (!IsAtEnd())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, "", null, _line));
            return _tokens;
        }

        private bool IsAtEnd()
        {
            return _current >= _source.Length;
        }

        /// <summary>
        /// Scans the next token from the source code.
        /// This is the core logic of the lexer.
        /// </summary>
        private void ScanToken()
        {
            char c = Advance();

            switch (c)
            {
                // Single character tokens
                case '(': AddToken(TokenType.LeftParen); break;
                case ')': AddToken(TokenType.RightParen); break;
                case '[': AddToken(TokenType.LeftBracket); break;
                case ']': AddToken(TokenType.RightBracket); break;
                case ',': AddToken(TokenType.Comma); break;
                case '.': AddToken(TokenType.Dot); break;
                case '+': AddToken(TokenType.Plus); break;
                case '-': AddToken(TokenType.Minus); break;
                case '*': AddToken(TokenType.Star); break;
                case '%': AddToken(TokenType.Percent); break;
                case '^': AddToken(TokenType.Caret); break;

                // Operators (potentially two characters)
                case '=': AddToken(TokenType.Equal); break; // Assumes '=' is assignment/equality, adjust if needed
                case '<': AddToken(Match('=') ? TokenType.LessEqual : (Match('>') ? TokenType.NotEqual : TokenType.Less)); break;
                case '>': AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater); break;

                // Slash or Comments (Handle Dhad comments '؟؟' and '؟؟؟')
                case '/': AddToken(TokenType.Slash); break; // Simple slash for division

                case '؟': // Potential start of a comment
                    if (Match('؟'))
                    {
                        if (Match('؟')) // Start of multi-line comment '؟؟؟'
                        {
                            // Consume until the closing '؟؟؟' or EOF
                            while (!(Peek() == '؟' && PeekNext() == '؟' && PeekNextNext() == '؟') && !IsAtEnd())
                            {
                                if (Peek() == '\n') _line++;
                                Advance();
                            }
                            if (!IsAtEnd()) // Consume the closing '؟؟؟'
                            {
                                Advance(); Advance(); Advance();
                            }
                        }
                        else // Single line comment '؟؟'
                        {
                            // Consume until the end of the line
                            while (Peek() != '\n' && !IsAtEnd())
                            {
                                Advance();
                            }
                        }
                    }
                    else
                    {
                        // Handle error: Stray '؟' ? Or ignore?
                        // For now, ignoring. Add error reporting if needed.
                    }
                    break;


                // Whitespace (ignored)
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;

                case '\n':
                    _line++;
                    break;

                // String literals
                case '"': StringLiteral(); break;

                default:
                    if (IsDigit(c))
                    {
                        NumberLiteral();
                    }
                    else if (IsAlpha(c)) // Check if it starts with a letter (Arabic or _)
                    {
                        Identifier();
                    }
                    else
                    {
                        // Handle unrecognized character - Add error reporting
                        // Example: Error($"Unexpected character '{c}' on line {_line}.");
                    }
                    break;
            }
        }

        // --- Helper Methods ---

        private char Advance()
        {
            return _source[_current++];
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object? literal)
        {
            string text = _source.Substring(_start, _current - _start);
            _tokens.Add(new Token(type, text, literal, _line));
        }

        // Checks if the current character matches the expected one. Consumes if it matches.
        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (_source[_current] != expected) return false;

            _current++;
            return true;
        }

        // Looks ahead one character without consuming.
        private char Peek()
        {
            if (IsAtEnd()) return '\0'; // Null character signifies end
            return _source[_current];
        }

        // Looks ahead two characters.
        private char PeekNext()
        {
            if (_current + 1 >= _source.Length) return '\0';
            return _source[_current + 1];
        }
        // Looks ahead three characters. Needed for '؟؟؟'
        private char PeekNextNext()
        {
            if (_current + 2 >= _source.Length) return '\0';
            return _source[_current + 2];
        }

        private void StringLiteral()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') _line++; // Allow multi-line strings
                Advance();
            }

            if (IsAtEnd())
            {
                // Handle error: Unterminated string.
                // Example: Error($"Unterminated string on line {_line}.");
                return;
            }

            // Consume the closing ".
            Advance();

            // Get the string value without the surrounding quotes.
            string value = _source.Substring(_start + 1, _current - _start - 2);
            AddToken(TokenType.String, value);
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private void NumberLiteral()
        {
            while (IsDigit(Peek())) Advance();

            // Look for a fractional part.
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                // Consume the "."
                Advance();

                while (IsDigit(Peek())) Advance();
            }
            // Using InvariantCulture to ensure '.' is the decimal separator
            string numberString = _source.Substring(_start, _current - _start);
            double value = double.Parse(numberString, CultureInfo.InvariantCulture);
            AddToken(TokenType.Number, value);
        }

        // Identifiers can start with an Arabic letter or underscore
        private bool IsAlpha(char c)
        {
            // Basic check for underscore or common Arabic letter ranges
            // This might need refinement for full Unicode Arabic support
            return c == '_' ||
                   (c >= '\u0621' && c <= '\u064A'); // Common Arabic letters range
                                                     // Potentially add English letters too if desired: || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')

        }

        // Subsequent characters can be letters or digits
        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            // Check if the identifier is a reserved keyword.
            string text = _source.Substring(_start, _current - _start);

            // Handle multi-word keywords like "نهاية إذا" - This requires lookahead or post-processing
            // Simple approach: Lexer outputs separate tokens "نهاية", "إذا" and parser combines them.
            // More complex lexer could peek ahead. Let's use the simpler approach for now.
            // Special case needed during parsing for `Keyword_Nihaya` followed by `Keyword_Itha` etc.

            // Check if the identifier matches a keyword
            if (Keywords.TryGetValue(text, out TokenType type))
            {
                // Handle boolean literals specifically
                if (type == TokenType.Keyword_Sah) AddToken(type, true);
                else if (type == TokenType.Keyword_Khata) AddToken(type, false);
                else AddToken(type);
            }
            else
            {
                // Not a keyword, so it's a user-defined identifier
                AddToken(TokenType.Identifier);
            }
        }

        // Basic error handling placeholder
        private void Error(string message)
        {
            Console.Error.WriteLine($"Lexer Error: {message}"); // Replace with proper error reporting later
        }
    }
}