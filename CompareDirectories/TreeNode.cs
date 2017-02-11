//------------------------------------------------------------------------------
// <copyright file="CompareDirectoriesControl.xaml.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp..  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace CompareDirectories
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    class TreeNode
    {
        public readonly string LeftPath;
        public readonly string RightPath;
        public readonly string Label;
        public readonly TreeNode Parent;
        public FileDifference FileDifference;

        public List<TreeNode> Children { get; private set; }

        public TreeNode(TreeNode parent, string leftPath, string rightPath, string label, FileDifference difference)
        {
            this.Parent = parent;
            this.LeftPath = leftPath;
            this.RightPath = rightPath;
            this.Label = label;

            this.FileDifference = difference;
        }

        internal void AddToTree(string leftPath, string rightPath, string label,
                                FileDifference differences, bool splitlabel)
        {
            var labelElements = splitlabel ? label.Split(Path.DirectorySeparatorChar) : new string[] { label };
            this.FileDifference = differences;

            var parent = this;

            for (int i = 0; (i < labelElements.Length); ++i)
            {
                string element = labelElements[i];

                if (!string.IsNullOrEmpty(element))
                {
                    if (parent.Children == null)
                        parent.Children = new List<TreeNode>();

                    var child = parent.Children.FirstOrDefault(n => StringComparer.OrdinalIgnoreCase.Equals(n.Label, element));
                    if (child == null)
                    {
                        child = (i < labelElements.Length - 1)
                                ? new TreeNode(parent, null, null, element, FileDifference.None)
                                : new TreeNode(parent, leftPath, rightPath, element, FileDifference.None);

                        parent.Children.Add(child);
                    }

                    parent = child;
                }
            }

            do
            {
                var newDifferences = differences | parent.FileDifference;
                if (parent.FileDifference == newDifferences)
                    break;

                parent.FileDifference = newDifferences;
                parent = parent.Parent;
            }
            while (parent != null);
        }

        static Brush GetBrush(FileDifference difference)
        {
            return difference.HasFlag(FileDifference.DifferentExcludingWhiteSpace)
                   ? Brushes.Red
                   : (difference.HasFlag(FileDifference.DifferentInWhiteSpaceOnly)
                      ? Brushes.Blue
                      : (difference.HasFlag(FileDifference.LeftOnly) || difference.HasFlag(FileDifference.RightOnly)
                        ? Brushes.Green
                        : Brushes.Black));
        }

        internal void AddToTreeView(ItemCollection items, FileDifference filterMask)
        {
            var tvi = new TreeViewItem();
            tvi.Tag = this;

            var box = new TextBox();
            box.Text = this.Label;
            box.Foreground = GetBrush(this.FileDifference);

            tvi.Header = box;

            tvi.IsExpanded = true;

            SetVisibility(tvi, filterMask);


            if (this.Children != null)
            {
                foreach (var c in this.Children)
                {
                    c.AddToTreeView(tvi.Items, filterMask);
                }
            }

            items.Add(tvi);
        }

        internal void SetVisibility(TreeViewItem item, FileDifference filterMask)
        {
            item.Visibility = ((this.FileDifference & filterMask) == FileDifference.None)
                              ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
