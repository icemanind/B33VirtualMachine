using System;
using System.Linq;
using System.Windows.Forms;
using B33Assembler;
using B33VirtualMachineAssembler.Help;

namespace B33VirtualMachineAssembler
{
    public partial class CodeAnalyzer : Form
    {
        public string SourceCode { get; set; }
        public bool ShowNumbersInHexadecimal { get; set; }

        public CodeAnalyzer()
        {
            InitializeComponent();
            ShowNumbersInHexadecimal = true;
        }

        private void CodeAnalyzer_Load(object sender, EventArgs e)
        {
            string output = "";
            var assembler = new Assembler(SourceCode, 0);
            Assembler.B33Program program = assembler.Assemble(OutputTypes.RawBinary);

            if (!assembler.Successful)
                return;

            for (ushort i = 0; i < program.ExecutionAddress;)
            {
                string labelName = program.LabelDictionary.ContainsValue(i)
                            ? program.LabelDictionary.FirstOrDefault(z => z.Value == i).Key
                            : "";
                if (labelName == "")
                {
                    i++;
                    continue;
                }
                output = output + labelName + " = " + ToHex(i) + Environment.NewLine;

                i++;
            }

            output = output + Environment.NewLine;

            for (ushort i = program.ExecutionAddress; i < program.ProgramBytes.Length;)
            {
                CpuInstruction instruction = Help.Help.GetInstructionByByteCode(program.ProgramBytes[i]);
                if (instruction == null)
                    break;
                ushort num;
                string labelName;
                string tmp1;

                if (program.LabelDictionary.ContainsValue(i))
                {
                    output = output + program.LabelDictionary.FirstOrDefault(z => z.Value == i).Key + ":" + Environment.NewLine;
                }
                switch (instruction.ByteCode)
                {
                    case 0x01:
                    case 0x02:
                    case 0x2A:
                    case 0x2B:
                    case 0x2F:
                    case 0x30:
                        output = output + "    " +
                                 string.Format(instruction.AnalyticalText,
                                     ShowNumbersInHexadecimal
                                         ? ToHex(program.ProgramBytes[i + 1])
                                         : program.ProgramBytes[i + 1].ToString());
                        break;
                    case 0x03:
                    case 0x04:
                    case 0x05:
                        num = BitConverter.ToUInt16(new[] { program.ProgramBytes[i + 1], program.ProgramBytes[i + 2] }, 0);
                        labelName = program.LabelDictionary.ContainsValue(num)
                            ? program.LabelDictionary.FirstOrDefault(z => z.Value == num).Key
                            : "";
                        output = output + "    " + string.Format(instruction.AnalyticalText, GetNumberOrLabel(labelName, num));
                        break;
                    case 0x06:
                    case 0x07:
                        num = BitConverter.ToUInt16(new[] { program.ProgramBytes[i + 1], program.ProgramBytes[i + 2] }, 0);
                        labelName = program.LabelDictionary.ContainsValue(num)
                            ? program.LabelDictionary.FirstOrDefault(z => z.Value == num).Key
                            : "";
                        output = output + "    " + string.Format(instruction.AnalyticalText,
                                     "[Data at " + GetNumberOrLabel(labelName, num) + "]");
                        break;
                    case 0x08:
                    case 0x09:
                    case 0x0A:
                        num = BitConverter.ToUInt16(new[] { program.ProgramBytes[i + 1], program.ProgramBytes[i + 2] }, 0);
                        labelName = program.LabelDictionary.ContainsValue(num)
                            ? program.LabelDictionary.FirstOrDefault(z => z.Value == num).Key
                            : "";
                        output = output + "    " + string.Format(instruction.AnalyticalText,
                                     "[Data at " + GetNumberOrLabel(labelName, num) + "]");
                        break;
                    case 0x0B:
                    case 0x0C:
                    case 0x0D:
                    case 0x0E:
                    case 0x0F:
                        num = BitConverter.ToUInt16(new[] { program.ProgramBytes[i + 1], program.ProgramBytes[i + 2] }, 0);
                        labelName = program.LabelDictionary.ContainsValue(num)
                            ? program.LabelDictionary.FirstOrDefault(z => z.Value == num).Key
                            : "";
                        output = output + "    " +
                                 string.Format(instruction.AnalyticalText, GetNumberOrLabel(labelName, num));
                        break;
                    case 0x10:
                        tmp1 = "[Data at " + GetRegister(program.ProgramBytes[i + 2]) + " register] = ";
                        tmp1 = tmp1 + "A.";
                        if ((program.ProgramBytes[i + 2] & 32) == 32)
                        {
                            tmp1 = tmp1 + " Increment " + GetRegister(program.ProgramBytes[i + 2]) + " by ";
                            tmp1 = (program.ProgramBytes[i + 2] & 128) == 128 ? tmp1 + "2" : tmp1 + "1";
                        }
                        if ((program.ProgramBytes[i + 2] & 64) == 64)
                        {
                            tmp1 = tmp1 + " Decrement " + GetRegister(program.ProgramBytes[i + 2]) + " by ";
                            tmp1 = (program.ProgramBytes[i + 2] & 128) == 128 ? tmp1 + "2" : tmp1 + "1";
                        }
                        output = output + "    " + string.Format(instruction.AnalyticalText, tmp1);
                        break;
                    case 0x11:
                        tmp1 = "[Data at " + GetRegister(program.ProgramBytes[i + 2]) + " register] = ";
                        tmp1 = tmp1 + "B.";
                        if ((program.ProgramBytes[i + 2] & 32) == 32)
                        {
                            tmp1 = tmp1 + " Increment " + GetRegister(program.ProgramBytes[i + 2]) + " by ";
                            tmp1 = (program.ProgramBytes[i + 2] & 128) == 128 ? tmp1 + "2" : tmp1 + "1";
                        }
                        if ((program.ProgramBytes[i + 2] & 64) == 64)
                        {
                            tmp1 = tmp1 + " Decrement " + GetRegister(program.ProgramBytes[i + 2]) + " by ";
                            tmp1 = (program.ProgramBytes[i + 2] & 128) == 128 ? tmp1 + "2" : tmp1 + "1";
                        }
                        output = output + "    " + string.Format(instruction.AnalyticalText, tmp1);
                        break;
                    case 0x12:
                    case 0x13:
                        output = output + "    " +
                                 string.Format(instruction.AnalyticalText,
                                     ShowNumbersInHexadecimal
                                         ? ToHex(program.ProgramBytes[i + 1])
                                         : program.ProgramBytes[i + 1].ToString());
                        break;
                    case 0x14:
                    case 0x15:
                    case 0x16:
                        num = BitConverter.ToUInt16(new[] { program.ProgramBytes[i + 1], program.ProgramBytes[i + 2] }, 0);
                        labelName = program.LabelDictionary.ContainsValue(num)
                            ? program.LabelDictionary.FirstOrDefault(z => z.Value == num).Key
                            : "";
                        output = output + "    " + string.Format(instruction.AnalyticalText, GetNumberOrLabel(labelName, num));
                        break;
                    case 0x17:
                    case 0x18:
                        num = BitConverter.ToUInt16(new[] { program.ProgramBytes[i + 1], program.ProgramBytes[i + 2] }, 0);
                        labelName = program.LabelDictionary.ContainsValue(num)
                            ? program.LabelDictionary.FirstOrDefault(z => z.Value == num).Key
                            : "";
                        output = output + "    " + string.Format(instruction.AnalyticalText, GetNumberOrLabel(labelName, num));
                        break;
                    case 0x19:
                        tmp1 = "A = ";
                        tmp1 = tmp1 + "[Data at " + GetRegister(program.ProgramBytes[i + 2]) + " register].";
                        if ((program.ProgramBytes[i + 2] & 32) == 32)
                        {
                            tmp1 = tmp1 + " Increment " + GetRegister(program.ProgramBytes[i + 2]) + " by ";
                            tmp1 = (program.ProgramBytes[i + 2] & 128) == 128 ? tmp1 + "2" : tmp1 + "1";
                        }
                        if ((program.ProgramBytes[i + 2] & 64) == 64)
                        {
                            tmp1 = tmp1 + " Decrement " + GetRegister(program.ProgramBytes[i + 2]) + " by ";
                            tmp1 = (program.ProgramBytes[i + 2] & 128) == 128 ? tmp1 + "2" : tmp1 + "1";
                        }
                        output = output + "    " + string.Format(instruction.AnalyticalText, tmp1);
                        break;
                    case 0x1A:
                        tmp1 = "B = ";
                        tmp1 = tmp1 + "[Data at " + GetRegister(program.ProgramBytes[i + 2]) + " register].";
                        if ((program.ProgramBytes[i + 2] & 32) == 32)
                        {
                            tmp1 = tmp1 + " Increment " + GetRegister(program.ProgramBytes[i + 2]) + " by ";
                            tmp1 = (program.ProgramBytes[i + 2] & 128) == 128 ? tmp1 + "2" : tmp1 + "1";
                        }
                        if ((program.ProgramBytes[i + 2] & 64) == 64)
                        {
                            tmp1 = tmp1 + " Decrement " + GetRegister(program.ProgramBytes[i + 2]) + " by ";
                            tmp1 = (program.ProgramBytes[i + 2] & 128) == 128 ? tmp1 + "2" : tmp1 + "1";
                        }
                        output = output + "    " + string.Format(instruction.AnalyticalText, tmp1);
                        break;
                    case 0x20:
                    case 0x21:
                    case 0x22:
                    case 0x23:
                    case 0x24:
                    case 0x27:
                        num = BitConverter.ToUInt16(new[] { program.ProgramBytes[i + 1], program.ProgramBytes[i + 2] }, 0);
                        labelName = program.LabelDictionary.ContainsValue(num)
                            ? program.LabelDictionary.FirstOrDefault(z => z.Value == num).Key
                            : "";
                        output = output + "    " + string.Format(instruction.AnalyticalText, GetNumberOrLabel(labelName, num));
                        break;
                    case 0x25:
                    case 0x26:
                        output = output + "    " + string.Format(instruction.AnalyticalText, GetRegister(program.ProgramBytes[i + 1]));
                        break;
                    case 0x2C:
                    case 0x2D:
                    case 0x2E:
                    case 0x31:
                    case 0x32:
                    case 0x33:
                        num = BitConverter.ToUInt16(new[] { program.ProgramBytes[i + 1], program.ProgramBytes[i + 2] }, 0);
                        labelName = ShowNumbersInHexadecimal ? ToHex(num) : num.ToString();
                        output = output + "    " + string.Format(instruction.AnalyticalText, GetNumberOrLabel(labelName, num));
                        break;
                    case 0x34:
                    case 0x35:
                    case 0x36:
                    case 0x37:
                    case 0x38:
                    case 0x39:
                    case 0x3A:
                    case 0x3B:
                    case 0x3C:
                    case 0x3D:
                        output = output + "    " + string.Format(instruction.AnalyticalText, GetRegister(program.ProgramBytes[i + 1]));
                        break;
                    case 0x3E:
                        output = output + "    " + string.Format(instruction.AnalyticalText, GetRegister(program.ProgramBytes[i + 2]), GetRegister(program.ProgramBytes[i + 1]));
                        break;
                    case 0x3F:
                    case 0x40:
                    case 0x41:
                    case 0x42:
                    case 0x43:
                    case 0x44:
                    case 0x45:
                        output = output + "    " + string.Format(instruction.AnalyticalText, GetRegister(program.ProgramBytes[i + 1]));
                        break;
                    case 0x46:
                    case 0x47:
                        num = BitConverter.ToUInt16(new[] { program.ProgramBytes[i + 1], program.ProgramBytes[i + 2] }, 0);
                        labelName = program.LabelDictionary.ContainsValue(num)
                            ? program.LabelDictionary.FirstOrDefault(z => z.Value == num).Key
                            : "";
                        output = output + "    " + string.Format(instruction.AnalyticalText, GetNumberOrLabel(labelName, num));
                        break;
                    case 0x48:
                    case 0x49:
                        output = output + "    " + string.Format(instruction.AnalyticalText, GetRegister(program.ProgramBytes[i + 1]), GetRegister(program.ProgramBytes[i + 2]));
                        break;
                    case 0x4A:
                    case 0x4B:
                    case 0x54:
                    case 0x55:
                    case 0x60:
                    case 0x61:
                        output = output + "    " + string.Format(instruction.AnalyticalText, ToHex(program.ProgramBytes[i + 1]));
                        break;
                    case 0x4C:
                    case 0x4D:
                    case 0x4E:
                    case 0x56:
                    case 0x57:
                    case 0x58:
                    case 0x62:
                    case 0x63:
                    case 0x64:
                        num = BitConverter.ToUInt16(new[] { program.ProgramBytes[i + 1], program.ProgramBytes[i + 2] }, 0);
                        output = output + "    " + string.Format(instruction.AnalyticalText, ToHex(num));
                        break;
                    case 0x4F:
                    case 0x50:
                    case 0x51:
                    case 0x52:
                    case 0x53:
                    case 0x59:
                    case 0x5A:
                    case 0x5B:
                    case 0x5C:
                    case 0x5D:
                    case 0x65:
                    case 0x66:
                    case 0x67:
                    case 0x68:
                    case 0x69:
                    case 0x6A:
                    case 0x6F:
                        output = output + "    " + string.Format(instruction.AnalyticalText, GetRegister(program.ProgramBytes[i + 1]));
                        break;
                    case 0x5E:
                    case 0x5F:
                        output = output + "    " + string.Format(instruction.AnalyticalText, GetRegister(program.ProgramBytes[i + 1]), GetRegister(program.ProgramBytes[i + 2]));
                        break;
                    default:
                        output = output + "    " + instruction.AnalyticalText;
                        break;
                }
                output = output + Environment.NewLine;
                i = (ushort) (i + instruction.NumberOfBytes);
            }

            txtEditor.Text = output;
        }

        private string GetRegister(byte r)
        {
            if ((r & 1) == 1)
                return "A";
            if ((r & 2) == 2)
                return "B";
            if ((r & 4) == 4)
                return "D";
            if ((r & 8) == 8)
                return "X";
            if ((r & 16) == 16)
                return "Y";

            return "";
        }

        private static string ToHex(byte num)
        {
            return "$" + num.ToString("X2");
        }

        private static string ToHex(ushort num)
        {
            return "$" + num.ToString("X4");
        }

        private string GetNumberOrLabel(string label, byte num)
        {
            if (ShowNumbersInHexadecimal && string.IsNullOrEmpty(label))
                return ToHex(num);
            if ((!ShowNumbersInHexadecimal) && string.IsNullOrEmpty(label))
                return num.ToString();

            return label;
        }

        private string GetNumberOrLabel(string label, ushort num)
        {
            if (ShowNumbersInHexadecimal && string.IsNullOrEmpty(label))
                return ToHex(num);
            if ((!ShowNumbersInHexadecimal) && string.IsNullOrEmpty(label))
                return num.ToString();

            return label;
        }

        private bool IsPrintableChar(byte b)
        {
            return !(b < 0x20 || b > 127);
        }
    }
}
