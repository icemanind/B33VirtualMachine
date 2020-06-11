using System;
using System.Collections.Generic;

namespace B33VirtualMachineBasicCompiler
{
    public class NativeExpressionParser
    {
        private Token _token;
        private ushort _value;
        private Stack<ushort> _stack;

        private void SkipWhiteSpace(TokenParser parser)
        {
            if (parser == null || parser.Peek() == null || parser.Peek().TokenPeek == null)
                return;
            Token token = parser.Peek().TokenPeek;

            while (token.TokenName == TokenParser.Tokens.WHITESPACE)
            {
                parser.GetToken();
                if (parser.Peek() == null || parser.Peek().TokenPeek == null)
                    return;
                token = parser.Peek().TokenPeek;
            }
        }

        private void Factor(TokenParser parser)
        {
            if (_token == null)
                return;

            if (_token.TokenName == TokenParser.Tokens.LPAREN)
            {
                SkipWhiteSpace(parser);
                _token = parser.GetToken();
                SkipWhiteSpace(parser);
                Expression(parser);
                if (_token == null)
                    return;
                SkipWhiteSpace(parser);
                _token = parser.GetToken();
                SkipWhiteSpace(parser);
            }
            else
            {
                _value = _token.TokenName == TokenParser.Tokens.HEXNUMBER
                    ? Convert.ToUInt16(_token.TokenValue.Replace("&h", "").Replace("&H", ""), 16)
                    : ushort.Parse(_token.TokenValue);
                SkipWhiteSpace(parser);
                _token = parser.GetToken();
                SkipWhiteSpace(parser);
            }
        }

        private void Term(TokenParser parser)
        {
            if (_token == null)
                return;
            Factor(parser);
            if (_token == null)
                return;
            while (_token.TokenName == TokenParser.Tokens.ASTERISK || _token.TokenName == TokenParser.Tokens.SLASH ||
                   _token.TokenName == TokenParser.Tokens.PERCENT)
            {
                _stack.Push(_value);

                if (_token.TokenName == TokenParser.Tokens.ASTERISK)
                {
                    SkipWhiteSpace(parser);
                    _token = parser.GetToken();
                    SkipWhiteSpace(parser);
                    Factor(parser);
                    _value = (ushort)(_value * _stack.Pop());
                }
                else if (_token.TokenName == TokenParser.Tokens.SLASH)
                {
                    SkipWhiteSpace(parser);
                    _token = parser.GetToken();
                    SkipWhiteSpace(parser);
                    Factor(parser);
                    _value = (ushort)(_stack.Pop() / _value);
                }
                else if (_token.TokenName == TokenParser.Tokens.PERCENT)
                {
                    SkipWhiteSpace(parser);
                    _token = parser.GetToken();
                    SkipWhiteSpace(parser);
                    Factor(parser);
                    _value = (ushort)(_value % _stack.Pop());
                }
                if (_token == null)
                    return;
            }
        }

        private void Expression(TokenParser parser)
        {
            if (_token == null)
                return;

            if (_token.TokenName == TokenParser.Tokens.PLUS || _token.TokenName == TokenParser.Tokens.MINUS)
            {
                _value = 0;
            }
            else
            {
                Term(parser);
            }
            if (_token == null)
                return;
            while (_token.TokenName == TokenParser.Tokens.PLUS || _token.TokenName == TokenParser.Tokens.MINUS)
            {
                _stack.Push(_value);

                if (_token.TokenName == TokenParser.Tokens.PLUS)
                {
                    SkipWhiteSpace(parser);
                    _token = parser.GetToken();
                    SkipWhiteSpace(parser);
                    Term(parser);
                    _value = (ushort)(_value + _stack.Pop());
                }
                else if (_token.TokenName == TokenParser.Tokens.MINUS)
                {
                    SkipWhiteSpace(parser);
                    _token = parser.GetToken();
                    SkipWhiteSpace(parser);
                    Term(parser);
                    _value = (ushort) ~((ushort)(_value - _stack.Pop()));
                    _value = (ushort) (_value + 1);
                }
                if (_token == null)
                    return;
            }
        }

        public ushort Compile(string expression)
        {
            _value = 0;
            _stack = new Stack<ushort>();
            TokenParser parser = new TokenParser { InputString = expression };
            SkipWhiteSpace(parser);

            _token = parser.GetToken();
            SkipWhiteSpace(parser);
            Expression(parser);

            return _value;
        }
    }
}
