//------------------------------------------------------------------------------
// <copyright file="CompareDirectoriesControl.xaml.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp..  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace CompareTrees
{
    struct PathData
    {
        public readonly string FullPath;
        public readonly string RelativePath;

        public PathData(string root, string path)
        {
            this.FullPath = path;
            this.RelativePath = path.Substring(root.Length);
        }

        public PathData(string path)
        {
            this.FullPath = path;
            this.RelativePath = path;
        }
    }
}