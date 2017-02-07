//------------------------------------------------------------------------------
// <copyright file="CompareDirectoriesControl.xaml.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp..  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace CompareTrees
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    class State
    {
        public readonly string LeftPath;
        public readonly string RightPath;
        public readonly bool LeftDirectoryExists;
        public readonly bool RightDirectoryExists;
        public readonly bool LeftFileExists;
        public readonly bool RightFileExists;
        public bool LeftExists { get { return this.LeftDirectoryExists || this.LeftFileExists; } }
        public bool RightExists { get { return this.RightDirectoryExists || this.RightFileExists; } }

        private HashSet<string> _included;
        private HashSet<string> _excluded;

        public State(string leftPath, string rightPath, string filters)
        {
            leftPath = State.GetFullPath(leftPath);
            rightPath = State.GetFullPath(rightPath);

            if (Directory.Exists(leftPath))
            {
                leftPath = GetNormalizedDirectoryName(leftPath);

                this.LeftDirectoryExists = true;
            }
            else if (File.Exists(leftPath))
            {
                this.LeftFileExists = true;
            }

            this.LeftPath = leftPath;

            if (Directory.Exists(rightPath))
            {
                rightPath = GetNormalizedDirectoryName(rightPath);

                this.RightDirectoryExists = true;
            }
            else if (File.Exists(rightPath))
            {
                this.RightFileExists = true;
            }

            this.RightPath = rightPath;

            if (!string.IsNullOrWhiteSpace(filters))
            {
                foreach (var extension in filters.Split(';'))
                {
                    if (extension.Length != 0)
                    {
                        if (extension[0] == '-')
                        {
                            if (_excluded == null)
                                _excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                            _excluded.Add(extension.Substring(1));
                        }
                        else
                        {
                            if (_included == null)
                                _included = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                            _included.Add(extension);
                        }
                    }
                }
            }
        }

        public bool PassesFilters(string fileName)
        {
            if ((_included == null) || _included.Any(p => Match(fileName, p)))
            {
                if ((_excluded == null) || !_excluded.Any(p => Match(fileName, p)))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Match(string text, string pattern)
        {
            return Match(text, 0, pattern, 0);
        }

        private static bool Match(string text, int textOffset, string pattern, int patternOffset)
        {
            while (true)
            {
                if (patternOffset >= pattern.Length)
                {
                    return text.Length == textOffset;
                }
                else
                {
                    char p = pattern[patternOffset];

                    if (p == '*')
                    {
                        while ((++patternOffset < pattern.Length) && (pattern[patternOffset] == '*'))
                            ;   //Intentionall empty

                        if (patternOffset == pattern.Length)
                            return true;

                        while (textOffset < text.Length)
                        {
                            if (Match(text, textOffset, pattern, patternOffset))
                                return true;

                            ++textOffset;
                        }

                        return false;
                    }
                    else if ((text.Length == textOffset) || !IsMatch(text[textOffset], p))
                    {
                        return false;
                    }
                }

                ++textOffset;
                ++patternOffset;
            }
        }

        private static bool IsMatch(char a, char b)
        {
            return (b == '?') || (char.ToLowerInvariant(a) == char.ToLowerInvariant(b));
        }

        private static string GetFullPath(string path)
        {
            try
            {
                path = Path.GetFullPath(path);
            }
            catch(Exception)
            {
            }

            return path;
        }

        private static string GetNormalizedDirectoryName(string path)
        {
            // Normalize directory names so they alhave a training \.
            if ((path.Length > 0) && (path[path.Length - 1] != Path.DirectorySeparatorChar))
                path = path + "\\";

            return path;
        }
    }
}