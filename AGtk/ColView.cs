using GObject;
using Gtk;

namespace AGtk;

[Subclass<GObject.Object>]
public partial class Row {
    public readonly object[] Values;

    public Row(object[] values) : this() {
        this.Values = values;
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
        label.SetText(row.Values[index].ToString()!);
    }
}

public delegate void SelectionChanged(uint rowIndex);

public class RowList(Gio.ListStore store) {
    public Row this[uint i] {
        get {
            object? o = store.GetObject(i);
            if (o == null)
                throw new Exception("out of bounds");
            else
                return (Row) o;
        }
    }
}

public class ColView : ColumnView {
    string[] names;

    Gio.ListStore store;
    FilterListModel filter_model;
    CustomFilter filter;
    SingleSelection selection_model;

    int? filter_column = null;
    string filter_text = "";

    public event SelectionChanged? OnSelectionChanged;

    public ColView(params string[] names) {
        this.names = names;
        
        store = Gio.ListStore.New(Row.GetGType());
        for (int i = 0 ; i < names.Length ; ++i)
            AppendColumn(ColumnViewColumn.New(names[i], new LabelFactory(i)));

        filter = CustomFilter.New(is_row_visible); 
        filter_model = FilterListModel.New(store, filter);
        Model = selection_model = SingleSelection.New(filter_model);

        Signal<SingleSelection> signal = new("selection-changed", "selection-changed");
        signal.Connect(selection_model, on_selection_changed);
    }

    public uint SelectedIndex => selection_model.Selected;

    public RowList Rows => new RowList(store);

    public int? FilterColumn {
        get => filter_column;

        set {
            filter_column = value;
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

    public void Add(params object[] values) {
        store.Append(new Row(values));
    }

    public void Clear() {
        store.RemoveAll();
    }

    bool is_row_visible(GObject.Object item) {
        if (filter_column is int col) {
            string s = ((Row) item).Values[col].ToString()!;
            return s.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
        } else return true;
    }

    void on_selection_changed(SingleSelection sender, EventArgs args) {
        if (OnSelectionChanged != null)
            OnSelectionChanged(SelectedIndex);
    }
}
