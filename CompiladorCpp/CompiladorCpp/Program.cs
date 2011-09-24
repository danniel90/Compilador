using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lexical;
using Syntax;

namespace CompiladorCpp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                /*Lexer lex = new Lexer("tokens.txt");//("test.txt");
                Token t = lex.nextToken();

                while (t.Tipo != TokenType.EOF)
                {
                    Console.WriteLine("{0} : {1} \t|| Line = {2} Column = {3}", t.Lexema, t.Tipo, lex.line, lex.column);
                    t = lex.nextToken();
                }*/ 
///*
                Parser parser = new Parser("intrepretation.txt");
                parser.compile();
                
                Console.WriteLine("\n\nFIN");
//*/
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n" + ex.ToString());
                Console.ReadKey();
            }
        }
    }
}
