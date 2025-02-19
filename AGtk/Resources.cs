using Assembly = System.Reflection.Assembly;

using Gdk;
using GLib;

public static class Resources {
    public static Texture? GetTexture(string name) {
        name = name.Replace("/", ".").Replace("\\", ".");
        Assembly a = Assembly.GetEntryAssembly()!;

        // It may be difficult to construct the exact resource name we need,
        // since it can depend e.g. on the assembly name or the RootNamespace
        // in a project file.  Instead, we choose the shortest resource name that
        // ends with the name we are seeking.
        string[] rnames = [..
            a.GetManifestResourceNames().Where(s => s.EndsWith("." + name))];
        if (rnames.Length == 0)
            return null;

        string rname = rnames.MinBy(s => s.Length)!;
        using Stream s = a.GetManifestResourceStream(rname)!;
        using MemoryStream ms = new();
        s.CopyTo(ms);
        return Texture.NewFromBytes(Bytes.New(ms.ToArray()));
    }
}
