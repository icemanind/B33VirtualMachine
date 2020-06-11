using System.Collections.Generic;

namespace B33VirtualMachineBasicCompiler
{
    public interface IExpressionParser
    {
        ExpressionParserOutput Output { get; set; }
        Dictionary<string, string> Variables { get; set; }
        void Compile(string expression, int stringCounter, int loopCounter);
    }
}
