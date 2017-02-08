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
                                ? new TreeNode(parent, null, null, element, FileDifference.Identical)
                                : new TreeNode(parent, leftPath, rightPath, element, FileDifference.Identical);

                        parent.Children.Add(child);
                    }

                    parent = child;
                }
            }

            do
            {
                if ((int)differences <= (int)(parent.FileDifference))
                    break;

                parent.FileDifference = differences;
                parent = parent.Parent;
            }
            while (parent != null);
        }

        internal void AddToTreeView(ItemCollection items, FileDifference threshold)
        {
            var tvi = new TreeViewItem();
            tvi.Tag = this;

            var box = new TextBox();
            box.Text = this.Label;
            box.Foreground = (this.FileDifference == FileDifference.DifferentExcludingWhiteSpace)
                             ? Brushes.Red : ((this.FileDifference == FileDifference.DifferentInWhiteSpaceOnly) ? Brushes.Blue : Brushes.Black);

            tvi.Header = box;

            tvi.IsExpanded = true;

            SetVisibility(tvi, threshold);


            if (this.Children != null)
            {
                foreach (var c in this.Children)
                {
                    c.AddToTreeView(tvi.Items, threshold);
                }
            }

            items.Add(tvi);
        }

        internal void SetVisibility(TreeViewItem item, FileDifference threshold)
        {
            item.Visibility = (((int)(this.FileDifference)) < (int)threshold) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
