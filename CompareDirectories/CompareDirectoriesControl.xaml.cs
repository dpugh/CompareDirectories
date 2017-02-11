//------------------------------------------------------------------------------
// <copyright file="CompareDirectoriesControl.xaml.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp..  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace CompareDirectories
{
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.Threading;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using Task = System.Threading.Tasks.Task;

    public partial class CompareDirectoriesControl : UserControl
    {
        private State _state = null;

        public CompareDirectoriesControl()
        {
            this.InitializeComponent();

            this.Filters.ItemsSource = CompareDirectoriesPackage.CommonFilters;
            this.Filters.Text = CompareDirectoriesPackage.CommonFilters[0];

            this.Differences.PreviewMouseLeftButtonDown += Differences_PreviewMouseLeftButtonDown;

            // Has to happen after InitializeComponents since the Checked/Unchecked event fires if we do it in the xaml.
            this.ShowIdentical.Checked += this.Show_Checked;
            this.ShowIdentical.Unchecked += this.Show_Checked;
            this.ShowLeftOnly.Checked += this.Show_Checked;
            this.ShowLeftOnly.Unchecked += this.Show_Checked;
            this.ShowRightOnly.Checked += this.Show_Checked;
            this.ShowRightOnly.Unchecked += this.Show_Checked;
            this.ShowDifferentInWhiteSpaceOnly.Checked += this.Show_Checked;
            this.ShowDifferentInWhiteSpaceOnly.Unchecked += this.Show_Checked;
            this.ShowDifferentExcludingWhiteSpace.Checked += this.Show_Checked;
            this.ShowDifferentExcludingWhiteSpace.Unchecked += this.Show_Checked;
        }

        private void Differences_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var clickedItem = GetClickedItem(e);
            if (clickedItem != null)
            {
                var node = (clickedItem as TreeViewItem)?.Tag as TreeNode;
                if (node != null)
                {
                    if (!(string.IsNullOrEmpty(node.LeftPath) || string.IsNullOrEmpty(node.RightPath)))
                    {
                        e.Handled = true;

                        var diff = CompareDirectoriesPackage.GetGlobalService(typeof(SVsDifferenceService)) as IVsDifferenceService;
                        if (diff != null)
                        {
                            using (new NewDocumentStateScope(__VSNEWDOCUMENTSTATE.NDS_Provisional, CompareDirectoriesPackage.PackageGuid))
                            {
                                diff.OpenComparisonWindow(node.LeftPath, node.RightPath);
                            }
                        }
                    }
                    else
                    {
                        var path = string.IsNullOrEmpty(node.LeftPath) ? (string.IsNullOrEmpty(node.RightPath) ? null : node.RightPath) : node.LeftPath;
                        if (path != null)
                        {
                            e.Handled = true;

                            var shell = CompareDirectoriesPackage.GetGlobalService(typeof(SVsUIShellOpenDocument)) as IVsUIShellOpenDocument;
                            if (shell != null)
                            {
                                Guid textViewLogicalView = VSConstants.LOGVIEWID_TextView;
                                Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProviderOfFrame = null;

                                IVsUIHierarchy hierarchy;
                                uint itemID;
                                IVsWindowFrame windowFrame = null;
                                using (new NewDocumentStateScope(__VSNEWDOCUMENTSTATE.NDS_Provisional, CompareDirectoriesPackage.PackageGuid))
                                {
                                    shell.OpenDocumentViaProject(path,
                                                             ref textViewLogicalView,
                                                             out serviceProviderOfFrame,
                                                             out hierarchy,
                                                             out itemID,
                                                             out windowFrame);
                                }

                                if (windowFrame != null)
                                {
                                    windowFrame.Show();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LeftBrowse_Click(object sender, RoutedEventArgs e)
        {
            this.Left.Text = GetFolder("Select left directory", this.Left.Text ?? string.Empty);
        }

        private void RightBrowse_Click(object sender, RoutedEventArgs e)
        {
            this.Right.Text = GetFolder("Select right directory", this.Right.Text ?? string.Empty);
        }

        private void CollapseAll_Click(object sender, RoutedEventArgs e)
        {
            bool any = CollapseTree(this.Differences.Items, isExpanded: false);
            if (!any)
                CollapseTree(this.Differences.Items, isExpanded: true);
        }

        private void Show_Checked(object sender, RoutedEventArgs e)
        {
            this.UpdateVisibilities(this.Differences.Items, this.GetFilterMask());
        }

        private void Go_Click(object sender, RoutedEventArgs e)
        {
            var filterText = this.Filters.Text;

            int index = -1;
            for (int i = 0; (i < CompareDirectoriesPackage.CommonFilters.Count); ++i)
            {
                if (StringComparer.OrdinalIgnoreCase.Equals(filterText, CompareDirectoriesPackage.CommonFilters[i]))
                {
                    index = i;
                    break;
                }
            }

            if (index != 0)
            {
                if (index != -1)
                {
                    CompareDirectoriesPackage.CommonFilters.RemoveAt(index);
                }

                CompareDirectoriesPackage.CommonFilters.Insert(0, filterText);
                this.Filters.Text = filterText;
            }

            this.UpdateState();
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Show.IsExpanded = false;
        }

        private void Differences_CleanUpVirtualizedItem(object sender, CleanUpVirtualizedItemEventArgs e)
        {

        }

        private bool IsCurrent(ExtendedState state)
        {
            return (_state == state.State);
        }

        private void UpdateVisibilities(ItemCollection items, FileDifference filterMask)
        {
            foreach (var i in items)
            {
                var item = i as TreeViewItem;
                if (item != null)
                {
                    var node = item.Tag as TreeNode;

                    node?.SetVisibility(item, filterMask);
                    this.UpdateVisibilities(item.Items, filterMask);
                }
            }
        }

        private FileDifference GetFilterMask()
        {
            return ((this.ShowIdentical.IsChecked ?? false) ? FileDifference.Identical : FileDifference.None) |
                   ((this.ShowLeftOnly.IsChecked ?? false) ? FileDifference.LeftOnly : FileDifference.None) |
                   ((this.ShowRightOnly.IsChecked ?? false) ? FileDifference.RightOnly : FileDifference.None) |
                   ((this.ShowDifferentInWhiteSpaceOnly.IsChecked ?? false) ? FileDifference.DifferentInWhiteSpaceOnly : FileDifference.None) |
                   ((this.ShowDifferentExcludingWhiteSpace.IsChecked ?? false) ? FileDifference.DifferentExcludingWhiteSpace : FileDifference.None);
        }

        private static TreeViewItem GetClickedItem(MouseButtonEventArgs e)
        {
            var hit = e.OriginalSource as DependencyObject;
            while (hit != null)
            {
                var tvi = hit as TreeViewItem;
                if (tvi != null)
                    return tvi;

                hit = VisualTreeHelper.GetParent(hit);
            }

            return null;
        }

        private static bool CollapseTree(ItemCollection items, bool isExpanded)
        {
            bool differences = false;
            foreach (var i in items)
            {
                var tvi = i as TreeViewItem;
                if (tvi != null)
                {
                    differences = differences || (tvi.IsExpanded != isExpanded);

                    tvi.IsExpanded = isExpanded;
                    if (CollapseTree(tvi.Items, isExpanded))
                        differences = true;
                }
            }

            return differences;
        }

        private string GetFolder(string title, string starting)
        {
            var shell = CompareDirectoriesPackage.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;
            if (shell != null)
            {
                const int MaxBuffer = 2048;
                VSBROWSEINFOW[] browseInfo = new VSBROWSEINFOW[1];
                IntPtr ownerHwnd;

                shell.GetDialogOwnerHwnd(out ownerHwnd);

                browseInfo[0].lStructSize = (uint)Marshal.SizeOf(typeof(VSBROWSEINFOW));
                browseInfo[0].pwzDlgTitle = title;
                browseInfo[0].dwFlags = 1; // RestrictToFilesystem;
                browseInfo[0].nMaxDirName = 1024;

                browseInfo[0].pwzInitialDir = string.IsNullOrWhiteSpace(starting) ? Environment.CurrentDirectory : starting;
                browseInfo[0].hwndOwner = ownerHwnd;

                try
                {
                    browseInfo[0].pwzDirName = Marshal.AllocCoTaskMem(MaxBuffer);

                    if (shell.GetDirectoryViaBrowseDlg(browseInfo) == VSConstants.S_OK)
                    {
                        starting = Marshal.PtrToStringAuto(browseInfo[0].pwzDirName);
                    }
                }
                finally
                {
                    Marshal.FreeCoTaskMem(browseInfo[0].pwzDirName);
                }
            }

            return starting;
        }

        private void UpdateState()
        {
            var newState = new State(this.Left.Text ?? string.Empty, this.Right.Text ?? string.Empty, this.Filters.Text ?? string.Empty);

            var oldState = Volatile.Read(ref _state);
            while (true)
            {
                var result = Interlocked.CompareExchange(ref _state, newState, oldState);
                if (result == oldState)
                {
                    if (oldState == null)
                    {
                        Task.Factory.StartNew(this.CalculateDiffs);
                    }

                    break;
                }

                oldState = result;
            }
        }

        private static readonly PathData _empty = new PathData(string.Empty);

        private async Task CalculateDiffs()
        {
            var oldState = new ExtendedState(Volatile.Read(ref _state));
            while (true)
            {
                await this.PostResults(null, "... <working> ...");
                TreeNode root = new TreeNode(null, null, null, string.Empty, FileDifference.All);

                if (oldState.LeftDirectoryExists && oldState.RightDirectoryExists)
                {
                    // Abandon the calculation if _state != oldState (which will cause the CompareExchange call below to fail and
                    // we'll try and compute the differences on the new version of _state;
                    var leftFiles = new List<PathData>(EnumerateFiles(oldState, oldState.LeftPath, oldState.LeftPath));
                    var rightFiles = new List<PathData>(EnumerateFiles(oldState, oldState.RightPath, oldState.RightPath));

                    var allRightFiles = new HashSet<string>(rightFiles.Select(p => p.RelativePath), StringComparer.OrdinalIgnoreCase);

                    int count = 0;
                    int leftIndex = 0;
                    int rightIndex = 0;
                    while (((leftIndex < leftFiles.Count) || (rightIndex < rightFiles.Count)) && this.IsCurrent(oldState))
                    {
                        if (!this.IsCurrent(oldState))
                            break;

                        var leftFile = (leftIndex < leftFiles.Count) ? leftFiles[leftIndex] : _empty;
                        var rightFile = (rightIndex < rightFiles.Count) ? rightFiles[rightIndex] : _empty;

                        if (StringComparer.OrdinalIgnoreCase.Equals(leftFile.RelativePath, rightFile.RelativePath))
                        {
                            root.AddToTree(leftFiles[leftIndex].FullPath, rightFiles[rightIndex].FullPath, rightFile.RelativePath,
                                           CompareFiles(oldState, leftFiles[leftIndex].FullPath, rightFiles[rightIndex].FullPath),
                                           splitlabel: true);

                            ++leftIndex;
                            ++rightIndex;
                        }
                        else if ((leftIndex >= leftFiles.Count) || allRightFiles.Contains(leftFile.RelativePath))
                        {
                            root.AddToTree(null, rightFiles[rightIndex].FullPath, rightFile.RelativePath + " (right only)",
                                           FileDifference.RightOnly, splitlabel: true);
                            ++rightIndex;
                        }
                        else
                        {
                            root.AddToTree(leftFiles[leftIndex].FullPath, null, leftFile.RelativePath + " (left only)",
                                           FileDifference.LeftOnly, splitlabel: true);
                            ++leftIndex;
                        }

                        if ((++count) % 100 == 0)
                        {
                            await this.PostResults(null, "... <working> (" + ((100 * (leftIndex + rightIndex)) / (leftFiles.Count + rightFiles.Count)).ToString() + "%) ...");
                        }
                    }
                }
                else if (oldState.LeftFileExists && oldState.RightFileExists)
                {
                    var leftFile = Path.GetFileName(oldState.LeftPath);
                    var rightFile = Path.GetFileName(oldState.RightPath);
                    var label = StringComparer.OrdinalIgnoreCase.Equals(leftFile, rightFile) ? rightFile : (leftFile + " " + rightFile);
                    root.AddToTree(oldState.LeftPath, oldState.RightPath, label,
                                   CompareFiles(oldState, oldState.LeftPath, oldState.RightPath),
                                   splitlabel: true);
                }
                else
                {
                    root.AddToTree(null, null, oldState.LeftPath + Suffix(oldState.LeftFileExists, oldState.LeftDirectoryExists),
                                   FileDifference.DifferentExcludingWhiteSpace, splitlabel: false);
                    root.AddToTree(null, null, oldState.RightPath + Suffix(oldState.RightFileExists, oldState.RightDirectoryExists),
                                   FileDifference.DifferentExcludingWhiteSpace, splitlabel: false);
                }

                await this.PostResults(root, oldState.LeftPath + " .vs. " + oldState.RightPath);

                var result = Interlocked.CompareExchange(ref _state, null, oldState.State);
                if (result == oldState.State)
                {
                    break;
                }

                oldState = new ExtendedState(result);
            }
        }

        private static string Suffix(bool fileExists, bool directoryExists)
        {
            return fileExists ? " is a file."
                              : (directoryExists ? " is a directory." : " does not exist.");
        }

        private List<string> BuildRelativePaths(string directory, List<string> fullFilePaths)
        {
            var relativeFilePaths = new List<string>(fullFilePaths.Count);
            foreach (var p in fullFilePaths)
            {
                relativeFilePaths.Add((p.Length > directory.Length) ? p.Substring(directory.Length) : p);
            }

            return relativeFilePaths;
        }

        private FileDifference CompareFiles(ExtendedState oldState, string left, string right)
        {
            try
            {
                using (var leftStream = new FileStream(left, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var rightStream = new FileStream(right, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        bool areDifferentInWhiteSpace = false;
                        bool leftWasWhiteSpace = true;
                        bool rightWasWhiteSpace = true;
                        bool skipRead = false;
                        int l = -1;
                        int r = -1;
                        while (true)
                        {
                            if (!this.IsCurrent(oldState))
                                return FileDifference.Identical;

                            if (skipRead)
                            {
                                skipRead = false;
                            }
                            else
                            {
                                l = leftStream.ReadByte();
                                r = rightStream.ReadByte();
                            }

                            if (l != r)
                            {
                                if (l == -1)
                                    return AreThereNonWhiteSpaceCharactersAtEndOfStream(r, rightStream) ? FileDifference.DifferentExcludingWhiteSpace : FileDifference.DifferentInWhiteSpaceOnly;
                                else if (r == -1)
                                    return AreThereNonWhiteSpaceCharactersAtEndOfStream(l, leftStream) ? FileDifference.DifferentExcludingWhiteSpace : FileDifference.DifferentInWhiteSpaceOnly;

                                areDifferentInWhiteSpace = true;

                                bool leftIsWhiteSpace = char.IsWhiteSpace((char)l);
                                bool rightIsWhiteSpace = char.IsWhiteSpace((char)r);

                                if (!(leftIsWhiteSpace && rightIsWhiteSpace))
                                {
                                    if (leftIsWhiteSpace)
                                    {
                                        if (!rightWasWhiteSpace)
                                        {
                                            return FileDifference.DifferentExcludingWhiteSpace;
                                        }

                                        skipRead = true;
                                        l = ConsumeWhitespace(leftStream);
                                    }
                                    else if (rightIsWhiteSpace)
                                    {
                                        if (!leftWasWhiteSpace)
                                        {
                                            return FileDifference.DifferentExcludingWhiteSpace;
                                        }

                                        skipRead = true;
                                        r = ConsumeWhitespace(rightStream);
                                    }
                                    else
                                    {
                                        return FileDifference.DifferentExcludingWhiteSpace;
                                    }
                                }

                                leftWasWhiteSpace = leftIsWhiteSpace;
                                rightWasWhiteSpace = rightIsWhiteSpace;
                            }
                            else if (l == -1)
                            {
                                return areDifferentInWhiteSpace ? FileDifference.DifferentInWhiteSpaceOnly : FileDifference.Identical;
                            }
                            else
                            {
                                leftWasWhiteSpace = rightWasWhiteSpace = char.IsWhiteSpace((char)l);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return FileDifference.Identical;
            }
        }

        private static bool AreThereNonWhiteSpaceCharactersAtEndOfStream(int c, FileStream stream)
        {
            while (true)
            {
                if (c == -1)
                    return false;
                else if (!char.IsWhiteSpace((char)c))
                    return true;

                c = stream.ReadByte();
            }
        }

        private static int ConsumeWhitespace(FileStream stream)
        {
            while (true)
            {
                int c = stream.ReadByte();
                if ((c == -1) || !char.IsWhiteSpace((char)c))
                {
                    return c;
                }
            }
        }

        private async Task PostResults(TreeNode root, string label)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            this.Differences.Items.Clear();

            var tvi = new TreeViewItem();
            tvi.Header = label;
            tvi.IsExpanded = true;
            this.Differences.Items.Add(tvi);


            if (root != null)
            {
                var filterMask = this.GetFilterMask();
                foreach (var c in root.Children)
                {
                    c.AddToTreeView(tvi.Items, filterMask);
                }
            }

            await TaskScheduler.Default;
        }

        private IEnumerable<PathData> EnumerateFiles(ExtendedState oldState, string rootDirectory, string directory)
        {
            if (Directory.Exists(directory))
            {
                foreach (var d in Directory.EnumerateDirectories(directory))
                {
                    if (!this.IsCurrent(oldState))
                        break;

                    foreach (var e in this.EnumerateFiles(oldState, rootDirectory, d))
                    {
                        yield return e;
                    }
                }

                foreach (var f in Directory.EnumerateFiles(directory))
                {
                    if (!this.IsCurrent(oldState))
                        break;

                    var path = new PathData(rootDirectory, f);

                    if (oldState.PassesFilters(path.RelativePath))
                    {
                        yield return path;
                    }
                }
            }
        }
    }
}