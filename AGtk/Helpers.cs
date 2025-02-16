using GLib;
using SimpleAction = Gio.SimpleAction;

namespace AGtk;

public static class Helpers {
    // helper method for adding actions
    public static SimpleAction AddAction(this Gio.ActionMap map, string name, Action f) {
        SimpleAction a = SimpleAction.New(name, null);
        a.OnActivate += (_, _) => f ();
        map.AddAction(a);   // call AddAction in base class
        return a;
    }

    // helper method for adding toggle actions
    public static SimpleAction AddToggleAction(
            this Gio.ActionMap map, string name, bool initial, Action<bool>? f) {

        SimpleAction a = SimpleAction.NewStateful(name, null, Variant.NewBoolean(initial));
        a.OnChangeState += (_, args) => {
            Variant v = args.Value!;
            a.SetState(v);
            if (f != null)
                f(v.GetBoolean());
        };
        map.AddAction(a);   // call AddAction in base class
        return a;
    }
}

