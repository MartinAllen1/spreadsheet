// <copyright file="Spreadsheet.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>

// Written by Joe Zachary for CS 3500, September 2013
// Update by Profs Kopta and de St. Germain, Fall 2021, Fall 2024
//     - Updated return types
//     - Updated documentation
// Modified by Kate Wang, Martin Allen Spring 2025

namespace CS3500.Spreadsheet;

using CS3500.Formula;
using CS3500.DependencyGraph;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
///   <para>
///     Thrown to indicate that a change to a cell will cause a circular dependency.
///   </para>
/// </summary>
public class CircularException : Exception
{
}

/// <summary>
///   <para>
///     Thrown to indicate that a name parameter was invalid.
///   </para>
/// </summary>
public class InvalidNameException : Exception
{
}

/// <summary>
/// <para>
///   Thrown to indicate that a read or write attempt has failed with
///   an expected error message informing the user of what went wrong.
/// </para>
/// </summary>
public class SpreadsheetReadWriteException : Exception
{
  /// <summary>
  ///   <para>
  ///     Creates the exception with a message defining what went wrong.
  ///   </para>
  /// </summary>
  /// <param name="msg"> An informative message to the user. </param>
  public SpreadsheetReadWriteException( string msg )
    : base( msg )
  {
  }
}

/// <summary>
///   <para>
///     A Spreadsheet object represents the state of a simple spreadsheet.  A
///     spreadsheet represents an infinite number of named cells.
///   </para>
/// </summary>
public class Spreadsheet
{

  /// <summary>
  ///   keeps track of non-empty cells
  /// </summary>
  [JsonInclude]
  private Dictionary<string, Cell> Cells { get; set; }

  /// <summary>
  ///   keeps track of all the dependencies
  /// </summary>
  private DependencyGraph Dependencies { get; }

  /// <summary>
  /// A Cell object represents a cell block in a spreadsheet.
  /// </summary>
  private class Cell
  {
    /// <summary>
    /// Content of a cell, can be either a double, string, or Formula
    /// </summary>
    internal object Content { get; }

    /// <summary>
    /// String Form of the cell content
    /// </summary>
    [JsonInclude]
    internal string StringForm { get; private set; }

    /// <summary>
    /// Value of a cell, can be a string, double, or FormulaError
    /// </summary>
    internal object Value { get; }

    /// <summary>
    /// parameterized constructor for constructing cells internally
    /// </summary>
    /// <param name="content">content of the cell</param>
    /// <param name="lookup">cell value lookups</param>
    public Cell(object content, Lookup lookup)
    {
      Content = content;
      StringForm = SetStringForm(content);
      Value = content switch
      {
        double d => d,
        Formula f => f.Evaluate(lookup),
        _ => (string) content
      };
    }

    /// <summary>
    /// Constructs cell from a string form with others as default values.
    /// </summary>
    /// <param name="stringForm"></param>
    [JsonConstructor]
    public Cell(string stringForm)
    {
      Content = string.Empty;
      StringForm = stringForm;
      Value = string.Empty;
    }
  }

  /// <summary>
  ///   <para>
  ///     Return the value of the named cell, as defined by
  ///     <see cref="GetCellValue(string)"/>.
  ///   </para>
  /// </summary>
  /// <param name="name"> The cell in question. </param>
  /// <returns>
  ///   <see cref="GetCellValue(string)"/>
  /// </returns>
  /// <exception cref="InvalidNameException">
  ///   If the provided name is invalid, throws an InvalidNameException.
  /// </exception>
  public object this[string name] => GetCellValue(name);

  /// <summary>
  /// True if this spreadsheet has been changed since it was
  /// created or saved (whichever happened most recently),
  /// False otherwise.
  /// </summary>
  [JsonIgnore]
  public bool Changed { get; private set; }

  /// <summary>
  /// Default constructor with default values
  /// </summary>
  public Spreadsheet()
  {
    Cells = new Dictionary<string, Cell>();
    Dependencies = new DependencyGraph();
    Changed = false;
  }

  /// <summary>
  /// Constructs a spreadsheet using the saved data in a json format
  /// <see cref="Save(string)"/>
  /// </summary>
  /// <exception cref="SpreadsheetReadWriteException">
  ///   Thrown if the file can not be loaded into a spreadsheet for any reason
  /// </exception>
  /// <param name="filename">The file containing the json string to load</param>
  public Spreadsheet(string filename)
  {
    try
    {
      var json = File.ReadAllText(filename);
      var spreadsheet = JsonSerializer.Deserialize<Spreadsheet>(json);

      if (spreadsheet is null)
        throw new SpreadsheetReadWriteException("File not found.");

      Cells = spreadsheet.Cells;
      Dependencies = spreadsheet.Dependencies;

      foreach (var cell in Cells)
      {
          var name = NameValidator(cell.Key);
          SetContentsOfCell(name, cell.Value.StringForm);
      }
      Changed = false;
    }
    catch (InvalidNameException)
    {
      throw new SpreadsheetReadWriteException("Invalid cell name.");
    }
    catch (FormulaFormatException)
    {
      throw new SpreadsheetReadWriteException("File contains an invalid formula.");
    }
    catch (CircularException)
    {
      throw new SpreadsheetReadWriteException("Circular dependencies detected.");
    }
    catch (Exception)
    {
      throw new SpreadsheetReadWriteException("An error occurred while loading the file.");
    }
  }

  /// <summary>
  /// Returns a Spreadsheet object created from a json string.
  /// </summary>
  /// <param name="json">a json string that represents a spreadsheet</param>
  /// <returns>Spreadsheet with data from json</returns>
  /// <exception cref="SpreadsheetReadWriteException">Thrown if there are issues with the json string.</exception>
  public static Spreadsheet LoadJson(string json)
  {
    var filename = Path.GetTempFileName();
    try
    {
      File.WriteAllText(filename, json);
      return new Spreadsheet(filename);
    }
    catch (Exception e)
    {
      throw new SpreadsheetReadWriteException(e.Message);
    }
    finally
    {
      if (File.Exists(filename))
        File.Delete(filename);
    }
  }

  /// <summary>
  /// Helper for setting string form based on a cell's content
  /// </summary>
  /// <param name="content">a cell's content</param>
  /// <returns>the string form</returns>
  public static string SetStringForm(object content)
  {
    return content switch
    {
      double d => "" + d,
      Formula f => "=" + f,
      _ => (string) content
    };
  }

  /// <summary>
  /// Saves this spreadsheet to a file
  /// </summary>
  /// <param name="filename"> The name (with path) of the file to save to.</param>
  /// <exception cref="SpreadsheetReadWriteException">
  ///   If there are any problems opening, writing, or closing the file,
  ///   the method should throw a SpreadsheetReadWriteException with an
  ///   explanatory message.
  /// </exception>
  public void Save( string filename )
  {
    try
    {
      var data = JsonSerializer.Serialize(this);
      File.WriteAllText(filename, data);
      Changed = false;
    }
    catch (Exception e)
    {
      throw new SpreadsheetReadWriteException(e.Message);
    }
	}

  /// <summary>
  /// Returns json representation of the spreadsheet.
  /// </summary>
  /// <returns></returns>
  /// <exception cref="SpreadsheetReadWriteException"></exception>
  public string Save()
  {
    try
    {
      var data = JsonSerializer.Serialize(this);
      Changed = false;
      return data;
    }
    catch (Exception e)
    {
      throw new SpreadsheetReadWriteException(e.Message);
    }
  }

  /// <summary>
  ///   <para>
  ///     Return the value of the named cell.
  ///   </para>
  /// </summary>
  /// <param name="name"> The cell in question. </param>
  /// <returns>
  ///   Returns the value (as opposed to the contents) of the named cell.  The return
  ///   value should be either a string, a double, or a CS3500.Formula.FormulaError.
  /// </returns>
  /// <exception cref="InvalidNameException">
  ///   If the provided name is invalid, throws an InvalidNameException.
  /// </exception>
  public object GetCellValue( string name )
  {
    name = NameValidator(name);
    return Cells.TryGetValue(name, out var cell) ? cell.Value : string.Empty;
  }

  /// <summary>
  /// Delegate for looking up cell numerical value if it has any.
  /// </summary>
  /// <param name="name">cell name</param>
  /// <returns></returns>
  /// <exception cref="ArgumentException">throw if no numerical value found</exception>
  private double Lookup(string name)
  {
    if (!Cells.TryGetValue(name, out var cell))
      throw new ArgumentException("unknown variable");
    if (cell.Value is double d)
      return d;
    throw new ArgumentException("unknown variable");
  }

  /// <summary>
  ///   <para>
  ///     Set the contents of the named cell to be the provided string
  ///     which will either represent (1) a string, (2) a number, or
  ///     (3) a formula (based on the prepended '=' character).
  ///   </para>
  /// </summary>
  /// <returns>
  ///   <para>
  ///     The method returns a list consisting of the name plus the names
  ///     of all other cells whose value depends, directly or indirectly,
  ///     on the named cell. The order of the list should be any order
  ///     such that if cells are re-evaluated in that order, their dependencies
  ///     are satisfied by the time they are evaluated.
  ///   </para>
  ///   <example>
  ///     For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
  ///     list {A1, B1, C1} is returned.
  ///   </example>
  /// </returns>
  /// <exception cref="InvalidNameException">
  ///     If name is invalid, throws an InvalidNameException.
  /// </exception>
  /// <exception cref="CircularException">
  ///     If a formula resulted in a circular dependency, throws CircularException.
  /// </exception>
  ///   /// <exception cref="FormulaFormatException">
  ///     If a formula format is invalid, throws FormulaFormatException.
  /// </exception>
  public IList<string> SetContentsOfCell( string name, string content )
  {
    name = NameValidator(name);
    var processedContent = ContentTypeProcessor(content);
    var oldDependees = Dependencies.GetDependees(name);

    HashSet<string> newDependees = processedContent is Formula formula ? [..formula.GetVariables()] : [];
    Dependencies.ReplaceDependees(name, newDependees);

    try
    {
      var list = processedContent switch
      {
        double d => SetCellContents(name, d),
        Formula f => SetCellContents(name, f),
        _ => SetCellContents(name, (string)processedContent)
      };

      // recalculate and update dependents cell values
      foreach (var dependent in list.Skip(1))
      {
        var cellContent = GetCellContents(dependent);
        Cells[dependent] = new Cell(cellContent, Lookup);
      }

      Changed = true;
      return list;
    }
    catch (CircularException)
    {
      // rollback to previous dependency if circular occurs
      Dependencies.ReplaceDependees(name, oldDependees);
      throw new SpreadsheetReadWriteException(
        "Circular dependencies detected.");
    }
  }

  /// <summary>
  ///   Helper for preprocessing content type.
  /// </summary>
  /// <param name="rawContent">raw content</param>
  /// <returns></returns>
  private static object ContentTypeProcessor(string rawContent)
  {
    if (rawContent.Equals(string.Empty))
      return rawContent;
    if (double.TryParse(rawContent, out var val))
      return val;
    if (rawContent[0] == '=')
      return new Formula(rawContent[1..]);
    return rawContent;
  }

  /// <summary>
  ///   Provides a copy of the normalized names of all the cells in the spreadsheet
  ///   that contain information (i.e., non-empty cells).
  /// </summary>
  /// <returns>
  ///   A set of the names of all the non-empty cells in the spreadsheet.
  /// </returns>
  public ISet<string> GetNamesOfAllNonemptyCells()
  {
    return Cells.Keys.ToHashSet();
  }

  /// <summary>
  ///   Returns the contents (as opposed to the value) of the named cell.
  /// </summary>
  ///
  /// <exception cref="InvalidNameException">
  ///   Thrown if the name is invalid.
  /// </exception>
  ///
  /// <param name="name">The name of the spreadsheet cell to query. </param>
  /// <returns>
  ///   The contents as either a string, a double, or a Formula.
  /// </returns>
  public object GetCellContents( string name )
  {
    return Cells.TryGetValue(NameValidator(name), out var cell) ? cell.Content : string.Empty;
  }

  /// <summary>
  ///  Set the contents of the named cell to the given number.
  /// </summary>
  ///
  /// <exception cref="InvalidNameException">
  ///   If the name is invalid, throw an InvalidNameException.
  /// </exception>
  ///
  /// <param name="name"> The name of the cell. </param>
  /// <param name="number"> The new contents of the cell. </param>
  /// <returns>
  ///   <para>
  ///     This method returns an ordered list consisting of the passed in name
  ///     followed by the names of all other cells whose value depends, directly
  ///     or indirectly, on the named cell.
  ///   </para>
  /// </returns>
  private IList<string> SetCellContents( string name, double number )
  {
    Cells[name] = new Cell(number, Lookup);
    return GetCellsToRecalculate(name).ToList();
  }

  /// <summary>
  ///   The contents of the named cell becomes the given text.
  /// </summary>
  ///
  /// <exception cref="InvalidNameException">
  ///   If the name is invalid, throw an InvalidNameException.
  /// </exception>
  /// <param name="name"> The name of the cell. </param>
  /// <param name="text"> The new contents of the cell. </param>
  /// <returns>
  ///   The same list as defined in <see cref="SetCellContents(string, double)"/>.
  /// </returns>
  private IList<string> SetCellContents( string name, string text )
  {
    if (text.Equals(string.Empty))
      Cells.Remove(name);
    else
      Cells[name] = new Cell(text, Lookup);

    return GetCellsToRecalculate(name).ToList();
  }

  /// <summary>
  ///   Set the contents of the named cell to the given formula.
  /// </summary>
  /// <exception cref="InvalidNameException">
  ///   If the name is invalid, throw an InvalidNameException.
  /// </exception>
  /// <exception cref="CircularException">
  ///   <para>
  ///     If changing the contents of the named cell to be the formula would
  ///     cause a circular dependency, throw a CircularException, and no
  ///     change is made to the spreadsheet.
  ///   </para>
  /// </exception>
  /// <param name="name"> The name of the cell. </param>
  /// <param name="formula"> The new contents of the cell. </param>
  /// <returns>
  ///   The same list as defined in <see cref="SetCellContents(string, double)"/>.
  /// </returns>
  private IList<string> SetCellContents( string name, Formula formula )
  {
      var list = GetCellsToRecalculate(name).ToList();
      Cells[name] = new Cell(formula, Lookup);
      return list;
  }

  /// <summary>
  ///   Helper validates name for cell.
  /// </summary>
  /// <param name="name">name of a cell</param>
  /// <returns></returns>
  /// <exception cref="InvalidNameException">the name is not in correct form</exception>
  private static string NameValidator(string name)
  {
    if (Formula.IsVar(name))
      return name.ToUpper();
    throw new InvalidNameException();
  }

  /// <summary>
  ///   Returns an enumeration, without duplicates, of the names of all cells whose
  ///   values depend directly on the value of the named cell.
  /// </summary>
  /// <param name="name"> This <b>MUST</b> be a valid name.  </param>
  /// <returns>
  ///   <para>
  ///     Returns an enumeration, without duplicates, of the names of all cells
  ///     that contain formulas containing name.
  ///   </para>
  /// </returns>
  private IEnumerable<string> GetDirectDependents( string name )
  {
    return Dependencies.GetDependents(name);
  }

  /// <summary>
  ///   <para>
  ///     Returns an enumeration of the names of all cells whose values must
  ///     be recalculated, assuming that the contents of the cell referred
  ///     to by name has changed.  The cell names are enumerated in an order
  ///     in which the calculations should be done.
  ///   </para>
  ///   <exception cref="CircularException">
  ///     If the cell referred to by name is involved in a circular dependency,
  ///     throws a CircularException.
  ///   </exception>
  /// </summary>
  /// <param name="name"> The name of the cell.  Requires that name be a valid cell name.</param>
  /// <returns>
  ///    Returns an enumeration of the names of all cells whose values must
  ///    be recalculated.
  /// </returns>
  private IEnumerable<string> GetCellsToRecalculate( string name )
  {
    LinkedList<string> changed = new();
    HashSet<string> visited = [];
    Visit( name, name, visited, changed );
    return changed;
  }

  /// <summary>
  ///   A helper for the GetCellsToRecalculate method for putting dependency in topological order of a given cell.
  /// </summary>
  /// <param name="start">cell to check</param>
  /// <param name="name">name of the given cell</param>
  /// <param name="visited">keeps track of cells checked</param>
  /// <param name="changed">list with updated topological order</param>
  /// <exception cref="CircularException">throws when a circular in dependency occurs</exception>
  private void Visit( string start, string name, ISet<string> visited, LinkedList<string> changed )
  {
    // cell that been modified
    visited.Add( name );
    foreach ( string n in GetDirectDependents( name ) )
    {
      // when a circle occurs throw exception
      if ( n.Equals( start ) )
      {
        throw new CircularException();
      }
      // if the cell not been visited
      else if ( !visited.Contains( n ) )
      {
        // recursively call to add all direct and indirect dependents of the start cell
        Visit( start, n, visited, changed );
      }
    }

    // recursively adding cells as the first element from most dependent cell to previous ones
    // making sure the linked list is in dependency order
    changed.AddFirst( name );
  }
}
