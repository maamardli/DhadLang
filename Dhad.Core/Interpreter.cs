using System;
using System.Collections.Generic;
using System.Linq;
using Dhad.Core;

namespace Dhad.Core
{
    /// <summary>
    /// The interpreter executes Dhad programs by walking through the AST (Abstract Syntax Tree)
    /// Think of it as a tour guide that visits each part of your program and makes things happen
    /// </summary>
    public class Interpreter : IAstVisitor<object?>
    {
        // This is like a notebook where we keep track of all variables and their values
        private readonly Environment _globalEnv;
        private Environment _currentEnv;
        
        // We need a way to communicate with the outside world (show text, draw graphics)
        private readonly IOutput _output;
        private readonly IGraphics _graphics;
        
        // Keep track of whether we've started drawing (so we can show the graphics window)
        private bool _hasUsedGraphics = false;

        public Interpreter(IOutput output, IGraphics graphics)
        {
            _output = output;
            _graphics = graphics;
            _globalEnv = new Environment();
            _currentEnv = _globalEnv;
            
            // Add built-in math functions that children can use
            DefineBuiltins();
        }

        // This is our main entry point - interpret a complete program
        public void Interpret(List<StatementNode> statements)
        {
            try
            {
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                _output.WriteLine($"خطأ: {error.Message}");
            }
        }

        // Helper method to execute a statement and handle return values
        private object? Execute(StatementNode stmt)
        {
            return stmt.Accept(this);
        }

        // Helper method to evaluate an expression and get its value
        private object? Evaluate(ExpressionNode expr)
        {
            return expr.Accept(this);
        }

        #region Expression Visitors

        public object? VisitNumberLiteral(NumberLiteralNode node)
        {
            // Numbers are simple - just return their value
            return node.Value;
        }

        public object? VisitStringLiteral(StringLiteralNode node)
        {
            // Strings are also simple - return the text
            return node.Value;
        }

        public object? VisitBooleanLiteral(BooleanLiteralNode node)
        {
            // Boolean values: صح (true) or خطأ (false)
            return node.Value;
        }

        public object? VisitVariable(VariableNode node)
        {
            // Look up the variable's value in our environment
            return _currentEnv.Get(node.Name);
        }

        public object? VisitAssignment(AssignmentNode node)
        {
            // Calculate the new value and store it
            object? value = Evaluate(node.Value);
            _currentEnv.Assign(node.Name, value);
            return value;
        }

        public object? VisitBinaryExpression(BinaryExpressionNode node)
        {
            // Get values from both sides of the operation
            object? left = Evaluate(node.Left);
            object? right = Evaluate(node.Right);

            // Handle different operators
            switch (node.Operator.Type)
            {
                // Math operations
                case TokenType.Plus:
                    return Add(left, right); // Special handling for string concatenation too
                case TokenType.Minus:
                    return ToNumber(left) - ToNumber(right);
                case TokenType.Star:
                    return ToNumber(left) * ToNumber(right);
                case TokenType.Slash:
                    {
                        double divisor = ToNumber(right);
                        if (divisor == 0) throw new RuntimeError("لا يمكن القسمة على صفر");
                        return ToNumber(left) / divisor;
                    }
                case TokenType.Percent:
                    return ToNumber(left) % ToNumber(right);
                case TokenType.Caret:
                    return Math.Pow(ToNumber(left), ToNumber(right));

                // Comparison operations
                case TokenType.Greater:
                    return ToNumber(left) > ToNumber(right);
                case TokenType.GreaterEqual:
                    return ToNumber(left) >= ToNumber(right);
                case TokenType.Less:
                    return ToNumber(left) < ToNumber(right);
                case TokenType.LessEqual:
                    return ToNumber(left) <= ToNumber(right);
                case TokenType.Equal:
                    return IsEqual(left, right);
                case TokenType.NotEqual:
                    return !IsEqual(left, right);

                // Logical operations
                case TokenType.Keyword_Wa: // و (and)
                    return IsTruthy(left) && IsTruthy(right);
                case TokenType.Keyword_Aw: // أو (or)
                    return IsTruthy(left) || IsTruthy(right);

                default:
                    throw new RuntimeError($"عملية غير معروفة: {node.Operator.Lexeme}");
            }
        }

        public object? VisitUnaryExpression(UnaryExpressionNode node)
        {
            object? right = Evaluate(node.Right);

            switch (node.Operator.Type)
            {
                case TokenType.Minus:
                    return -ToNumber(right);
                case TokenType.Keyword_Nafi: // نفي (not)
                    return !IsTruthy(right);
                default:
                    throw new RuntimeError($"عملية أحادية غير معروفة: {node.Operator.Lexeme}");
            }
        }

        public object? VisitFunctionCall(FunctionCallNode node)
        {
            // Get the function to call
            object? callee = Evaluate(node.Callee);
            
            // Evaluate all arguments
            List<object?> arguments = new List<object?>();
            foreach (var arg in node.Arguments)
            {
                arguments.Add(Evaluate(arg));
            }

            // Call the function
            if (callee is ICallable function)
            {
                if (arguments.Count != function.Arity && function.Arity >= 0)
                {
                    throw new RuntimeError($"الدالة {((VariableNode)node.Callee).Name.Lexeme} تحتاج {function.Arity} معامل، لكن تم إعطاء {arguments.Count}");
                }
                return function.Call(this, arguments);
            }

            throw new RuntimeError("يمكن استدعاء الدوال فقط");
        }

        public object? VisitArrayIndex(ArrayIndexNode node)
        {
            // Get the array
            object? array = Evaluate(node.Array);
            
            if (array is List<object?> list)
            {
                // For now, support single index only
                if (node.Indices.Count != 1)
                {
                    throw new RuntimeError("المصفوفات أحادية البعد فقط مدعومة حالياً");
                }
                
                int index = (int)ToNumber(Evaluate(node.Indices[0]));
                if (index < 0 || index >= list.Count)
                {
                    throw new RuntimeError($"الفهرس {index} خارج حدود المصفوفة");
                }
                
                return list[index];
            }
            
            throw new RuntimeError("محاولة الوصول لفهرس في غير مصفوفة");
        }

        #endregion

        #region Statement Visitors

        public object? VisitExpressionStatement(ExpressionStatementNode node)
        {
            // Just evaluate the expression (like a function call or assignment)
            Evaluate(node.Expression);
            return null;
        }

        public object? VisitVariableDeclaration(VariableDeclarationNode node)
        {
            // Create a new variable, optionally with an initial value
            object? value = null;
            if (node.Initializer != null)
            {
                value = Evaluate(node.Initializer);
            }
            
            _currentEnv.Define(node.Name.Lexeme, value, false);
            return null;
        }

        public object? VisitConstantDeclaration(ConstantDeclarationNode node)
        {
            // Constants must have a value and cannot be changed later
            object? value = Evaluate(node.Initializer);
            _currentEnv.Define(node.Name.Lexeme, value, true);
            return null;
        }

        public object? VisitArrayDeclaration(ArrayDeclarationNode node)
        {
            // Create an array with specified dimensions
            // For simplicity, start with 1D arrays
            if (node.Dimensions.Count != 1)
            {
                throw new RuntimeError("المصفوفات أحادية البعد فقط مدعومة حالياً");
            }
            
            int size = (int)ToNumber(Evaluate(node.Dimensions[0]));
            List<object?> array = new List<object?>(size);
            
            // Initialize with zeros or provided values
            if (node.Initializer != null)
            {
                foreach (var initValue in node.Initializer)
                {
                    array.Add(Evaluate(initValue));
                }
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    array.Add(0.0);
                }
            }
            
            _currentEnv.Define(node.Name.Lexeme, array, false);
            return null;
        }

        public object? VisitFunctionDeclaration(FunctionDeclarationNode node)
        {
            // Create a function object and store it
            var function = new DhadFunction(node, _currentEnv);
            _currentEnv.Define(node.Name.Lexeme, function, false);
            return null;
        }

        public object? VisitReturnStatement(ReturnStatementNode node)
        {
            // Calculate return value and throw special exception to unwind
            object? value = null;
            if (node.Value != null)
            {
                value = Evaluate(node.Value);
            }
            throw new ReturnValue(value);
        }

        public object? VisitBlock(BlockStatementNode node)
        {
            // Create a new environment for this block (local variables)
            ExecuteBlock(node.Statements, new Environment(_currentEnv));
            return null;
        }

        public void ExecuteBlock(List<StatementNode> statements, Environment environment)
        {
            // Remember the previous environment
            Environment previous = _currentEnv;
            try
            {
                // Use the new environment for this block
                _currentEnv = environment;
                
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                // Always restore the previous environment
                _currentEnv = previous;
            }
        }

        public object? VisitIfStatement(IfStatementNode node)
        {
            // Check the condition and execute the appropriate branch
            if (IsTruthy(Evaluate(node.Condition)))
            {
                Execute(node.ThenBranch);
            }
            else if (node.ElseBranch != null)
            {
                Execute(node.ElseBranch);
            }
            return null;
        }

        public object? VisitWhileStatement(WhileStatementNode node)
        {
            // Keep executing the body while the condition is true
            while (IsTruthy(Evaluate(node.Condition)))
            {
                Execute(node.Body);
            }
            return null;
        }

        public object? VisitForStatement(ForStatementNode node)
        {
            // Traditional for loop with start, end, and optional step
            double start = ToNumber(Evaluate(node.StartValue));
            double end = ToNumber(Evaluate(node.EndValue));
            double step = 1;
            
            if (node.StepValue != null)
            {
                step = ToNumber(Evaluate(node.StepValue));
            }
            
            // Create new environment for loop variable
            Environment loopEnv = new Environment(_currentEnv);
            loopEnv.Define(node.Variable.Lexeme, start, false);
            
            Environment previous = _currentEnv;
            try
            {
                _currentEnv = loopEnv;
                
                // Handle both increasing and decreasing loops
                if (step > 0)
                {
                    while (ToNumber(_currentEnv.Get(node.Variable)) <= end)
                    {
                        Execute(node.Body);
                        double current = ToNumber(_currentEnv.Get(node.Variable));
                        _currentEnv.Assign(node.Variable, current + step);
                    }
                }
                else if (step < 0)
                {
                    while (ToNumber(_currentEnv.Get(node.Variable)) >= end)
                    {
                        Execute(node.Body);
                        double current = ToNumber(_currentEnv.Get(node.Variable));
                        _currentEnv.Assign(node.Variable, current + step);
                    }
                }
            }
            finally
            {
                _currentEnv = previous;
            }
            
            return null;
        }

        public object? VisitPrintStatement(PrintStatementNode node)
        {
            // Print each expression, separated by spaces
            List<string> parts = new List<string>();
            foreach (var expr in node.Expressions)
            {
                object? value = Evaluate(expr);
                parts.Add(Stringify(value));
            }
            
            _output.WriteLine(string.Join(" ", parts));
            return null;
        }

        public object? VisitInputStatement(InputStatementNode node)
        {
            // Read input for each variable
            foreach (var variable in node.Variables)
            {
                _output.Write($"أدخل قيمة {variable.Lexeme}: ");
                string? input = _output.ReadLine();
                
                // Try to parse as number, otherwise treat as string
                object? value = input;
                if (double.TryParse(input, out double number))
                {
                    value = number;
                }
                
                _currentEnv.Assign(variable, value);
            }
            return null;
        }

        #endregion

        #region Graphics Statement Visitors

        public object? VisitDrawStatement(DrawStatementNode node)
        {
            EnsureGraphicsVisible();
            
            // Get coordinates and other parameters
            List<double> args = node.Arguments.Select(arg => ToNumber(Evaluate(arg))).ToList();
            
            switch (node.Shape.Type)
            {
                case TokenType.Keyword_Noqta: // نقطة (point)
                    if (args.Count >= 2)
                        _graphics.DrawPoint((int)args[0], (int)args[1]);
                    break;
                    
                case TokenType.Keyword_Khat: // خط (line)
                    if (args.Count >= 4)
                        _graphics.DrawLine((int)args[0], (int)args[1], (int)args[2], (int)args[3]);
                    break;
                    
                case TokenType.Keyword_Daera: // دائرة (circle)
                    if (args.Count >= 3)
                    {
                        string? fillColor = args.Count >= 4 ? GetColorName((int)args[3]) : null;
                        _graphics.DrawCircle((int)args[0], (int)args[1], (int)args[2], fillColor);
                    }
                    break;
                    
                case TokenType.Keyword_Mostateel: // مستطيل (rectangle)
                    if (args.Count >= 4)
                    {
                        string? fillColor = args.Count >= 5 ? GetColorName((int)args[4]) : null;
                        _graphics.DrawRectangle((int)args[0], (int)args[1], (int)args[2], (int)args[3], fillColor);
                    }
                    break;
                    
                case TokenType.Keyword_Nas: // نص (text)
                    if (node.Arguments.Count >= 3)
                    {
                        double x = ToNumber(Evaluate(node.Arguments[0]));
                        double y = ToNumber(Evaluate(node.Arguments[1]));
                        string text = Stringify(Evaluate(node.Arguments[2]));
                        _graphics.DrawText((int)x, (int)y, text);
                    }
                    break;
            }
            
            return null;
        }

        public object? VisitPenStatement(PenStatementNode node)
        {
            EnsureGraphicsVisible();
            
            switch (node.Property.Type)
            {
                case TokenType.Keyword_Lawnaho: // لونه (its color)
                    string colorName = Stringify(Evaluate(node.Value));
                    _graphics.SetPenColor(colorName);
                    break;
                    
                case TokenType.Keyword_Ardoho: // عرضه (its width)
                    int width = (int)ToNumber(Evaluate(node.Value));
                    _graphics.SetPenWidth(width);
                    break;
            }
            
            return null;
        }

        public object? VisitWindowStatement(WindowStatementNode node)
        {
            EnsureGraphicsVisible();
            
            // Window properties would be handled by the IDE
            // For now, just ensure the graphics window is visible
            return null;
        }

        public object? VisitClearScreenStatement(ClearScreenStatementNode node)
        {
            EnsureGraphicsVisible();
            _graphics.ClearSurface();
            return null;
        }

        private void EnsureGraphicsVisible()
        {
            if (!_hasUsedGraphics)
            {
                _hasUsedGraphics = true;
                _graphics.EnsureVisible();
            }
        }

        #endregion

        #region Helper Methods

        // Convert a value to a number, with helpful error messages
        private double ToNumber(object? value)
        {
            if (value is double d) return d;
            if (value is int i) return i;
            if (value is bool b) return b ? 1 : 0;
            throw new RuntimeError($"لا يمكن تحويل {Stringify(value)} إلى رقم");
        }

        // Check if a value is "truthy" (considered true in conditions)
        private bool IsTruthy(object? value)
        {
            if (value == null) return false;
            if (value is bool b) return b;
            if (value is double d) return d != 0;
            if (value is string s) return s.Length > 0;
            return true;
        }

        // Check if two values are equal
        private bool IsEqual(object? a, object? b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;
            return a.Equals(b);
        }

        // Add two values (handles both numbers and string concatenation)
        private object? Add(object? left, object? right)
        {
            if (left is double l && right is double r)
                return l + r;
            
            if (left is string || right is string)
                return Stringify(left) + Stringify(right);
                
            return ToNumber(left) + ToNumber(right);
        }

        // Convert any value to a string for display
        private string Stringify(object? value)
        {
            if (value == null) return "عدم";
            if (value is double d)
            {
                string text = d.ToString();
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }
            if (value is bool b) return b ? "صح" : "خطأ";
            return value.ToString() ?? "عدم";
        }

        // Map color numbers to Arabic color names
        private string? GetColorName(int colorCode)
        {
            return colorCode switch
            {
                1 => "أحمر",
                2 => "أخضر",
                3 => "أزرق",
                4 => "أصفر",
                5 => "أسود",
                6 => "أبيض",
                _ => null
            };
        }

        // Define built-in math functions
        private void DefineBuiltins()
        {
            // Math functions
            _globalEnv.Define("جا", new BuiltinFunction(1, args => Math.Sin(ToNumber(args[0]))), true);
            _globalEnv.Define("جتا", new BuiltinFunction(1, args => Math.Cos(ToNumber(args[0]))), true);
            _globalEnv.Define("ظا", new BuiltinFunction(1, args => Math.Tan(ToNumber(args[0]))), true);
            _globalEnv.Define("مطلق", new BuiltinFunction(1, args => Math.Abs(ToNumber(args[0]))), true);
            _globalEnv.Define("جذر", new BuiltinFunction(1, args => Math.Sqrt(ToNumber(args[0]))), true);
            _globalEnv.Define("قرب", new BuiltinFunction(1, args => Math.Round(ToNumber(args[0]))), true);
            _globalEnv.Define("صحيح", new BuiltinFunction(1, args => Math.Floor(ToNumber(args[0]))), true);
            _globalEnv.Define("عشوائي", new BuiltinFunction(0, args => new Random().NextDouble()), true);
            
            // Constants
            _globalEnv.Define("ط", Math.PI, true);
            
            // String functions
            _globalEnv.Define("طول_النص", new BuiltinFunction(1, args => (double)Stringify(args[0]).Length), true);
            _globalEnv.Define("الحرف", new BuiltinFunction(2, args =>
            {
                string str = Stringify(args[0]);
                int index = (int)ToNumber(args[1]);
                if (index < 0 || index >= str.Length)
                    throw new RuntimeError("الفهرس خارج حدود النص");
                return str[index].ToString();
            }), true);
            
            // Date/Time functions
            _globalEnv.Define("الوقت_و_التاريخ", new BuiltinFunction(0, args => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), true);
            _globalEnv.Define("التاريخ", new BuiltinFunction(0, args => DateTime.Today.ToString("yyyy-MM-dd")), true);
            _globalEnv.Define("الوقت", new BuiltinFunction(0, args => DateTime.Now.ToString("HH:mm:ss")), true);
        }

        #endregion
    }

    #region Supporting Classes

    // Interface for output operations (will be implemented by the IDE)
    public interface IOutput
    {
        void Write(string text);
        void WriteLine(string text);
        string? ReadLine();
    }

    // Environment to store variables and their values
    public class Environment
    {
        private readonly Dictionary<string, object?> _values = new Dictionary<string, object?>();
        private readonly Dictionary<string, bool> _constants = new Dictionary<string, bool>();
        private readonly Environment? _enclosing;

        public Environment(Environment? enclosing = null)
        {
            _enclosing = enclosing;
        }

        public void Define(string name, object? value, bool isConstant)
        {
            if (_values.ContainsKey(name))
            {
                throw new RuntimeError($"المتغير '{name}' معرف مسبقاً");
            }
            _values[name] = value;
            _constants[name] = isConstant;
        }

        public object? Get(Token name)
        {
            if (_values.TryGetValue(name.Lexeme, out object? value))
            {
                return value;
            }

            if (_enclosing != null)
            {
                return _enclosing.Get(name);
            }

            throw new RuntimeError($"متغير غير معرف '{name.Lexeme}'");
        }

        public void Assign(Token name, object? value)
        {
            if (_values.ContainsKey(name.Lexeme))
            {
                if (_constants.GetValueOrDefault(name.Lexeme, false))
                {
                    throw new RuntimeError($"لا يمكن تغيير قيمة الثابت '{name.Lexeme}'");
                }
                _values[name.Lexeme] = value;
                return;
            }

            if (_enclosing != null)
            {
                _enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError($"متغير غير معرف '{name.Lexeme}'");
        }
    }

    // Interface for callable objects (functions)
    public interface ICallable
    {
        int Arity { get; }
        object? Call(Interpreter interpreter, List<object?> arguments);
    }

    // Built-in functions
    public class BuiltinFunction : ICallable
    {
        private readonly int _arity;
        private readonly Func<List<object?>, object?> _function;

        public int Arity => _arity;

        public BuiltinFunction(int arity, Func<List<object?>, object?> function)
        {
            _arity = arity;
            _function = function;
        }

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            return _function(arguments);
        }
    }

    // User-defined functions
    public class DhadFunction : ICallable
    {
        private readonly FunctionDeclarationNode _declaration;
        private readonly Environment _closure;

        public int Arity => _declaration.Parameters.Count;

        public DhadFunction(FunctionDeclarationNode declaration, Environment closure)
        {
            _declaration = declaration;
            _closure = closure;
        }

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            // Create environment for function execution
            Environment environment = new Environment(_closure);
            
            // Bind parameters to arguments
            for (int i = 0; i < _declaration.Parameters.Count; i++)
            {
                environment.Define(_declaration.Parameters[i].Lexeme, arguments[i], false);
            }
            
            try
            {
                interpreter.ExecuteBlock(_declaration.Body, environment);
            }
            catch (ReturnValue returnValue)
            {
                return returnValue.Value;
            }
            
            return null;
        }
    }

    // Exceptions for runtime errors
    public class RuntimeError : Exception
    {
        public RuntimeError(string message) : base(message) { }
    }

    // Special exception for return statements
    public class ReturnValue : Exception
    {
        public object? Value { get; }
        public ReturnValue(object? value) { Value = value; }
    }

    #endregion
}
