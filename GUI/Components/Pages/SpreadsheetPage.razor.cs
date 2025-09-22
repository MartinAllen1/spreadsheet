// <copyright file="SpreadsheetPage.razor.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>

// Modified by Kate Wang, Martin Allen Spring 2025

using CS3500.Formula;
using CS3500.Spreadsheet;

namespace GUI.Client.Pages;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

/// <summary>
/// Web page that shows a functional spreadsheet
/// </summary>
public partial class SpreadsheetPage
{
    /// <summary>
    /// Holds all spreadsheet data.
    /// </summary>
    private static Spreadsheet _spreadsheet = new ();

    /// <summary>
    /// Converts a cell name to it's row x column position.
    /// </summary>
    private static Dictionary<string, (int, int)> RowCol { get; } = new ();

    /// <summary>
    /// Number of rows in the spreadsheet.
    /// </summary>
    private static int _rows = 50;

    /// <summary>
    /// Number of columns, which will be labeled A-Z.
    /// </summary>
    private static int _cols = 26;

    /// <summary>
    /// Provides an easy way to convert from an index to a letter (0 -> A)
    /// </summary>
    private char[] Alphabet { get; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    /// <summary>
    ///   Gets or sets the name of the file to be saved.
    /// </summary>
    private string FileSaveName { get; set; } = "Spreadsheet.sprd";

    /// <summary>
    ///   <para> Gets or sets the data for all the cells in the spreadsheet GUI. </para>
    ///   <remarks>Backing Store for HTML</remarks>
    /// </summary>
    private Dictionary<(int, int), string> CellsValue { get; set; } = new();

    /// <summary>
    /// The name of the currently selected cell.
    /// The default starting cell is A1.
    /// </summary>
    private static string _currentCell = "A1";

    /// <summary>
    /// Holds the reference to the cell contents HTML element.
    /// </summary>
    private ElementReference _contentEntryBox;

    /// <summary>
    /// The current value of the cell.
    /// It is displayed to the user in the selected cell.
    /// </summary>
    private string _cellContentValue = string.Empty;

    /// <summary>
    /// The current row of the selected cell.
    /// </summary>
    private static int _currentRow;

    /// <summary>
    /// The current column of the selected cell.
    /// </summary>
    private static int _currentColumn;

    /// <summary>
    /// Error message used to display to user.
    /// </summary>
    private string _errMsg = string.Empty;

    /// <summary>
    /// Toggles displaying error messages to user.
    /// </summary>
    private bool _showErr;

    /// <summary>
    /// Virtualize UI render range
    /// </summary>
    private int[] RenderedRows => Enumerable.Range(0, _rows).ToArray();

    /// <summary>
    /// A reference to this object for javascript invocation.
    /// </summary>
    private DotNetObjectReference<SpreadsheetPage>? _jsHandler;

    /// <summary>
    /// Checks if the key listener been registered.
    /// </summary>
    private bool _isKeyListenerAdded;

    /// <summary>
    /// Toggle for hover function.
    /// </summary>
    private bool _isHoverable;

    /// <summary>
    /// Used to avoid errors when restoring position.
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Ensures data is initialized before rendering anything.
    /// </summary>
   protected override void OnInitialized()
   {
       // restore data when page reload (not when restarting application)
       if (_spreadsheet.GetNamesOfAllNonemptyCells().Count == 0) return;
       _cellContentValue = Spreadsheet.SetStringForm(_spreadsheet.GetCellContents(_currentCell));
       UpdateCellValue(_spreadsheet.GetNamesOfAllNonemptyCells().ToList());
   }

    /// <summary>
    /// Ensures data is initialized before rendering anything.
    /// </summary>
    /// <param name="firstRender">ensure it only performed once</param>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // setting default cell and focus
            RowCol["A1"] = (0,0);
            await _contentEntryBox.FocusAsync();

            // creating a reference for javascript interaction
            _jsHandler = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("infiniteScroll", _jsHandler);

            if (!_isKeyListenerAdded)
            {
                await JSRuntime.InvokeVoidAsync("addKeyPressListener", _jsHandler);
                _isKeyListenerAdded = true;
            }
        }

        try
        {
            await Task.Delay(50);
            if (!_disposed)
                await JSRuntime.InvokeVoidAsync("restoreScrollPosition");
        }
        catch (Exception)
        {
            _disposed = true;
        }

    }

    /// <summary>
    /// Dispose the DotNetObjectReference after its lifecycle to prevent memory leaks.
    /// </summary>
    public void Dispose()
    {
        _jsHandler?.Dispose();
    }

    /// <summary>
    /// Adds a new row to spreadsheet.
    /// </summary>
    /// <returns>success status</returns>
    [JSInvokable]
    public Task AddRow()
    {
        _rows++;
        InvokeAsync(StateHasChanged);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Add a new column to spreadsheet.
    /// </summary>
    /// <returns>success status</returns>
    [JSInvokable]
    public Task AddCol()
    {
        _cols++;
        InvokeAsync(StateHasChanged);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handler for when a cell is clicked.
    /// </summary>
    /// <param name="row">The row component of the cell's coordinates</param>
    /// <param name="col">The column component of the cell's coordinates</param>
    private async Task CellClicked( int row, int col )
    {
        _currentRow = row;
        _currentColumn = col;
        _currentCell = ColToAlphabet(_currentColumn) + (_currentRow+1); // displays the currently selected cell
        RowCol[_currentCell] = (_currentRow, _currentColumn);

        await JSRuntime.InvokeVoidAsync("localStorage.setItem", "selectedCell", _currentCell);

        await _contentEntryBox.FocusAsync();
        _cellContentValue = Spreadsheet.SetStringForm(_spreadsheet.GetCellContents(_currentCell));
    }

    /// <summary>
    /// Add alternative keys to spreadsheet editing.
    /// </summary>
    /// <param name="key">the key that user pressed</param>
    [JSInvokable]
    public async Task HandleKeyPress(string key)
    {
        switch (key)
        {
            case "ArrowLeft":
                _currentColumn = (_currentColumn > 1) ? _currentColumn - 1 : 0;
                break;
            case "ArrowRight":
                _currentColumn += 1;
                break;
            case "ArrowUp":
                _currentRow = (_currentRow > 1) ? _currentRow - 1 : 0;
                break;
            case "ArrowDown":
                _currentRow += 1;
                break;
            case "Escape":
                _cellContentValue = string.Empty;
                UpdateCellValue();
                break;
        }

        _currentCell = ColToAlphabet(_currentColumn) + (_currentRow+1);
        RowCol[_currentCell] = (_currentRow, _currentColumn);

        _cellContentValue = Spreadsheet.SetStringForm(_spreadsheet.GetCellContents(_currentCell));

        await _contentEntryBox.FocusAsync();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Disables the hover function.
    /// </summary>
    [JSInvokable]
    public void DisableHover()
    {
        _isHoverable = false;
        StateHasChanged();
    }

    /// <summary>
    /// Enables the hover function.
    /// </summary>
    [JSInvokable]
    public void EnableHover()
    {
        _isHoverable = true;
        StateHasChanged();
    }

    /// <summary>
    /// Highlights the cell that is currently being edited.
    /// </summary>
    /// <param name="row">the row of the selected cell</param>
    /// <param name="col">the column of the selected cell</param>
    /// <returns></returns>
    private string CellOnClickHighlighter(int row, int col)
    {
        return row == _currentRow && col == _currentColumn ? "active-cell m-1 z-2" : "m-1";
    }

    /// <summary>
    /// Updates the cell when contents have changed.
    /// </summary>
    /// <param name="e"></param>
    private Task OnContentChanged(ChangeEventArgs e)
    {
        try
        {
            _cellContentValue =
                e.Value?.ToString() ??
                string.Empty; // grabs the string the user provided
            UpdateCellValue();
            if (_showErr)
                DismissAlert();
        }
        catch (FormulaFormatException exception)
        {
            _errMsg = exception.Message;
            _showErr = true;
            StateHasChanged();
        }
        catch (SpreadsheetReadWriteException exception)
        {
            _errMsg = exception.Message;
            _showErr = true;
            StateHasChanged();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Helper method for updating cell values.
    /// </summary>
    /// <param name="list">list with non-empty cells been store if null and with dependencies if not null</param>
    /// <param name="isLoad">if this helper is use for loading file or not</param>
    private void UpdateCellValue(IList<string>? list = null, bool isLoad = false)
    {
        if (isLoad)
        {
            RowCol.Clear();
            CellsValue.Clear();
        }

        list ??= _spreadsheet.SetContentsOfCell(_currentCell, _cellContentValue);

        // displays the updated data in the cells
        foreach (var cell in list)
        {
            var value = _spreadsheet[cell];
            var coordinate = isLoad ? SetCoord(cell) : RowCol[cell];

            if (value is FormulaError err)
                CellsValue[coordinate] = err.Reason;
            else
                CellsValue[coordinate] = value.ToString() ?? string.Empty;
        }
    }

    /// <summary>
    /// Dismisses alert and refocuses to content box.
    /// </summary>
    private void DismissAlert()
    {
        _showErr = false;
        _contentEntryBox.FocusAsync();
    }

    /// <summary>
    /// Helper for getting coordinate in view for a cell.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private (int, int) SetCoord(string s)
    {
        var index = s.IndexOfAny("0123456789".ToCharArray());
        var colStr = s[..index];// AAA
        var rowStr = s[index..]; //111

        var col = AlphabetToColNum(colStr);
        var row = int.Parse(rowStr) - 1;
        RowCol[s] = (row, col);

        return (row, col);
    }

    /// <summary>
    /// Translates a cell name to column number.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private int AlphabetToColNum(string s)
    {
        var num = (s.Length - 1) * 26;
        foreach (var t in s)
            num += t - 65;
        return num;
    }

    /// <summary>
    /// Translates a column number to a cell name.
    /// </summary>
    /// <param name="n">column number</param>
    /// <returns></returns>
    private string ColToAlphabet(int n)
    {
        var reverseStr = "";
        while (n >= 0)
        {
            var r = n % 26;
            reverseStr += Alphabet[r];
            n = n / 26 - 1;
        }
        return string.Join("", reverseStr.Reverse());
    }

    /// <summary>
    /// Saves the current spreadsheet by providing a download of a file
    /// containing the json representation of the spreadsheet.
    /// </summary>
    private async Task SaveFile()
    {
        var data = _spreadsheet.Save();
        await JSRuntime.InvokeVoidAsync("downloadFile", FileSaveName, data);
    }

    /// <summary>
    /// This method will run when the file chooser is used, for loading a file.
    /// Uploads a file containing a json representation of a spreadsheet, and
    /// replaces the current sheet with the loaded one.
    /// </summary>
    /// <param name="args">The event arguments, which contains the selected file name</param>
    private async Task HandleFileChooser( EventArgs args )
    {
        try
        {
            var eventArgs = args as InputFileChangeEventArgs ?? throw new Exception("unable to get file name");
            if (eventArgs.FileCount == 1)
            {
                var file = eventArgs.File;

                await using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);

                var fileContent = await reader.ReadToEndAsync();

                _spreadsheet = Spreadsheet.LoadJson(fileContent);

                UpdateCellValue(_spreadsheet.GetNamesOfAllNonemptyCells().ToList(), true);

                StateHasChanged();
            }
        }
        catch (SpreadsheetReadWriteException e)
        {
            _errMsg = e.Message;
            _showErr = true;
            StateHasChanged();
        }
        catch (Exception)
        {
            _errMsg = "An error occurred while loading the file.";
            _showErr = true;
            StateHasChanged();
        }
    }
}
