// <copyright file="Formula_PS2.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>
// <authors> Kate Wang, Martin Allen UofU-CS3500 </authors>
// <date> 02/27/2024 </date>


namespace CS3500.Formula;

using System.Text.RegularExpressions;

/// <summary>
/// Contains extension methods for class Stack.
/// </summary>
public static class StackExtensions {

    /// <summary>
    /// Checks if stack is empty.
    /// </summary>
    /// <param name="stack"> The stack to check.</param>
    /// <returns> True if the stack is empty. False otherwise.</returns>
    public static bool IsEmpty<T>(this Stack<T> stack)
    {
        return stack.Count == 0;
    }

    /// <summary>
    /// Checks the stack to see if the given string is on top.
    /// </summary>
    /// <param name="stack"> The stack to check.</param>
    /// <param name="str"> The string to compare to the top of the stack.</param>
    /// <returns> True if the stack has the given string on top. False otherwise.</returns>
    public static bool IsOnTop(this Stack<string> stack, string str)
    {
        return stack.Count > 0 && stack.Peek() == str;
    }
}

/// <summary>
///   <para>
///     This class represents formulas written in standard infix notation using standard precedence
///     rules.  The allowed symbols are non-negative numbers written using double-precision
///     floating-point syntax; variables that consist of one or more letters followed by
///     one or more numbers; parentheses; and the four operator symbols +, -, *, and /.
///   </para>
/// </summary>
public class Formula
{
    /// <summary>
    ///   This pattern represents valid variable name strings.
    /// </summary>
    private const string VariableRegExPattern = @"[a-zA-Z]+\d+";

    /// <summary>
    ///   A list of tokens to be validated.
    /// </summary>
    private readonly List<string> _tokens;

    /// <summary>
    ///   A string stores normalized formula.
    /// </summary>
    private readonly string _normalizedFormula = "";

    /// <summary>
    ///   Initializes a new instance of the <see cref="Formula"/> class.
    ///   <para>
    ///     Creates a Formula from a string that consists of an infix expression written as
    ///     described in the class comment.  If the expression is syntactically incorrect,
    ///     throws a FormulaFormatException with an explanatory Message.  See the assignment
    ///     specifications for the syntax rules you are to implement.
    ///   </para>
    /// </summary>
    /// <param name="formula"> The string representation of the formula to be created.</param>
    public Formula( string formula )
    {
        _tokens = GetTokens(formula);

        if (_tokens.Count == 0)
            throw new FormulaFormatException("Empty formula.");

        var openingParenthesisCounter = 0;
        var closingParenthesisCounter = 0;

        var tokenCounter = 0;
        var lastTokenIndicator = _tokens.Count - 1;

        var firstFollowingFlag = false;
        var secondFollowingFlag = false;

        foreach (var token in _tokens)
        {
            if (!IsValidToken(token))
                throw new FormulaFormatException("Invalid token.");

            // check first, last token rules and token following rules.
            if ((tokenCounter == 0 && (token.Equals(")") || IsOperator(token)))
                || (tokenCounter == lastTokenIndicator && (token.Equals("(") || IsOperator(token)))
                || (firstFollowingFlag && !(IsNum(token) || IsVar(token) || token.Equals("(")))
                || (secondFollowingFlag && !(IsOperator(token) || token.Equals(")"))))
                throw new FormulaFormatException("Illegal token.");

            firstFollowingFlag = false;
            secondFollowingFlag = false;

            if (IsOperator(token) || token.Equals("("))
                firstFollowingFlag = true;

            if (token.Equals("("))
                openingParenthesisCounter++;

            if (openingParenthesisCounter < closingParenthesisCounter)
                throw new FormulaFormatException("Illegal parentheses.");

            if (IsNum(token) || IsVar(token) || token.Equals(")"))
                secondFollowingFlag = true;

            if (token.Equals(")"))
                closingParenthesisCounter++;

            tokenCounter++;
            _normalizedFormula += IsNum(token) ? double.Parse(token) : token.ToUpper();
        }

        if (openingParenthesisCounter != closingParenthesisCounter)
            throw new FormulaFormatException("Unbalanced parentheses.");
    }

    /// <summary>
    ///   Helper method for checking whether a token is a number.
    /// </summary>
    /// <param name="token">formula token</param>
    /// <returns></returns>
    private static bool IsNum(string token)
    {
        return double.TryParse(token, out _);
    }

    /// <summary>
    ///   Helper method for checking whether a token is an operator.
    /// </summary>
    /// <param name="token">formula token</param>
    /// <returns></returns>
    private static bool IsOperator(string token)
    {
        return token.Equals("+") || token.Equals("-") || token.Equals("*") || token.Equals("/");
    }

    /// <summary>
    ///   Checks if a token is valid.
    /// </summary>
    /// <param name="token">formula token</param>
    /// <returns></returns>
    private static bool IsValidToken(string token)
    {
        return IsNum(token) || IsOperator(token) || token.Equals("(") || token.Equals(")") || IsVar(token);
    }

    /// <summary>
    ///   Returns a set of all the variables in the formula.
    /// </summary>
    /// <returns> the set of variables (string names) representing the variables referenced by the formula. </returns>
    public ISet<string> GetVariables()
    {
        var variables = new HashSet<string>();

        foreach (var token in _tokens.Where(IsVar))
            variables.Add(token.ToUpper());

        return variables;
    }

    /// <summary>
    ///   Returns a string representation of a canonical form of the formula.
    /// </summary>
    /// <returns>
    ///   A canonical version (string) of the formula. All "equal" formulas
    ///   should have the same value here.
    /// </returns>
    public override string ToString( )
    {
        return _normalizedFormula;
    }

    /// <summary>
    ///   Reports whether "token" is a variable.  It must be one or more letters
    ///   followed by one or more numbers.
    /// </summary>
    /// <param name="token"> A token that may be a variable. </param>
    /// <returns> true if the string matches the requirements, e.g., A1 or a1. </returns>
    public static bool IsVar( string token )
    {
        string standaloneVarPattern = $"^{VariableRegExPattern}$";
        return Regex.IsMatch( token, standaloneVarPattern );
    }

    /// <summary>
    ///   <para>
    ///     Given an expression, enumerates the tokens that compose it.
    ///   </para>
    ///   <para>
    ///     Tokens returned are:
    ///   </para>
    ///   <list type="bullet">
    ///     <item>left paren</item>
    ///     <item>right paren</item>
    ///     <item>one of the four operator symbols</item>
    ///     <item>a string consisting of one or more letters followed by one or more numbers</item>
    ///     <item>a double literal</item>
    ///     <item>and anything that doesn't match one of the above patterns</item>
    ///   </list>
    ///   <para>
    ///     There are no empty tokens; white space is ignored (except to separate other tokens).
    ///   </para>
    /// </summary>
    /// <param name="formula"> A string representing an infix formula such as 1*B1/3.0. </param>
    /// <returns> The ordered list of tokens in the formula. </returns>
    private static List<string> GetTokens( string formula )
    {
        List<string> results = [];

        string lpPattern = @"\(";
        string rpPattern = @"\)";
        string opPattern = @"[\+\-*/]";
        string doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
        string spacePattern = @"\s+";

        // Overall pattern
        string pattern = string.Format(
                                        "({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                        lpPattern,
                                        rpPattern,
                                        opPattern,
                                        VariableRegExPattern,
                                        doublePattern,
                                        spacePattern);

        // Enumerate matching tokens that don't consist solely of white space.
        foreach ( string s in Regex.Split( formula, pattern, RegexOptions.IgnorePatternWhitespace ) )
        {
            if ( !Regex.IsMatch( s, @"^\s*$", RegexOptions.Singleline ) )
            {
                results.Add(s);
            }
        }

        return results;
    }

    /// <summary>
    ///   <para>
    ///     Reports whether f1 == f2, using the notion of equality from the <see cref="Equals"/> method.
    ///   </para>
    /// </summary>
    /// <param name="f1"> The first of two formula objects. </param>
    /// <param name="f2"> The second of two formula objects. </param>
    /// <returns> true if the two formulas are the same.</returns>
    public static bool operator ==( Formula f1, Formula f2 )
    {
        return f1.Equals(f2);
    }

    /// <summary>
    ///   <para>
    ///     Reports whether f1 != f2, using the notion of equality from the <see cref="Equals"/> method.
    ///   </para>
    /// </summary>
    /// <param name="f1"> The first of two formula objects. </param>
    /// <param name="f2"> The second of two formula objects. </param>
    /// <returns> true if the two formulas are not equal to each other.</returns>
    public static bool operator !=( Formula f1, Formula f2 )
    {
        return !(f1 == f2);
    }

    /// <summary>
    ///   <para>
    ///     Determines if two formula objects represent the same formula.
    ///   </para>
    ///   <para>
    ///     By definition, if the parameter is null or does not reference
    ///     a Formula Object then return false.
    ///   </para>
    ///   <para>
    ///     Two Formulas are considered equal if their canonical string representations
    ///     (as defined by ToString) are equal.
    ///   </para>
    /// </summary>
    /// <param name="obj"> The other object.</param>
    /// <returns>
    ///   True if the two objects represent the same formula.
    /// </returns>
    public override bool Equals( object? obj )
    {
        return obj is Formula && ToString().Equals(obj.ToString());
    }

    /// <summary>
    /// <para>
    /// Evaluates this Formula, using the lookup delegate to determine the values
    /// of variables.
    /// </para>
    /// <remarks>
    /// When the lookup method is called, it will always be passed a normalized (capitalized)
    /// variable name. The lookup method will throw an ArgumentException if there
    /// is not a definition for that variable token.
    /// </remarks>
    /// <para>
    /// If no undefined variables or divisions by zero are encountered when evaluating
    /// this Formula, the numeric value of the formula is returned. Otherwise, a
    /// FormulaError is returned (with a meaningful explanation as the Reason property).
    /// </para>
    /// <para>
    /// This method should never throw an exception.
    /// </para>
    /// </summary>
    /// <param name="lookup">
    /// <para>
    /// Given a variable symbol as its parameter, lookup returns the variable's value
    /// (if it has one) or throws an ArgumentException (otherwise). This method
    /// will expect variable names to be normalized.
    /// </para>
    /// </param>
    /// <returns> Either a double or a FormulaError, based on evaluating the formula.</returns>
    public object Evaluate(Lookup lookup)
    {

        var valStack = new Stack<double>();
        var opStack = new Stack<string>();

        foreach (var token in GetTokens(_normalizedFormula))
        {
            if (double.TryParse(token, out var d)) // token is a number case
            {
                if (opStack.IsOnTop("*") || opStack.IsOnTop("/"))
                {
                    var returnError = ApplyMultiplicationOrDivision(valStack, opStack, d);
                    if (returnError)
                        return new FormulaError("Division by zero.");
                }
                else
                    valStack.Push(d);
            }
            else if (IsVar(token)) // token is a variable case
            {
                double val;

                try
                {
                    val = lookup(token);
                }
                catch (ArgumentException)
                {
                    return new FormulaError("Unknown variable.");
                }

                if (opStack.IsOnTop("*") || opStack.IsOnTop("/"))
                {
                    var returnError = ApplyMultiplicationOrDivision(valStack, opStack, val);
                    if (returnError)
                        return new FormulaError("Division by zero.");
                }
                else
                    valStack.Push(val);
            }
            else if (token == "+" || token == "-")
            {
                if (opStack.IsOnTop("+") || opStack.IsOnTop("-"))
                    ApplyAdditionOrSubtraction(valStack, opStack);

                opStack.Push(token);
            }
            else if (token == "*" || token == "/")
            {
                opStack.Push(token);
            }
            else if (token == "(")
            {
                opStack.Push(token);
            }
            else if (token == ")")
            {
                if (opStack.IsOnTop("+") || opStack.IsOnTop("-"))
                {
                    ApplyAdditionOrSubtraction(valStack, opStack);
                }

                opStack.Pop(); // popping guaranteed opening parenthesis

                if (opStack.IsOnTop("*") || opStack.IsOnTop("/"))
                {
                    var returnError = ParenMultiplicationOrDivision(valStack, opStack);
                    if (returnError)
                        return new FormulaError("Division by zero.");
                }
            }
        }

        return ProcessFinalResult(opStack, valStack);
    }

    /// <summary>
    /// Helper function for the <see
    /// cref= "Evaluate"/> method.
    /// Applies special multiplication or division process and pushes calculated value to the value stack.
    /// If division by zero were to occur, no value is pushed to the value stack.
    /// </summary>
    /// <param name="valStack"> The stack containing values calculated in the formula.</param>
    /// <param name="opStack"> The stack containing the operators in the formula.</param>
    /// <returns> True if a FormulaError needs to be returned, false otherwise.</returns>
    private static bool ParenMultiplicationOrDivision(Stack<double> valStack, Stack<string> opStack)
    {
        var rightVal = valStack.Pop();
        var leftVal = valStack.Pop();
        var op = opStack.Pop();

        var isZero = ZeroCheck(rightVal);

        if (op == "*")
            valStack.Push(leftVal * rightVal);
        else if (op == "/")
        {
            if (isZero)
                return true;
            valStack.Push(leftVal / rightVal);
        }

        return false;
    }

    /// <summary>
    /// Helper function for the <see
    /// cref= "Evaluate"/> method.
    /// Returns the final value after all tokens have been processed.
    /// </summary>
    /// <param name="valStack"> The stack containing values calculated in the formula.</param>
    /// <param name="opStack"> The stack containing the operators in the formula.</param>
    /// <returns> The final result of the formula.</returns>
    private static double ProcessFinalResult(Stack<string> opStack, Stack<double> valStack)
    {
        if (opStack.IsEmpty())
            return valStack.Pop();
        ApplyAdditionOrSubtraction(valStack, opStack);
            return valStack.Pop();
    }

    /// <summary>
    /// Helper function for the <see
    /// cref= "Evaluate"/> method.
    /// Multiplies or divides two values from the value stack depending on
    /// the operator at the top of the operator stack. Processed values are pushed to the value stack.
    /// If division by zero were to occur, no value is pushed to the value stack.
    /// </summary>
    /// <param name="valStack"> The stack containing values calculated in the formula.</param>
    /// <param name="opStack"> The stack containing the operators in the formula.</param>
    /// <param name="currTokenVal"> The value of the current token.</param>
    /// <returns> True if a FormulaError needs to be returned, false otherwise.</returns>
    private static bool ApplyMultiplicationOrDivision(Stack<double> valStack, Stack<string> opStack, double currTokenVal)
    {
        var num = valStack.Pop();
        var op = opStack.Pop();

        if (op == "*")
            valStack.Push(num * currTokenVal);
        else if (op == "/")
        {
            var isZero = ZeroCheck(currTokenVal);
            if (isZero)
                return true;
            valStack.Push(num / currTokenVal);
        }

        return false;
    }

    /// <summary>
    /// Checks if given value is zero. Returns true if it is and false otherwise.
    /// </summary>
    /// <param name="possibleZero"> The value to check.</param>
    /// <returns> True if zero, false otherwise.</returns>
    private static bool ZeroCheck(double possibleZero)
    {
        const double epsilon = 1e-9;
        return Math.Abs(possibleZero) < epsilon;
    }

    /// <summary>
    /// Helper function for the <see
    /// cref= "Evaluate"/> method.
    /// Adds or subtracts two values from the value stack depending on
    /// the operator at the top of the operator stack. Processed values are pushed to the value stack.
    /// </summary>
    /// <param name="valStack"> The stack containing values calculated in the formula.</param>
    /// <param name="opStack"> The stack containing the operators in the formula.</param>
    private static void ApplyAdditionOrSubtraction(Stack<double> valStack, Stack<string> opStack)
    {
        var rightVal = valStack.Pop();
        var leftVal = valStack.Pop();
        var op = opStack.Pop();

        if (op == "+")
            valStack.Push(leftVal + rightVal);
        else if (op == "-")
            valStack.Push(leftVal - rightVal);
    }

    /// <summary>
    ///   <para>
    ///     Returns a hash code for this Formula.
    ///     If f1.Equals(f2), then f1.GetHashCode() == f2.GetHashCode().
    ///   </para>
    /// </summary>
    /// <returns> The hashcode for the object. </returns>
    public override int GetHashCode( )
    {
        return ToString().GetHashCode();
    }
}

/// <summary>
///   Used to report syntax errors in the argument to the Formula constructor.
/// </summary>
public class FormulaFormatException : Exception
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="FormulaFormatException"/> class.
    ///   <para>
    ///      Constructs a FormulaFormatException containing the explanatory message.
    ///   </para>
    /// </summary>
    /// <param name="message"> A developer defined message describing why the exception occured.</param>
    public FormulaFormatException( string message )
        : base( message )
    {
    }
}

/// <summary>
/// Used as a possible return value of the Formula.Evaluate method.
/// </summary>
public class FormulaError
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="FormulaError"/> class.
    ///   <para>
    ///     Constructs a FormulaError containing the explanatory reason.
    ///   </para>
    /// </summary>
    /// <param name="message"> Contains a message for why the error occurred.</param>
    public FormulaError( string message )
    {
        Reason = message;
    }

    /// <summary>
    ///  Gets the reason why this FormulaError was created.
    /// </summary>
    public string Reason { get; private set; }
}

/// <summary>
///   Any method meeting this type signature can be used for
///   looking up the value of a variable.
/// </summary>
/// <exception cref="ArgumentException">
///   If a variable name is provided that is not recognized by the implementing method,
///   then the method should throw an ArgumentException.
/// </exception>
/// <param name="variableName">
///   The name of the variable (e.g., "A1") to lookup.
/// </param>
/// <returns> The value of the given variable (if one exists). </returns>
public delegate double Lookup( string variableName );