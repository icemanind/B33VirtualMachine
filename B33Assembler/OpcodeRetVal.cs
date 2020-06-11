namespace B33Assembler
{
    internal class OpcodeRetVal
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public OpcodeRetVal()
        {
            Success = true;
            ErrorMessage = "";
        }
    }
}
