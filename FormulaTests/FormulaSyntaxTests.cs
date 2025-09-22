// <copyright file="FormulaSyntaxTests.cs" company="UofU-CS3500">
//   Copyright © 2024 UofU-CS3500. All rights reserved.
// </copyright>
// <authors> Kate Wang </authors>
// <date> 01/23/2024 </date>

namespace CS3500.FormulaTests;

using CS3500.Formula;

/// <summary>
///   This class tests the syntax validation in FormulaConstructor.
/// </summary>
[TestClass]
public class FormulaSyntaxTests
{
    // --- Tests for One Token Rule ---
    /// <summary>
    ///   Throws FormulaFormatException when the input is empty.
    /// </summary>
    [TestMethod]
    [ExpectedException( typeof( FormulaFormatException ))]
    public void FormulaConstructor_TestNoTokens_Invalid()
    {
        _ = new Formula( string.Empty );
    }

    /// <summary>
    ///   Correction - Tests for formula with empty spaces.
    /// </summary>
    [TestMethod]
    [ExpectedException( typeof( FormulaFormatException ))]
    public void FormulaConstructor_TestEmptySpaces_Invalid()
    {
        _ = new Formula("  ");
    }

    // --- Tests for Valid Token Rule ---

    /// <summary>
    ///   Throw FormulaFormatException when a token is invalid.
    /// </summary>
    [TestMethod]
    [ExpectedException( typeof( FormulaFormatException ))]
    public void FormulaConstructor_TestValidTokens_Invalid()
    {
        _ = new Formula( "$" );
    }

    // --- Tests for Closing Parenthesis Rule

    /// <summary>
    ///   Throws FormulaFormatException when number of closing parentheses is greater than opening ones.
    /// </summary>
    [TestMethod]
    [ExpectedException( typeof( FormulaFormatException ))]
    public void FormulaConstructor_TestClosingParentheses_Invalid()
    {
        _ = new Formula( "(1+1))))" );
    }

    // --- Tests for Balanced Parentheses Rule

    /// <summary>
    ///   Number of opening and closing parentheses must equal.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestBalancedParentheses_Valid()
    {
        _ = new Formula( "((1+1))" );
    }

    /// <summary>
    ///   Throws FormulaFormatException when the numbers of opening and closing parentheses not equal, in this case, opening greater than closing.
    /// </summary>
    [TestMethod]
    [ExpectedException( typeof( FormulaFormatException ))]
    public void FormulaConstructor_TestUnbalancedParentheses_Invalid()
    {
        _ = new Formula( "(((1+1))" );
    }

    /// <summary>
    ///   Throws FormulaFormatException when only one opening parenthesis.
    /// </summary>
    [TestMethod]
    [ExpectedException( typeof( FormulaFormatException ))]
    public void FormulaConstructor_TestInvalidOpeningParenthesis_Invalid()
    {
        _ = new Formula( "(" );
    }

    /// <summary>
    ///   Throws FormulaFormatException when only one closing parenthesis.
    /// </summary>
    [TestMethod]
    [ExpectedException( typeof( FormulaFormatException ))]
    public void FormulaConstructor_TestInvalidClosingParenthesis_Invalid()
    {
        _ = new Formula( ")" );
    }

    // --- Tests for First Token Rule

    /// <summary>
    ///   First token is a valid number.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestFirstTokenNumber_Valid( )
    {
        _ = new Formula( "1+1" );
    }

    /// <summary>
    ///   First token is a valid opening parenthesis.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestFirstTokenOpeningParenthesis_Valid()
    {
        _ = new Formula("(1+1)");
    }

    /// <summary>
    ///   First token is a valid variable.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestFirstTokenVariable_Valid()
    {
        _ = new Formula("a1+1");
    }

    /// <summary>
    ///   Throws FormulaFormatException when the first illegal token is invalid.
    /// </summary>
    [TestMethod]
    [ExpectedException( typeof( FormulaFormatException ))]
    public void FormulaConstructor_TestFirstIllegalTokenInvalid_Invalid()
    {
        _ = new Formula( "a+1" );
    }

    /// <summary>
    ///   Throws FormulaFormatException when the first legal token is invalid.
    /// </summary>
    [TestMethod]
    [ExpectedException( typeof( FormulaFormatException ))]
    public void FormulaConstructor_TestFirstLegalTokenInvalid_Invalid()
    {
        _ = new Formula( "+1" );
    }

    // --- Tests for  Last Token Rule ---

    /// <summary>
    ///   Last token is a valid number.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestLastTokenNumber_Valid( )
    {
        _ = new Formula( "1+1" );
    }

    /// <summary>
    ///   Last token is a valid closing parenthesis.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestLastTokenClosingParenthesis_Valid()
    {
        _ = new Formula("(1+1)");
    }

    /// <summary>
    ///   Last token is a valid variable.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestLastTokenVariable_Valid()
    {
        _ = new Formula("1+a1");
    }

    /// <summary>
    ///   Throws FormulaFormatException when the last token is illegal.
    /// </summary>
    [TestMethod]
    [ExpectedException( typeof( FormulaFormatException ))]
    public void FormulaConstructor_TestLastIllegalTokenInvalid_Invalid()
    {
        _ = new Formula( "1+a" );
    }

    /// <summary>
    ///   Throws FormulaFormatException when the last legal token is invalid.
    /// </summary>
    [TestMethod]
    [ExpectedException( typeof( FormulaFormatException ))]
    public void FormulaConstructor_TestLastLegalTokenInvalid_Invalid()
    {
        _ = new Formula( "1+" );
    }

    // --- Tests for Parentheses/Operator Following Rule ---

    /// <summary>
    ///   Token following opening parenthesis is a number.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestNumberFollowingOpeningParenthesis_Valid()
    {
        _ = new Formula("(1+1)");
    }

    /// <summary>
    ///   Token following opening parenthesis is a variable.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestVariableFollowingOpeningParenthesis_Valid()
    {
        _ = new Formula("(a1+1)");
    }

    /// <summary>
    ///   Token following opening parenthesis is an opening parenthesis.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestOpeningParenthesisFollowingOpeningParenthesis_Valid()
    {
        _ = new Formula("((1+1)*1)");
    }

    /// <summary>
    ///   Throws FormulaFormatException when invalid token following opening parenthesis.
    /// </summary>
    [TestMethod]
    [ExpectedException( typeof( FormulaFormatException ))]
    public void FormulaConstructor_TestInvalidTokenFollowingOpeningParenthesis_Invalid()
    {
        _ = new Formula("(a+1)");
    }

    /// <summary>
    ///   Token following operator is a number.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestNumberFollowingOperator_Valid()
    {
        _ = new Formula("1+1");
    }

    /// <summary>
    ///   Token following operator is a variable.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestVariableFollowingOperator_Valid()
    {
        _ = new Formula("1+a1");
    }

    /// <summary>
    ///   Token following operator is an opening parenthesis.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestOpeningParenthesisFollowingOperator_Valid()
    {
        _ = new Formula("1+(1+1)");
    }

    /// <summary>
    ///   Throws FormulaFormatException when an invalid token following operator.
    /// </summary>
    [TestMethod]
    [ExpectedException( typeof( FormulaFormatException ))]
    public void FormulaConstructor_TestInvalidTokenFollowingOperator_Invalid()
    {
        _ = new Formula("1+a");
    }

    // --- Tests for Extra Following Rule ---

    /// <summary>
    ///   Token following number is an operator.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestOperatorFollowingNumber_Valid()
    {
        _ = new Formula("1+1");
    }

    /// <summary>
    ///   Token following number is a closing parenthesis.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestClosingParenthesisFollowingNumber_Valid()
    {
        _ = new Formula("(1+1)");
    }

    /// <summary>
    ///   Throws FormulaFormatException when invalid token following number.
    /// </summary>
    [TestMethod]
    [ExpectedException( typeof( FormulaFormatException ))]
    public void FormulaConstructor_TestInvalidTokenFollowingNumber_Invalid()
    {
        _ = new Formula("1a");
    }

    /// <summary>
    ///   Token following variable is an operator.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestOperatorFollowingVariable_Valid()
    {
        _ = new Formula("a1+1");
    }

    /// <summary>
    ///   Token following variable is a closing parenthesis.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestClosingParenthesisFollowingVariable_Valid()
    {
        _ = new Formula("(1+a1)");
    }

    /// <summary>
    ///   Throws FormulaFormatException when invalid token following variable.
    /// </summary>
    [TestMethod]
    [ExpectedException( typeof( FormulaFormatException ))]
    public void FormulaConstructor_TestInvalidTokenFollowingVariable_Invalid()
    {
        _ = new Formula("a1$");
    }

    /// <summary>
    ///   Token following closing parenthesis is an operator.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestOperatorFollowingClosingParenthesis_Valid()
    {
        _ = new Formula("(1+1)*1");
    }

    /// <summary>
    ///   Token following closing parenthesis is a closing parenthesis.
    /// </summary>
    [TestMethod]
    public void FormulaConstructor_TestClosingParenthesisFollowingClosingParenthesis_Valid()
    {
        _ = new Formula("((1+1))");
    }

    /// <summary>
    ///   Throws FormulaFormatException when invalid token following closing parenthesis.
    /// </summary>
    [TestMethod]
    [ExpectedException( typeof( FormulaFormatException ))]
    public void FormulaConstructor_TestInvalidTokenFollowingClosingParenthesis_Invalid()
    {
        _ = new Formula("(1+1)1");
    }

    // --- Tests for GetVariables ---

    /// <summary>
    ///   Checks every element in the string set is in uppercase
    /// </summary>
    [TestMethod]
    public void GetVariables_TestElementsForm_Canonical()
    {
        var set = new Formula("x1+y1*z1").GetVariables();

        foreach (var variable in set)
        {
            Assert.AreEqual(variable.ToUpper(), variable);
        }
    }

    /// <summary>
    ///   Tests duplicate variables in case-sensitive form
    /// </summary>
    [TestMethod]
    public void GetVariables_TestDuplicateVariables_SetWithNoDuplicate()
    {
        var set = new Formula("x1+X1").GetVariables();

        const int expected = 1;
        var actual = set.Count;

        Assert.AreEqual(expected, actual);
    }

    // --- Tests for ToString ---

    /// <summary>
    ///   Tests ToString spaces trimming.
    /// </summary>
    [TestMethod]
    public void ToString_TestStringContainsMultipleSpaces_StringWithNoSpaces()
    {
        const string expected = "X1+Y1";
        var actual = new Formula(" X1 +  Y1 ").ToString();
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///   Tests the equality of different forms of same formula.
    /// </summary>
    [TestMethod]
    public void ToString_TestSameFormulaInDifferentForms_Equal()
    {
        var formula1 = new Formula("z3 + 5E3").ToString();
        var formula2 = new Formula("Z3 + 5000.00").ToString();
        Assert.AreEqual(formula1, formula2);
    }

    // --- Tests for operator == and != ---

    /// <summary>
    ///   Tests when two formulas with only numbers are equal
    /// </summary>
    [TestMethod]
    public void OperatorEqualTest_EquivalentFormulasWithoutVariable_True()
    {
        var f1 = new Formula("1+1");
        var f2 = new Formula("1+1.000");
        Assert.IsTrue(f1 == f2);
        Assert.IsFalse(f1 != f2);
    }

    /// <summary>
    ///   Tests when two formulas with only numbers are not equal
    /// </summary>
    [TestMethod]
    public void OperatorEqualTest_UnequivalentFormulaWithoutVariable_False()
    {
        var f1 = new Formula("1+1.1");
        var f2 = new Formula("1+1");
        Assert.IsFalse(f1 == f2);
        Assert.IsTrue(f1 != f2);
    }

    /// <summary>
    ///   Tests when two formulas with variable are equal
    /// </summary>
    [TestMethod]
    public void OperatorEqualTest_EquivalentFormulasWithVariable_True()
    {
        var f1 = new Formula("a1+1");
        var f2 = new Formula("A1+1.000");
        Assert.IsTrue(f1 == f2);
        Assert.IsFalse(f1 != f2);
    }

    /// <summary>
    ///   Tests when two formulas with variable are not equal
    /// </summary>
    [TestMethod]
    public void OperatorEqualTest_UnequivalentFormulaWithVariable_False()
    {
        var f1 = new Formula("a1+1");
        var f2 = new Formula("b1+1.000");
        Assert.IsFalse(f1 == f2);
        Assert.IsTrue(f1 != f2);
    }

    // --- Tests for Equals and GetHashCode ---

    /// <summary>
    ///   Tests when Equals called with an null parameter
    /// </summary>
    [TestMethod]
    public void Equals_NullObject_False()
    {
        var f1 = new Formula("1+1");
        Formula? f2 = null;
        Assert.IsFalse(f1.Equals(f2));
    }

    /// <summary>
    ///   Tests when Equals called on diff type
    /// </summary>
    [TestMethod]
    public void Equals_WrongTypeObject_False()
    {
        var f1 = new Formula("1+1");
        object f2 = "1+1";
        Assert.IsFalse(f1.Equals(f2));
    }

    /// <summary>
    ///   Tests when two formulas with only numbers are equal, then their hashcode should be equal.
    /// </summary>
    [TestMethod]
    public void EqualsAndGetHashCode_SameFormulaWithNum_True()
    {
        var f1 = new Formula(" 1e2+   1   ");
        var f2 = new Formula("100 + 1.000");
        Assert.IsTrue(f1.Equals(f2));
        Assert.IsTrue(f1.GetHashCode() == f2.GetHashCode());
    }

    /// <summary>
    ///   Tests when two formulas with variables are equal, then their hashcode should be equal.
    /// </summary>
    [TestMethod]
    public void EqualsAndGetHashCode_SameFormulaWithVariable_True()
    {
        var f1 = new Formula(" A1+   1   ");
        var f2 = new Formula("a1 + 1e+0");
        Assert.IsTrue(f1.Equals(f2));
        Assert.IsTrue(f1.GetHashCode() == f2.GetHashCode());
    }

    // --- Tests for Evaluate ---

    /// <summary>
    ///   Tests when denominator is number 0.
    /// </summary>
    [TestMethod]
    public void Evaluate_NumDivisionByZero_FormulaError()
    {
        var f = new Formula("1 / 0");
        var res = f.Evaluate(s => 0);
        var formulaError = (FormulaError) res;
        var msg = formulaError.Reason;

        Assert.IsInstanceOfType(res, typeof(FormulaError));
        Assert.AreEqual("Division by zero.", msg);
    }

    /// <summary>
    ///   Tests when denominator is a variable with 0 value.
    /// </summary>
    [TestMethod]
    public void Evaluate_NumDivisionByZeroVar_FormulaError()
    {
        var f = new Formula("3 / A1");
        var res = f.Evaluate(s => 0);
        var formulaError = (FormulaError) res;
        var msg = formulaError.Reason;

        Assert.IsInstanceOfType(res, typeof(FormulaError));
        Assert.AreEqual("Division by zero.", msg);
    }

    /// <summary>
    ///   Tests when a variable divided by number 0.
    /// </summary>
    [TestMethod]
    public void Evaluate_VarDivisionByZeroNum_FormulaError()
    {
        var f = new Formula("A1 / 0");
        var res = f.Evaluate(s => 3);
        var formulaError = (FormulaError) res;
        var msg = formulaError.Reason;

        Assert.IsInstanceOfType(res, typeof(FormulaError));
        Assert.AreEqual("Division by zero.", msg);
    }

    /// <summary>
    ///   Tests when denominator is number expression that evaluate to 0.
    /// </summary>
    [TestMethod]
    public void Evaluate_DenominatorEvaluateToZero_FormulaError()
    {
        var f = new Formula("(1 + 1) / (1 - 1)");
        var res = f.Evaluate(Lookups);
        var formulaError = (FormulaError) res;
        var msg = formulaError.Reason;

        Assert.IsInstanceOfType(res, typeof(FormulaError));
        Assert.AreEqual("Division by zero.", msg);
    }

    /// <summary>
    ///   Tests when denominator is expression with variable that evaluate to 0.
    /// </summary>
    [TestMethod]
    public void Evaluate_DenominatorWithVarEvaluateToZero_FormulaError()
    {
        var f = new Formula("(1 + 1) / (3 - A1)");
        var res = f.Evaluate(Lookups);
        Console.WriteLine(res);
        var formulaError = (FormulaError) res;
        var msg = formulaError.Reason;

        Assert.IsInstanceOfType(res, typeof(FormulaError));
        Assert.AreEqual("Division by zero.", msg);
    }

    /// <summary>
    ///   Tests undefined variable with no value.
    /// </summary>
    [TestMethod]
    public void Evaluate_UndefinedVariable_FormulaError()
    {
        var f = new Formula("A1 + B1 - C1 * D1 / Z1");
        var res = f.Evaluate(Lookups);
        Assert.IsInstanceOfType(res, typeof(FormulaError));

        var formulaError = (FormulaError) res;
        var msg = formulaError.Reason;
        Assert.AreEqual("Unknown variable.", msg);
    }

    /// <summary>
    ///   Tests four basic arithmetic operations
    /// </summary>
    [TestMethod]
    public void Evaluate_BasicNumCalc_CorrectResult()
    {
        var f1 = new Formula("1 + 1");
        var f2 = new Formula("1.5 - 1");
        var f3 = new Formula("1.5 * 2");
        var f4 = new Formula("1 / 3");

        Assert.AreEqual(1 + 1, (double) f1.Evaluate(s=>0), 1e-9);
        Assert.AreEqual(1.5 - 1, (double) f2.Evaluate(s=>0), 1e-9);
        Assert.AreEqual(1.5 * 2, (double) f3.Evaluate(s=>0), 1e-9);
        Assert.AreEqual(1 / 3d, (double) f4.Evaluate(s=>0), 1e-9);
    }

    /// <summary>
    ///   Tests repeat basic arithmetic operations
    /// </summary>
    [TestMethod]
    public void Evaluate_RepeatOperators_CorrectResult()
    {
        var f1 = new Formula("1 + 2 + 3 + 4");
        var f2 = new Formula("7.4 - 6.3 - 5.2 - 4.1");
        var f3 = new Formula("1e2 * 2e-3 * 3e4 * 4e-5");
        var f4 = new Formula("A1 / C1 / d1");

        Assert.AreEqual(1 + 2 + 3 + 4, (double) f1.Evaluate(s=>0), 1e-9);
        Assert.AreEqual(7.4 - 6.3 - 5.2 - 4.1, (double) f2.Evaluate(s=>0), 1e-9);
        Assert.AreEqual(1e2 * 2e-3 * 3e4 * 4e-5, (double) f3.Evaluate(s=>0), 1e-9);
        Assert.AreEqual(3 / 3.4 / 2e-2, (double) f4.Evaluate(Lookups), 1e-9);
    }

    /// <summary>
    ///   Tests four basic arithmetic operations without parentheses
    /// </summary>
    [TestMethod]
    public void Evaluate_MixedNumberCalcWithoutParentheses_CorrectResult()
    {
        var f = new Formula("1 + 1 / 3 - 4 * 7");

        Assert.AreEqual(1 + 1 / 3d - 4 * 7, (double) f.Evaluate(s=>0), 1e-9);
    }

    /// <summary>
    ///   Tests formula with multiple variables without parentheses
    /// </summary>
    [TestMethod]
    public void Evaluate_MixedVarCalcWithoutParentheses_CorrectResult()
    {
        var f = new Formula("A1 + B1 / C1");

        Assert.AreEqual(3 + 0 / 3.4, (double) f.Evaluate(Lookups), 1e-9);
    }

    /// <summary>
    ///   Tests complex calculation of numbers
    /// </summary>
    [TestMethod]
    public void Evaluate_MixedNumCalcWithParentheses_CorrectResult()
    {
        var f = new Formula("(1 + 1 - 8.3e+2) / (3e-3 * (7.6 - 1.4))");
        Assert.AreEqual((1 + 1 - 8.3e2) / (3e-3 * (7.6 - 1.4)), (double) f.Evaluate(s=>0), 1e-9);
    }

    /// <summary>
    ///   Tests complex calculation including variable
    /// </summary>
    [TestMethod]
    public void Evaluate_SingleVarCalcWithParentheses_CorrectResult()
    {
        var f = new Formula("(1 + 1e2) / (3 - B1)");
        Assert.AreEqual((1 + 1e2) / (3 - 0d), (double) f.Evaluate(Lookups), 1e-9);
    }

    /// <summary>
    ///   Tests complex calculation including multiple variables
    /// </summary>
    [TestMethod]
    public void Evaluate_MultiVarCalcWithParentheses_CorrectResult()
    {
        var f = new Formula("(A1 + 1) / (3 - B1) * 4.3 / ((2 + 1.6) * C1)");
        Assert.AreEqual((3 + 1) / (3 - 0d) * 4.3 / ((2 + 1.6) * 3.4), (double) f.Evaluate(Lookups), 1e-9);
    }

    /// <summary>
    ///   Tests very large number calculation precision
    /// </summary>
    [TestMethod]
    public void Evaluate_MaxEdgeCase_CorrectResult()
    {
        var f = new Formula("1.7976931348623157E+308 + .7976931348623157E+308");
        Assert.AreEqual(1.7976931348623157E+308 + .7976931348623157E+308, (double)f.Evaluate(s => 0), 1e-9);
    }

    /// <summary>
    ///   Tests very small number calculation precision
    /// </summary>
    [TestMethod]
    public void Evaluate_MinEdgeCase_CorrectResult()
    {
        var f = new Formula("1e-1000 + 2e-1000");
        Assert.AreEqual(1e-1000 + 2e-1000, (double)f.Evaluate(s => 0), 1e-9);
    }

    /// <summary>
    ///   Universal Lookup used for tests
    /// </summary>
    /// <param name="variable">a valid variable to lookup</param>
    /// <returns>corresponding value</returns>
    /// <exception cref="ArgumentException">if not such value exist for a variable</exception>
    private static double Lookups(string variable)
    {
        return variable switch
        {
            "A1" => 3,
            "B1" => 0,
            "C1" => 3.4,
            "D1" => 2e-2,
            _ => throw new ArgumentException("undefined variable")
        };
    }
}