using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lexical;
using SyntaxTree;

namespace Syntax
{
    public class Parser
    {
        #region variables
        public Token currentToken;
        public Lexer lex;        
        #endregion

        #region constructores
        public Parser(string path)
        {
            lex = new Lexer(path);            
        }
        #endregion

        #region funciones
        public void compile()
        {
            currentToken = lex.nextToken();
            program();
        }

        void program()
        {
            global_sentence(); match("\0");
        }

        void global_sentence()
        {
            switch(currentToken.Tipo)
            {
                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:
                case TokenType.VOID:
                case TokenType.CONST:
                case TokenType.STRUCT:
                case TokenType.ENUM:
                    sentence(); global_sentenceP();
                    break;
                default:
                    throw new Exception("Global sentence exception! wtf!!! linea: " + lex.line + " columna: " + lex.column + " currentToken -> " + currentToken.Lexema);
            }
        }

        void global_sentenceP()
        {
            switch(currentToken.Tipo)
            {
                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:
                case TokenType.VOID:
                case TokenType.CONST:
                case TokenType.STRUCT:
                case TokenType.ENUM:            
                    sentence(); global_sentenceP();
                    break;
            }
            //null
        }

        void sentence()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.CONST:
                    constant_declaration(); variable_declarationP();
                    break;
                case TokenType.ENUM:
                    enum_declaration();  match(";");
                    break;
                case TokenType.STRUCT:
                    struct_declaration(); match(";");
                    break;
                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:
                    variable_declaration();  variable_declarationP();
                    break;
                case TokenType.VOID:
                    void_declaration();  function_declaration();
                    break;
            }
        }

        #region variable declaration

        void variable_declaration()
        {
            variable_type(); direct_variable_declarator();
        }

        void variable_type()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:
                    currentToken = lex.nextToken();
                    break;
                default:
                    throw new Exception("Error en la declaracion de tipo linea: " + lex.line + " columna: " + lex.column + " currenttoken = " + currentToken.Lexema);
            }
        }

        void direct_variable_declarator()
        {
            variable_name(); variable_array();
        }

        void variable_name()
        {
            match(TokenType.ID);
        }

        void variable_array()
        {
            if (peek("["))
            {
                match("["); OR_expr(); match("]"); variable_array();//arreglos multidimensionales
            }
            //null
        }

        #endregion

        #region variable_declarationP

        void variable_declarationP()
        {
            if (peek("("))
            {
                function_declaration();
            }
            else
            {
                variable_declarator(); match(";");
            }
        }

        void variable_declarator()
        {
            variable_initializer(); variable_declarators();
        }

        void variable_declarators()
        {
            if (peek(","))
            {
                match(","); direct_variable_declarator(); variable_initializer(); variable_declarators();
            }
            //null
        }

        #endregion

        #region variable_initializer

        void variable_initializer()
        {
            if (peek("="))
            {
                initializers();
            }
            //null
        }

        void initializers()
        {
            match("="); initializer();
        }

        void initializer()
        {
            if (peek("{"))
            {
                match("{"); initializer_list(); match("}");
            }            
            else
            {
                assign_expr();
            }
        }

        void initializer_list()
        {
            initializer(); initializer_listP();
        }

        void initializer_listP()
        {
            if (peek(","))
            {
                match(","); initializer(); initializer_listP();
            }
            //null
        }

        #endregion                

        #region function_declaration -> ()

        void function_declaration()
        {
            match("("); parameter_type_list(); match(")"); function_definition();
        }

        #region parameters_type_list

        void parameter_type_list()
        {
            parameter_declaration(); parameter_type_listP();
        }

        void parameter_type_listP()
        {
            if (peek(","))
            {
                match(","); parameter_declaration(); parameter_type_listP();
            }
            //null
        }

        void parameter_declaration()
        {
            declaration_specifer(); parameter_type();
        }

        void declaration_specifer()
        {
            if (peek("const"))
            {
                match("const");
            }
            //null
        }

        void parameter_type()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:
                    variable_type(); reference(); parameter_name();
                    break;
            }
            //null
        }

        void reference()
        {
            if (peek("&"))
            {
                match("&");
            }
            //null
        } 

        void parameter_name()
        {
            if (peek(TokenType.ID))
            {
                direct_variable_declarator();
            }
            //null
        }        

        #endregion

        void function_definition()
        {
            compound_statement();
        }

        void compound_statement()
        {
            match("{"); statement_list(); match("}");
        }

        void statement_list()
        {
            statement(); statement_listP(); 
        }

        void statement_listP()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:
                case TokenType.VOID:
                case TokenType.STRUCT:
                case TokenType.ID:

                case TokenType.DECREMENT:
                case TokenType.INCREMENT:

                case TokenType.IF:
                case TokenType.DO:
                case TokenType.WHILE:
                case TokenType.FOR:
                case TokenType.RETURN:
                case TokenType.BREAK:
                case TokenType.CONTINUE:
                    statement(); statement_listP();
                    break;
            }
            //null
        }

        void statement()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:
                case TokenType.VOID:
                case TokenType.STRUCT:
                    declaration_statement(); match(";");
                    break;
                case TokenType.DECREMENT:
                case TokenType.INCREMENT:                    
                    expression_statement(); match(";");
                    break;
                case TokenType.ID:
                    expression_statement(); match(";");
                    break;
                case TokenType.IF:
                    if_statement();
                    break;

                case TokenType.DO:
                    do_while();
                    break;

                case TokenType.WHILE:
                    while_statement();
                    break;

                case TokenType.FOR:
                    for_statement();
                    break;

                case TokenType.BREAK:
                    break_statement(); match(";");
                    break;

                case TokenType.CONTINUE:
                    continue_statement(); match(";");
                    break;

                case TokenType.RETURN:
                    return_statement(); match(";");
                    break;
            }
            //null
        }

        #region declaration_statement
        void declaration_statement()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:
                    variable_declaration(); variable_declarator();
                    break;                
                case TokenType.STRUCT:
                    struct_declarator(); match(TokenType.ID);
                    break;
            }
        }                

        #endregion

        #region prefix_postfix_statements

        void prefix_operator()
        {
            if (peek("++"))
            {
                match("++");
            }
            else if (peek("--"))
            {
                match("--");
            }
            else if (peek("!"))
            {
                match("!");
            }
            else
                throw new Exception("Error en el prefix operator linea: " + lex.line + " columna: " + lex.column + " currenttoken = " + currentToken.Lexema + " -prefixoperator");
        }
        
        #endregion

        #region expression_statement
        
        void expression_statement()
        {
            expr();           
        }

        void identifier_access()
        {        
            direct_variable_declarator(); identifier_accessP();
        }

        void identifier_accessP()
        {
            if (peek("."))
            {
                match("."); direct_variable_declarator(); identifier_accessP();
            }
            //null
        }
        
        #endregion

        #region if_statement

        void if_statement()
        {
            match("if"); match("("); expr(); ; match(")"); 
            if_compound_statement();
            elseif_();
        }        

        void if_compound_statement()
        {
            if (peek("{"))
            {
                compound_statement();
            }
            else
                statement();
        }

        void elseif_()
        {
            if (peek("else"))
            {
                match("else"); elseif_P(); 
            }
            //null
        }

        void elseif_P()
        {
            if (peek("if"))
            {
                if_statement(); elseif_();
            }
            else
                if_compound_statement();
        }        

        #endregion

        #region return, continue, break statements

        void return_statement()
        {
            match("return"); return_statementP();
        }

        void return_statementP()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.INCREMENT:
                case TokenType.DECREMENT:
                case TokenType.ID:
                case TokenType.REAL_LITERAL:
                case TokenType.INTEGER_LITERAL:
                case TokenType.LEFT_PARENTHESIS:
                    expr();
                    break;
            }
            //null
        }

        void break_statement()
        {
            match("break");
        }

        void continue_statement()
        {
            match("continue");
        }

        #endregion

        #region while_statement

        void while_statement()
        {
            match("while"); match("("); expr(); match(")"); 
            if_compound_statement();
        }

        #endregion

        #region do while

        void do_while()
        {
            match("do"); 
            compound_statement(); 
            do_whileP(); 
        }

        void do_whileP()
        {
            match("while"); match("("); expr(); match(")"); match(";");
        }

        #endregion

        #region for_statement

        void for_statement()
        {
            match("for"); match("("); for_initialization(); match(";"); for_control(); match(";"); for_iteration(); match(")");
            if_compound_statement();
        }

        void for_initialization()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:
                    variable_declaration();
                    break;
                case TokenType.ID:
                case TokenType.INCREMENT:
                case TokenType.DECREMENT:
                case TokenType.NOT:
                    expr();
                    break;
            }
            //null
        }

        void for_control()
        {
            if (peek(TokenType.INTEGER_LITERAL) || peek(TokenType.ID))
            {
                expr();
            }
            //null
        }

        void for_iteration()
        {
            if (peek(TokenType.INTEGER_LITERAL) || peek(TokenType.ID))
            {
                expr();
            }
            //null
        }

        #endregion

        #endregion

        #region constant declaration

        void constant_declaration()
        {
            match("const"); variable_declaration();
        }

        #endregion        

        #region enum_declaration

        void enum_declaration()
        {
            enum_declarator(); enum_initializer(); 
        }

        void enum_declarator()
        {
            match("enum"); match(TokenType.ID);
        }

        void enum_initializer()
        {
            if (peek("{"))
            {
                match("{"); enum_initializer_list(); match("}");
            }
            else
            {
                match(TokenType.ID);
            }
        }

        void enum_initializer_list()
        {
            enum_constant_expression(); enum_initializer_listP();
        }

        void enum_initializer_listP()
        {
            if (peek(","))
            {
                match(","); enum_constant_expression(); enum_initializer_listP();
            }
            //null
        }

        void enum_constant_expression()
        {
            variable_name(); enum_constant_expressionP();
        }

        void enum_constant_expressionP()
        {
            if (peek("="))
            {
                match("="); OR_expr();
            }
            //null
        }

        #endregion

        #region struct_declaration
        void struct_declaration()
        {
            struct_declarator(); struct_declarationP();
        }

        void struct_declarator()
        {
            match("struct"); match(TokenType.ID);
        }

        void struct_declarationP()
        {
            if (peek("{"))
            {
                match("{"); variable_declaration_list(); match("}");
            }
            else
                match(TokenType.ID);
        }

        void variable_declaration_list()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:
                    variable_type(); direct_variable_declarator(); match(";"); variable_declaration_listP();
                    break;
                case TokenType.STRUCT:
                    struct_declarator(); match(TokenType.ID); match(";"); variable_declaration_listP();
                    break;
                default:
                    throw new Exception("Error en la declaracion de variables de struct linea: " + lex.line + " columna: " + lex.column + " currenttoken = " + currentToken.Lexema);
            }           
        }

        void variable_declaration_listP()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:
                    variable_type(); direct_variable_declarator(); match(";"); variable_declaration_listP();
                    break;
                case TokenType.STRUCT:
                    struct_declarator(); match(TokenType.ID); match(";"); variable_declaration_listP();
                    break;                
            }            
            //null
        }
        #endregion
        
        #region void declaration
        
        void void_declaration()
        {
            match("void"); direct_variable_declarator();
        }
        
        #endregion

        #region expressions

        void expr()
        {
            sequence_expr();
        }

        void sequence_expr()
        {
            assign_expr(); sequence_exprP();
        }

        void sequence_exprP()
        {
            if (peek(","))
            {
                match(","); assign_expr(); sequence_exprP();
            }
            //null
        }

        void assign_expr()
        {
            OR_expr(); assign_exprP();
        }

        void assign_exprP()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.ASSIGNMENT:
                case TokenType.ADDITION_ASSINGMENT:
                case TokenType.SUBSTRACTION_ASSIGNMENT:
                case TokenType.MULTIPLICATION_ASSIGNMENT:
                case TokenType.DIVISION_ASSIGNMENT:
                    currentToken = lex.nextToken();
                    OR_expr(); assign_exprP();
                    break;
            }
            //null
        }
        
        void OR_expr()
        {
            AND_expr(); OR_exprP();
        }
        
        void OR_exprP()
        {
            if (peek("||"))
            {
                match("||"); AND_expr(); OR_exprP();
            }
            //null
        }
        
        void AND_expr()
        {
            equal_expr(); AND_exprP();
        }

        void AND_exprP()
        {
            if (peek("&&"))
            {
                match("&&"); equal_expr(); AND_exprP();
            }
            //null
        }

        void equal_expr()
        {
            relation_expr(); equal_exprP();
        }

        void equal_exprP()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.EQUAL:
                case TokenType.NOTEQUAL:
                    currentToken = lex.nextToken();
                    relation_expr(); equal_exprP();
                    break;
            }
            //null
        }

        void relation_expr()
        {
            additive_expr(); relation_exprP();
        }

        void relation_exprP()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.GREATER:
                case TokenType.GREATER_EQUAL:
                case TokenType.LESS:
                case TokenType.LESS_EQUAL:
                    currentToken = lex.nextToken();
                    additive_expr(); relation_exprP();
                    break;
            }
            //null
        }
        
        void additive_expr()
        {
            multiplicative_expr(); additive_exprP();
        }

        void additive_exprP()
        {
            switch(currentToken.Tipo){
                case TokenType.ADDITION:
                case TokenType.SUBSTRACTION:
                    currentToken = lex.nextToken();
                    multiplicative_expr(); additive_exprP();
                    break;
            }
            //null
        }

        void multiplicative_expr()
        {
            unary_expr(); multiplicative_exprP();
        }

        void multiplicative_exprP()
        {            
            switch (currentToken.Tipo)
            {
                case TokenType.MULTIPLICATION:
                case TokenType.DIVISION:
                case TokenType.REMAINDER:
                    currentToken = lex.nextToken();
                    unary_expr(); multiplicative_exprP();
                    break;
            }
            //null
        }

        void unary_expr()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.INCREMENT:
                case TokenType.DECREMENT:
                case TokenType.NOT:
                    prefix_operator(); postfix_expr(); unary_exprP();
                    break;
                default:
                    postfix_expr(); unary_exprP();
                    break;
            }
        }

        void unary_exprP()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.INCREMENT:
                case TokenType.DECREMENT:
                case TokenType.NOT:
                    prefix_operator(); postfix_expr(); unary_exprP();
                    break;
            }
            //null
        }

        void postfix_expr()
        {
            if (peek(TokenType.ID))
            {
                variable_name(); id_postfix();
            }
            else
                primary_expr();
        }

        void id_postfix()
        {
            if (peek("++") || peek("--"))
            {
                postfix_operator();
            }
            else if (peek("["))
            {
                variable_array(); identifier_accessP();
            }
            else if (peek("("))
            {
                function_call();
            }
            else if (peek("."))
            {
                identifier_accessP();
            }
            //null
        }

        void postfix_operator()
        {
            if (peek("--"))
            {
                match("--");//postdecrement
            }
            else if (peek("++"))
            {
                match("++");//postincrement
            }
            else
                throw new Exception("Error en el postfix operator linea: " + lex.line + " columna: " + lex.column + " currenttoken = " + currentToken.Lexema);
        }

        void primary_expr()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.TRUE:
                case TokenType.FALSE:

                case TokenType.CHARACTER_LITERAL:
                case TokenType.STRING_LITERAL:

                case TokenType.INTEGER_LITERAL:
                case TokenType.REAL_LITERAL:
                    currentToken = lex.nextToken();
                    break;
                case TokenType.LEFT_PARENTHESIS:
                    match("("); expr(); match(")");
                    break;
                default:
                    throw new Exception("Error en la expresion linea: " + lex.line + " columna: " + lex.column + " currenttoken -> " + currentToken.Lexema);
            }
        }

        #region function_call

        void function_call()
        {
            match("("); parameter_list(); match(")");
        }

        void parameter_list()
        {
            if (peek(TokenType.ID))
            {
                identifier_access(); parameter_listP();
            }
            //null
        }

        void parameter_listP()
        {
            if (peek(","))
            {
                match(","); identifier_access(); parameter_listP();
            }
            //null
        }

        #endregion

        #endregion

        #region matchers

        void match(string token)
        {
            try
            {
                TokenType type = lex.getTokenType(token);

                if (currentToken.Tipo == type)
                    currentToken = lex.nextToken();
                else
                    throw new Exception("En la linea: " + lex.line + " columna: " + lex.column + " se esperaba \'" + token + "\' con id \'" + type + "\' token actual -> " + currentToken.Lexema);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }        

        void match(TokenType tokentype)
        {
            if (currentToken.Tipo == tokentype)
                currentToken = lex.nextToken();
            else
                throw new Exception("En la linea: " + lex.line + " columna: " + lex.column + " se esperaba \'" + tokentype + "\' token actual -> " + currentToken.Tipo);
        }

        bool peek(string token)
        {
            try
            {

                TokenType type = lex.getTokenType(token);
                return (currentToken.Tipo == type);
            }
            catch (Exception ex)
            {
                throw new Exception("En la linea: " + lex.line + " columna: " + lex.column + " | Peek Token -> \'" + token + "\' " + ex.ToString());
            }
        }

        bool peek(TokenType tokentype)
        {
            return (currentToken.Tipo == tokentype);
        }

        #endregion
        #endregion
    }
}
