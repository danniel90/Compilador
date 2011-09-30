using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SyntaxTree;

namespace Environment
{
    public class EnvTypes
    {        
        public Dictionary<string, Tipo> tablaSimbolos;
        public static Stack<EnvTypes> pila;
        protected EnvTypes previo;
        
        public EnvTypes(EnvTypes entornoTipos)
        {
            tablaSimbolos = new Dictionary<string, Tipo>();
            previo = entornoTipos;
            pila = new Stack<EnvTypes>();
        }

        public void put(string key, Tipo tipo)
        {
            if (!tablaSimbolos.ContainsKey(key))
                tablaSimbolos.Add(key, tipo);
            else
                throw new Exception("La variable \"" + key + "\" ya fue declarada. EnvT.");
        }

        public Tipo get(string key)
        {
            for (EnvTypes e = this; e != null; e = e.previo)
            {
                if (e.tablaSimbolos.ContainsKey(key))
                    return e.tablaSimbolos[key];
            }

            throw new Exception("La variable " + key + " no existe. EnvT.");
        }
    }

    public class EnvValues
    {
        public static int cont = 0;
        private int id;
        public Dictionary<string, Valor> tablaValores;
        public static Stack<EnvValues> pila;
        public EnvValues previo;

        public EnvValues(EnvValues entorno)
        {            
            tablaValores = new Dictionary<string, Valor>();
            previo = entorno;
            pila = new Stack<EnvValues>();
            id = cont++;
        }

        public void put(string key, Valor valor)
        {
            /*for (EnvValues e = this; e != null; e = e.previo)
            {
                if (e.tablaValores.ContainsKey(key))
                {
                    e.tablaValores[key] = valor;
                    return;
                }
            }*/
            if (tablaValores.ContainsKey(key))
                throw new Exception("Ya existe la variable \"" + key + "\". EnvV.");
            tablaValores.Add(key, valor);
        }

        public void set(string key, Valor valor)
        {
            for (EnvValues e = this; e != null; e = e.previo)
            {
                if (e.tablaValores.ContainsKey(key))
                {
                    e.tablaValores.Remove(key);
                    e.tablaValores.Add(key, valor);
                    return;
                }
            }

            throw new Exception("No existe variable " + key + ". EnvV.");
        }

        public Valor get(string key)
        {
            for (EnvValues e = this; e != null; e = e.previo)
            {
                if (e.tablaValores.ContainsKey(key))
                    return e.tablaValores[key];
            }

            throw new Exception("La variable " + key + " no existe. EnvV.");
        }
    }
}
