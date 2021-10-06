using System.Windows;

namespace PrettyScatter.Utils.Ext
{
    public static class DragEventArgsExt
    {
        public static bool IsGottenFile(this DragEventArgs ev) => ev.Data.GetDataPresent(DataFormats.FileDrop);

        public static string[] GetFilePathsUnsafe(this DragEventArgs ev) =>
            (string[])ev.Data.GetData(DataFormats.FileDrop)!;
    }
}
