using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SyntaxTree;

namespace Environment
{
    public class Env
    {        
        public Dictionary<string, Tipo> tablaSimbolos;
        public static Stack<Env> pila;
        protected Env previo;
        
        public Env(Env entorno)
        {
            tablaSimbolos = new Dictionary<string, Tipo>();
            previo = entorno;
            pila = new Stack<Env>();
        }

        public void put(string key, Tipo tipo)
        {
            if (!tablaSimbolos.ContainsKey(key))
                tablaSimbolos.Add(key, tipo);
            else
                throw new Exception("La variable \"" + key + "\" ya fue declarada.");
        }

        public Tipo get(string key)
        {
            for (Env e = this; e != null; e = e.previo)
            {
                if (e.tablaSimbolos.ContainsKey(key))
                    return e.tablaSimbolos[key];
            }

            throw new Exception("La variable " + key + " no existe.");
        }
    }
}
