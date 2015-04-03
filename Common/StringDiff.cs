using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiffPlex;
using DiffPlex.DiffBuilder;

namespace Relational.Octapus.BuildEngine
{
    public class StringDiffResult
    {
    }

    public class StringDiff
    {
        private SideBySideDiffBuilder diffEngine;

        public StringDiff()
        {
            diffEngine = new SideBySideDiffBuilder( new Differ() );                    
        }

        public StringDiffResult GetDiff(string string1, string string2)
        {
            var diffModel = diffEngine.BuildDiffModel(string1, string2);
            hasDifferences = diffModel.NewText!= diffModel.OldText ? true : false;

            return new StringDiffResult();
        }

        public bool hasDifferences { get; set; }
    }
}
