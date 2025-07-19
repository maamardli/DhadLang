using System;
using System.Collections.Generic;
using System.Linq;
using Dhad.Core;

namespace Dhad.Core
{
    /// <summary>
    /// The Parser transforms a list of tokens into an Abstract Syntax Tree (AST)
    /// Think of it as organizing words into meaningful sentences with proper grammar
    /// </summary>
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _current = 0;
        private readonly List<ParseError> _errors = new List<ParseError>();

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        /// <summary>
        /// Parse the entire program and return a list of statements
        /// </summary>
        public List<StatementNode> Parse()
        {
            List<StatementNode> statements = new List<StatementNode>();
            
            while (!IsAtEnd())
            {
                try
                {
                    var stmt = Declaration();
                    if (stmt != null)
                        statements.Add(stmt);
                }
                catch (ParseError error)
                {
                    _errors.Add(error);
                    Synchronize(); // Try to recover and continue parsing
                }
            }
            
            return statements;
        }

        public List<ParseError> GetErrors() => _errors;

        #region Statement Parsing

        // Top level: Check for declarations or regular statements
        private StatementNode? Declaration()
        {
            try
            {
                // Variable declaration: متغير اسم = قيمة
                if (Match(TokenType.Keyword_Mutaghayyer))
                    return VariableDeclaration();
                
                // Constant declaration: ثابت اسم = قيمة
                if (Match(TokenType.Keyword_Thabit))
                    return ConstantDeclaration();
                
                // Array declaration: مصفوفة اسم[حجم]
                if (Match(TokenType.Keyword_Masfoofa))
                    return ArrayDeclaration();
                
                // Function declaration: دالة اسم(معاملات)
                if (Match(TokenType.Keyword_Dalla))
                    return FunctionDeclaration();
                
                return Statement();
            }
            catch (ParseError)
            {
                throw; // Re-throw to be caught by Parse()
            }
        }

        private StatementNode VariableDeclaration()
        {
            Token name = Consume(TokenType.Identifier, "توقعت اسم المتغير");
            
            ExpressionNode? initializer = null;
            if (Match(TokenType.Equal))
            {
                initializer = Expression();
            }
            
            return new VariableDeclarationNode(name, initializer);
        }

        private StatementNode ConstantDeclaration()
        {
            Token name = Consume(TokenType.Identifier, "توقعت اسم الثابت");
            Consume(TokenType.Equal, "الثابت يجب أن يكون له قيمة أولية");
            ExpressionNode initializer = Expression();
            
            return new ConstantDeclarationNode(name, initializer);
        }

        private StatementNode ArrayDeclaration()
        {
            Token name = Consume(TokenType.Identifier, "توقعت اسم المصفوفة");
            
            // Parse dimensions [size] or [size1, size2]
            Consume(TokenType.LeftBracket, "توقعت '[' بعد اسم المصفوفة");
            List<ExpressionNode> dimensions = new List<ExpressionNode>();
            dimensions.Add(Expression());
            
            while (Match(TokenType.Comma))
            {
                dimensions.Add(Expression());
            }
            
            Consume(TokenType.RightBracket, "توقعت ']' بعد أبعاد المصفوفة");
            
            // Optional initialization
            List<ExpressionNode>? initializer = null;
            if (Match(TokenType.Equal))
            {
                initializer = new List<ExpressionNode>();
                initializer.Add(Expression());
                
                while (Match(TokenType.Comma))
                {
                    initializer.Add(Expression());
                }
            }
            
            return new ArrayDeclarationNode(name, dimensions, initializer);
        }

        private StatementNode FunctionDeclaration()
        {
            Token name = Consume(TokenType.Identifier, "توقعت اسم الدالة");
            
            Consume(TokenType.LeftParen, "توقعت '(' بعد اسم الدالة");
            List<Token> parameters = new List<Token>();
            
            if (!Check(TokenType.RightParen))
            {
                do
                {
                    parameters.Add(Consume(TokenType.Identifier, "توقعت اسم المعامل"));
                } while (Match(TokenType.Comma));
            }
            
            Consume(TokenType.RightParen, "توقعت ')' بعد المعاملات");
            
            // Parse function body until نهاية الدالة
            List<StatementNode> body = new List<StatementNode>();
            
            while (!Check(TokenType.Keyword_Nihaya_Dalla) && !IsAtEnd())
            {
                // Check for compound keywords like "نهاية الدالة"
                if (Check(TokenType.Identifier) && Peek().Lexeme == "نهاية")
                {
                    Token next = PeekNext();
                    if (next != null && next.Type == TokenType.Identifier && next.Lexeme == "الدالة")
                    {
                        Advance(); // Skip "نهاية"
                        Advance(); // Skip "الدالة"
                        break;
                    }
                }
                
                body.Add(Declaration());
            }
            
            return new FunctionDeclarationNode(name, parameters, body);
        }

        // Parse different types of statements
        private StatementNode Statement()
        {
            // Print statement: أظهر تعبير
            if (Match(TokenType.Keyword_Azher))
                return PrintStatement();
            
            // Input statement: أدخل متغير
            if (Match(TokenType.Keyword_Adkhel))
                return InputStatement();
            
            // If statement: إذا شرط فإنه
            if (Match(TokenType.Keyword_Itha))
                return IfStatement();
            
            // While loop: بينما شرط كرر
            if (Match(TokenType.Keyword_Baynama))
                return WhileStatement();
            
            // For loop: من متغير = بداية إلى نهاية
            if (Match(TokenType.Keyword_Min))
                return ForStatement();
            
            // Return statement: أرجع قيمة
            if (Match(TokenType.Keyword_Arjea))
                return ReturnStatement();
            
            // Drawing statements
            if (Match(TokenType.Keyword_Irsem))
                return DrawStatement();
            
            if (Match(TokenType.Keyword_AlQalam))
                return PenStatement();
            
            if (Match(TokenType.Keyword_Nafitha))
                return WindowStatement();
            
            if (Match(TokenType.Keyword_Masah))
                return ClearScreenStatement();
            
            // Default: expression statement (assignment, function call, etc.)
            return ExpressionStatement();
        }

        private StatementNode PrintStatement()
        {
            List<ExpressionNode> expressions = new List<ExpressionNode>();
            expressions.Add(Expression());
            
            // Handle multiple expressions separated by commas
            while (Match(TokenType.Comma))
            {
                expressions.Add(Expression());
            }
            
            // Optional newline with سطر
            if (Match(TokenType.Keyword_Sater))
            {
                // Add empty string to force newline
                expressions.Add(new StringLiteralNode(""));
            }
            
            return new PrintStatementNode(expressions);
        }

        private StatementNode InputStatement()
        {
            List<Token> variables = new List<Token>();
            variables.Add(Consume(TokenType.Identifier, "توقعت اسم المتغير للإدخال"));
            
            while (Match(TokenType.Comma))
            {
                variables.Add(Consume(TokenType.Identifier, "توقعت اسم المتغير"));
            }
            
            return new InputStatementNode(variables);
        }

        private StatementNode IfStatement()
        {
            ExpressionNode condition = Expression();
            
            Consume(TokenType.Keyword_FaInnaho, "توقعت 'فإنه' بعد شرط إذا");
            
            // Parse the "then" branch - could be a single statement or a block
            List<StatementNode> thenStatements = new List<StatementNode>();
            
            // Keep parsing statements until we hit وإلا or نهاية إذا
            while (!Check(TokenType.Keyword_WaIlla) && !CheckEndIf() && !IsAtEnd())
            {
                thenStatements.Add(Declaration());
            }
            
            StatementNode thenBranch = new BlockStatementNode(thenStatements);
            
            // Check for else branch
            StatementNode? elseBranch = null;
            if (Match(TokenType.Keyword_WaIlla))
            {
                List<StatementNode> elseStatements = new List<StatementNode>();
                
                while (!CheckEndIf() && !IsAtEnd())
                {
                    elseStatements.Add(Declaration());
                }
                
                elseBranch = new BlockStatementNode(elseStatements);
            }
            
            // Consume "نهاية إذا"
            ConsumeEndIf();
            
            return new IfStatementNode(condition, thenBranch, elseBranch);
        }

        private StatementNode WhileStatement()
        {
            ExpressionNode condition = Expression();
            
            List<StatementNode> bodyStatements = new List<StatementNode>();
            
            // Parse body until كرر
            while (!Check(TokenType.Keyword_Karrer) && !IsAtEnd())
            {
                bodyStatements.Add(Declaration());
            }
            
            Consume(TokenType.Keyword_Karrer, "توقعت 'كرر' في نهاية حلقة بينما");
            
            return new WhileStatementNode(condition, new BlockStatementNode(bodyStatements));
        }

        private StatementNode ForStatement()
        {
            Token variable = Consume(TokenType.Identifier, "توقعت اسم متغير الحلقة");
            Consume(TokenType.Equal, "توقعت '=' بعد متغير الحلقة");
            ExpressionNode start = Expression();
            
            Consume(TokenType.Keyword_Ila, "توقعت 'إلى' في حلقة من");
            ExpressionNode end = Expression();
            
            // Optional step
            ExpressionNode? step = null;
            if (Match(TokenType.Keyword_Bekhotwa))
            {
                step = Expression();
            }
            
            List<StatementNode> bodyStatements = new List<StatementNode>();
            
            // Parse body until كرر
            while (!Check(TokenType.Keyword_Karrer) && !IsAtEnd())
            {
                bodyStatements.Add(Declaration());
            }
            
            Consume(TokenType.Keyword_Karrer, "توقعت 'كرر' في نهاية الحلقة");
            
            return new ForStatementNode(variable, start, end, step, new BlockStatementNode(bodyStatements));
        }

        private StatementNode ReturnStatement()
        {
            Token keyword = Previous();
            ExpressionNode? value = null;
            
            // Check if there's a return value
            if (!Check(TokenType.EOF) && !IsStatementEnd())
            {
                value = Expression();
            }
            
            return new ReturnStatementNode(keyword, value);
        }

        private StatementNode DrawStatement()
        {
            // ارسم شكل معاملات
            Token shape = Consume(TokenType.Identifier, "توقعت نوع الشكل (نقطة، خط، دائرة، مستطيل، نص)");
            
            // Validate shape type
            if (shape.Type != TokenType.Keyword_Noqta &&
                shape.Type != TokenType.Keyword_Khat &&
                shape.Type != TokenType.Keyword_Daera &&
                shape.Type != TokenType.Keyword_Mostateel &&
                shape.Type != TokenType.Keyword_Nas)
            {
                // Check if it's actually one of the shape keywords but lexer didn't recognize it
                if (shape.Lexeme == "نقطة" || shape.Lexeme == "خط" || 
                    shape.Lexeme == "دائرة" || shape.Lexeme == "مستطيل" || shape.Lexeme == "نص")
                {
                    // Convert to appropriate token type
                    shape = new Token(GetShapeTokenType(shape.Lexeme), shape.Lexeme, null, shape.Line);
                }
                else
                {
                    throw Error(shape, "نوع شكل غير معروف");
                }
            }
            
            // Parse arguments
            List<ExpressionNode> arguments = new List<ExpressionNode>();
            
            // Each shape has different number of required arguments
            int minArgs = GetMinArgsForShape(shape.Type);
            
            for (int i = 0; i < minArgs; i++)
            {
                if (i > 0) Consume(TokenType.Comma, "توقعت ',' بين المعاملات");
                arguments.Add(Expression());
            }
            
            // Check for optional arguments
            while (Match(TokenType.Comma))
            {
                arguments.Add(Expression());
            }
            
            return new DrawStatementNode(shape, arguments);
        }

        private StatementNode PenStatement()
        {
            // القلم خاصية = قيمة
            Token property = Consume(TokenType.Identifier, "توقعت خاصية القلم (لونه، عرضه)");
            
            // Validate property
            if (property.Lexeme != "لونه" && property.Lexeme != "عرضه")
            {
                throw Error(property, "خاصية قلم غير معروفة");
            }
            
            // Convert to token type if needed
            if (property.Type == TokenType.Identifier)
            {
                TokenType type = property.Lexeme == "لونه" ? TokenType.Keyword_Lawnaho : TokenType.Keyword_Ardoho;
                property = new Token(type, property.Lexeme, null, property.Line);
            }
            
            Consume(TokenType.Equal, "توقعت '=' بعد خاصية القلم");
            ExpressionNode value = Expression();
            
            return new PenStatementNode(property, value);
        }

        private StatementNode WindowStatement()
        {
            // نافذة خاصية = قيمة
            Token property = Consume(TokenType.Identifier, "توقعت خاصية النافذة (طولها، عرضها)");
            
            // Validate property
            if (property.Lexeme != "طولها" && property.Lexeme != "عرضها")
            {
                throw Error(property, "خاصية نافذة غير معروفة");
            }
            
            // Convert to token type if needed
            if (property.Type == TokenType.Identifier)
            {
                TokenType type = property.Lexeme == "طولها" ? TokenType.Keyword_Tooloha : TokenType.Keyword_Ardoho;
                property = new Token(type, property.Lexeme, null, property.Line);
            }
            
            Consume(TokenType.Equal, "توقعت '=' بعد خاصية النافذة");
            ExpressionNode value = Expression();
            
            return new WindowStatementNode(property, value);
        }

        private StatementNode ClearScreenStatement()
        {
            return new ClearScreenStatementNode(Previous());
        }

        private StatementNode ExpressionStatement()
        {
            ExpressionNode expr = Expression();
            return new ExpressionStatementNode(expr);
        }

        #endregion

        #region Expression Parsing (Precedence climbing)

        private ExpressionNode Expression()
        {
            return Or();
        }

        // Logical OR: تعبير أو تعبير
        private ExpressionNode Or()
        {
            ExpressionNode expr = And();
            
            while (Match(TokenType.Keyword_Aw))
            {
                Token op = Previous();
                ExpressionNode right = And();
                expr = new BinaryExpressionNode(expr, op, right);
            }
            
            return expr;
        }

        // Logical AND: تعبير و تعبير
        private ExpressionNode And()
        {
            ExpressionNode expr = Equality();
            
            while (Match(TokenType.Keyword_Wa))
            {
                Token op = Previous();
                ExpressionNode right = Equality();
                expr = new BinaryExpressionNode(expr, op, right);
            }
            
            return expr;
        }

        // Equality: == or <> or !=
        private ExpressionNode Equality()
        {
            ExpressionNode expr = Comparison();
            
            while (Match(TokenType.Equal, TokenType.NotEqual))
            {
                Token op = Previous();
                ExpressionNode right = Comparison();
                expr = new BinaryExpressionNode(expr, op, right);
            }
            
            return expr;
        }

        // Comparison: > >= < <=
        private ExpressionNode Comparison()
        {
            ExpressionNode expr = Term();
            
            while (Match(TokenType.Greater, TokenType.GreaterEqual, 
                         TokenType.Less, TokenType.LessEqual))
            {
                Token op = Previous();
                ExpressionNode right = Term();
                expr = new BinaryExpressionNode(expr, op, right);
            }
            
            return expr;
        }

        // Addition and subtraction
        private ExpressionNode Term()
        {
            ExpressionNode expr = Factor();
            
            while (Match(TokenType.Plus, TokenType.Minus))
            {
                Token op = Previous();
                ExpressionNode right = Factor();
                expr = new BinaryExpressionNode(expr, op, right);
            }
            
            return expr;
        }

        // Multiplication, division, modulo
        private ExpressionNode Factor()
        {
            ExpressionNode expr = Power();
            
            while (Match(TokenType.Star, TokenType.Slash, TokenType.Percent))
            {
                Token op = Previous();
                ExpressionNode right = Power();
                expr = new BinaryExpressionNode(expr, op, right);
            }
            
            return expr;
        }

        // Exponentiation: رقم ^ رقم
        private ExpressionNode Power()
        {
            ExpressionNode expr = Unary();
            
            if (Match(TokenType.Caret))
            {
                Token op = Previous();
                // Right associative
                ExpressionNode right = Power();
                expr = new BinaryExpressionNode(expr, op, right);
            }
            
            return expr;
        }

        // Unary: - تعبير or نفي تعبير
        private ExpressionNode Unary()
        {
            if (Match(TokenType.Minus, TokenType.Keyword_Nafi))
            {
                Token op = Previous();
                ExpressionNode right = Unary();
                return new UnaryExpressionNode(op, right);
            }
            
            return Call();
        }

        // Function calls and array indexing
        private ExpressionNode Call()
        {
            ExpressionNode expr = Primary();
            
            while (true)
            {
                if (Match(TokenType.LeftParen))
                {
                    expr = FinishCall(expr);
                }
                else if (Match(TokenType.LeftBracket))
                {
                    expr = FinishArrayIndex(expr);
                }
                else
                {
                    break;
                }
            }
            
            return expr;
        }

        private ExpressionNode FinishCall(ExpressionNode callee)
        {
            List<ExpressionNode> arguments = new List<ExpressionNode>();
            
            if (!Check(TokenType.RightParen))
            {
                do
                {
                    arguments.Add(Expression());
                } while (Match(TokenType.Comma));
            }
            
            Token paren = Consume(TokenType.RightParen, "توقعت ')' بعد المعاملات");
            
            return new FunctionCallNode(callee, paren, arguments);
        }

        private ExpressionNode FinishArrayIndex(ExpressionNode array)
        {
            Token bracket = Previous();
            List<ExpressionNode> indices = new List<ExpressionNode>();
            
            indices.Add(Expression());
            
            while (Match(TokenType.Comma))
            {
                indices.Add(Expression());
            }
            
            Consume(TokenType.RightBracket, "توقعت ']' بعد فهرس المصفوفة");
            
            return new ArrayIndexNode(array, bracket, indices);
        }

        // Primary expressions: literals, variables, grouped expressions
        private ExpressionNode Primary()
        {
            // Boolean literals
            if (Match(TokenType.Keyword_Sah))
            {
                return new BooleanLiteralNode(true);
            }
            
            if (Match(TokenType.Keyword_Khata))
            {
                return new BooleanLiteralNode(false);
            }
            
            // Number literal
            if (Match(TokenType.Number))
            {
                return new NumberLiteralNode((double)Previous().Literal!);
            }
            
            // String literal
            if (Match(TokenType.String))
            {
                return new StringLiteralNode((string)Previous().Literal!);
            }
            
            // Variable or built-in function/constant
            if (Match(TokenType.Identifier))
            {
                Token identifier = Previous();
                
                // Check for built-in math functions and constants
                if (IsBuiltinFunction(identifier.Lexeme) || IsBuiltinConstant(identifier.Lexeme))
                {
                    return new VariableNode(identifier);
                }
                
                return new VariableNode(identifier);
            }
            
            // Check for keyword that should be treated as identifier (built-ins)
            if (Check(TokenType.Keyword_Ja) || Check(TokenType.Keyword_Jata) || 
                Check(TokenType.Keyword_Za) || Check(TokenType.Keyword_Motlaq) ||
                Check(TokenType.Keyword_Sahih) || Check(TokenType.Keyword_Qarrab) ||
                Check(TokenType.Keyword_Jathr) || Check(TokenType.Keyword_Ashwaey) ||
                Check(TokenType.Keyword_Taa) || Check(TokenType.Keyword_ToolAlnas) ||
                Check(TokenType.Keyword_Alharf) || Check(TokenType.Keyword_JuzAlnas) ||
                Check(TokenType.Keyword_Alwaqt_W_Altareekh) || Check(TokenType.Keyword_Altareekh) ||
                Check(TokenType.Keyword_Alwaqt))
            {
                Token builtin = Advance();
                // Convert to identifier token for the variable node
                Token asIdentifier = new Token(TokenType.Identifier, builtin.Lexeme, null, builtin.Line);
                return new VariableNode(asIdentifier);
            }
            
            // Grouped expression
            if (Match(TokenType.LeftParen))
            {
                ExpressionNode expr = Expression();
                Consume(TokenType.RightParen, "توقعت ')' بعد التعبير");
                return expr;
            }
            
            throw Error(Peek(), "توقعت تعبير");
        }

        #endregion

        #region Helper Methods

        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd()) _current++;
            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().Type == TokenType.EOF;
        }

        private Token Peek()
        {
            return _tokens[_current];
        }

        private Token? PeekNext()
        {
            if (_current + 1 >= _tokens.Count) return null;
            return _tokens[_current + 1];
        }

        private Token Previous()
        {
            return _tokens[_current - 1];
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();
            throw Error(Peek(), message);
        }

        private ParseError Error(Token token, string message)
        {
            return new ParseError(token, message);
        }

        // Check for compound "نهاية إذا"
        private bool CheckEndIf()
        {
            if (!Check(TokenType.Identifier)) return false;
            if (Peek().Lexeme != "نهاية") return false;
            
            Token? next = PeekNext();
            return next != null && next.Type == TokenType.Keyword_Itha;
        }

        private void ConsumeEndIf()
        {
            if (Check(TokenType.Identifier) && Peek().Lexeme == "نهاية")
            {
                Advance(); // Skip "نهاية"
                Consume(TokenType.Keyword_Itha, "توقعت 'إذا' بعد 'نهاية'");
            }
            else
            {
                throw Error(Peek(), "توقعت 'نهاية إذا'");
            }
        }

        // Check if current position looks like a statement boundary
        private bool IsStatementEnd()
        {
            TokenType current = Peek().Type;
            return current == TokenType.Keyword_Azher ||
                   current == TokenType.Keyword_Adkhel ||
                   current == TokenType.Keyword_Itha ||
                   current == TokenType.Keyword_Baynama ||
                   current == TokenType.Keyword_Min ||
                   current == TokenType.Keyword_Arjea ||
                   current == TokenType.Keyword_Mutaghayyer ||
                   current == TokenType.Keyword_Thabit ||
                   current == TokenType.Keyword_Masfoofa ||
                   current == TokenType.Keyword_Dalla ||
                   current == TokenType.Keyword_Karrer ||
                   CheckEndIf() ||
                   current == TokenType.Keyword_Nihaya_Dalla ||
                   current == TokenType.Keyword_WaIlla;
        }

        // Error recovery: skip tokens until we find a likely statement start
        private void Synchronize()
        {
            Advance();
            
            while (!IsAtEnd())
            {
                if (IsStatementEnd()) return;
                Advance();
            }
        }

        // Helper to check if identifier is a built-in function
        private bool IsBuiltinFunction(string name)
        {
            return name == "جا" || name == "جتا" || name == "ظا" || 
                   name == "مطلق" || name == "صحيح" || name == "قرب" || 
                   name == "جذر" || name == "عشوائي" || name == "طول_النص" || 
                   name == "الحرف" || name == "جزء_النص" || 
                   name == "الوقت_و_التاريخ" || name == "التاريخ" || name == "الوقت";
        }

        // Helper to check if identifier is a built-in constant
        private bool IsBuiltinConstant(string name)
        {
            return name == "ط";
        }

        // Get the token type for a shape name
        private TokenType GetShapeTokenType(string shapeName)
        {
            return shapeName switch
            {
                "نقطة" => TokenType.Keyword_Noqta,
                "خط" => TokenType.Keyword_Khat,
                "دائرة" => TokenType.Keyword_Daera,
                "مستطيل" => TokenType.Keyword_Mostateel,
                "نص" => TokenType.Keyword_Nas,
                _ => TokenType.Identifier
            };
        }

        // Get minimum required arguments for each shape
        private int GetMinArgsForShape(TokenType shapeType)
        {
            return shapeType switch
            {
                TokenType.Keyword_Noqta => 2,      // x, y
                TokenType.Keyword_Khat => 4,       // x1, y1, x2, y2
                TokenType.Keyword_Daera => 3,      // x, y, radius
                TokenType.Keyword_Mostateel => 4,  // x, y, width, height
                TokenType.Keyword_Nas => 3,        // x, y, text
                _ => 0
            };
        }

        #endregion
    }

    /// <summary>
    /// Represents a parsing error with location information
    /// </summary>
    public class ParseError : Exception
    {
        public Token Token { get; }
        
        public ParseError(Token token, string message) 
            : base($"خطأ في السطر {token.Line} عند '{token.Lexeme}': {message}")
        {
            Token = token;
        }
    }
}
