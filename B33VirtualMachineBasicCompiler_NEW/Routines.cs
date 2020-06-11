using System;
using System.Collections.Generic;
using System.Text;

namespace B33VirtualMachineBasicCompiler
{
    public enum Functions
    {
        FuncInit = 0,
        FuncUcase = 1,
        FuncLcase = 2,
        FuncPrint = 3,
        FuncMid = 4,
        FuncPrintNum = 5,
        FuncLen = 6,
        FuncRnd = 7,
        FuncAttribs = 8,
        FuncPeek = 9,
        FuncAsc = 10,
        FuncKey = 11,
        FuncInput = 12,
        FuncVal = 13,
        FuncInput2 = 14,
        FuncGetX = 15,
        FuncGetY = 16,
        FuncCls = 17
    }

    public static class Routines
    {
        private static readonly Dictionary<Functions, string> RoutineTable = new Dictionary<Functions, string>
        {
            {
                Functions.FuncInit, GetInitFunction()  
            },
            {
                Functions.FuncCls, GetClsFunction()
            }
            //{
            //    Functions.FuncUcase, GetUcaseFunction()
            //},
            //{
            //    Functions.FuncLcase, GetLcaseFunction()
            //},
            //{
            //    Functions.FuncPrint, GetPrintFunction()
            //},
            //{
            //    Functions.FuncMid, GetMidFunction()
            //},
            //{
            //    Functions.FuncPrintNum, GetPrintNumFunction()
            //},
            //{
            //    Functions.FuncLen, GetLenFunction()
            //},
            //{
            //    Functions.FuncRnd, GetRndFunction()
            //},
            //{
            //    Functions.FuncAttribs, GetAttribFunctions()
            //},
            //{
            //    Functions.FuncPeek, GetPeekFunction()
            //},
            //{
            //    Functions.FuncAsc, GetAscFunction()
            //},
            //{
            //    Functions.FuncKey, GetKeyFunction()
            //},
            //{
            //    Functions.FuncInput, GetInputFunction()
            //},
            //{
            //    Functions.FuncVal, GetValFunction()
            //},
            //{
            //    Functions.FuncInput2, GetInput2Function()
            //},
            //{
            //    Functions.FuncGetX, GetGetXFunction()
            //},
            //{
            //    Functions.FuncGetY, GetGetYFunction()
            //}
        };

        public static string GetRoutine(Functions function)
        {
            if (RoutineTable.ContainsKey(function))
                return RoutineTable[function];

            return "";
        }

        private static string GetClsFunction()
        {
            StringBuilder str = new StringBuilder();

            str.AppendLine("; This routine just clears the screen and resets");
            str.AppendLine("; the cursor to 0, 0 (Upper left corner)");
            str.AppendLine("funcCLS push x");
            str.AppendLine(" push d");
            str.AppendLine(" cls");
            str.AppendLine(" ldd #0");
            str.AppendLine(" ldx #curY");
            str.AppendLine(" stb ,x");
            str.AppendLine(" ldx #curX");
            str.AppendLine(" stb ,x");
            str.AppendLine(" call updatecursorpos");
            str.AppendLine(" pop d");
            str.AppendLine(" pop x");
            str.AppendLine(" ret");

            return str.ToString();
        }

        private static string GetInitFunction()
        {
            StringBuilder str = new StringBuilder();

            str.AppendLine("; This routine is called first when the program");
            str.AppendLine("; is first started. It does some initialization. First ");
            str.AppendLine("; it resets the cursor position to 0,0 (upper left corner)");
            str.AppendLine("; and then resets the colors to the default colors (white on black)");
            str.AppendLine("; and finally, it turns on the flashing underline cursor");
            str.AppendLine("TheBasicInit ldx #curX");
            str.AppendLine(" lda #0");
            str.AppendLine(" sta ,x");
            str.AppendLine(" ldx #curY");
            str.AppendLine(" sta ,x");
            str.AppendLine(" ldx #bgcolor");
            str.AppendLine(" sta ,x");
            str.AppendLine(" ldx #fgcolor");
            str.AppendLine(" lda #7");
            str.AppendLine(" sta ,x");
            str.AppendLine(" ldx #colorattr");
            str.AppendLine(" sta ,x");
            str.AppendLine("; Turn on the cursor by default, by storing");
            str.AppendLine("; a 1 into $EFA0.");
            str.AppendLine(" ldx #$efa0");
            str.AppendLine(" lda #1");
            str.AppendLine(" sta ,x");
            str.AppendLine(" ret");

            str.AppendLine("; This routine updates the cursor position");
            str.AppendLine("; based on the values of CurX and CurY");
            str.AppendLine("updatecursorpos push y");
            str.AppendLine(" push x");
            str.AppendLine(" ldx #$efa1");
            str.AppendLine(" call getcurpos2");
            str.AppendLine(" suby #$e000");
            str.AppendLine(" push d");
            str.AppendLine(" tfr y,d");
            str.AppendLine(" stb ,x+");
            str.AppendLine(" sta ,x");
            str.AppendLine(" pop d");
            str.AppendLine(" pop x");
            str.AppendLine(" pop y");
            str.AppendLine(" ret");

            str.AppendLine("; This routine converts the cursor position");
            str.AppendLine("; from CurX and CurY into a screen memory address.");
            str.AppendLine("getcurpos2 push a");
            str.AppendLine(" push b");
            str.AppendLine(" push x");
            str.AppendLine(" ldx #cury");
            str.AppendLine(" ldb ,x");
            str.AppendLine(" lda #0");
            str.AppendLine(" ldy #80");
            str.AppendLine(" mul16 y,b");
            str.AppendLine(" lda #2");
            str.AppendLine(" ldx #curx");
            str.AppendLine(" ldb ,x");
            str.AppendLine(" lda #0");
            str.AppendLine(" addy d");
            str.AppendLine(" addy #$e000");
            str.AppendLine(" pop x");
            str.AppendLine(" pop b");
            str.AppendLine(" pop a");
            str.AppendLine(" ret");

            return str.ToString();
        }
    }
}
