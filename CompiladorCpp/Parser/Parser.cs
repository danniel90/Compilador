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

        ReferenceAccess direct_variable_declarator()
        {
            Id id = variable_name();

            if (peek("["))
            {
                List<Expr> indexList = new List<Expr>();
                variable_array(indexList);

                IndiceArreglo indicearreglo = new IndiceArreglo(indexList, id.lexeme);
                return indicearreglo;
            }
            else
                return id;
        }

        Id variable_name()
        {
            Id Id = new Id(currentToken.Lexema);
            match(TokenType.ID);
            return Id;
        }

        void variable_array(List<Expr> indexList)
        {
            if (peek("["))
            {
                match("[");
                Expr expresion = expr(); ;
                indexList.Add(expresion);
                match("]"); 
                variable_array(indexList);//arreglos multidimensionales
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

        #region expression_statement
        
        void expression_statement()
        {
            expr();           
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

        Expr expr()
        {
            return sequence_expr();
        }

        Expr sequence_expr()
        {
            Expr expresion = assign_expr();
            if (peek(","))
            {
                List<Expr> listaExpresiones = new List<Expr>();
                sequence_exprP(listaExpresiones);

                SequenceExpr sequenceexpr = new SequenceExpr(listaExpresiones);
                return sequenceexpr;
            }
            else
                return expresion;
        }

        void sequence_exprP(List<Expr> listaExpresiones)
        {
            if (peek(","))
            {
                match(","); listaExpresiones.Add(assign_expr()); sequence_exprP(listaExpresiones);
            }
            //null
        }

        Expr assign_expr()
        {
            Expr leftExpr = OR_expr();

            switch (currentToken.Tipo)
            {
                case TokenType.ASSIGNMENT:
                case TokenType.ADDITION_ASSIGNMENT:
                case TokenType.SUBSTRACTION_ASSIGNMENT:
                case TokenType.MULTIPLICATION_ASSIGNMENT:
                case TokenType.DIVISION_ASSIGNMENT:
                    return assign_exprP(leftExpr);
                default:
                    return leftExpr;
            }
        }

        Expr assign_exprP(Expr id)
        {
            switch (currentToken.Tipo)
            {
                case TokenType.ASSIGNMENT:
                    match(TokenType.ASSIGNMENT);

                    Expr assignvalue = OR_expr();
                    AssignExpr assignexpr = new AssignExpr(id, assignvalue);

                    switch (currentToken.Tipo)
                    {
                        case TokenType.ASSIGNMENT:
                        case TokenType.ADDITION_ASSIGNMENT:
                        case TokenType.SUBSTRACTION_ASSIGNMENT:
                        case TokenType.MULTIPLICATION_ASSIGNMENT:
                        case TokenType.DIVISION_ASSIGNMENT:
                            return assign_exprP(assignexpr);
                        default:
                            return assignexpr;
                    }

                case TokenType.ADDITION_ASSIGNMENT:
                    match(TokenType.ADDITION_ASSIGNMENT);

                    Expr addassignvalue = OR_expr();
                    AdditionAssignExpr additionassignexpr = new AdditionAssignExpr(id, addassignvalue);

                    switch (currentToken.Tipo)
                    {
                        case TokenType.ASSIGNMENT:
                        case TokenType.ADDITION_ASSIGNMENT:
                        case TokenType.SUBSTRACTION_ASSIGNMENT:
                        case TokenType.MULTIPLICATION_ASSIGNMENT:
                        case TokenType.DIVISION_ASSIGNMENT:
                            return assign_exprP(additionassignexpr);
                        default:
                            return additionassignexpr;
                    }

                case TokenType.SUBSTRACTION_ASSIGNMENT:
                    match(TokenType.SUBSTRACTION_ASSIGNMENT);

                    Expr subassignvalue = OR_expr();
                    SubstractionAssignExpr substractionassignexpr = new SubstractionAssignExpr(id, subassignvalue);

                    switch (currentToken.Tipo)
                    {
                        case TokenType.ASSIGNMENT:
                        case TokenType.ADDITION_ASSIGNMENT:
                        case TokenType.SUBSTRACTION_ASSIGNMENT:
                        case TokenType.MULTIPLICATION_ASSIGNMENT:
                        case TokenType.DIVISION_ASSIGNMENT:
                            return assign_exprP(substractionassignexpr);
                        default:
                            return substractionassignexpr;
                    }

                case TokenType.MULTIPLICATION_ASSIGNMENT:
                    match(TokenType.MULTIPLICATION_ASSIGNMENT);

                    Expr mulassignvalue = OR_expr();
                    MultiplicationAssignExpr multiplicationassignexpr = new MultiplicationAssignExpr(id, mulassignvalue);
                    
                    switch (currentToken.Tipo)
                    {
                        case TokenType.ASSIGNMENT:
                        case TokenType.ADDITION_ASSIGNMENT:
                        case TokenType.SUBSTRACTION_ASSIGNMENT:
                        case TokenType.MULTIPLICATION_ASSIGNMENT:
                        case TokenType.DIVISION_ASSIGNMENT:
                            return assign_exprP(multiplicationassignexpr);
                        default:
                            return multiplicationassignexpr;
                    }

                case TokenType.DIVISION_ASSIGNMENT:
                    match(TokenType.DIVISION_ASSIGNMENT);

                    Expr divassignvalue = OR_expr();
                    DivisionAssignExpr divisionassignexpr = new DivisionAssignExpr(id, divassignvalue);

                    switch (currentToken.Tipo)
                    {
                        case TokenType.ASSIGNMENT:
                        case TokenType.ADDITION_ASSIGNMENT:
                        case TokenType.SUBSTRACTION_ASSIGNMENT:
                        case TokenType.MULTIPLICATION_ASSIGNMENT:
                        case TokenType.DIVISION_ASSIGNMENT:
                            return assign_exprP(divisionassignexpr);
                        default:
                            return divisionassignexpr;
                    }

                default:
                    throw new Exception("Error en la expresion assign linea: " + lex.line + " columna: " + lex.column + " currenttoken -> " + currentToken.Lexema);
            }
            //null
        }
        
        Expr OR_expr()
        {
            Expr leftExpr = AND_expr();

            if (peek(TokenType.OR))
                return OR_exprP(leftExpr);
            else
                return leftExpr;
        }
        
        Expr OR_exprP(Expr leftExpr)
        {            
            match(TokenType.OR); 
            
            Expr rightor = AND_expr();
            OrExpr orexpr = new OrExpr(leftExpr, rightor);

            if (peek(TokenType.OR))
                return OR_exprP(orexpr);
            else
                return orexpr;
        }
        
        Expr AND_expr()
        {
            Expr leftExpr = equal_expr(); 
            
            if (peek(TokenType.AND))
                return AND_exprP(leftExpr);
            else
                return leftExpr;
        }

        Expr AND_exprP(Expr leftExpr)
        {
            match(TokenType.AND);

            Expr rightand = equal_expr();
            AndExpr andexpr = new AndExpr(leftExpr, rightand);

            if (peek(TokenType.AND))           
                return AND_exprP(andexpr);
            else
                return andexpr;
        }

        Expr equal_expr()
        {
            Expr leftExpr = relation_expr();

            switch (currentToken.Tipo)
            {
                case TokenType.EQUAL:
                case TokenType.NOTEQUAL:
                    return equal_exprP(leftExpr);
                default:
                    return leftExpr;
            }
        }

        Expr equal_exprP(Expr leftExpr)
        {
            switch (currentToken.Tipo)
            {
                case TokenType.EQUAL:
                    match(TokenType.EQUAL);

                    Expr rightequal = relation_expr();
                    EqualExpr equalexpr = new EqualExpr(leftExpr, rightequal);

                    switch (currentToken.Tipo)
                    {
                        case TokenType.EQUAL:
                        case TokenType.NOTEQUAL:
                            return equal_exprP(equalexpr);
                        default:
                            return equalexpr;
                    }


                case TokenType.NOTEQUAL:
                    match(TokenType.NOTEQUAL);

                    Expr rightnotequal = relation_expr();
                    NotEqualExpr notequalexpr = new NotEqualExpr(leftExpr, rightnotequal);

                    switch (currentToken.Tipo)
                    {
                        case TokenType.EQUAL:
                        case TokenType.NOTEQUAL:
                            return equal_exprP(notequalexpr);
                        default:
                            return notequalexpr;
                    }

                default:
                    throw new Exception("Error en la expresion relation linea: " + lex.line + " columna: " + lex.column + " currenttoken -> " + currentToken.Lexema);
            }
            //null
        }

        Expr relation_expr()
        {
            Expr leftExpr = additive_expr();

            switch (currentToken.Tipo)
            {
                case TokenType.GREATER:
                case TokenType.GREATER_EQUAL:
                case TokenType.LESS:
                case TokenType.LESS_EQUAL:
                    return relation_exprP(leftExpr);
                default:
                    return leftExpr;
            }
        }

        Expr relation_exprP(Expr leftExpr)
        {
            switch (currentToken.Tipo)
            {
                case TokenType.GREATER:
                    match(TokenType.GREATER);

                    Expr rightgreater = additive_expr();
                    GreaterExpr greaterexpr = new GreaterExpr(leftExpr, rightgreater);

                    switch (currentToken.Tipo)
                    {
                        case TokenType.GREATER:
                        case TokenType.GREATER_EQUAL:
                        case TokenType.LESS:
                        case TokenType.LESS_EQUAL:
                            return relation_exprP(greaterexpr);
                        default:
                            return greaterexpr;
                    }

                case TokenType.GREATER_EQUAL:
                    match(TokenType.GREATER_EQUAL);

                    Expr rightgreaterequal = additive_expr();
                    GreaterEqualExpr greaterequalexpr = new GreaterEqualExpr(leftExpr, rightgreaterequal);

                    switch (currentToken.Tipo)
                    {
                        case TokenType.GREATER:
                        case TokenType.GREATER_EQUAL:
                        case TokenType.LESS:
                        case TokenType.LESS_EQUAL:
                            return relation_exprP(greaterequalexpr);
                        default:
                            return greaterequalexpr;
                    }

                case TokenType.LESS:
                    match(TokenType.LESS);

                    Expr rightless = additive_expr();
                    LessExpr lessexpr = new LessExpr(leftExpr, rightless);

                    switch (currentToken.Tipo)
                    {
                        case TokenType.GREATER:
                        case TokenType.GREATER_EQUAL:
                        case TokenType.LESS:
                        case TokenType.LESS_EQUAL:
                            return relation_exprP(lessexpr);
                        default:
                            return lessexpr;
                    }

                case TokenType.LESS_EQUAL:
                    match(TokenType.LESS_EQUAL);

                    Expr rightlessequal = additive_expr();
                    LessEqualExpr lessequalexpr = new LessEqualExpr(leftExpr, rightlessequal);

                    switch (currentToken.Tipo)
                    {
                        case TokenType.GREATER:
                        case TokenType.GREATER_EQUAL:
                        case TokenType.LESS:
                        case TokenType.LESS_EQUAL:
                            return relation_exprP(lessequalexpr);
                        default:
                            return lessequalexpr;
                    }

                default:
                    throw new Exception("Error en la expresion relation linea: " + lex.line + " columna: " + lex.column + " currenttoken -> " + currentToken.Lexema);
            }
            //null
        }
        
        Expr additive_expr()
        {
            Expr leftExpr = multiplicative_expr();

            switch (currentToken.Tipo)
            {
                case TokenType.ADDITION:
                case TokenType.SUBSTRACTION:
                    return additive_exprP(leftExpr);
                default:
                    return leftExpr;
            }
        }

        Expr additive_exprP(Expr leftExpr)
        {
            switch(currentToken.Tipo){
                case TokenType.ADDITION:
                    match(TokenType.ADDITION);
                    
                    Expr rightadd = multiplicative_expr();
                    AdditionExpr additionexpr = new AdditionExpr(leftExpr, rightadd);

                    switch(currentToken.Tipo)
                    {
                        case TokenType.ADDITION:
                        case TokenType.SUBSTRACTION:
                            return additive_exprP(additionexpr);
                        default:
                            return additionexpr;
                    }

                case TokenType.SUBSTRACTION:
                    match(TokenType.SUBSTRACTION);

                    Expr rightsub = multiplicative_expr();
                    SubstractionExpr substractionexpr = new SubstractionExpr(leftExpr, rightsub);

                    switch (currentToken.Tipo)
                    {
                        case TokenType.ADDITION:
                        case TokenType.SUBSTRACTION:
                            return additive_exprP(substractionexpr);
                        default:
                            return substractionexpr;
                    }

                default:
                    throw new Exception("Error en la expresion additive linea: " + lex.line + " columna: " + lex.column + " currenttoken -> " + currentToken.Lexema);
            }
            //null
        }

        Expr multiplicative_expr()//E.node = multiplicative_expr(unary_expr())
        {
            Expr leftExpr = unary_expr();//T

            switch(currentToken.Tipo)
            {
                case TokenType.MULTIPLICATION:
                case TokenType.DIVISION:
                case TokenType.REMAINDER:
                    return multiplicative_exprP(leftExpr);//E'
                
                default:
                    return leftExpr;
            }
        }

        Expr multiplicative_exprP(Expr leftExpr)//E'
        {            
            switch (currentToken.Tipo)
            {
                case TokenType.MULTIPLICATION:
                    match(TokenType.MULTIPLICATION);
                    
                    Expr rightmul = unary_expr();
                    MultiplicationExpr multiplicationexpr = new MultiplicationExpr(leftExpr, rightmul);

                    switch (currentToken.Tipo)
                    {
                        case TokenType.MULTIPLICATION:
                        case TokenType.DIVISION:
                        case TokenType.REMAINDER:
                            return multiplicative_exprP(multiplicationexpr);
                        default:
                            return multiplicationexpr;
                    }

                case TokenType.DIVISION:
                    match(TokenType.DIVISION);

                    Expr rightdiv = unary_expr();
                    DivisionExpr divisionexpr = new DivisionExpr(leftExpr, rightdiv);

                    switch (currentToken.Tipo)
                    {
                        case TokenType.MULTIPLICATION:
                        case TokenType.DIVISION:
                        case TokenType.REMAINDER:
                            return multiplicative_exprP(divisionexpr);
                        default:
                            return divisionexpr;
                    }

                case TokenType.REMAINDER:
                    match(TokenType.REMAINDER);

                    Expr rightmod = unary_expr();
                    RemainderExpr remainderexpr = new RemainderExpr(leftExpr, rightmod);
                    
                    switch (currentToken.Tipo)
                    {
                        case TokenType.MULTIPLICATION:
                        case TokenType.DIVISION:
                        case TokenType.REMAINDER:
                            return multiplicative_exprP(remainderexpr);
                        default:
                            return remainderexpr;
                    }

                default:
                    throw new Exception("Error en la expresion multiplicative linea: " + lex.line + " columna: " + lex.column + " currenttoken -> " + currentToken.Lexema);
            }
            //null
        }

        Expr unary_expr()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.INCREMENT:
                    match(TokenType.INCREMENT); 
                    Expr preincrementthis = postfix_expr();

                    PreIncrementExpr preincrement = new PreIncrementExpr(preincrementthis);
                    return preincrement;
                
                case TokenType.DECREMENT:
                    match(TokenType.DECREMENT); 
                    Expr predecrementthis = postfix_expr();

                    PreDecrementExpr predecrement = new PreDecrementExpr(predecrementthis);
                    return predecrement;
                
                case TokenType.NOT:
                    match(TokenType.NOT); 
                    Expr notthis = postfix_expr();

                    NotExpr notexpr = new NotExpr(notthis);
                    return notexpr;
                
                default:
                    return postfix_expr();
            }
        }

        Expr postfix_expr()
        {
            if (peek(TokenType.ID))
            {
                Id id = variable_name();

                switch (currentToken.Tipo)
                {
                    case TokenType.LEFT_SQUARE_BRACKET:
                    case TokenType.LEFT_PARENTHESIS:
                    case TokenType.DOT:
                        ReferenceAccess postid = postId_expr(id);
                        
                        if (peek(TokenType.INCREMENT) || 
                            peek(TokenType.DECREMENT))
                            return postfix_exprP(postid);
                        else
                            return postid;
                    
                    case TokenType.INCREMENT:
                    case TokenType.DECREMENT:
                        return postfix_exprP(id);

                    default:
                        return id;
                }
                //postfix_exprP();
            }
            else
                return primary_expr();
        }

        ReferenceAccess postId_expr(ReferenceAccess id)
        {
            switch (currentToken.Tipo)
            {
                case TokenType.LEFT_SQUARE_BRACKET:
                    List<Expr> indexList = new List<Expr>();
                    variable_array(indexList);
                    
                    IndiceArreglo indicearreglo = new IndiceArreglo(indexList, id.lexeme);
                    return indicearreglo;                    

                case TokenType.LEFT_PARENTHESIS:
                    List <Expr> parameterList = function_call();
                    
                    LlamadaFuncion llamadafuncion = new LlamadaFuncion(id.lexeme, parameterList);
                    return llamadafuncion;

                case TokenType.DOT:
                    List<ReferenceAccess> membersList = new List<ReferenceAccess>();
                    id_access(membersList);
                    MiembroRegistro miembroRegistro = new MiembroRegistro(id.lexeme, membersList);
                    return miembroRegistro;

                default:
                    throw new Exception("Error en la expresion postId linea: " + lex.line + " columna: " + lex.column + " currenttoken -> " + currentToken.Lexema);
            }            
        }

        void id_access(List<ReferenceAccess> memberslist)
        {
            if (peek("."))
            {
                match(".");
                ReferenceAccess reference = direct_variable_declarator();
                memberslist.Add(reference);
                id_access(memberslist);
            }//null
        }

        Expr postfix_exprP(Expr expresion)
        {
            switch (currentToken.Tipo)
            {
                case TokenType.INCREMENT:
                    PostIncrementExpr postIncrement = new PostIncrementExpr(expresion);
                    match(TokenType.INCREMENT);
                    return postIncrement;

                case TokenType.DECREMENT:
                    PostDecrementExpr postDecrement = new PostDecrementExpr(expresion);
                    match(TokenType.DECREMENT);
                    return postDecrement;

                default:
                    throw new Exception("Error en la expresion postfix linea: " + lex.line + " columna: " + lex.column + " currenttoken -> " + currentToken.Lexema);
            }
            //null
        }
        
        Expr primary_expr()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.TRUE:
                    BooleanoLiteral booleanoTrue = new BooleanoLiteral(true);
                    match(TokenType.TRUE);
                    return booleanoTrue;

                case TokenType.FALSE:
                    BooleanoLiteral booleanoFalse = new BooleanoLiteral(false);
                    match(TokenType.FALSE);
                    return booleanoFalse;

                case TokenType.CHARACTER_LITERAL:
                    CaracterLiteral caracter = new CaracterLiteral(char.Parse(currentToken.Lexema));
                    currentToken = lex.nextToken();
                    return caracter;

                case TokenType.STRING_LITERAL:
                    CadenaLiteral cadena = new CadenaLiteral(currentToken.Lexema);
                    currentToken = lex.nextToken();
                    return cadena;

                case TokenType.INTEGER_LITERAL:
                    EnteroLiteral entero = new EnteroLiteral(int.Parse(currentToken.Lexema));
                    match(TokenType.INTEGER_LITERAL);
                    return entero;

                case TokenType.REAL_LITERAL:
                    RealLiteral real = new RealLiteral(float.Parse(currentToken.Lexema));
                    match(TokenType.INTEGER_LITERAL);
                    return real;

                case TokenType.LEFT_PARENTHESIS:
                    match("("); Expr expresion = expr(); match(")");
                    return expresion;

                default:
                    throw new Exception("Error en la expresion linea: " + lex.line + " columna: " + lex.column + " currenttoken -> " + currentToken.Lexema);
            }
        }

        #region function_call

        List<Expr> function_call()
        {
            List<Expr> parametersList = new List<Expr>();
            
            match("("); 
            parameter_list(parametersList); 
            match(")");
            
            return parametersList;
        }

        void parameter_list(List<Expr> parameters)
        {
            if (peek(TokenType.ID))
            {
                parameters.Add(expr()); parameter_listP(parameters);
            }
            //null
        }

        void parameter_listP(List<Expr> parameters)
        {
            if (peek(","))
            {
                match(","); parameters.Add(expr()); parameter_listP(parameters);
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
