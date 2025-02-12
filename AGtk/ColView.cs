using GObject;
using Gtk;

namespace AGtk;

[Subclass<GObject.Object>]
partial class Row {
    public object[] values;

    public Row(object[] values) : this() {
        this.values = values;
    }
}

// A ListItemFactory that creates a Label with text from a specific column.
class LabelFactory : SignalListItemFactory {
    int index;

    public LabelFactory(int index) : base([]) {
        this.index = index;
        OnSetup += on_setup;
        OnBind += on_bind;
    }

    private void on_setup(SignalListItemFactory sender, SetupSignalArgs args) {
        ListItem listItem = (ListItem) args.Object;
        Label label = Label.New(null);
        label.Halign = Align.Start;
        listItem.Child = label;
    }

    private void on_bind(SignalListItemFactory sender, BindSignalArgs args) {
        ListItem listItem = (ListItem) args.Object;
        Label label = (Label) listItem.Child!;
        Row row = (Row) listItem.Item!;
        label.SetText(row.values[index].ToString()!);
    }
}

public class ColView : ColumnView {
    string[] names;

    Gio.ListStore store;
    FilterListModel filter_model;
    CustomFilter filter;

    int? filter_column = null;
    string filter_text = "";

    public ColView(string[] names) {
        this.names = names;
        
        store = Gio.ListStore.New(Row.GetGType());
        for (int i = 0 ; i < names.Length ; ++i)
            AppendColumn(ColumnViewColumn.New(names[i], new LabelFactory(i)));

        filter = CustomFilter.New(is_row_visible); 
        filter_model = FilterListModel.New(store, filter);
        Model = SingleSelection.New(filter_model);
    }

    public string? FilterColumn {
        get => filter_column == null ? null : names[(int) filter_column];

        set {
            if (value == null)
                filter_column = null;
            else {
                filter_column = Array.IndexOf(names, value);
                if (filter_column < 0)
                    throw new Exception("column not found");
            }
            filter.Changed(FilterChange.Different);
        }
    }

    public string FilterText {
        get => filter_text;
        set {
            filter_text = value;
            filter.Changed(FilterChange.Different);
        }
    }

    public void Add(object[] values) {
        store.Append(new Row(values));
    }

    public void Clear() {
        store.RemoveAll();
    }

    bool is_row_visible(GObject.Object item) {
        if (filter_column is int col) {
            string s = ((Row) item).values[col].ToString()!;
            return s.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
        } else return true;
    }
}
