//------------------------------------------------------------------------------
// <copyright file="CompareDirectories.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp..  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace CompareTrees
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("dddd4ab2-2a17-42fd-bb5b-7230d687b23a")]
    public class CompareDirectories : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompareDirectories"/> class.
        /// </summary>
        public CompareDirectories() : base(null)
        {
            this.Caption = "CompareDirectories";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new CompareDirectoriesControl();
        }
    }
}
