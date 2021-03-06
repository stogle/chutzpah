﻿using Chutzpah.Wrappers;
using SourceMapDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chutzpah.Models;

namespace Chutzpah.Coverage
{
    public class SourceMapDotNetLineCoverageMapper : ILineCoverageMapper
    {
        IFileSystemWrapper fileSystem;

        public SourceMapDotNetLineCoverageMapper(IFileSystemWrapper fileSystem) 
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException("fileSystem");
            }

            this.fileSystem = fileSystem;
        }

        public int?[] GetOriginalFileLineExecutionCounts(int?[] generatedSourceLineExecutionCounts, int sourceLineCount, ReferencedFile referencedFile)
        {
            if (generatedSourceLineExecutionCounts == null)
            {
                throw new ArgumentNullException("generatedSourceLineExecutionCounts");
            }
            if (referencedFile == null)
            {
                throw new ArgumentNullException("referencedFile");
            }
            else if (string.IsNullOrWhiteSpace(referencedFile.SourceMapFilePath)) 
            {
                return generatedSourceLineExecutionCounts;
            }
            else if (!fileSystem.FileExists(referencedFile.SourceMapFilePath)) 
            {
                throw new ArgumentException("mapFilePath", string.Format("Cannot find map file '{0}'", referencedFile.SourceMapFilePath));
            }

            var consumer = this.GetConsumer(fileSystem.GetText(referencedFile.SourceMapFilePath));

            var accumulated = new List<int?>(new int?[sourceLineCount + 1]);
            for (var i = 1; i < generatedSourceLineExecutionCounts.Length; i++)
            {
                int? generatedCount = generatedSourceLineExecutionCounts[i];
                if (generatedCount == null)
                {
                    continue;
                }

                var matches = consumer.OriginalPositionsFor(i);
                if (matches.Any())
                {
                    foreach (var match in matches)
                    {
                        if (match.File.ToLower() != fileSystem.GetFileName(referencedFile.Path.ToLower())) continue;

                        accumulated[match.LineNumber] = (accumulated[match.LineNumber] ?? 0) + generatedCount.Value;
                    }
                }
            }

            return accumulated.ToArray();
        }

        protected virtual ISourceMapConsumer GetConsumer(string mapFileContents)
        {
            return new SourceMapConsumer(mapFileContents);
        }
    }
}
