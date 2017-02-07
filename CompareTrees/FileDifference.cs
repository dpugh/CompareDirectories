using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareTrees
{
    enum FileDifference
    {
        Identical = 0,
        DifferentInWhiteSpaceOnly = 1,
        DifferentExcludingWhiteSpace = 2
    }
}
