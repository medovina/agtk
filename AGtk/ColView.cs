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
    string?[] names;

    Gio.ListStore store;
    FilterListModel filter_model;
    CustomFilter filter;
    SingleSelection selection_model;

    int? filter_column = null;
    string filter_text = "";

    public event SelectionChanged? OnSelectionChanged;

    public ColView(params string?[] names) {
        this.names = names;
        
        store = Gio.ListStore.New(Row.GetGType());
        for (int i = 0 ; i < names.Length ; ++i)
            if (names[i] != null) {
                ColumnViewColumn col = ColumnViewColumn.New(names[i], new LabelFactory(i));
                col.Expand = true;
                int j = i;  // prevent lambda from capturing i, which will change
                CustomSorter sorter = CustomSorter.New((a, b) => compare(j, a, b));
                col.Sorter = sorter;
                AppendColumn(col);
            }

        filter = CustomFilter.New(is_row_visible); 
        filter_model = FilterListModel.New(store, filter);

        Sorter view_sorter = Sorter!;
        SortListModel sort_model = SortListModel.New(filter_model, view_sorter);
        Model = selection_model = SingleSelection.New(sort_model);

        Signal<SingleSelection> signal = new("selection-changed", "selection-changed");
        signal.Connect(selection_model, on_selection_changed);
    }

    int compare(int column, nint a, nint b) {
        Row rowA = (Row) GObject.Internal.InstanceWrapper.WrapHandle<GObject.Object>(a, false);
        Row rowB = (Row) GObject.Internal.InstanceWrapper.WrapHandle<GObject.Object>(b, false);
        object objA = rowA.Values[column];
        object objB = rowB.Values[column];
        return ((IComparable) objA).CompareTo(objB);
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
        if (values.Length != names.Length)
            throw new Exception("number of values doesn't match number of columns");
            
        for (int i = 0; i < values.Length; ++i)
            if (names[i] != null && values[i] is not IComparable)
                throw new Exception("value in visible column must be comparable");

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
