# AGtk

AGtk is a library of helper classes for GTK 4 in C#.  Well, actually at the moment there's only one helper class: AGtk.ColView.  But perhaps I'll add more in the future.

## AGtk.ColView

AGtk.ColView is a subclass of the Gtk.ColumnView class, which displays a multi-column list.  Gtk.ColumnView has a complicated API, so AGtk.ColView exists to make it much easier to use.  Here's a brief code sample to show how easy it is:

```C#
AGtk.ColView col_view = new(["year", "title", "director"]);

col_view.Add([1981, "Raiders of the Lost Ark", "Steven Spielberg"]);
col_view.Add([1994, "The Shawshank Redemption", "Frank Darabont"]);
col_view.Add([1995, "Clueless", "Amy Heckerling"]);
...

my_window.Child = col_view;
```

That will produce a column view that looks like this:

![column view](col_view.png)


You can sort on any column by clicking on the column's header.  AGtk.ColView compares objects by casting them to [IComparable](https://learn.microsoft.com/en-us/dotnet/api/system.icomparable?view=net-8.0).  For this reason, currently all objects added to an AGtk.ColView must implement the IComparable interface.  (This is not too restrictive, since many useful types such as int and string do implement IComparable.)

### Adding filtering

AGtk.ColView supports dynamic filtering.  Typically you'll use this with a search box: as the user types in the box, rows will be filtered.  Here's how to do this in code:

```C#
// specify the column to use for filtering
col_view.FilterColumn = 1;   // title

SearchEntry entry = new();
entry.OnSearchChanged += on_search_changed;

...

void on_search_changed(SearchEntry sender, EventArgs args) {
    col_view.FilterText = entry.GetText();
}
```

### A larger example

My page [Introduction to GTK 4 in C#](https://ksvi.mff.cuni.cz/~dingle/2024-5/prog_2/gtk4_introduction.html) contains a [complete example]( https://ksvi.mff.cuni.cz/~dingle/2024-5/prog_2/gtk4_introduction.html#Column%20views|outline) of a program using AGtk.ColView.

## Using the library

AGtk is available as a nuget package:

```
$ dotnet add package AGtk
```

The current version number is 0.2.0.

## API reference
All members listed below are public.

### ColView

<dl>
<dt><code>ColView(params string[] names)</code></dt>
<dd>Create a ColView with the given column names.</dd>
<dt><code>void Add(params IComparable[] values)</code></dt>
<dd>Append a row of values to the view.</dd>
<dt><code>void Clear()</code></dt>
<dd>Remove all rows from the view.</dd>
<dt><code>int? FilterColumn {get; set; }</code></dt>
<dd>The 0-based index of the column used for filtering.</dd>
<dt><code>string FilterText {get; set; }</code></dt>
<dd>The text to use for filtering.  Only rows whose filter column contains this text will be shown.  If the value is "", all rows will be displayed.</dd>
<dt><code>RowList Rows {get;}</code></dt>
<dd>A list of rows in this ColView.</dd>
<dt><code>uint SelectedIndex {get;}</code></dt>
<dd>The index of the row that is currently selected.</dd>
<dt><code>event SelectionChanged? OnSelectionChanged</code></dt>
<dd>An event that fires whenever the selection changes.</dd>
</dl>

### Row

A row in the column view.

<dl>
<dt><code>object[] Values { get; }</code></dt>
<dd>An array of values in the row.</dd>
</dl>

### RowList

A list of rows.

<dl>
<dt><code>Row this[uint i] { get; }</code></dt>
<dd>An indexer to retrieve a Row.</dd>
</dl>

### SelectedChanged

<dl>
<dt><code>delegate void SelectionChanged(uint rowIndex)</code></dt>
<dd>A delegate type for a selection change event.  rowIndex will contain the currently selected row.</dd>
</dl>
