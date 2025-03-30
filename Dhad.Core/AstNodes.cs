using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dhad.Core
{
    // --- Visitor Interface ---
    // Defines the Visit methods for the Interpreter (or other AST processors)
    public interface IAstVisitor<T>
    {
        // Expressions
        T VisitNumberLiteral(NumberLiteralNode node);
        T VisitStringLiteral(StringLiteralNode node);
        T VisitBooleanLiteral(BooleanLiteralNode node); // Added for Sah/Khata
        T VisitVariable(VariableNode node);
        T VisitAssignment(AssignmentNode node); // Assignment is often treated as expression in some languages
        T VisitBinaryExpression(BinaryExpressionNode node);
        T VisitUnaryExpression(UnaryExpressionNode node);
        T VisitFunctionCall(FunctionCallNode node); // Added for function calls
        T VisitArrayIndex(ArrayIndexNode node); // Added for array access m[i]

        // Statements
        T VisitExpressionStatement(ExpressionStatementNode node);
        T VisitVariableDeclaration(VariableDeclarationNode node);
        T VisitConstantDeclaration(ConstantDeclarationNode node); // Added
        T VisitArrayDeclaration(ArrayDeclarationNode node); // Added
        T VisitFunctionDeclaration(FunctionDeclarationNode node); // Added
        T VisitReturnStatement(ReturnStatementNode node); // Added
        T VisitBlock(BlockStatementNode node); // Represents a sequence of statements {}
        T VisitIfStatement(IfStatementNode node);
        T VisitWhileStatement(WhileStatementNode node);
        T VisitForStatement(ForStatementNode node);
        T VisitPrintStatement(PrintStatementNode node); // Azher
        T VisitInputStatement(InputStatementNode node); // Adkhel
        T VisitDrawStatement(DrawStatementNode node); // Irsem
        T VisitPenStatement(PenStatementNode node); // AlQalam
        T VisitWindowStatement(WindowStatementNode node); // Nafitha
        T VisitClearScreenStatement(ClearScreenStatementNode node); // Masah
    }


    // --- Base Node Classes ---
    public abstract class AstNode
    {
        public abstract T Accept<T>(IAstVisitor<T> visitor);
    }

    public abstract class ExpressionNode : AstNode { }

    public abstract class StatementNode : AstNode { }


    // --- Expression Node Examples ---
    public class NumberLiteralNode : ExpressionNode
    {
        public double Value { get; }
        public NumberLiteralNode(double value) { Value = value; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitNumberLiteral(this);
    }

    public class StringLiteralNode : ExpressionNode
    {
        public string Value { get; }
        public StringLiteralNode(string value) { Value = value; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitStringLiteral(this);
    }

    public class BooleanLiteralNode : ExpressionNode // Added
    {
        public bool Value { get; }
        public BooleanLiteralNode(bool value) { Value = value; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitBooleanLiteral(this);
    }

    public class VariableNode : ExpressionNode
    {
        public Token Name { get; } // Use Token to keep line info etc.
        public VariableNode(Token name) { Name = name; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitVariable(this);
    }

    public class AssignmentNode : ExpressionNode // Or StatementNode depending on language rules
    {
        public Token Name { get; } // Variable or Array Index Target
        // If allowing array assignment, 'Name' might need to be an ExpressionNode instead of Token
        public ExpressionNode Value { get; }
        public AssignmentNode(Token name, ExpressionNode value) { Name = name; Value = value; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitAssignment(this);
    }

    public class BinaryExpressionNode : ExpressionNode
    {
        public ExpressionNode Left { get; }
        public Token Operator { get; }
        public ExpressionNode Right { get; }
        public BinaryExpressionNode(ExpressionNode left, Token op, ExpressionNode right) { Left = left; Operator = op; Right = right; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitBinaryExpression(this);
    }

    public class UnaryExpressionNode : ExpressionNode
    {
        public Token Operator { get; }
        public ExpressionNode Right { get; }
        public UnaryExpressionNode(Token op, ExpressionNode right) { Operator = op; Right = right; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitUnaryExpression(this);
    }

    public class FunctionCallNode : ExpressionNode // Added
    {
        public ExpressionNode Callee { get; } // Usually a VariableNode for the function name
        public Token Paren { get; } // Keep the '(' token for location info
        public List<ExpressionNode> Arguments { get; }
        public FunctionCallNode(ExpressionNode callee, Token paren, List<ExpressionNode> arguments) { Callee = callee; Paren = paren; Arguments = arguments; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitFunctionCall(this);
    }

    public class ArrayIndexNode : ExpressionNode // Added for m[i] or m[i, j]
    {
        public ExpressionNode Array { get; } // Usually a VariableNode for the array name
        public Token Bracket { get; } // Keep the '[' token
        public List<ExpressionNode> Indices { get; }
        public ArrayIndexNode(ExpressionNode array, Token bracket, List<ExpressionNode> indices) { Array = array; Bracket = bracket; Indices = indices; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitArrayIndex(this);
    }

    // --- Statement Node Examples ---
    public class ExpressionStatementNode : StatementNode
    {
        public ExpressionNode Expression { get; }
        public ExpressionStatementNode(ExpressionNode expression) { Expression = expression; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitExpressionStatement(this);
    }

    public class VariableDeclarationNode : StatementNode // Added
    {
        public Token Name { get; }
        public ExpressionNode? Initializer { get; } // Nullable if initialization is optional
        // Could add Type information here if language becomes typed
        public VariableDeclarationNode(Token name, ExpressionNode? initializer) { Name = name; Initializer = initializer; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitVariableDeclaration(this);
    }
    public class ConstantDeclarationNode : StatementNode // Added
    {
        public Token Name { get; }
        public ExpressionNode Initializer { get; } // Must have initializer
        public ConstantDeclarationNode(Token name, ExpressionNode initializer) { Name = name; Initializer = initializer; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitConstantDeclaration(this);
    }
    public class ArrayDeclarationNode : StatementNode // Added
    {
        public Token Name { get; }
        public List<ExpressionNode> Dimensions { get; } // Sizes
        public List<ExpressionNode>? Initializer { get; } // Optional list of initial values
        public ArrayDeclarationNode(Token name, List<ExpressionNode> dimensions, List<ExpressionNode>? initializer) { Name = name; Dimensions = dimensions; Initializer = initializer; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitArrayDeclaration(this);
    }

    public class FunctionDeclarationNode : StatementNode // Added
    {
        public Token Name { get; }
        public List<Token> Parameters { get; }
        public List<StatementNode> Body { get; }
        public FunctionDeclarationNode(Token name, List<Token> parameters, List<StatementNode> body) { Name = name; Parameters = parameters; Body = body; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitFunctionDeclaration(this);
    }

    public class ReturnStatementNode : StatementNode // Added
    {
        public Token Keyword { get; } // The 'أرجع' token
        public ExpressionNode? Value { get; } // Nullable if return value is optional
        public ReturnStatementNode(Token keyword, ExpressionNode? value) { Keyword = keyword; Value = value; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitReturnStatement(this);
    }

    public class BlockStatementNode : StatementNode // Added
    {
        public List<StatementNode> Statements { get; }
        public BlockStatementNode(List<StatementNode> statements) { Statements = statements; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitBlock(this);
    }

    public class IfStatementNode : StatementNode // Added
    {
        public ExpressionNode Condition { get; }
        public StatementNode ThenBranch { get; } // Could be a BlockStatementNode
        public StatementNode? ElseBranch { get; } // Nullable
        public IfStatementNode(ExpressionNode condition, StatementNode thenBranch, StatementNode? elseBranch) { Condition = condition; ThenBranch = thenBranch; ElseBranch = elseBranch; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitIfStatement(this);
    }

    public class WhileStatementNode : StatementNode // Added
    {
        public ExpressionNode Condition { get; }
        public StatementNode Body { get; }
        public WhileStatementNode(ExpressionNode condition, StatementNode body) { Condition = condition; Body = body; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitWhileStatement(this);
    }

    public class ForStatementNode : StatementNode // Added
    {
        public Token Variable { get; }
        public ExpressionNode StartValue { get; }
        public ExpressionNode EndValue { get; }
        public ExpressionNode? StepValue { get; } // Nullable
        public StatementNode Body { get; }
        public ForStatementNode(Token variable, ExpressionNode start, ExpressionNode end, ExpressionNode? step, StatementNode body) { Variable = variable; StartValue = start; EndValue = end; StepValue = step; Body = body; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitForStatement(this);
    }

    public class PrintStatementNode : StatementNode // Added
    {
        public List<ExpressionNode> Expressions { get; } // Handle multiple items separated by comma
        public PrintStatementNode(List<ExpressionNode> expressions) { Expressions = expressions; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitPrintStatement(this);
    }
    public class InputStatementNode : StatementNode // Added
    {
        public List<Token> Variables { get; } // List of variables to input into
        public InputStatementNode(List<Token> variables) { Variables = variables; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitInputStatement(this);
    }

    public class DrawStatementNode : StatementNode // Added
    {
        public Token Shape { get; } // نقطة, خط, etc.
        public List<ExpressionNode> Arguments { get; }
        public DrawStatementNode(Token shape, List<ExpressionNode> arguments) { Shape = shape; Arguments = arguments; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitDrawStatement(this);
    }

    public class PenStatementNode : StatementNode // Added
    {
        public Token Property { get; } // لونه, عرضه
        public ExpressionNode Value { get; }
        public PenStatementNode(Token property, ExpressionNode value) { Property = property; Value = value; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitPenStatement(this);
    }
    public class WindowStatementNode : StatementNode // Added
    {
        public Token Property { get; } // طولها, عرضها
        public ExpressionNode Value { get; }
        public WindowStatementNode(Token property, ExpressionNode value) { Property = property; Value = value; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitWindowStatement(this);
    }
    public class ClearScreenStatementNode : StatementNode // Added
    {
        public Token Keyword { get; } // The مسح token
        public ClearScreenStatementNode(Token keyword) { Keyword = keyword; }
        public override T Accept<T>(IAstVisitor<T> visitor) => visitor.VisitClearScreenStatement(this);
    }

}