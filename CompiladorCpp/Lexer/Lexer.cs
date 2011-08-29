using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace Lexical
{
    public enum TokenType
    {
        ID,       

        STRING, BOOL, CHAR, FLOAT, INT, STRUCT, ENUM,
        CONST, VOID, IF, ELSE, DO, WHILE, FOR, 
        BREAK, CONTINUE, RETURN, 
        TRUE, FALSE,

        ADDITION, SUBSTRACTION, MULTIPLICATION, DIVISION, REMAINDER,
        AND, OR, NOT,
        EQUAL, NOTEQUAL, GREATER, LESS, GREATER_EQUAL, LESS_EQUAL,
        DECREMENT, INCREMENT,
        ASSIGNMENT, ADDITION_ASSIGNMENT, SUBSTRACTION_ASSIGNMENT, MULTIPLICATION_ASSIGNMENT, DIVISION_ASSIGNMENT,

        INTEGER_LITERAL, REAL_LITERAL, STRING_LITERAL, CHARACTER_LITERAL,

        LEFT_PARENTHESIS, RIGHT_PARENTHESIS,
        LEFT_CURLY_BRACKET, RIGHT_CURLY_BRACKET,
        LEFT_SQUARE_BRACKET, RIGHT_SQUARE_BRACKET,
        SEMICOLON, COLON, AMPERSAND,
        COMMA, DOT, ESCAPE_SYMBOL,

        EOF
    }

    public class Token
    {
        #region variables
        string lexema;
        TokenType tipo;
        #endregion

        #region sets y gets
        public string Lexema
        {
            get { return lexema; }
            set { lexema = value; }
        }
        
        public TokenType Tipo
        {
            get { return tipo; }
            set { tipo = value; }
        }
        #endregion

        #region constructor
        public Token(string lexema, TokenType tipo)
        {
            this.lexema = lexema;
            this.tipo = tipo;
        }
        #endregion        
    }

    public class Lexer
    {
        #region variables

        string bufferInput;
        int caracterActual = 0;     
        SortedDictionary<string, TokenType> tblOperadores = new SortedDictionary<string, TokenType>();
        SortedDictionary<string, TokenType> tblPblReservadas = new SortedDictionary<string, TokenType>();
        public int column = 1, line = 1;
        
        #endregion

        #region constructores

        public Lexer(string path)
        {            
            tblOperadores.Add("+", TokenType.ADDITION);
            tblOperadores.Add("-", TokenType.SUBSTRACTION);
            tblOperadores.Add("*", TokenType.MULTIPLICATION);
            tblOperadores.Add("/", TokenType.DIVISION);
            tblOperadores.Add("%", TokenType.REMAINDER);


            tblOperadores.Add("&&", TokenType.AND);
            tblOperadores.Add("||", TokenType.OR);
            tblOperadores.Add("!", TokenType.NOT);            

           
            tblOperadores.Add("==", TokenType.EQUAL);
            tblOperadores.Add("!=", TokenType.NOTEQUAL);
            tblOperadores.Add(">", TokenType.GREATER);
            tblOperadores.Add("<", TokenType.LESS);
            tblOperadores.Add(">=", TokenType.GREATER_EQUAL);
            tblOperadores.Add("<=", TokenType.LESS_EQUAL);
            

            tblOperadores.Add("--", TokenType.DECREMENT);
            tblOperadores.Add("++", TokenType.INCREMENT);


            tblOperadores.Add("=", TokenType.ASSIGNMENT);
            tblOperadores.Add("+=", TokenType.ADDITION_ASSIGNMENT);
            tblOperadores.Add("-=", TokenType.SUBSTRACTION_ASSIGNMENT);
            tblOperadores.Add("*=", TokenType.MULTIPLICATION_ASSIGNMENT);
            tblOperadores.Add("/=", TokenType.DIVISION_ASSIGNMENT);


            tblOperadores.Add("&", TokenType.AMPERSAND);
            tblOperadores.Add("(", TokenType.LEFT_PARENTHESIS);
            tblOperadores.Add(")", TokenType.RIGHT_PARENTHESIS);
            tblOperadores.Add("{", TokenType.LEFT_CURLY_BRACKET); 
            tblOperadores.Add("}", TokenType.RIGHT_CURLY_BRACKET);
            tblOperadores.Add("[", TokenType.LEFT_SQUARE_BRACKET);
            tblOperadores.Add("]", TokenType.RIGHT_SQUARE_BRACKET); 
            tblOperadores.Add(";", TokenType.SEMICOLON); 
            tblOperadores.Add(":", TokenType.COLON);
            tblOperadores.Add(",", TokenType.COMMA); 
            tblOperadores.Add(".", TokenType.DOT);

            
            tblOperadores.Add("\0", TokenType.EOF);
                     

            tblPblReservadas.Add("string", TokenType.STRING);
            tblPblReservadas.Add("bool", TokenType.BOOL);
            tblPblReservadas.Add("char", TokenType.CHAR);
            tblPblReservadas.Add("float", TokenType.FLOAT);
            tblPblReservadas.Add("int", TokenType.INT);
            tblPblReservadas.Add("struct", TokenType.STRUCT);
            tblPblReservadas.Add("enum", TokenType.ENUM);

            
            tblPblReservadas.Add("const", TokenType.CONST);
            tblPblReservadas.Add("void", TokenType.VOID);
            tblPblReservadas.Add("if", TokenType.IF); 
            tblPblReservadas.Add("else", TokenType.ELSE);
            tblPblReservadas.Add("do", TokenType.DO);
            tblPblReservadas.Add("while", TokenType.WHILE);
            tblPblReservadas.Add("for", TokenType.FOR);
            
            
            tblPblReservadas.Add("break", TokenType.BREAK);
            tblPblReservadas.Add("continue", TokenType.CONTINUE);
            tblPblReservadas.Add("return", TokenType.RETURN);

            
            tblPblReservadas.Add("true", TokenType.TRUE);
            tblPblReservadas.Add("false", TokenType.FALSE);

            
            StreamReader sr = File.OpenText(path);
            bufferInput = sr.ReadToEnd();
            sr.Close();
        }

        #endregion

        #region funciones  

        public TokenType getTokenType(string token)
        {
            if (tblOperadores.ContainsKey(token))
                return tblOperadores[token];
            else
                return tblPblReservadas[token];
        }

        bool esDigito(char c)
        {
            return (c >= '0' && c <= '9');
        }

        bool esLetra(char c)
        {
            return ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'));
        }

        bool esEstorbo(char c)
        {
            return (c == '\t' || c == '\n' || c == ' ' || c == '\r');
        }

        bool esPuntuacion(char c)
        {
            return (
                (c == '(') || 
                (c == ')') || 
                (c == '{') || 
                (c == '}') ||
                (c == '[') ||
                (c == ']') ||
                (c == ';') ||
                (c == ':') ||
                (c == ',') ||
                (c == '.')
                );
        }

        char currentSymbol()
        {
            if (caracterActual < bufferInput.Length)
                return bufferInput[caracterActual];
            else
                return '\0';
        }

        void nextSymbol()
        {
            caracterActual++;
            column++;

            if (currentSymbol() == '\n')
            {
                column = 0;
                line++;
            }
        }

        public Token nextToken()
        {
            int estado = 0;
            string lexema = "";

            while (true)
            {
                char c = currentSymbol();

                switch (estado)
                {
                    case 0:
                        #region caso

                        if (esLetra(c) || c == '_')//letra
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 1;
                        }
                        else if (esDigito(c))//digito
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 2;
                        }
                        else if (c == '\"')//string lit
                        {                         
                            nextSymbol();
                            estado = 6;
                        }
                        else if (c == '\'')//char lit
                        {
                            nextSymbol();
                            estado = 8;
                        }
                        else if (c == '&')
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 11;
                        }
                        else if (c == '|')
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 12;
                        }
                        else if (c == '!')
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 13;
                        }
                        else if (c == '=')
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 13;
                        }
                        else if (c == '<')
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 13;
                        }
                        else if (c == '>')
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 13;
                        }                        
                        else if (esPuntuacion(c))
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 10;
                        }                       
                        else if (c == '%')
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 10;
                        }
                        else if (c == '+')
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 18;
                        }
                        else if (c == '-')
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 19;
                        }
                        else if (c == '*')
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 13;
                        }
                        else if (c == '/')
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 14;
                        }

                        else if (esEstorbo(c))
                        {
                            nextSymbol();
                            estado = 0;
                        }
                        else if (c == '\0')
                        {
                            return new Token(null, TokenType.EOF);
                        }
                        else
                            throw new Exception("Simbolo no reconocido {"+ c +"} en Linea: " + line + " Columna: " + column);                        
                        
                        break;
                        #endregion
                    
                    case 1:
                        #region caso
                        if (esDigito(c) || esLetra(c) || c == '_')
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 1;
                        }
                        else
                        {
                            if (tblPblReservadas.ContainsKey(lexema))
                                return new Token(lexema, tblPblReservadas[lexema]);
                            else 
                                return new Token(lexema, TokenType.ID);
                        }
                        break;
                        #endregion
                    
                    
                    case 2:
                        #region caso
                        if (esDigito(c))
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 2;
                        }
                        else if (c == '.')
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 3;
                        }
                        else
                        {
                            return new Token(lexema, TokenType.INTEGER_LITERAL);
                        }
                        break;
                        #endregion
                    case 3:
                        #region caso
                        if (esDigito(c))
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 4;
                        }
                        else
                            throw new Exception("Se esperaba un numero despues del punto. {" + lexema + "} Linea: " + line + " Columna: " + column);
                        break;
                        #endregion
                    case 4:
                        #region caso
                        if (esDigito(c))
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 4;
                        }
                        else
                            estado = 5;
                        break;
                        #endregion
                    case 5:
                        #region caso
                        return new Token(lexema, TokenType.REAL_LITERAL);
                        #endregion
                    
                    case 6:
                        #region caso
                        if (c == '\"')
                        {
                            nextSymbol();
                            estado = 7;
                        }
                        else if (c == '\0')
                        {
                            throw new Exception("Se esperaba el simbolo \". Linea:" + line + " Columna:" + column);
                        }
                        else
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 6;
                        }                         
                        break;
                        #endregion
                    
                    case 7:
                        #region caso
                        return new Token(lexema, TokenType.STRING_LITERAL);
                        #endregion
                                       
                    case 8:
                        #region caso                    
                        if (c == '\0')
                        {
                            throw new Exception("Se esperaba el simbolo \"\'\". Linea:" + line + " Columna:" + column);
                        }
                        else if (c == '\\')
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 20;
                        }
                        else
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 21;
                        }
                        break;
                        #endregion
                    
                    case 9:
                        #region caso
                        return new Token(lexema, TokenType.CHARACTER_LITERAL);
                        #endregion

                    case 10:
                        #region caso
                        return new Token(lexema, tblOperadores[lexema]);
                        #endregion

                    case 11:// and &&
                        #region caso
                        if (c == '&')
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 10;
                        }
                        else
                        {                            
                            estado = 10;                            
                        }
                        break;
                        #endregion
                    
                    case 12: //or ||
                        #region caso
                        if (c == '|')
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 10;
                        } 
                        else
                            throw new Exception("Se esperaba el simbolo |. Linea:" + line + " Columna:" + column);
                        break;
                        #endregion

                    case 13:
                        #region caso
                        if (c == '=')
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 10;
                        }
                        else
                        {
                            estado = 10;
                        }
                        break;
                        #endregion

                    case 14:// division operator / and comments
                        #region caso
                        if (c == '=')
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 10;
                        }
                        else if (c == '*')
                        {
                            lexema = "";
                            nextSymbol();
                            estado = 15;
                        }
                        else if (c == '/')
                        {
                            lexema = "";
                            nextSymbol();
                            estado = 17;
                        }
                        else
                        {
                            //nextSymbol();
                            estado = 10;
                        }
                        break;
                        #endregion
   
                    case 15:// /*..*/ comments
                        #region caso
                        if (c == '*')
                        {
                            nextSymbol();
                            estado = 16;
                        }
                        else
                        {
                            nextSymbol();
                            estado = 15;
                        }
                        break;
                        #endregion

                    case 16:// "/*..*/" comments
                        #region caso
                        if (c == '/')
                        {
                            nextSymbol();
                            estado = 0;
                        }
                        else if (c == '*')
                        {
                            nextSymbol();
                            estado = 16;
                        }
                        else if (c == '\0')
                        {
                            estado = 0;
                            throw new Exception("Error se llego al EOF sin cerrar el bloque de comentarios");
                        }
                        else
                        {
                            nextSymbol();
                            estado = 15;
                        }
                        break;
                        #endregion

                    case 17:// "//" comments
                        #region caso
                        if (c == '\n')
                        {
                            nextSymbol();
                            estado = 0;
                        }
                        else if (c == '\0')
                        {
                            estado = 0;
                        }
                        else //otro
                        {
                            nextSymbol();
                            estado = 17;
                        }
                        break;
                        #endregion                    
                    
                    case 18:
                        #region caso
                        if (c == '+')
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 10;
                        }
                        else
                            estado = 13;
                        break;
                        #endregion

                    case 19:
                        #region caso
                        if (c == '-')
                        {
                            lexema += c;
                            nextSymbol();
                            estado = 10;
                        }
                        else
                            estado = 13;
                        break;
                        #endregion

                    case 20:
                        #region caso
                        lexema += c;
                        nextSymbol();
                        estado = 21;
                        break;
                        #endregion

                    case 21:
                        #region caso
                        if (c == '\'')
                        {
                            nextSymbol();
                            estado = 9;
                        }
                        else
                            throw new Exception("Se esperaba el simbolo \"\'\". Linea:" + line + " Columna:" + column);     
                        break;
                        #endregion
                }
            }
        }

        #endregion
    }
}
