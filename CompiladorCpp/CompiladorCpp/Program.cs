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
///*
                Parser parser = new Parser("arreglo.txt");
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
