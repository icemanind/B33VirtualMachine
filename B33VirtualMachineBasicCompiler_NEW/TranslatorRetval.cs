namespace B33VirtualMachineBasicCompiler
{
    public class TranslatorRetval
    {
        public bool HasErrors { get; set; }
        public int ErrorLineNumber { get; set; }
        public string ErrorMessage { get; set; }

        public TranslatorRetval()
        {
            HasErrors = false;
            ErrorLineNumber = 0;
            ErrorMessage = "";
        }
    }
}
