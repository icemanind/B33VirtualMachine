using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace B33VirtualMachineAssembler.Help
{
    public static class Help
    {
        public static CpuInstruction GetInstructionByByteCode(byte code)
        {
            return GetInstructions().FirstOrDefault(z => z.ByteCode == code);
        }

        private static List<CpuInstruction> GetInstructions()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string csv = "";
            var instructions = new List<CpuInstruction>();

            using (Stream stream = assembly.GetManifestResourceStream("B33VirtualMachineAssembler.Help.HelpData.txt"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    csv = reader.ReadToEnd();
                }
            }

            foreach (string line in csv.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n'))
            {
                if (string.IsNullOrEmpty(line) || line.Trim() == "")
                    continue;
                var instruction = new CpuInstruction();
                var insideQuotes = false;

                int ndx = 0;
                int oldNdx = 0;
                string tmp = "";

                while (line[ndx] != ',')
                {
                    tmp = tmp + line[ndx];
                    ndx++;
                }

                instruction.OpcodeId = int.Parse(tmp.Trim());
                tmp = "";
                ndx++;
                
                while (line[ndx] != ',')
                {
                    tmp = tmp + line[ndx];
                    ndx++;
                }

                instruction.Mnemonic = DeQuote(tmp);
                tmp = "";
                ndx++;
                
                while (line[ndx] != ',')
                {
                    tmp = tmp + line[ndx];
                    ndx++;
                }

                instruction.ByteCode = byte.Parse(tmp.Trim());
                tmp = "";
                ndx++;
                
                while (line[ndx] != ',' || insideQuotes)
                {
                    if (line[ndx] == '"')
                        insideQuotes = !insideQuotes;
                    tmp = tmp + line[ndx];
                    ndx++;
                }

                instruction.Usage = DeQuote(tmp.Trim());
                tmp = "";
                ndx++;

                while (line[ndx] != ',')
                {
                    tmp = tmp + line[ndx];
                    ndx++;
                }

                instruction.AddressingMode = DeQuote(tmp);
                tmp = "";
                ndx++;

                while (line[ndx] != ',')
                {
                    tmp = tmp + line[ndx];
                    ndx++;
                }

                instruction.NumberOfBytes = byte.Parse(tmp.Trim());
                tmp = "";
                ndx++;

                while (line[ndx] != ',' || insideQuotes)
                {
                    if (line[ndx] == '"')
                        insideQuotes = !insideQuotes;
                    tmp = tmp + line[ndx];
                    ndx++;
                }

                instruction.Flow = DeQuote(tmp.Trim());
                tmp = "";
                ndx++;

                while (line[ndx] != ',' || insideQuotes)
                {
                    if (line[ndx] == '"')
                        insideQuotes = !insideQuotes;
                    tmp = tmp + line[ndx];
                    ndx++;
                }

                instruction.Description = DeQuote(tmp.Trim());
                tmp = "";
                ndx++;

                while (ndx < line.Length || insideQuotes)
                {
                    if (line[ndx] == '"')
                        insideQuotes = !insideQuotes;
                    tmp = tmp + line[ndx];
                    ndx++;
                }

                instruction.AnalyticalText = DeQuote(tmp.Trim());

                instructions.Add(instruction);
            }

            return instructions;
        }

        private static string DeQuote(string str)
        {
            str = str.Replace("\"\"", "\"");
            if (str[0] != '"')
                return str;

            str = str.Remove(0, 1);
            if (str.Length > 0)
                str = str.Remove(str.Length - 1, 1);

            return str;
        }
    }

    public class CpuInstruction
    {
        public int OpcodeId { get; set; }
        public string Mnemonic { get; set; }
        public byte ByteCode { get; set; }
        public string Usage { get; set; }
        public string AddressingMode { get; set; }
        public byte NumberOfBytes { get; set; }
        public string Flow { get; set; }
        public string Description { get; set; }
        public string AnalyticalText { get; set; }
    }
}
