//------------------------------------------------------------------------------
// <copyright file="CompareDirectoriesControl.xaml.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp..  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace CompareDirectories
{
    class State
    {
        public readonly string LeftPath;
        public readonly string RightPath;
        public readonly string Filters;

        public State(string leftPath, string rightPath, string filters)
        {
            this.LeftPath = leftPath;
            this.RightPath = rightPath;
            this.Filters = filters;
        }
    }
}
