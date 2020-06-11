using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B33Assembler
{
    internal static class Opcodes
    {
        private static void Error(string error, OpcodeRetVal opcodeRetVal)
        {
            opcodeRetVal.ErrorMessage = error;
            opcodeRetVal.Success = false;
        }

        public static OpcodeRetVal Str(TokenParser tokenParser, int phase, ref ushort progLoc, List<byte> programData, int lineNumber)
        {
            var retval = new OpcodeRetVal();

            Token token = tokenParser.GetToken();

            if (token == null)
            {
                Error(string.Format("Unexpected End of File at line {0}!", lineNumber), retval);
                return retval;
            }
            if (token.TokenName != TokenParser.Tokens.WHITESPACE)
            {
                Error(string.Format("Expected Whitespace at line {0}!", lineNumber), retval);
                return retval;
            }
            token = tokenParser.GetToken();
            if (token == null || token.TokenName != TokenParser.Tokens.STRING)
            {
                Error(string.Format("Expected string at line {0}!", lineNumber), retval);
                return null;
            }
            byte[] bt = Encoding.ASCII.GetBytes(token.TokenValue.Remove(token.TokenValue.Length - 1, 1).Remove(0, 1));
            if (phase == 2)
            {
                for (int index = 0; index < bt.Length; index++)
                {
                    byte bty = bt[index];
                    programData.Add(bty);
                }
            }
            progLoc = (ushort)(progLoc + bt.Length);
            token = tokenParser.GetToken();
            while (token != null &&
                   (token.TokenName == TokenParser.Tokens.WHITESPACE ||
                    token.TokenName == TokenParser.Tokens.COMMENT))
            {
                token = tokenParser.GetToken();
            }
            return retval;
        }
    }
}
