using System.Collections.Generic;

namespace B33VirtualMachineBasicCompiler
{
    public class ExpressionParserOutput
    {
        public string InitCode { get; set; }
        public string Output { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
        public int StringCounter { get; set; }
        public int LoopCounter { get; set; }
        public int BoolCounter { get; set; }
        public List<Functions> Functions { get; set; }

        public ExpressionParserOutput()
        {
            InitCode = "";
            Output = "";
            ErrorMessage = "";
            HasError = false;
            StringCounter = 0;
            LoopCounter = 0;
            BoolCounter = 0;
            Functions = new List<Functions>();
        }
    }
}
