using GObject;
using Gtk;

namespace AGtk;

[Subclass<GObject.Object>]
public partial class Row {
    ColView? col_view;      // optional to suppress compiler warning
    public readonly object[] Values = [];
    Dictionary<int, Label> labels = [];

    public Row(ColView col_view, object[] values) : this() {
        this.col_view = col_view;
        this.Values = values;
    }

    public void add_label(int column, Label label) {
        labels[column] = label;
        label.SetText(Values[column].ToString()!);
    }

    public bool contains_label(Label label) => labels.ContainsValue(label);

    public void SetValue(int col, object o) {
        if (col < 0 || col >= Values.Length)
            throw new Exception("index out of bounds");

        col_view!.validate(col, o);
        Values[col] = o;
        labels[col].SetText(o.ToString()!);
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
        row.add_label(index, label);
    }
}

public delegate void SelectionChanged(uint rowIndex);

public delegate void RightClick(int x, int y, uint rowIndex);

public class RowList(Gio.ListModel model) {
    public Row this[uint i] {
        get {
            object? o = model.GetObject(i);
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
    public event RightClick? OnRightClick;

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

        GestureClick g = new();
        g.Button = 3;
        g.OnReleased += on_right_click;
        AddController(g);
    }

    int compare(int column, nint a, nint b) {
        Row rowA = (Row) GObject.Internal.InstanceWrapper.WrapHandle<GObject.Object>(a, false);
        Row rowB = (Row) GObject.Internal.InstanceWrapper.WrapHandle<GObject.Object>(b, false);
        object objA = rowA.Values[column];
        object objB = rowB.Values[column];
        return ((IComparable) objA).CompareTo(objB);
    }

    public uint SelectedIndex {
        get => selection_model.Selected;
        set { selection_model.Selected = value; }
    }

    public RowList Rows => new RowList(selection_model);

    public uint RowCount => selection_model.GetNItems();

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

    public void validate(int col, object val) {
        if (names[col] != null && val is not IComparable)
            throw new Exception("value in visible column must be comparable");
    }

    public void Add(params object[] values) {
        if (values.Length != names.Length)
            throw new Exception("number of values doesn't match number of columns");
            
        for (int i = 0; i < values.Length; ++i)
            validate(i, values[i]);

        store.Append(new Row(this, values));
    }

    public void Clear() {
        store.RemoveAll();
    }

    uint store_index(Row row) {
        for (uint i = 0 ; i < store.GetNItems(); ++i)
            if (store.GetObject(i) == row)
                return i;

        throw new Exception("row not found");
    }

    public void DeleteRow(uint index) {
        store.Remove(store_index(Rows[index]));
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

    uint? label_row(Label label) {
        for (uint i = 0; i < RowCount ; ++i)
            if (Rows[i].contains_label(label))
                return i;
        return null;
    }

    void on_right_click(GestureClick sender, GestureClick.ReleasedSignalArgs args) {
        if (OnRightClick != null) {
            Widget? w = Pick(args.X, args.Y, 0);
            Label? label = w as Label;
            if (label == null && w != null)
                label = w.GetFirstChild() as Label;
            if (label != null && label_row(label) is uint index)
                OnRightClick((int) args.X, (int) args.Y, index);
        }
    }
}
