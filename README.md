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

### Adding filtering

AGtk.ColView supports dynamic filtering.  Typically you'll use this with a search box: as the user types in the box, rows will be filtered.  Here's how to do this in code:

```C#
// specify the column to use for filtering
col_view.FilterColumn = "title";

SearchEntry entry = new();
entry.OnSearchChanged += on_search_changed;

...

void on_search_changed(SearchEntry sender, EventArgs args) {
    col_view.FilterText = entry.GetText();
}
```

### Sorting

It would be nice if the user could click a column header to sort by that column.  Unfortunately that's not yet possible due to [limitations in the Gir.Core binding for GTK 4](https://github.com/gircore/gir.core/issues/1180).  I hope I'll be able to work with the Gir.Core developers to improve this situation before long.

## Using the library

Perhaps I'll publish a nuget package at some point, but this is an early work in progress so I haven't done that yet.  In the meantime, you can clone this repository to your machine, then use the `dotnet add reference` command to add a reference to agtk.csproj in your project:

```
$ dotnet add reference /path/to/agtk.csproj
```
