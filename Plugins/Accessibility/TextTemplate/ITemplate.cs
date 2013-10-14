using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextTemplate
{
    public interface ITemplate
    {
        string GetText();

        List<string> Gaps { get; }
    }
}
