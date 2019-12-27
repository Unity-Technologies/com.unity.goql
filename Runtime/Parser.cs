using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.GoQL
{

    public enum ParseResult
    {
        UnexpectedEndOfInput,
        Empty,
        OK,
        ClosingTokenMismatch,
        InvalidNumberFormat
    }

    public class Parser
    {
        public static void Parse(string code, List<object> instructions, out ParseResult parseResult)
        {
            var tokenizer = new Tokenizer();
            var tokens = tokenizer.Tokenize(code);
            parseResult = ParseResult.Empty;
            while (tokens.Count > 0) {
                parseResult = _Parse(tokens, instructions);
                if(parseResult != ParseResult.OK) {
                    return;
                }
            }
        }

        public static List<object> Parse(string code, out ParseResult parseResult)
        {
            var tokenizer = new Tokenizer();
            var tokens = tokenizer.Tokenize(code);
            var instructions = new List<object>();
            parseResult = ParseResult.Empty;
            while (tokens.Count > 0) {
                parseResult = _Parse(tokens, instructions);
                if(parseResult != ParseResult.OK) {
                    return instructions;
                }
            }
            return instructions;
        }

        static ParseResult _Parse(List<Token> tokens, List<object> instructions)
        {
            if (tokens.Count == 0)
            {
                return ParseResult.UnexpectedEndOfInput;
                //throw new System.Exception("Syntax Error: unexpected EOF");
            }

            var token = tokens[0];
            tokens.RemoveAt(0);
            switch (token.type)
            {
                //a string in this context is a name discrimator instruction
                case TokenType.String:
                    instructions.Add((string)(token.value));
                    instructions.Add(GoQLCode.FilterName);
                    // instructions.Add(GoQLCode.Discriminate);
                    return ParseResult.OK;
                case TokenType.OpenSquare:
                    // instructions.Add(GoQLCode.BeginIndexMatch);
                    return _ParseDiscriminators(GoQLCode.FilterIndex, tokens, instructions, closeToken: TokenType.CloseSquare);
                case TokenType.OpenAngle:
                    // instructions.Add(GoQLCode.BeginComponentMatch);
                    return _ParseDiscriminators(GoQLCode.FilterComponent, tokens, instructions, closeToken: TokenType.CloseAngle);
                case TokenType.Slash:
                    instructions.Add(GoQLCode.EnterChildren);
                    return ParseResult.OK;
            }
            return ParseResult.OK;
        }

        static ParseResult _ParseDiscriminators(GoQLCode discriminatorType, List<Token> tokens, List<object> instructions, TokenType closeToken)
        {
            var elements = new List<object>();
            while (true)
            {
                if (tokens.Count == 0)
                {
                    return ParseResult.UnexpectedEndOfInput;
                    //throw new System.Exception("Syntax Error: unexpected EOF");
                }

                switch (tokens[0].type)
                {
                    //end of discriminator
                    case TokenType.CloseAngle:
                    case TokenType.CloseSquare:
                        if (closeToken != tokens[0].type)
                            return ParseResult.ClosingTokenMismatch;
                        tokens.RemoveAt(0);
                        if (elements.Count > 0)
                        {
                            instructions.AddRange(elements);
                            instructions.Add(elements.Count);
                            instructions.Add(discriminatorType);
                            // instructions.Add(GoQLCode.Discriminate);
                        }
                        // instructions.Add(GoQLCode.FinalizeDiscrimination);
                        return ParseResult.OK;
                    case TokenType.Comma:
                        tokens.RemoveAt(0);
                        break;
                    case TokenType.Colon:
                        //Make a range object
                        int start, end;
                        tokens.RemoveAt(0);
                        start = 0;
                        if (elements.Count > 0)
                        {
                            var last = elements.Last();
                            //start range was specified, so grab it then remove from discriminator instructions.
                            if (last is int)
                            {
                                start = (int)last;
                                elements.RemoveAt(elements.Count - 1);
                            }

                        }
                        //if next token exists and is a number, it is the end range value.
                        if (tokens.Count > 0 && tokens[0].type == TokenType.Number)
                        {
                            if (!int.TryParse(tokens[0].value, out end))
                                end = -1;
                            tokens.RemoveAt(0);
                        }
                        else
                        //no end range specified, default to end index.
                        {
                            end = -1;
                        }
                        elements.Add(new Range(start, end));
                        break;
                    case TokenType.String:
                        elements.Add((string)tokens[0].value);
                        tokens.RemoveAt(0);
                        break;
                    case TokenType.Number:
                        if (int.TryParse(tokens[0].value, out int n))
                            elements.Add(n);
                        else
                            return ParseResult.InvalidNumberFormat;
                        tokens.RemoveAt(0);
                        break;
                    default:
                        // ignore everything else, it is a syntax error.
                        break;
                }
            }
        }

    }

}