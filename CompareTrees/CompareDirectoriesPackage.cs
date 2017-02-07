//------------------------------------------------------------------------------
// <copyright file="CompareDirectoriesPackage.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp..  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using System.Collections.Generic;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using System.Collections.ObjectModel;

namespace CompareTrees
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(CompareDirectories))]
    [Guid(CompareDirectoriesPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class CompareDirectoriesPackage : Package
    {
        /// <summary>
        /// CompareDirectoriesPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "a260a769-353f-4410-9ee3-c850da682628";
        public static readonly Guid PackageGuid = new Guid(PackageGuidString);
        public const string FilterSettings = @"CompareDirectories\Filters";

        public static ObservableCollection<string> CommonFilters = new ObservableCollection<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareDirectories"/> class.
        /// </summary>
        public CompareDirectoriesPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            CompareDirectoriesCommand.Initialize(this);
            base.Initialize();

            using (ServiceProvider serviceProvider = new ServiceProvider((IServiceProvider)(Package.GetGlobalService(typeof(IServiceProvider)))))
            {
                SettingsManager settingsManager = new ShellSettingsManager(serviceProvider);
                SettingsStore settingsStore = settingsManager.GetReadOnlySettingsStore(SettingsScope.UserSettings);

                if (settingsStore.CollectionExists(FilterSettings))
                {
                    IEnumerable<string> propertyNames = settingsStore.GetPropertyNames(FilterSettings);

                    foreach (string propertyName in propertyNames)
                    {
                        var filter = settingsStore.GetString(FilterSettings, propertyName, null);
                        if (filter != null)
                        {
                            CommonFilters.Add(filter);
                        }
                    }
                }
            }

            if (CommonFilters.Count == 0)
            {
                CommonFilters.Add("*.cs;*.vb;*.c;*.cpp;*.h");
                CommonFilters.Add(string.Empty);
            }

            #endregion
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                using (ServiceProvider serviceProvider = new ServiceProvider((IServiceProvider)(Package.GetGlobalService(typeof(IServiceProvider)))))
                {
                    SettingsManager settingsManager = new ShellSettingsManager(serviceProvider);
                    WritableSettingsStore settingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

                    if (settingsStore.CollectionExists(FilterSettings))
                    {
                        settingsStore.DeleteCollection(FilterSettings);
                    }

                    settingsStore.CreateCollection(FilterSettings);
                    for (int i = 0; (i < CommonFilters.Count); ++i)
                    {
                        settingsStore.SetString(FilterSettings, i.ToString(), CommonFilters[i]);
                    }
                }
            }

            base.Dispose(disposing);
        }
    }
}
