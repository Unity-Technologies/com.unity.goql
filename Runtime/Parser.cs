using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.GoQL
{

    public class Parser
    {
        public static List<object> Parse(string code)
        {
            var tokenizer = new Tokenizer();
            var tokens = tokenizer.Tokenize(code);
            var instructions = new List<object>();
            while (tokens.Count > 0)
                _Parse(tokens, instructions);
            return instructions;
        }

        static void _Parse(List<Token> tokens, List<object> instructions)
        {
            if (tokens.Count == 0)
            {
                return;
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
                    break;
                case TokenType.OpenSquare:
                    // instructions.Add(GoQLCode.BeginIndexMatch);
                    _ParseDiscriminators(GoQLCode.FilterIndex, tokens, instructions, closeToken: TokenType.CloseSquare);
                    break;
                case TokenType.OpenAngle:
                    // instructions.Add(GoQLCode.BeginComponentMatch);
                    _ParseDiscriminators(GoQLCode.FilterComponent, tokens, instructions, closeToken: TokenType.CloseAngle);
                    break;
                case TokenType.Slash:
                    instructions.Add(GoQLCode.EnterChildren);
                    break;
            }
        }

        static void _ParseDiscriminators(GoQLCode discriminatorType, List<Token> tokens, List<object> instructions, TokenType closeToken)
        {
            var elements = new List<object>();
            while (true)
            {
                if (tokens.Count == 0)
                {
                    return;
                    //throw new System.Exception("Syntax Error: unexpected EOF");
                }

                switch (tokens[0].type)
                {
                    //end of discriminator
                    case TokenType.CloseAngle:
                    case TokenType.CloseSquare:
                        if (closeToken != tokens[0].type)
                            Debug.Log("Error closing type");
                        tokens.RemoveAt(0);
                        if (elements.Count > 0)
                        {
                            instructions.AddRange(elements);
                            instructions.Add(elements.Count);
                            instructions.Add(discriminatorType);
                            // instructions.Add(GoQLCode.Discriminate);
                        }
                        // instructions.Add(GoQLCode.FinalizeDiscrimination);
                        return;
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