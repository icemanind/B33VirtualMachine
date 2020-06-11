namespace B33VirtualMachineBasicCompiler
{
    public class AssemblyRetVal
    {
        public string AssemblyCode { get; set; }
        public string InitializationCode { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
        public int LineNumber { get; set; }
    }
}
