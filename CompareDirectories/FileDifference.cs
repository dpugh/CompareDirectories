//------------------------------------------------------------------------------
// <copyright file="CompareDirectoriesCommand.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp..  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace CompareDirectories
{
    using System;

    [Flags]
    internal enum FileDifference
    {
        None = 0,
        Identical = 1,
        LeftOnly = 2,
        RightOnly = 4,
        DifferentInWhiteSpaceOnly = 8,
        DifferentExcludingWhiteSpace = 16,
        All = 31
    }
}
