using System.Text.Json;
using CS3500.Formula;
using CS3500.Spreadsheet;

namespace SpreadsheetTests;

/// <summary>
///   Test class covers tests for Spreadsheet
/// </summary>
[TestClass]
public class SpreadsheetTests
{
    // --- GetNamesOfAllNonemptyCells ---

    /// <summary>
    ///   if a spreadsheet is empty, no cell name in the set
    /// </summary>
    [TestMethod]
    public void GetNamesOfAllNonemptyCells_EmptySpreadSheet_EmptySet()
    {
        var spreadsheet = new Spreadsheet();
        Assert.IsFalse(spreadsheet.GetNamesOfAllNonemptyCells().Any());
    }

    /// <summary>
    ///   spreadsheet nonempty, correct set elements with normalized names
    /// </summary>
    [TestMethod]
    public void GetNamesOfAllNonemptyCells_NonEmptySpreadSheet_CorrectResults()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("AaA1", "1.3");
        spreadsheet.SetContentsOfCell("b1", "2e3");
        HashSet<string> expected = ["AAA1", "B1"];
        Assert.IsTrue(expected.SetEquals(spreadsheet.GetNamesOfAllNonemptyCells()));
    }

    /// <summary>
    ///   if a cell is set to empty string, it should not be in the non-empty cells set
    /// </summary>
    [TestMethod]
    public void GetNamesOfAllNonemptyCells_EmptyStringCell_CorrectResults()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("b1", string.Empty);
        Assert.IsFalse(spreadsheet.GetNamesOfAllNonemptyCells().Any());
    }

    // --- Name Validation ---

    /// <summary>
    ///   Checks invalid cell names in each setter and getter, InvalidNameException should throw
    /// </summary>
    /// <param name="value"></param>
    [TestMethod]
    [DataRow ("string")]
    [DataRow ("123")]
    [DataRow ("Ad1fb2")]
    [DataRow ("123bC")]
    public void CellNameValidation_InvalidNamesForDouble_Throw(string value)
    {
        var spreadsheet = new Spreadsheet();
        Assert.ThrowsException<InvalidNameException>(() => spreadsheet.SetContentsOfCell(value, "1.3"));
        Assert.ThrowsException<InvalidNameException>(() => spreadsheet.SetContentsOfCell(value, "string"));
        Assert.ThrowsException<InvalidNameException>(() => spreadsheet.SetContentsOfCell(value, "=2+3"));
        Assert.ThrowsException<InvalidNameException>(() => spreadsheet.GetCellContents(value));
    }

    // --- GetCellContents ---

    /// <summary>
    /// correctly get cell content from an empty spreadsheet, default value is empty string
    /// </summary>
    [TestMethod]
    public void GetCellContents_EmptySpreadsheet_EmptyString()
    {
        var spreadsheet = new Spreadsheet();
        Assert.AreEqual(string.Empty, spreadsheet.GetCellContents("a1"));
    }

    /// <summary>
    ///   Tests name variations for a single cell, the content should be equal
    /// </summary>
    [TestMethod]
    public void GetCellContents_CellNameVariation_Equal()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("aaA1", "1.3");
        var lowercase = spreadsheet.GetCellContents("aaa1");
        var uppercase = spreadsheet.GetCellContents("AAA1");

        Assert.AreEqual(lowercase, uppercase);
    }

    /// <summary>
    ///   correctly sets and gets string content
    /// </summary>
    [TestMethod]
    public void GetCellContents_StringContent_CorrectlyReturn()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("A1", "string");
        Assert.AreEqual("string", spreadsheet.GetCellContents("a1"));
    }

    /// <summary>
    ///   correctly sets and gets double content
    /// </summary>
    [TestMethod]
    public void GetCellContents_DoubleContent_CorrectlyReturn()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("A1", "3.2");
        Assert.AreEqual(3.2, spreadsheet.GetCellContents("a1"));
    }

    /// <summary>
    ///   correctly sets and gets formula content
    /// </summary>
    [TestMethod]
    public void GetCellContents_FormulaContent_CorrectlyReturn()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("A1", "=1.3+2");
        Assert.IsInstanceOfType(spreadsheet.GetCellContents("a1"), typeof(Formula));
        Assert.AreEqual("1.3+2", spreadsheet.GetCellContents("a1").ToString());
    }

    // --- SetContentsOfCell ---

    /// <summary>
    ///   correctly update double content
    /// </summary>
    [TestMethod]
    public void SetContentsOfCell_DoubleContent_CorrectlyUpdated()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "1.3");
        spreadsheet.SetContentsOfCell("A1", "2.3");
        Assert.AreEqual(2.3, spreadsheet.GetCellContents("A1"));
    }

    /// <summary>
    ///   correctly update string content
    /// </summary>
    [TestMethod]
    public void SetContentsOfCell_StringContent_CorrectlyUpdated()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "string");
        spreadsheet.SetContentsOfCell("A1", "value");
        Assert.AreEqual("value", spreadsheet.GetCellContents("A1"));
    }

    /// <summary>
    ///   correctly clear cell from spreadsheet if it's an empty string
    /// </summary>
    [TestMethod]
    public void SetContentsOfCell_EmptyStringCell_CorrectlyUpdated()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("b1", "3.1");
        spreadsheet.SetContentsOfCell("b1", string.Empty);
        Assert.IsFalse(spreadsheet.GetNamesOfAllNonemptyCells().Any());
    }

    /// <summary>
    ///   invalid formula format should throw FormulaFormatException
    /// </summary>
    [TestMethod]
    [ExpectedException( typeof( FormulaFormatException ))]
    public void SetContentsOfCell_InvalidFormulaContent_Invalid()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "=invalid formula");
    }

    /// <summary>
    ///   directly circle to a cell itself should throw CircularException
    /// </summary>
    [TestMethod]
    [ExpectedException( typeof( CircularException ))]
    public void SetContentsOfCell_DirectCircularFormulaContent_Invalid()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "=a1+3");
    }

    /// <summary>
    ///   if circulation occurs when set a new cell, not adding it to spreadsheet non-empty cell set
    /// </summary>
    [TestMethod]
    public void SetContentsOfCell_DirectCircular_NotAddingToDictionary()
    {
        var spreadsheet = new Spreadsheet();
        try
        {
            spreadsheet.SetContentsOfCell("a1", "=a1+3");
        }
        catch (CircularException)
        {
            Assert.IsFalse(spreadsheet.GetNamesOfAllNonemptyCells().Any());
        }
    }

    /// <summary>
    ///   if circulation occurs when updating a cell from double to formula, cell remain unchanged
    /// </summary>
    [TestMethod]
    public void SetContentsOfCell_CircularFormulaContentRollBackToDouble_NoChange()
    {
        var spreadsheet = new Spreadsheet();
        try
        {
            spreadsheet.SetContentsOfCell("a1", "3");
            spreadsheet.SetContentsOfCell("b1", "=a1+3");
            spreadsheet.SetContentsOfCell("a1", "=b1+3");
        }
        catch (CircularException)
        {
            Assert.AreEqual(3d, spreadsheet.GetCellContents("a1"));
        }
    }

    /// <summary>
    ///   if circulation occurs when updating a cell from string to formula, cell remain unchanged
    /// </summary>
    [TestMethod]
    public void SetContentsOfCell_CircularFormulaContentRollBackToString_NoChange()
    {
        var spreadsheet = new Spreadsheet();
        try
        {
            spreadsheet.SetContentsOfCell("a1", "hello");
            spreadsheet.SetContentsOfCell("b1", "=a1+3");
            spreadsheet.SetContentsOfCell("a1", "=b1+3");
        }
        catch (CircularException)
        {
            Assert.IsTrue("hello".Equals(spreadsheet.GetCellContents("a1")));
        }
    }

    /// <summary>
    ///   if circulation occurs when updating a cell from formula to formula, cell remain unchanged
    /// </summary>
    [TestMethod]
    public void SetContentsOfCell_CircularFormulaContentRollBackToFormula_NoChange()
    {
        var spreadsheet = new Spreadsheet();
        try
        {
            spreadsheet.SetContentsOfCell("a1", "=3+2");
            spreadsheet.SetContentsOfCell("b1", "=a1+3");
            spreadsheet.SetContentsOfCell("a1", "=b1+3");
        }
        catch (CircularException)
        {
            Assert.IsTrue("3+2".Equals(spreadsheet.GetCellContents("a1").ToString()));
        }
    }

    /// <summary>
    ///   indirectly circulation occurs, throw CircularException
    /// </summary>
    [TestMethod]
    [ExpectedException( typeof( CircularException ))]
    public void SetContentsOfCell_IndirectCircularFormulaContent_Invalid()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "=b1+1");
        spreadsheet.SetContentsOfCell("b1", "=c1+1");
        spreadsheet.SetContentsOfCell("c1", "=a1+1");
    }

    /// <summary>
    ///   the dependency of a given cell should be in topological order
    /// </summary>
    [TestMethod]
    public void SetContentsOfCell_ValidFormulaDependency_CorrectOrder()
    {
        var spreadsheet = new Spreadsheet();

        spreadsheet.SetContentsOfCell("b1", "=a1+2");
        spreadsheet.SetContentsOfCell("c1", "=a1+b1");
        spreadsheet.SetContentsOfCell("d1", "=c1+a1");

        var list = spreadsheet.SetContentsOfCell("a1", "4.1");
        List<string> expected = ["A1", "B1", "C1", "D1"];

        Assert.IsTrue(expected.SequenceEqual(list));
    }

    /// <summary>
    /// even when a cell update to empty string, its dependents should be the same
    /// </summary>
    [TestMethod]
    public void SetContentsOfCell_EmptyStringCell_CorrectOrder()
    {
        var spreadsheet = new Spreadsheet();

        spreadsheet.SetContentsOfCell("b1", "=a1+2");
        spreadsheet.SetContentsOfCell("c1", "=a1+b1");
        spreadsheet.SetContentsOfCell("d1", "=c1+a1");

        var list = spreadsheet.SetContentsOfCell("a1", "");
        List<string> expected = ["A1", "B1", "C1", "D1"];

        Assert.IsTrue(expected.SequenceEqual(list));
    }

    /// <summary>
    ///   the dependents should update correctly when a formula update to a string
    /// </summary>
    [TestMethod]
    public void SetContentsOfCell_FormulaUpdateToString_UpdateDependency()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "=1+2");
        spreadsheet.SetContentsOfCell("b1", "=a1+3");
        spreadsheet.SetContentsOfCell("c1", "=a1+b1");

        var list = spreadsheet.SetContentsOfCell("a1", "");
        List<string> expected = ["A1", "B1", "C1"];

        Assert.IsTrue(expected.SequenceEqual(list));
    }

    /// <summary>
    ///   the dependents should update correctly when a formula update to a double
    /// </summary>
    [TestMethod]
    public void SetContentsOfCell_FormulaUpdateToDouble_UpdateDependency()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "=1+2");
        spreadsheet.SetContentsOfCell("b1", "=a1+3");
        spreadsheet.SetContentsOfCell("c1", "=a1+b1");

        var list = spreadsheet.SetContentsOfCell("a1", "3.1");
        List<string> expected = ["A1", "B1", "C1"];

        Assert.IsTrue(expected.SequenceEqual(list));
    }

    /// <summary>
    ///   the dependents should update correctly when a formula update to another formula
    /// </summary>
    [TestMethod]
    public void SetContentsOfCell_FormulaUpdateToFormula_UpdateDependency()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "=1+2");
        spreadsheet.SetContentsOfCell("b1", "=a1+3");
        spreadsheet.SetContentsOfCell("c1", "=a1+4");

        var list = spreadsheet.SetContentsOfCell("b1", "=a1+c1");
        List<string> expected = ["B1"];
        Assert.IsTrue(expected.SequenceEqual(list));
    }

    // --- GetCellValue and Spreadsheet Getter---

    /// <summary>
    /// correctly get cell value from an empty spreadsheet, default value is empty string
    /// </summary>
    [TestMethod]
    public void GetCellValue_EmptySpreadsheet_EmptyString()
    {
        var spreadsheet = new Spreadsheet();
        Assert.AreEqual(string.Empty, spreadsheet["a1"]);
    }

    /// <summary>
    /// get cell value from a cell with empty string content
    /// </summary>
    [TestMethod]
    public void GetCellValue_EmptyStringContent_EmptyString()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", string.Empty);
        Assert.AreEqual(string.Empty, spreadsheet["a1"]);
    }

    /// <summary>
    /// correctly get cell value from a cell with string content
    /// </summary>
    [TestMethod]
    public void GetCellValue_StringContent_String()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "string");
        Assert.AreEqual("string", spreadsheet["a1"]);
    }

    /// <summary>
    /// correctly get cell value from a cell with double content
    /// </summary>
    [TestMethod]
    public void GetCellValue_DoubleContent_Double()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "3.5");
        Assert.AreEqual(3.5, spreadsheet["a1"]);
    }

    /// <summary>
    /// correctly get cell value from a cell with formula content that evaluates to double
    /// </summary>
    [TestMethod]
    public void GetCellValue_ValidFormulaContent_Double()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "3.5");
        spreadsheet.SetContentsOfCell("b1", "=a1+4");
        Assert.AreEqual(7.5, spreadsheet["b1"]);
    }

    /// <summary>
    /// correctly get cell value from a chain of formula content that evaluates to double
    /// </summary>
    [TestMethod]
    public void GetCellValue_MultiDependencyContent_Double()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "3.5");
        spreadsheet.SetContentsOfCell("b1", "=a1+4");
        spreadsheet.SetContentsOfCell("c1", "=b1+a1");
        Assert.AreEqual(11d, spreadsheet["c1"]);
    }

    /// <summary>
    /// unknown variable in a formula should return FormulaError
    /// </summary>
    [TestMethod]
    public void GetCellValue_UnknownCell_FormulaError()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "=b1+3");
        Assert.IsTrue(spreadsheet["a1"] is FormulaError);
    }

    /// <summary>
    /// if cell has string value it is unknown variable that should return FormulaError
    /// </summary>
    [TestMethod]
    public void GetCellValue_MultiDependencyUnknownContent_FormulaError()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "3.5");
        spreadsheet.SetContentsOfCell("b1", "=a1+4");
        spreadsheet.SetContentsOfCell("c1", "string");
        spreadsheet.SetContentsOfCell("d1", "=c1+b1");
        Assert.IsTrue(spreadsheet["d1"] is FormulaError);
    }

    /// <summary>
    /// cell update to new double and dependents' value should update correctly
    /// </summary>
    [TestMethod]
    public void GetCellValue_ChainedDoubleValues_UpdateSuccessfully()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "3.5");
        spreadsheet.SetContentsOfCell("b1", "=a1+4");
        spreadsheet.SetContentsOfCell("c1", "=b1+3");
        spreadsheet.SetContentsOfCell("d1", "=c1+b1");

        spreadsheet.SetContentsOfCell("a1", "1.5");
        Assert.AreEqual(1.5, spreadsheet["a1"]);
        Assert.AreEqual(5.5, spreadsheet["b1"]);
        Assert.AreEqual(8.5, spreadsheet["c1"]);
        Assert.AreEqual(14d, spreadsheet["d1"]);
    }

    /// <summary>
    /// cell update to new string and dependents' value should update to FormulaError
    /// </summary>
    [TestMethod]
    public void GetCellValue_ChainedStringValues_UpdateSuccessfully()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "3.5");
        spreadsheet.SetContentsOfCell("b1", "=a1+4");
        spreadsheet.SetContentsOfCell("c1", "=b1+3");
        spreadsheet.SetContentsOfCell("d1", "=c1+b1");

        spreadsheet.SetContentsOfCell("a1", "string");
        Assert.IsTrue(spreadsheet["b1"] is FormulaError);
        Assert.IsTrue(spreadsheet["c1"] is FormulaError);
        Assert.IsTrue(spreadsheet["d1"] is FormulaError);
    }

    /// <summary>
    /// cell update to new Formula and dependents' value should also update correspondingly
    /// </summary>
    [TestMethod]
    public void GetCellValue_ChainedFormulaValues_UpdateSuccessfully()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "3.5");
        spreadsheet.SetContentsOfCell("b1", "=a1+4");
        spreadsheet.SetContentsOfCell("c1", "=b1+3");
        spreadsheet.SetContentsOfCell("d1", "=c1+b1");

        spreadsheet.SetContentsOfCell("b1", "=a1+0.5");
        Assert.AreEqual(3.5, spreadsheet["a1"]);
        Assert.AreEqual(4d, spreadsheet["b1"]);
        Assert.AreEqual(7d, spreadsheet["c1"]);
        Assert.AreEqual(11d, spreadsheet["d1"]);
    }

    /// <summary>
    /// cell name is invalid, InvalidNameException should throw
    /// </summary>
    [TestMethod]
    [ExpectedException( typeof( InvalidNameException ))]
    public void GetCellValue_InvalidCellName_Invalid()
    {
        var spreadsheet = new Spreadsheet();
        _ = spreadsheet["a1c"];
    }

    // --- Changed Status---

    /// <summary>
    /// After construction, the changed status should be false
    /// </summary>
    [TestMethod]
    public void Changed_ConstructByDefault_False()
    {
        var spreadsheet = new Spreadsheet();
        Assert.IsFalse(spreadsheet.Changed);
    }

    /// <summary>
    /// After construction by JSON, the changed status should be false
    /// </summary>
    [TestMethod]
    public void Changed_ConstructByJSON_False()
    {
        const string filename = "testFile";
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "=1+3");
        spreadsheet.SetContentsOfCell("b1", "3.1");
        spreadsheet.SetContentsOfCell("c1", "hello");
        spreadsheet.Save(filename);

        var newSpreadsheet = new Spreadsheet(filename);
        Assert.IsFalse(newSpreadsheet.Changed);
    }

    /// <summary>
    /// After a change is made, the changed status should be true
    /// </summary>
    [TestMethod]
    public void Changed_ChangeMadeAfterConstruction_True()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "=1+3");
        spreadsheet.SetContentsOfCell("b1", "3.1");
        spreadsheet.SetContentsOfCell("c1", "hello");
        Assert.IsTrue(spreadsheet.Changed);
    }

    /// <summary>
    /// After a circular exception occurs, the changed status should be the last state
    /// </summary>
    [TestMethod]
    public void Changed_NoChangeAfterCirculationOccurs_False()
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "=1+3");
        spreadsheet.SetContentsOfCell("b1", "=a1+3.1");
        spreadsheet.SetContentsOfCell("c1", "hello");

        var lastState = spreadsheet.Changed;

        try
        {
            spreadsheet.SetContentsOfCell("a1", "=b1+3");
        }
        catch (CircularException)
        {
            Assert.AreEqual(lastState, spreadsheet.Changed);
        }
    }

    // --- Save To File ---

    /// <summary>
    /// invalid file or path should throw SpreadsheetReadWriteException
    /// </summary>
    [TestMethod]
    [DataRow("/some/nonsense/path.txt")]
    [DataRow("")]
    [ExpectedException(typeof( SpreadsheetReadWriteException ))]
    public void Save_InvalidFileOrPath_Invalid(string value)
    {
        var spreadsheet = new Spreadsheet();
        spreadsheet.Save(value);
    }

    /// <summary>
    /// test if the file is saved correctly
    /// </summary>
    [TestMethod]
    public void Save_EmptySpreadsheet_Valid()
    {
        const string filename = "testFile";
        var spreadsheet = new Spreadsheet();
        spreadsheet.Save(filename);
        Assert.AreEqual(JsonSerializer.Serialize(spreadsheet), File.ReadAllText(filename));
        File.Delete(filename);
    }

    /// <summary>
    /// test if the file is saved in the right string form
    /// </summary>
    [TestMethod]
    public void Save_ValidFileName_SavedInRightStringForm()
    {
        const string filename = "testFile";
        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "=1+3");
        spreadsheet.SetContentsOfCell("b1", "3.1");
        spreadsheet.SetContentsOfCell("c1", "hello");
        spreadsheet.Save(filename);

        Assert.IsTrue(File.Exists(filename));

        var actual = File.ReadAllText(filename);
        Assert.AreEqual(JsonSerializer.Serialize(spreadsheet), actual);

        File.Delete(filename);
    }

    /// <summary>
    /// test if the file already exist, it should be overwritten.
    /// </summary>
    [TestMethod]
    public void Save_FileAlreadyExist_Override()
    {
        const string filename = "testFile";
        File.WriteAllText(filename, "old content");

        var spreadsheet = new Spreadsheet();
        spreadsheet.SetContentsOfCell("a1", "=1+3");
        spreadsheet.SetContentsOfCell("b1", "3.1");
        spreadsheet.SetContentsOfCell("c1", "hello");
        spreadsheet.Save(filename);

        var actual = File.ReadAllText(filename);
        Assert.AreEqual(JsonSerializer.Serialize(spreadsheet), actual);

        File.Delete(filename);
    }

    // --- Construct From A File ---

    /// <summary>
    /// invalid cell name should throw SpreadsheetReadWriteException when constructing from a file
    /// </summary>
    [TestMethod]
    public void SpreadsheetConstructor_InvalidCellName_Throw()
    {
        const string filename = "testFile";
        const string dataStr =
            "{\"Cells\":{\"A1\":{\"StringForm\":\"=1\\u002B3\"},\"B1A\":{\"StringForm\":\"3.1\"},\"C1\":{\"StringForm\":\"hello\"}}}";
        File.WriteAllText(filename, dataStr);

        var exception =
            Assert.ThrowsException<SpreadsheetReadWriteException>(() =>
            {
                _ = new Spreadsheet(filename);
            });

        Assert.AreEqual("invalid cell name", exception.Message);

        File.Delete(filename);
    }

    /// <summary>
    /// invalid formula format should throw SpreadsheetReadWriteException when constructing from a file
    /// </summary>
    [TestMethod]
    public void SpreadsheetConstructor_InvalidFormulaFormat_Throw()
    {
        const string filename = "testFile";
        const string dataStr =
            "{\"Cells\":{\"A1\":{\"StringForm\":\"=1\\u002B\"},\"B1\":{\"StringForm\":\"3.1\"},\"C1\":{\"StringForm\":\"hello\"}}}";
        File.WriteAllText(filename, dataStr);

        var exception =
            Assert.ThrowsException<SpreadsheetReadWriteException>(() =>
            {
                _ = new Spreadsheet(filename);
            });

        Assert.AreEqual("invalid formula format", exception.Message);

        File.Delete(filename);
    }

    /// <summary>
    /// formula circulation should throw SpreadsheetReadWriteException when constructing from a file
    /// </summary>
    [TestMethod]
    public void SpreadsheetConstructor_FormulaCirculation_Throw()
    {
        const string filename = "testFile";
        const string dataStr =
            "{\"Cells\":{\"A1\":{\"StringForm\":\"=1\\u002BB1\"},\"B1\":{\"StringForm\":\"=3.1\\u002BA1\"},\"C1\":{\"StringForm\":\"hello\"}}}";
        File.WriteAllText(filename, dataStr);

        var exception =
            Assert.ThrowsException<SpreadsheetReadWriteException>(() =>
            {
                _ = new Spreadsheet(filename);
            });

        Assert.AreEqual("formula circulation", exception.Message);

        File.Delete(filename);
    }

    /// <summary>
    /// invalid and non-existed file name or path should throw SpreadsheetReadWriteException
    /// </summary>
    /// <param name="value"></param>
    [TestMethod]
    [DataRow("/some/nonsense/path.txt")]
    [DataRow("")]
    [DataRow("/")]
    [DataRow("@")]
    [ExpectedException(typeof( SpreadsheetReadWriteException ))]
    public void SpreadsheetConstructor_InvalidFileNamesOrPaths_Throw(string value)
    {
        _ = new Spreadsheet(value);
    }

    /// <summary>
    /// invalid file contents should throw SpreadsheetReadWriteException
    /// </summary>
    /// <param name="content"></param>
    [TestMethod]
    [DataRow("{\"Cells\":{\"A1\":{\"InvalidStringForm\":\"=1\\u002BB1\"},\"B1\":{\"InvalidStringForm\":\"=3.1\\u002BA1\"},\"C1\":{\"InvalidStringForm\":\"hello\"}}}")]
    [DataRow("{\"Cells\":{\"A1\":{\"StringForm\":\"=1\\u002BB1\"}")]
    [DataRow("null")]
    [ExpectedException(typeof( SpreadsheetReadWriteException ))]
    public void SpreadsheetConstructor_InvalidFileContents_Throw(string content)
    {
        const string filename = "testFile";
        File.WriteAllText(filename, content);

        _ = new Spreadsheet(filename);

        File.Delete(filename);
    }

    /// <summary>
    /// try to read from a file when it's locked by another process
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof( SpreadsheetReadWriteException ))]
    public void SpreadsheetConstructor_IOErrors_Throw()
    {
        const string filename = "testFile";
        const string dataStr = "some content";
        File.WriteAllText(filename, dataStr);

        using (_ = File.Open(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            _ = new Spreadsheet(filename);

        File.Delete(filename);
    }

    /// <summary>
    /// valid file should construct the spreadsheet correctly with content and value restored
    /// </summary>
    [TestMethod]
    public void SpreadsheetConstructor_ValidFile_Success()
    {
        const string filename = "testFile";
        const string dataStr = "{\"Cells\":{\"A1\":{\"StringForm\":\"=1\\u002B3\"},\"B1\":{\"StringForm\":\"3.1\"},\"C1\":{\"StringForm\":\"hello\"}}}";
        File.WriteAllText(filename, dataStr);

        var spreadsheet = new Spreadsheet(filename);
        Assert.AreEqual(4d, spreadsheet["a1"]);
        Assert.AreEqual(new Formula("1+3"), spreadsheet.GetCellContents("a1"));

        File.Delete(filename);
    }

    // --- Stress Tests ---

    /// <summary>
    ///   Tests if spreadsheet can handle large data efficiently
    /// </summary>
    [TestMethod]
    [Timeout(2000)]
    public void StressTests_1()
    {
        var spreadsheet = new Spreadsheet();
        const int size = 1000;
        var rand = new Random();
        for (var i = 0; i < size; i++)
            spreadsheet.SetContentsOfCell($"A{i}", "" + rand.NextDouble());

        Assert.AreEqual(size, spreadsheet.GetNamesOfAllNonemptyCells().Count);
    }

    /// <summary>
    /// checks whether the value is stored instead of calculating on demand and correctly
    /// </summary>
    [TestMethod]
    [Timeout(2000)]
    public void StressTests_2()
    {
        var spreadsheet = new Spreadsheet();
        const int size = 100;

        spreadsheet.SetContentsOfCell("A1", "1");

        for ( var i = 2; i < size; i++ )
            spreadsheet.SetContentsOfCell( "A" + i, "=A" + ( i - 1 ) + "+1" );

        Assert.AreEqual(size - 2 + 1, spreadsheet.GetNamesOfAllNonemptyCells().Count);

        Assert.AreEqual(99d, spreadsheet["A99"]);
    }
}