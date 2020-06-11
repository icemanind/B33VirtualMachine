using System;
using System.Collections.Generic;

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
        FuncLtrim = 17,
        FuncRtrim = 18,
        FuncStr = 19,
        FuncInstr = 20
    }

    public static class Routines
    {
        private static readonly Dictionary<Functions, string> RoutineTable = new Dictionary<Functions, string>
        {
            {
                Functions.FuncInit, GetInitFunction()  
            },
            {
                Functions.FuncUcase, GetUcaseFunction()
            },
            {
                Functions.FuncLcase, GetLcaseFunction()
            },
            {
                Functions.FuncPrint, GetPrintFunction()
            },
            {
                Functions.FuncMid, GetMidFunction()
            },
            {
                Functions.FuncPrintNum, GetPrintNumFunction()
            },
            {
                Functions.FuncLen, GetLenFunction()
            },
            {
                Functions.FuncRnd, GetRndFunction()
            },
            {
                Functions.FuncAttribs, GetAttribFunctions()
            },
            {
                Functions.FuncPeek, GetPeekFunction()
            },
            {
                Functions.FuncAsc, GetAscFunction()
            },
            {
                Functions.FuncKey, GetKeyFunction()
            },
            {
                Functions.FuncInput, GetInputFunction()
            },
            {
                Functions.FuncVal, GetValFunction()
            },
            {
                Functions.FuncInput2, GetInput2Function()
            },
            {
                Functions.FuncGetX, GetGetXFunction()
            },
            {
                Functions.FuncGetY, GetGetYFunction()
            },
            {
                Functions.FuncLtrim, GetLtrimFunction()
            },
            {
                Functions.FuncRtrim, GetRtrimFunction()
            },
            {
                Functions.FuncStr, GetStrFunction()
            },
            {
                Functions.FuncInstr, GetInstrFunction()
            }
        };

        public static string GetRoutine(Functions function)
        {
            if (RoutineTable.ContainsKey(function))
                return RoutineTable[function];

            return "";
        }

        private static string GetInstrFunction()
        {
            string str = "";

            return str;
        }

        private static string GetGetXFunction()
        {
            string str = "funcGETX push d" + Environment.NewLine;

            str = str + " ldx #curX" + Environment.NewLine;
            str = str + " ldb ,x" + Environment.NewLine;
            str = str + " lda #0" + Environment.NewLine;
            str = str + " tfr d,x" + Environment.NewLine;
            str = str + " pop d" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            return str;
        }

        private static string GetGetYFunction()
        {
            string str = "funcGETX push d" + Environment.NewLine;

            str = str + " ldx #curY" + Environment.NewLine;
            str = str + " ldb ,x" + Environment.NewLine;
            str = str + " lda #0" + Environment.NewLine;
            str = str + " tfr d,x" + Environment.NewLine;
            str = str + " pop d" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            return str;
        }

        private static string GetValFunction()
        {
            string str = "";

            str = str + "funcVAL push y" + Environment.NewLine;
            str = str + " push d" + Environment.NewLine;
            str = str + " ldy #0" + Environment.NewLine;
            str = str + "valloop1 lda ,x+" + Environment.NewLine;
            str = str + " cmpa #0" + Environment.NewLine;
            str = str + " jeq valexit" + Environment.NewLine;
            str = str + " cmpa #48" + Environment.NewLine;
            str = str + " jlt valloopzero" + Environment.NewLine;
            str = str + " cmpa #57" + Environment.NewLine;
            str = str + " jgt valloopzero" + Environment.NewLine;
            str = str + " suba #48" + Environment.NewLine;
            str = str + " ldb #10" + Environment.NewLine;
            str = str + " mul16 y,b" + Environment.NewLine;
            str = str + " addy a" + Environment.NewLine;
            str = str + " jmp valloop1" + Environment.NewLine;
            str = str + "valexit tfr y,x" + Environment.NewLine;
            str = str + "valloopreturn pop d" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;
            str = str + "valloopzero ldx #0" + Environment.NewLine;
            str = str + " jmp valloopreturn" + Environment.NewLine;
            
            return str;
        }

        private static string GetInput2Function()
        {
            string str = "";

            str = str + "funcINPUT2 push y" + Environment.NewLine;
            str = str + " push d" + Environment.NewLine;
            str = str + " ldy #InputBuffer" + Environment.NewLine;
            str = str + " ldx #$fffe" + Environment.NewLine;
            str = str + " lda #0" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " ldx #$ffff" + Environment.NewLine;
            str = str + "funcKeyLoop122 ldb ,x" + Environment.NewLine;
            str = str + " cmpb #0" + Environment.NewLine;
            str = str + " jeq funcKeyLoop122" + Environment.NewLine;
            str = str + " cmpb #8" + Environment.NewLine;
            str = str + " jeq funcKeyBs22" + Environment.NewLine;
            str = str + " cmpb #13" + Environment.NewLine;
            str = str + " jeq funcKeyExit22" + Environment.NewLine;
            str = str + " cmpb #47" + Environment.NewLine;
            str = str + " jle funcKeyLoop122" + Environment.NewLine;
            str = str + " cmpb #58" + Environment.NewLine;
            str = str + " jge funcKeyLoop122" + Environment.NewLine;
            str = str + " push a" + Environment.NewLine;
            str = str + " tfr b,a" + Environment.NewLine;
            str = str + " call prntchar" + Environment.NewLine;
            str = str + " pop a" + Environment.NewLine;
            str = str + " adda #1" + Environment.NewLine;
            str = str + " stb ,y+" + Environment.NewLine;
            str = str + " jmp funcKeyLoop122" + Environment.NewLine;
            str = str + "funcKeyBs22 cmpa #0" + Environment.NewLine;
            str = str + " jeq funcKeyLoop122" + Environment.NewLine;
            str = str + " suby #1" + Environment.NewLine;
            str = str + " suba #1" + Environment.NewLine;
            str = str + " call prntBksp22" + Environment.NewLine;
            str = str + " jmp funcKeyLoop122" + Environment.NewLine;
            str = str + "funcKeyExit22 lda #0" + Environment.NewLine;
            str = str + " sta ,y" + Environment.NewLine;
            str = str + " ldy #InputBuffer" + Environment.NewLine;
            str = str + " tfr y,x" + Environment.NewLine;
            str = str + " call funcVAL" + Environment.NewLine;
            str = str + " pop d" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            str = str + "prntBksp22 push x" + Environment.NewLine;
            str = str + " push y" + Environment.NewLine;
            str = str + " push d" + Environment.NewLine;
            str = str + " ldx #curX" + Environment.NewLine;
            str = str + " lda ,x" + Environment.NewLine;
            str = str + " cmpa #0" + Environment.NewLine;
            str = str + " jeq prntBksp222" + Environment.NewLine;
            str = str + " suba #1" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " call updatecursorpos" + Environment.NewLine;
            str = str + "prntBksp322 lda #32" + Environment.NewLine;
            str = str + " call prntchar" + Environment.NewLine;
            str = str + " ldx #curX" + Environment.NewLine;
            str = str + " lda ,x" + Environment.NewLine;
            str = str + " cmpa #0" + Environment.NewLine;
            str = str + " jeq prntBksp422" + Environment.NewLine;
            str = str + " suba #1" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " call updatecursorpos" + Environment.NewLine;
            str = str + "prntBksp522 jmp prntBkspExit22" + Environment.NewLine;
            str = str + "prntBksp222 ldx #curY" + Environment.NewLine;
            str = str + " lda ,x" + Environment.NewLine;
            str = str + " suba #1" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " ldx #curX" + Environment.NewLine;
            str = str + " lda #79" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " jmp prntBksp322" + Environment.NewLine;
            str = str + "prntBksp422 ldx #curY" + Environment.NewLine;
            str = str + " lda ,x" + Environment.NewLine;
            str = str + " suba #1" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " ldx #curX" + Environment.NewLine;
            str = str + " lda #79" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " call updatecursorpos" + Environment.NewLine;
            str = str + " jmp prntBksp522" + Environment.NewLine;

            str = str + "prntBkspExit22 pop d" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            return str;
        }

        private static string GetInputFunction()
        {
            string str = "";

            str = str + "funcINPUT push y" + Environment.NewLine;
            str = str + " push d" + Environment.NewLine;
            str = str + " call allocatememory" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + " tfr x,y" + Environment.NewLine;
            //str = str + " ldy #InputBuffer" + Environment.NewLine;
            str = str + " ldx #$fffe" + Environment.NewLine;
            str = str + " lda #0" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " ldx #$ffff" + Environment.NewLine;
            str = str + "funcKeyLoop1 ldb ,x" + Environment.NewLine;
            str = str + " cmpb #0" + Environment.NewLine;
            str = str + " jeq funcKeyLoop1" + Environment.NewLine;
            str = str + " cmpb #8" + Environment.NewLine;
            str = str + " jeq funcKeyBs" + Environment.NewLine;
            str = str + " cmpb #13" + Environment.NewLine;
            str = str + " jeq funcKeyExit" + Environment.NewLine;
            str = str + " cmpb #31" + Environment.NewLine;
            str = str + " jle funcKeyLoop1" + Environment.NewLine;
            str = str + " push a" + Environment.NewLine;
            str = str + " tfr b,a" + Environment.NewLine;
            str = str + " call prntchar" + Environment.NewLine;
            str = str + " pop a" + Environment.NewLine;
            str = str + " adda #1" + Environment.NewLine;
            str = str + " stb ,y+" + Environment.NewLine;
            str = str + " jmp funcKeyLoop1" + Environment.NewLine;
            str = str + "funcKeyBs cmpa #0" + Environment.NewLine;
            str = str + " jeq funcKeyLoop1" + Environment.NewLine;
            str = str + " suby #1" + Environment.NewLine;
            str = str + " suba #1" + Environment.NewLine;
            str = str + " call prntBksp" + Environment.NewLine;
            str = str + " jmp funcKeyLoop1" + Environment.NewLine;
            str = str + "funcKeyExit lda #0" + Environment.NewLine;
            str = str + " sta ,y" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " pop d" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            str = str + "prntBksp push x" + Environment.NewLine;
            str = str + " push y" + Environment.NewLine;
            str = str + " push d" + Environment.NewLine;
            str = str + " ldx #curX" + Environment.NewLine;
            str = str + " lda ,x" + Environment.NewLine;
            str = str + " cmpa #0" + Environment.NewLine;
            str = str + " jeq prntBksp2" + Environment.NewLine;
            str = str + " suba #1" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " call updatecursorpos" + Environment.NewLine;
            str = str + "prntBksp3 lda #32" + Environment.NewLine;
            str = str + " call prntchar" + Environment.NewLine;
            str = str + " ldx #curX" + Environment.NewLine;
            str = str + " lda ,x" + Environment.NewLine;
            str = str + " cmpa #0" + Environment.NewLine;
            str = str + " jeq prntBksp4" + Environment.NewLine;
            str = str + " suba #1" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " call updatecursorpos" + Environment.NewLine;
            str = str + "prntBksp5 jmp prntBkspExit" + Environment.NewLine;
            str = str + "prntBksp2 ldx #curY" + Environment.NewLine;
            str = str + " lda ,x" + Environment.NewLine;
            str = str + " suba #1" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " ldx #curX" + Environment.NewLine;
            str = str + " lda #79" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " jmp prntBksp3" + Environment.NewLine;
            str = str + "prntBksp4 ldx #curY" + Environment.NewLine;
            str = str + " lda ,x" + Environment.NewLine;
            str = str + " suba #1" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " ldx #curX" + Environment.NewLine;
            str = str + " lda #79" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " call updatecursorpos" + Environment.NewLine;
            str = str + " jmp prntBksp5" + Environment.NewLine;

            str = str + "prntBkspExit pop d" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            return str;
        }

        private static string GetKeyFunction()
        {
            string str = "";

            str = str + "funckey push a" + Environment.NewLine;
            str = str + " push b" + Environment.NewLine;
            str = str + " ldx #$ffff" + Environment.NewLine;
            str = str + " ldb ,x" + Environment.NewLine;
            str = str + " lda #0" + Environment.NewLine;
            str = str + " tfr d,x" + Environment.NewLine;
            str = str + " pop b" + Environment.NewLine;
            str = str + " pop a" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            return str;
        }

        private static string GetAscFunction()
        {
            string str = "";

            str = str + "funcASC push a" + Environment.NewLine;
            str = str + " push b" + Environment.NewLine;
            str = str + " ldb ,x" + Environment.NewLine;
            str = str + " lda #0" + Environment.NewLine;
            str = str + " tfr d,x" + Environment.NewLine;
            str = str + " pop b" + Environment.NewLine;
            str = str + " pop a" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            return str;
        }

        private static string GetPeekFunction()
        {
            string str = "";

            str = str + "funcpeek push a" + Environment.NewLine;
            str = str + " push b" + Environment.NewLine;
            str = str + " ldb ,x" + Environment.NewLine;
            str = str + " lda #0" + Environment.NewLine;
            str = str + " tfr d,x" + Environment.NewLine;
            str = str + " pop b" + Environment.NewLine;
            str = str + " pop a" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            return str;
        }

        private static string GetAttribFunctions()
        {
            string str = "";
            str = str + "; Returns an attribute color based" + Environment.NewLine;
            str = str + "; on background and foreground color" + Environment.NewLine;
            str = str + "; A register should contain background color" + Environment.NewLine;
            str = str + "; B register should contain foreground color" + Environment.NewLine;
            str = str + "; The attribute will be returned into the A register" + Environment.NewLine;
            str = str + "getattrib lsft a" + Environment.NewLine;
            str = str + " lsft a" + Environment.NewLine;
            str = str + " lsft a" + Environment.NewLine;
            str = str + " lsft a" + Environment.NewLine;
            str = str + " adda b" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            str = str + "setattrib push x" + Environment.NewLine;
            str = str + " ldx #bgcolor" + Environment.NewLine;
            str = str + " lda ,x" + Environment.NewLine;
            str = str + " ldx #fgcolor" + Environment.NewLine;
            str = str + " ldb ,x" + Environment.NewLine;
            str = str + " call getattrib" + Environment.NewLine;
            str = str + " ldx #colorattr" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            return str;
        }
        private static string GetRndFunction()
        {
            string str = "";

            str = str + "funcrnd rnd x" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            return str;
        }

        private static string GetLenFunction()
        {
            string str = "";

            str = str + "; Returns the length of a string" + Environment.NewLine;
            str = str + "; X register should point to a string buffer" + Environment.NewLine;
            str = str + "; The length is returned in the X register" + Environment.NewLine;
            str = str + "funclen push a" + Environment.NewLine;
            str = str + " push b" + Environment.NewLine;
            //str = str + " push x" + Environment.NewLine;
            str = str + " ldb #0" + Environment.NewLine;
            str = str + "lenloop lda ,x+" + Environment.NewLine;
            str = str + " cmpa #0" + Environment.NewLine;
            str = str + " jeq lendone" + Environment.NewLine;
            str = str + " addb #1" + Environment.NewLine;
            str = str + " jmp lenloop" + Environment.NewLine;
            str = str + "lendone tfr d,x" + Environment.NewLine;
            //str = str + " pop x" + Environment.NewLine;
            str = str + " pop b" + Environment.NewLine;
            str = str + " pop a" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;
            return str;
        }

        private static string GetPrintNumFunction()
        {
            string str = "";

            str = str + "; Converts a number to a string" + Environment.NewLine;
            str = str + "; X = Number to convert" + Environment.NewLine;
            str = str + "; Y = Buffer to store number" + Environment.NewLine;
            str = str + "num2txt push a" + Environment.NewLine;
            str = str + " push b" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + " push y" + Environment.NewLine;
            str = str + " cmpx #0" + Environment.NewLine;
            str = str + " jeq num2txtw0" + Environment.NewLine;
            str = str + "num2txtloop cmpx #0" + Environment.NewLine;
            str = str + " jle num2txtxit2" + Environment.NewLine;
            str = str + " ldb #10" + Environment.NewLine;
            str = str + " push y" + Environment.NewLine;
            str = str + " div16 x,b" + Environment.NewLine;
            str = str + " push a" + Environment.NewLine;
            str = str + " tfr y,d" + Environment.NewLine;
            str = str + " pop a" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " addb #48" + Environment.NewLine;
            str = str + " stb ,y+" + Environment.NewLine;
            str = str + " jmp num2txtloop" + Environment.NewLine;
            str = str + "num2txtw0 lda #48" + Environment.NewLine;
            str = str + " sta ,y+" + Environment.NewLine;
            str = str + "num2txtxit2 lda #0" + Environment.NewLine;
            str = str + " sta ,y" + Environment.NewLine;
            str = str + "num2txtexit pop y" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " pop b" + Environment.NewLine;
            str = str + " pop a" + Environment.NewLine;
            str = str + " push a" + Environment.NewLine;
            str = str + " push b" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + " push y" + Environment.NewLine;
            str = str + " tfr y,x" + Environment.NewLine;
            str = str + " call reverse" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " pop b" + Environment.NewLine;
            str = str + " pop a" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            str = str + "reverse push x" + Environment.NewLine;
            str = str + " ldb #0" + Environment.NewLine;
            str = str + "reverselp1 lda ,x+" + Environment.NewLine;
            str = str + " cmpa #0" + Environment.NewLine;
            str = str + " jeq reversedn1" + Environment.NewLine;
            str = str + " addb #1" + Environment.NewLine;
            str = str + " jmp reverselp1" + Environment.NewLine;
            str = str + "reversedn1 pop x" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + " ldy #revbuffer" + Environment.NewLine;
            str = str + " subb #1" + Environment.NewLine;
            str = str + "reverselp2 lda b,x" + Environment.NewLine;
            str = str + " sta ,y+" + Environment.NewLine;
            str = str + " cmpb #0" + Environment.NewLine;
            str = str + " jeq reversedn2" + Environment.NewLine;
            str = str + " subb #1" + Environment.NewLine;
            str = str + " jmp reverselp2" + Environment.NewLine;
            str = str + "reversedn2 lda #0" + Environment.NewLine;
            str = str + " sta ,y" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + " ldy #revbuffer" + Environment.NewLine;
            str = str + "reverselp3 lda ,y+" + Environment.NewLine;
            str = str + " sta ,x+" + Environment.NewLine;
            str = str + " cmpa #0" + Environment.NewLine;
            str = str + " jeq reversedn3" + Environment.NewLine;
            str = str + " jmp reverselp3" + Environment.NewLine;
            str = str + "reversedn3 pop x" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            return str;
        }

        private static string GetMidFunction()
        {
            string str = "";

            str = str + "funcMID tfr x,d" + Environment.NewLine;
            str = str + " tfr b,a" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " push a" + Environment.NewLine;
            str = str + " tfr x,d" + Environment.NewLine;
            str = str + " pop a" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " push y" + Environment.NewLine; // Y = Save
            str = str + " push x" + Environment.NewLine; // X = memory to deallocate
            str = str + " suba #1" + Environment.NewLine;
            str = str + " tfr x,y" + Environment.NewLine;
            str = str + " call allocatememory" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + "midloop push b" + Environment.NewLine;
            str = str + " ldb a,y" + Environment.NewLine;
            str = str + " adda #1" + Environment.NewLine;
            str = str + " stb ,x+" + Environment.NewLine;
            str = str + " pop b" + Environment.NewLine;
            str = str + " subb #1" + Environment.NewLine;
            str = str + " cmpb #0" + Environment.NewLine;
            str = str + " jeq midexit" + Environment.NewLine;
            str = str + " jmp midloop" + Environment.NewLine;
            str = str + "midexit stb ,x" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " tfr x,y" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " call freememory" + Environment.NewLine;
            str = str + " tfr y,x" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            return str;
        }

        private static string GetLcaseFunction()
        {
            string str = "funcLCASE push a" + Environment.NewLine;
            str = str + " push y" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + " tfr x,y" + Environment.NewLine;
            str = str + " call allocatememory" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + "lowerloop1 lda ,y+" + Environment.NewLine;
            str = str + " cmpa #0" + Environment.NewLine;
            str = str + " jeq lowerend" + Environment.NewLine;
            str = str + " cmpa #65" + Environment.NewLine;
            str = str + " jlt lowerloop2" + Environment.NewLine;
            str = str + " cmpa #90" + Environment.NewLine;
            str = str + " jgt lowerloop2" + Environment.NewLine;
            str = str + " adda #32" + Environment.NewLine;
            str = str + "lowerloop2 sta ,x+" + Environment.NewLine;
            str = str + " jmp lowerloop1" + Environment.NewLine;
            str = str + "lowerend sta ,x" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " call freememory" + Environment.NewLine;
            str = str + " tfr y,x" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " pop a" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            return str;
        }

        private static string GetUcaseFunction()
        {
            string str = "funcUCASE push a" + Environment.NewLine;
            str = str + " push y" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + " tfr x,y" + Environment.NewLine;
            str = str + " call allocatememory" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + "upperloop1 lda ,y+" + Environment.NewLine;
            str = str + " cmpa #0" + Environment.NewLine;
            str = str + " jeq upperend" + Environment.NewLine;
            str = str + " cmpa #97" + Environment.NewLine;
            str = str + " jlt upperloop2" + Environment.NewLine;
            str = str + " cmpa #122" + Environment.NewLine;
            str = str + " jgt upperloop2" + Environment.NewLine;
            str = str + " suba #32" + Environment.NewLine;
            str = str + "upperloop2 sta ,x+" + Environment.NewLine;
            str = str + " jmp upperloop1" + Environment.NewLine;
            str = str + "upperend sta ,x" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " call freememory" + Environment.NewLine;
            str = str + " tfr y,x" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " pop a" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            return str;
        }

        private static string GetStrFunction()
        {
            string str = "";
            
            str = str + "funcSTR push a" + Environment.NewLine;
            str = str + " push b" + Environment.NewLine;
            str = str + " push y" + Environment.NewLine;
            str = str + " tfr x,y" + Environment.NewLine;
            str = str + " call allocatememory" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + "strloop1 cmpy #10" + Environment.NewLine;
            str = str + " jlt strdone" + Environment.NewLine;
            str = str + " lda #10" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + " div16 y,a" + Environment.NewLine;
            str = str + " tfr y,d" + Environment.NewLine;
            str = str + " tfr x,y" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " addb #$30" + Environment.NewLine;
            str = str + " stb ,x+" + Environment.NewLine;
            str = str + " jmp strloop1" + Environment.NewLine;
            str = str + "strdone tfr y,d" + Environment.NewLine;
            str = str + " addb #$30" + Environment.NewLine;
            str = str + " stb ,x+" + Environment.NewLine;
            str = str + " ldb #0" + Environment.NewLine;
            str = str + " stb ,x" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " tfr x,y" + Environment.NewLine;
            str = str + " call allocatememory" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + " push y" + Environment.NewLine;
            
            str = str + " ldb #0" + Environment.NewLine;

            str = str + "strloop2 lda ,y+" + Environment.NewLine;
            str = str + " cmpa #0" + Environment.NewLine;
            str = str + " jeq strdone2" + Environment.NewLine;
            str = str + " addb #1" + Environment.NewLine;
            str = str + " jmp strloop2" + Environment.NewLine;
            str = str + "strdone2 suby #2" + Environment.NewLine;
            str = str + "strdone3 cmpb #0" + Environment.NewLine;
            str = str + " jeq strdone4" + Environment.NewLine;
            str = str + " lda ,y-" + Environment.NewLine;
            str = str + " sta ,x+" + Environment.NewLine;
            str = str + " subb #1" + Environment.NewLine;
            str = str + " jmp strdone3" + Environment.NewLine;
            str = str + "strdone4 stb ,x" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " call freememory" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " pop b" + Environment.NewLine;
            str = str + " pop a" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            return str;
        }

        private static string GetLtrimFunction()
        {
            string str = "funcLTRIM push a" + Environment.NewLine;
            str = str + " push y" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + " tfr x,y" + Environment.NewLine;
            str = str + " call allocatememory" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + "ltrimloop1 lda ,y+" + Environment.NewLine;
            str = str + " cmpa #0" + Environment.NewLine;
            str = str + " jeq ltrimexit" + Environment.NewLine;
            str = str + " cmpa #32" + Environment.NewLine;
            str = str + " jeq ltrimloop1" + Environment.NewLine;
            str = str + " suby #1" + Environment.NewLine;
            str = str + "ltrimloop2 lda ,y+" + Environment.NewLine;
            str = str + " cmpa #0" + Environment.NewLine;
            str = str + " jeq ltrimexit" + Environment.NewLine;
            str = str + " sta ,x+" + Environment.NewLine;
            str = str + " jmp ltrimloop2" + Environment.NewLine;
            str = str + "ltrimexit sta ,x" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " call freememory" + Environment.NewLine;
            str = str + " tfr y,x" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " pop a" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            return str;
        }

        private static string GetRtrimFunction()
        {
            string str = "funcRTRIM push a" + Environment.NewLine;
            str = str + " push b" + Environment.NewLine;
            str = str + " push y" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + " tfr x,y" + Environment.NewLine;
            str = str + " call allocatememory" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + " push y" + Environment.NewLine;
            str = str + " ldb #0" + Environment.NewLine;
            str = str + "rtrimloop1 lda ,y+" + Environment.NewLine;
            str = str + " cmpa #0" + Environment.NewLine;
            str = str + " jeq rtrimend1" + Environment.NewLine;
            str = str + " addb #1" + Environment.NewLine;
            str = str + " jmp rtrimloop1" + Environment.NewLine;
            str = str + "rtrimend1 suby #2" + Environment.NewLine;
            str = str + "rtrimloop2 lda ,y-" + Environment.NewLine;
            str = str + " cmpa #0" + Environment.NewLine;
            str = str + " jeq rtrimend2" + Environment.NewLine;
            str = str + " subb #1" + Environment.NewLine;
            str = str + " cmpb #0" + Environment.NewLine;
            str = str + " jeq rtrimend4" + Environment.NewLine;
            str = str + " cmpa #32" + Environment.NewLine;
            str = str + " jeq rtrimloop2" + Environment.NewLine;
            str = str + " addy #1" + Environment.NewLine;
            str = str + "rtrimend4 addy #1" + Environment.NewLine;
            str = str + " lda #0" + Environment.NewLine;
            str = str + " sta ,y" + Environment.NewLine;
            str = str + "rtrimend2 pop y" + Environment.NewLine;
            str = str + "rtrimloop3 lda ,y+" + Environment.NewLine;
            str = str + " cmpa #0" + Environment.NewLine;
            str = str + " jeq rtrimend3" + Environment.NewLine;
            str = str + " sta ,x+" + Environment.NewLine;
            str = str + " jmp rtrimloop3" + Environment.NewLine;
            str = str + "rtrimend3 sta ,x" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " call freememory" + Environment.NewLine;
            str = str + " tfr y,x" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " pop b" + Environment.NewLine;
            str = str + " pop a" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            return str;
        }

        private static string GetPrintFunction()
        {
            string str = "funcPRINT push x" + Environment.NewLine;
            str = str + " push a" + Environment.NewLine;
            str = str + "prntloop lda ,x+" + Environment.NewLine;
            str = str + " cmpa #0" + Environment.NewLine;
            str = str + " jeq prntdone" + Environment.NewLine;
            str = str + " cmpa #13" + Environment.NewLine;
            str = str + " jeq basprntcrsc2" + Environment.NewLine;
            str = str + " call prntchar" + Environment.NewLine;
            str = str + " jmp prntloop" + Environment.NewLine;
            str = str + "prntdone pop a" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            str = str + "basprntcrsc2 call basprntCR" + Environment.NewLine;
            str = str + " jmp prntloop" + Environment.NewLine;
            str = str + "basprntCR push y" + Environment.NewLine;
            str = str + " push a" + Environment.NewLine;
            str = str + " lda #0" + Environment.NewLine;
            str = str + " ldy #curx" + Environment.NewLine;
            str = str + " sta ,y" + Environment.NewLine;
            str = str + " ldy #cury" + Environment.NewLine;
            str = str + " lda ,y" + Environment.NewLine;
            str = str + " adda #1" + Environment.NewLine;
            str = str + " cmpa #25" + Environment.NewLine;
            str = str + " jeq basprntcrscr" + Environment.NewLine;
            str = str + " sta ,y" + Environment.NewLine;
            str = str + "basprntcr2 pop a" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + " ldx #$efa1" + Environment.NewLine;
            str = str + " call getcurpos2" + Environment.NewLine;
            str = str + " suby #$e000" + Environment.NewLine;
            str = str + " push d" + Environment.NewLine;
            str = str + " tfr y,d" + Environment.NewLine;
            str = str + " stb ,x+" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " pop d" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;
            str = str + "basprntcrscr scrup" + Environment.NewLine;
            str = str + " lda #24" + Environment.NewLine;
            str = str + " sta ,y" + Environment.NewLine;
            str = str + " jmp basprntcr2" + Environment.NewLine;

            str = str + "prntchar push y" + Environment.NewLine;
            str = str + " call getcurpos" + Environment.NewLine;
            str = str + " push y" + Environment.NewLine;
            str = str + " sta ,y+" + Environment.NewLine;
            str = str + " push a" + Environment.NewLine;
            str = str + " push y" + Environment.NewLine;
            str = str + " ldy #colorattr" + Environment.NewLine;
            str = str + " lda ,y" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " sta ,y" + Environment.NewLine;
            str = str + " pop a" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " push a" + Environment.NewLine;
            str = str + " ldy #curx" + Environment.NewLine;
            str = str + " lda ,y" + Environment.NewLine;
            str = str + " adda #1" + Environment.NewLine;
            str = str + " sta ,y" + Environment.NewLine;
            str = str + " cmpa #80" + Environment.NewLine;
            str = str + " jlt getcurposbr1" + Environment.NewLine;
            str = str + " lda #0" + Environment.NewLine;
            str = str + " sta ,y" + Environment.NewLine;
            str = str + " ldy #cury" + Environment.NewLine;
            str = str + " lda ,y" + Environment.NewLine;
            str = str + " adda #1" + Environment.NewLine;
            str = str + " cmpa #25" + Environment.NewLine;
            str = str + " jeq prntcharj2" + Environment.NewLine;
            str = str + " sta ,y" + Environment.NewLine;
            str = str + "getcurposbr1 pop a" + Environment.NewLine;
            str = str + " call updatecursorpos" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;
            str = str + "prntcharj2 scrup" + Environment.NewLine;
            str = str + " lda #24" + Environment.NewLine;
            str = str + " sta ,y" + Environment.NewLine;
            str = str + " jmp getcurposbr1" + Environment.NewLine;

            str = str + "getcurpos push a" + Environment.NewLine;
            str = str + " push b" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + " ldx #cury" + Environment.NewLine;
            str = str + " ldb ,x" + Environment.NewLine;
            str = str + " lda #0" + Environment.NewLine;
            str = str + " ldy #160" + Environment.NewLine;
            str = str + " mul16 y,b" + Environment.NewLine;
            str = str + " lda #2" + Environment.NewLine;
            str = str + " ldx #curx" + Environment.NewLine;
            str = str + " ldb ,x" + Environment.NewLine;
            str = str + " mul8 b,a" + Environment.NewLine;
            str = str + " lda #0" + Environment.NewLine;
            str = str + " addy d" + Environment.NewLine;
            str = str + " addy #$e000" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " pop b" + Environment.NewLine;
            str = str + " pop a" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            return str;
        }

        private static string GetInitFunction()
        {
            string str = "";
            str = str + "TheBasicInit ldx #curX" + Environment.NewLine;
            str = str + " lda #0" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " ldx #curY" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " ldx #bgcolor" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " ldx #fgcolor" + Environment.NewLine;
            str = str + " lda #7" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " ldx #colorattr" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " ldx #$efa0" + Environment.NewLine;
            str = str + " lda #1" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;
            str = str + "tmpstrcopy push a" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + " push y" + Environment.NewLine;
            str = str + "tmpstrcopylop lda ,x+" + Environment.NewLine;
            str = str + " cmpa #0" + Environment.NewLine;
            str = str + " jeq tmpstrcopyfin" + Environment.NewLine;
            str = str + " sta ,y+" + Environment.NewLine;
            str = str + " jmp tmpstrcopylop" + Environment.NewLine;
            str = str + "tmpstrcopyfin sta ,y" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " pop a" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;
            str = str + "bufcomp push a" + Environment.NewLine;
            str = str + " push y" + Environment.NewLine;
            str = str + "bufcomploop lda ,x+" + Environment.NewLine;
            str = str + " ldb ,y+" + Environment.NewLine;
            str = str + " cmpa b" + Environment.NewLine;
            str = str + " jne bufcompexit1" + Environment.NewLine;
            str = str + " cmpa #0" + Environment.NewLine;
            str = str + " jeq bufcompdone" + Environment.NewLine;
            str = str + " jmp bufcomploop" + Environment.NewLine;
            str = str + "bufcompexit1 ldx #0" + Environment.NewLine;
            str = str + " jmp bufcompexit2" + Environment.NewLine;
            str = str + "bufcompdone ldx #1" + Environment.NewLine;
            str = str + "bufcompexit2 pop y" + Environment.NewLine;
            str = str + " pop a" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;
            
            str = str + "; This function copys a string for x register to y register" + Environment.NewLine;
            str = str + "; X=source    Y=destination" + Environment.NewLine;
            str = str + "copystring push a" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + " copystringloop1 lda ,x+" + Environment.NewLine;
            str = str + " cmpa #0" + Environment.NewLine;
            str = str + " jeq copystringexit1" + Environment.NewLine;
            str = str + " sta ,y+" + Environment.NewLine;
            str = str + " jmp copystringloop1" + Environment.NewLine;
            str = str + "copystringexit1 sta ,y" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " pop a" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;


            str = str + "; This function adjusts a numeric array index" + Environment.NewLine;
            str = str + "; X = Starting address of array" + Environment.NewLine;
            str = str + "; A = index of array (first index = 1)" + Environment.NewLine;
            str = str + "; Returns X = Adjusted address" + Environment.NewLine;
            str = str + "ArrayIndexNumeric push y" + Environment.NewLine;
            str = str + " push d" + Environment.NewLine;
            str = str + " tfr a,b" + Environment.NewLine;
            str = str + " lda #0" + Environment.NewLine;
            str = str + " subd #1" + Environment.NewLine;
            str = str + " ldy #2" + Environment.NewLine;
            str = str + " mul16 y,d" + Environment.NewLine;
            str = str + " addx y" + Environment.NewLine;
            str = str + " pop d" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            str = str + "ArrayIndexString push y" + Environment.NewLine;
            str = str + " push d" + Environment.NewLine;
            str = str + " tfr a,b" + Environment.NewLine;
            str = str + " lda #0" + Environment.NewLine;
            str = str + " subd #1" + Environment.NewLine;
            str = str + " ldy #250" + Environment.NewLine;
            str = str + " mul16 d,y" + Environment.NewLine;
            str = str + " cmpd #0" + Environment.NewLine;
            str = str + " addx d" + Environment.NewLine;
            str = str + "aisExit pop d" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            str = str + "getcurpos2 push a" + Environment.NewLine;
            str = str + " push b" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + " ldx #cury" + Environment.NewLine;
            str = str + " ldb ,x" + Environment.NewLine;
            str = str + " lda #0" + Environment.NewLine;
            str = str + " ldy #80" + Environment.NewLine;
            str = str + " mul16 y,b" + Environment.NewLine;
            str = str + " lda #2" + Environment.NewLine;
            str = str + " ldx #curx" + Environment.NewLine;
            str = str + " ldb ,x" + Environment.NewLine;
            str = str + " lda #0" + Environment.NewLine;
            str = str + " addy d" + Environment.NewLine;
            str = str + " addy #$e000" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " pop b" + Environment.NewLine;
            str = str + " pop a" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            str = str + "updatecursorpos push y" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + " ldx #$efa1" + Environment.NewLine;
            str = str + " call getcurpos2" + Environment.NewLine;
            str = str + " suby #$e000" + Environment.NewLine;
            str = str + " push d" + Environment.NewLine;
            str = str + " tfr y,d" + Environment.NewLine;
            str = str + " stb ,x+" + Environment.NewLine;
            str = str + " sta ,x" + Environment.NewLine;
            str = str + " pop d" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " pop y" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            str = str + "allocatememory push d" + Environment.NewLine;
            str = str + " push y" + Environment.NewLine;
            str = str + " ldx #" + Constants.MemoryManagerMemoryStart + Environment.NewLine;
            str = str + "allocloop2 ldy #" + Constants.MemoryManagerDataTableStart + Environment.NewLine;
            str = str + "allocloop1 ldb ,y+" + Environment.NewLine;
            str = str + " lda ,y+" + Environment.NewLine;
            str = str + " cmpd #0" + Environment.NewLine;
            str = str + " jeq allocfound" + Environment.NewLine;
            str = str + " cmpx d" + Environment.NewLine;
            str = str + " jeq allocycle" + Environment.NewLine;
            str = str + " jmp allocloop1" + Environment.NewLine;
            str = str + " allocycle addx #256" + Environment.NewLine;
            str = str + " jmp allocloop2" + Environment.NewLine;
            str = str + "allocfound suby #2" + Environment.NewLine;
            str = str + " tfr x,d" + Environment.NewLine;
            str = str + " stb ,y+" + Environment.NewLine;
            str = str + " sta ,y" + Environment.NewLine;
            str = str + "allocexit pop y" + Environment.NewLine;
            str = str + " pop d" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;

            str = str + "freememory push d" + Environment.NewLine;
            str = str + " push x" + Environment.NewLine;
            str = str + " push y" + Environment.NewLine;
            str = str + " ldy #" + Constants.MemoryManagerDataTableStart + Environment.NewLine;
            str = str + "fmloop1 ldb ,y+" + Environment.NewLine;
            str = str + " lda ,y+" + Environment.NewLine;
            str = str + " cmpd #0" + Environment.NewLine;
            str = str + " jeq fmleave" + Environment.NewLine;
            str = str + " cmpx d" + Environment.NewLine;
            str = str + " jeq memfound" + Environment.NewLine;
            str = str + " jmp fmloop1" + Environment.NewLine;
            str = str + "memfound tfr y,x " + Environment.NewLine;
            str = str + " suby #2" + Environment.NewLine;
            str = str + "fmloop2 ldb ,x+" + Environment.NewLine;
            str = str + " lda ,x+" + Environment.NewLine;
            str = str + " stb ,y+" + Environment.NewLine;
            str = str + " stb ,y+" + Environment.NewLine;
            str = str + " cmpd #0" + Environment.NewLine;
            str = str + " jeq fmleave" + Environment.NewLine;
            str = str + " jmp fmloop2" + Environment.NewLine;
            str = str + "fmleave pop y" + Environment.NewLine;
            str = str + " pop x" + Environment.NewLine;
            str = str + " pop d" + Environment.NewLine;
            str = str + " ret" + Environment.NewLine;
            return str;
        }
    }
}
