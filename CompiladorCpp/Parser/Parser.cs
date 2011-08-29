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
            List<Sentence> sentencias = program();

            int cont = 1;
            foreach (Sentence s in sentencias)
            {
                Console.Write(cont++ + " - "); 
                s.print();                
            }
        }

        List<Sentence> program()
        {
            List<Sentence> sentencias = new List<Sentence>();
            global_sentence(sentencias); match("\0");
            return sentencias;
        }

        void global_sentence(List<Sentence> sentencias)
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
                    Sentence sent = sentence();
                    sentencias.Add(sent);
                    global_sentenceP(sentencias);
                    break;
                default:
                    throw new Exception("Global sentence exception! wtf!!! linea: " + lex.line + " columna: " + lex.column + " currentToken -> " + currentToken.Lexema);
            }
        }

        void global_sentenceP(List<Sentence> sentencias)
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
                    Sentence sent = sentence();
                    sentencias.Add(sent);
                    global_sentenceP(sentencias);
                    break;
            }
            //null
        }

        Sentence sentence()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.CONST:                    
                    VariableSubDeclaration constVarSubDeclaration = (VariableSubDeclaration)constant_declaration();
                    
                    if (peek("("))
                    {
                        FuncionDefinition funcionDefinition = (FuncionDefinition)function_declaration(constVarSubDeclaration.tipo, constVarSubDeclaration.Id);
                        return funcionDefinition;
                    }
                    else
                    {
                        List<VariableDeclarator> varDecs = variable_declarator(constVarSubDeclaration); 
                        match(";");

                        VariableDeclaration varDeclaration = new VariableDeclaration(constVarSubDeclaration.tipo, varDecs);
                        return varDeclaration;
                    }                    
                    
                case TokenType.ENUM:
                    Sentence sentence =  enum_declaration();  
                    match(";");
                    return sentence;

                case TokenType.STRUCT:
                    Sentence structDec = struct_declaration(); 
                    match(";");
                    return structDec;

                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:
                    VariableSubDeclaration varSubDeclaration = (VariableSubDeclaration)variable_declaration();
                    
                    if (peek("("))
                    {
                        FuncionDefinition funcionDefinition = (FuncionDefinition)function_declaration(varSubDeclaration.tipo, varSubDeclaration.Id);
                        return funcionDefinition;                        
                    }
                    else
                    {
                        List<VariableDeclarator> varDecs = variable_declarator(varSubDeclaration);
                        match(";");

                        VariableDeclaration varDeclaration = new VariableDeclaration(varSubDeclaration.tipo, varDecs);
                        return varDeclaration;
                    }

                case TokenType.VOID:
                    string id = void_declaration();  
                    return function_declaration(null, id);

                default:
                    return null;
            }
        }

        #region variable declaration

        Sentence variable_declaration()
        {
            Tipo t = variable_type();
            string id = direct_variable_declarator();            

            VariableSubDeclaration variableSubDeclaration = new VariableSubDeclaration(t, id);
            return variableSubDeclaration;
        }

        Tipo variable_type()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.STRING:
                    match(TokenType.STRING);
                    return new Cadena();
                
                case TokenType.BOOL:
                    match(TokenType.BOOL);
                    return new Booleano();
                
                case TokenType.CHAR:
                    match(TokenType.CHAR);
                    return new Caracter();
                
                case TokenType.FLOAT:
                    match(TokenType.FLOAT);
                    return new Flotante();

                case TokenType.INT:
                    match(TokenType.INT);
                    return new Entero();
                    
                default:
                    throw new Exception("Error en la declaracion de tipo linea: " + lex.line + " columna: " + lex.column + " currenttoken = " + currentToken.Lexema);
            }
        }

        string direct_variable_declarator()
        {
            string id = variable_name();            
            
            variable_array();
            return id;
        }

        string variable_name()
        {
            string id = currentToken.Lexema;
            match(TokenType.ID);
            return id;
        }

        void variable_array()
        {
            if (peek("["))
            {
                match("["); expr(); match("]");
                variable_array();//arreglos multidimensionales
            }
            //null
        }

        #endregion

        #region variable_declarator

        List<VariableDeclarator> variable_declarator(VariableSubDeclaration variableSubDeclaration)
        {
            Initializers init = variable_initializer();
            
            List<VariableDeclarator> variableDeclaratorList = new List<VariableDeclarator>();
            variableDeclaratorList.Add(new VariableDeclarator(variableSubDeclaration.Id, init));

            variable_declarators(variableDeclaratorList);

            return variableDeclaratorList;
        }

        void variable_declarators(List<VariableDeclarator> varDeclarationList)
        {
            if (peek(","))
            {
                match(","); 
                string id = direct_variable_declarator(); 
                Initializers init = variable_initializer(); 
                varDeclarationList.Add(new VariableDeclarator(id, init));
                variable_declarators(varDeclarationList);
            }
            //null
        }

        #endregion

        #region variable_initializer

        Initializers variable_initializer()
        {
            if (peek("="))
            {
                //initializers();
                match("=");
                return initializer();
            }
            return null;
            //null
        }

        Initializers initializer()
        {
            if (peek("{"))
            {
                match("{");
                List<Initializers> varInitializerList = new List<Initializers>();
                VariableInitializerList varInitList = initializer_list(varInitializerList); 
                match("}");

                return varInitList;
            }            
            else
            {
                Expr expresion = expr();
                VariableInitializer varInitializer = new VariableInitializer(expresion);
                return varInitializer;
            }
        }

        VariableInitializerList initializer_list(List<Initializers> initlist)
        {
            Initializers varInit = initializer();
            initlist.Add(varInit);
            initializer_listP(initlist);

            return new VariableInitializerList(initlist);
        }

        void initializer_listP(List<Initializers> initlist)
        {
            if (peek(","))
            {
                match(","); 
                Initializers varInit = initializer();
                initlist.Add(varInit);
                initializer_listP(initlist);
            }
            //null
        }

        #endregion

        #region functions ()

        Sentence function_declaration(Tipo retorno, string id)
        {
            match("(");
            List<Product> paramsTypeList = parameter_type_list(); 
            match(")");
            Statement compoundStmnt = function_definition();

            FuncionDefinition funcDefinition = new FuncionDefinition(paramsTypeList, compoundStmnt, id, retorno);
            return funcDefinition;
        }

        #region parameters_type_list

        List<Product> parameter_type_list()
        {
            List<Product> parameterTypeList = new List<Product>();
            
            Product param = parameter_declaration();
            parameterTypeList.Add(param);            
            parameter_type_listP(parameterTypeList);

            return parameterTypeList;
        }

        void parameter_type_listP(List<Product> paramTypeList)
        {
            if (peek(","))
            {
                match(",");
                Product param = parameter_declaration();
                paramTypeList.Add(param);
                parameter_type_listP(paramTypeList);
            }
            //null
        }

        Product parameter_declaration()
        {
            declaration_specifer(); 
            return parameter_type();
        }

        void declaration_specifer()
        {
            if (peek("const"))
            {
                match("const");
            }
            //null
        }

        Product parameter_type()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:
                    Tipo tipoParametro = variable_type(); 
                    string idParametro = parameter();
                    
                    Product parametro = new Product(tipoParametro, idParametro);
                    return parametro;

                default:
                    return null;
            }
            //null
        }

        string parameter()
        {
            reference(); 
            string parameter = parameter_name();
            return parameter;
        }

        void reference()
        {
            if (peek("&"))
            {
                match("&");
            }
            //null
        } 

        string parameter_name()
        {
            string parameterid = direct_variable_declarator();
            return parameterid;
        }     

        #endregion

        #region function_definition
        
        Statement function_definition()
        {
            return compound_statement();
        }

        Statement compound_statement()
        {
            match("{"); 
            List<Statement> stList = statement_list(); 
            match("}");

            CompoundStatement cpStmt = new CompoundStatement(stList);
            return cpStmt;
        }

        List<Statement> statement_list()
        {
            List<Statement> statementList = new List<Statement>();
            Statement stmnt = statement();
            statementList.Add(stmnt);

            statement_listP(statementList);
            return statementList;
        }

        void statement_listP(List<Statement> statementList)
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
                    Statement stmnt = statement();
                    statementList.Add(stmnt);

                    statement_listP(statementList);
                    break;
            }
            //null
        }

        Statement statement()
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
                    Statement decStmt = declaration_statement(); 
                    match(";");
                    return decStmt;

                case TokenType.DECREMENT:
                case TokenType.INCREMENT:
                case TokenType.ID:
                    Statement expStmt = expression_statement();
                    match(";");
                    return expStmt;

                case TokenType.IF:
                    return if_statement();

                case TokenType.DO:
                    return do_while();

                case TokenType.WHILE:
                    return while_statement();

                case TokenType.FOR:
                    return for_statement();

                case TokenType.BREAK:
                    Statement brkStmnt = break_statement(); 
                    match(";");
                    return brkStmnt;

                case TokenType.CONTINUE:
                    Statement contStmnt = continue_statement(); 
                    match(";");
                    return contStmnt;

                case TokenType.RETURN:
                    Statement ret = return_statement(); 
                    match(";");
                    return ret;
                
                default:
                    return null;
            }
            //null
        }

        #region declaration_statement
        
        Statement declaration_statement()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:
                    //variable_declaration(); variable_declarator();
                    VariableSubDeclaration varSubDeclaration = (VariableSubDeclaration)variable_declaration();
                    List<VariableDeclarator> varDecList = variable_declarator(varSubDeclaration);

                    DeclarationStatement decStatement = new DeclarationStatement(varSubDeclaration.tipo, varDecList);
                    return decStatement;

                case TokenType.STRUCT:
                    string strId = struct_declarator();
                    string strVar = currentToken.Lexema;
                    match(TokenType.ID);

                    StructVariableDeclarationStatement strVarDec = new StructVariableDeclarationStatement(strId, strVar);
                    return strVarDec;

                default:
                    throw new Exception("Error en la declaracion de variables de statement linea: " + lex.line + " columna: " + lex.column + " currenttoken = " + currentToken.Lexema);
            }
        }            

        #endregion      

        #region expression_statement
        
        Statement expression_statement()
        {
            Expr expresion = expr();
            ExpressionStatement exprStatement = new ExpressionStatement(expresion);
            return exprStatement;
        }
        
        #endregion

        #region if_statement

        Statement if_statement()
        {
            match("if"); 
            match("(");
            Statement expresion = expression_statement();
            match(")");
 
            Statement ifCpnd = if_compound_statement();
            //IfStatement ifStmnt = new IfStatement(expresion, compound_statement, );
            return elseif_(expresion, ifCpnd);
        }        

        Statement if_compound_statement()
        {
            if (peek("{"))
            {
                return compound_statement();
            }
            else
                return statement();
        }

        Statement elseif_(Statement expr, Statement ifCompound)
        {
            if (peek("else"))
            {
                match("else");
                return elseif_P(expr, ifCompound);
            }
            else
            {
                IfStatement ifStmnt = new IfStatement(expr, ifCompound, null);
                return ifStmnt;
            }
            //null
        }

        Statement elseif_P(Statement expr, Statement ifCompound)
        {
            if (peek("if"))
            {
                Statement Stmnt = if_statement(); //elseif_();
                IfStatement ifStmnt = new IfStatement(expr, ifCompound, Stmnt);
                return ifStmnt;
            }
            else
            {
                Statement elseCompound = if_compound_statement();

                IfStatement ifStmnt = new IfStatement(expr, ifCompound, elseCompound);
                return ifStmnt;
            }
        }

        #endregion

        #region return, continue, break statements

        Statement return_statement()
        {
            match("return"); 
            Statement expresion = return_statementP();

            ReturnStatement returnStmnt = new ReturnStatement(expresion);
            return returnStmnt;
        }

        Statement return_statementP()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.INCREMENT:
                case TokenType.DECREMENT:
                case TokenType.ID:
                case TokenType.REAL_LITERAL:
                case TokenType.INTEGER_LITERAL:
                case TokenType.LEFT_PARENTHESIS:
                    Statement expresion = expression_statement();
                    return expresion;

                default:
                    return null;
            }
            //null
        }

        Statement break_statement()
        {
            match("break");
            return new BreakStatement();
        }

        Statement continue_statement()
        {
            match("continue"); 
            return new ContinueStatement();
        }

        #endregion

        #region while_statement

        Statement while_statement()
        {
            match("while");
            match("(");
            Statement expresion = expression_statement();
            match(")"); 
            Statement ifCpStmnt = if_compound_statement();

            WhileStatement whileStmnt = new WhileStatement(expresion, ifCpStmnt);
            return whileStmnt;
        }

        #endregion

        #region do while

        Statement do_while()
        {
            match("do"); 
            Statement stmnt = compound_statement();
            Statement doWhileStmnt = do_whileP(stmnt);

            return doWhileStmnt;
        }

        Statement do_whileP(Statement compoundStatemnt)
        {
            match("while");
            match("("); 
            //expr();
            Statement exprStmnt = expression_statement();
            match(")");
            match(";");

            DoWhileStatement doWhileStmnt = new DoWhileStatement(exprStmnt, compoundStatemnt);
            return doWhileStmnt;
        }

        #endregion

        #region for_statement

        Statement for_statement()
        {
            match("for");
            match("(");
            Statement forInitialization = for_initialization(); 
            match(";"); 
            Statement forControl = for_control(); 
            match(";");
            Statement forIteration = for_iteration();
            match(")");

            Statement ifCompoundStatement = if_compound_statement();

            ForStatement forStmnt = new ForStatement(forInitialization, forControl, forIteration, ifCompoundStatement);
            return forStmnt;
        }

        Statement for_initialization()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:
                    //variable_declaration();
                    return declaration_statement();
                
                case TokenType.ID:
                case TokenType.INCREMENT:
                case TokenType.DECREMENT:
                case TokenType.NOT:
                    return expression_statement();

                default:
                    return null;
            }
            //null
        }

        Statement for_control()
        {
            if (peek(TokenType.INTEGER_LITERAL) || peek(TokenType.ID))
            {
                return expression_statement();
            } else 
                return null;
            //null
        }

        Statement for_iteration()
        {
            if (peek(TokenType.INTEGER_LITERAL) || peek(TokenType.ID))
            {
                return expression_statement();
            }
            else
                return null;
            //null
        }

        #endregion

        #endregion

        #endregion

        #region constant declaration

        Sentence constant_declaration()
        {
            match("const"); 
            return variable_declaration();
        }

        #endregion        

        #region enum_declaration

        Sentence enum_declaration()
        {
            string enumName = enum_declarator();
            return enum_initializer(enumName); 
        }

        string enum_declarator()
        {
            match("enum");
            string enumName = currentToken.Lexema;
            match(TokenType.ID);
            return enumName;
        }

        Sentence enum_initializer(string enumName)
        {
            if (peek("{"))
            {
                match("{"); 
                List<VariableDeclarator> varsDec = enum_initializer_list();
                match("}");

                EnumerationDeclaration enumDec = new EnumerationDeclaration(enumName, varsDec);
                return enumDec;
            }
            else
            {
                string enumVarName = currentToken.Lexema;
                match(TokenType.ID);

                EnumerationVariableDeclaration enumVarDec = new EnumerationVariableDeclaration(enumName, enumVarName);
                return enumVarDec;
            }
        }

        List<VariableDeclarator> enum_initializer_list()
        {
            List<VariableDeclarator> varDecList = new List<VariableDeclarator>();
            VariableDeclarator varDec = enum_constant_expression();
            varDecList.Add(varDec);
            enum_initializer_listP(varDecList);
            return varDecList;
        }

        void enum_initializer_listP(List<VariableDeclarator> vars)
        {
            if (peek(","))
            {
                match(",");
                VariableDeclarator varDec = enum_constant_expression();
                vars.Add(varDec);
                enum_initializer_listP(vars);
            }
            //null
        }

        VariableDeclarator enum_constant_expression()
        {
            string varName = variable_name();
            VariableInitializer varInit = enum_constant_expressionP();

            VariableDeclarator varDeclarator = new VariableDeclarator(varName, varInit);
            return varDeclarator;
        }

        VariableInitializer enum_constant_expressionP()
        {
            if (peek("="))
            {
                match("=");
                Expr constantExpr = OR_expr();
                VariableInitializer varInit = new VariableInitializer(constantExpr);
                return varInit;
            }
            else
                return null;
            //null
        }

        #endregion

        #region struct_declaration
        Sentence struct_declaration()
        {
            string strId = struct_declarator(); 
            return struct_declarationP(strId);
        }

        string struct_declarator()
        {
            match("struct");
            string strId = currentToken.Lexema;
            match(TokenType.ID);

            return strId;
        }

        Sentence struct_declarationP(string structName)
        {
            if (peek("{"))
            {
                match("{");
                ListaCampos lCampos = new ListaCampos();
                variable_declaration_list(lCampos);
                match("}");

                StructDeclaration strDec = new StructDeclaration(structName, lCampos);
                return strDec;
            }
            else
            {
                string strVarName = currentToken.Lexema;
                match(TokenType.ID);
                StructVariableDeclaration strVarDec = new StructVariableDeclaration(structName, strVarName);
                return strVarDec;
            }
        }

        void variable_declaration_list(ListaCampos variables)
        {
            try
            {
                switch (currentToken.Tipo)
                {
                    case TokenType.STRING:
                    case TokenType.BOOL:
                    case TokenType.CHAR:
                    case TokenType.FLOAT:
                    case TokenType.INT:
                        Tipo t = variable_type();
                        string id = direct_variable_declarator();

                        variables.Campos.Add(id);
                        variables.tiposCampos.Add(id, t);

                        match(";");
                        variable_declaration_listP(variables);
                        break;
                    /*case TokenType.STRUCT:
                        string structName = struct_declarator();

                        string structVarName = currentToken.Lexema;
                        match(TokenType.ID); 
                        match(";");

                        Registro r = new Registro(

                        variable_declaration_listP();
                        break;*/
                    default:
                        throw new Exception("Error en la declaracion de variables de struct linea: " + lex.line + " columna: " + lex.column + " currenttoken = " + currentToken.Lexema);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString() + " En la linea:" + lex.line + " columna:" + lex.column );
            }
        }

        void variable_declaration_listP(ListaCampos variables)
        {
            try
            {
                switch (currentToken.Tipo)
                {
                    case TokenType.STRING:
                    case TokenType.BOOL:
                    case TokenType.CHAR:
                    case TokenType.FLOAT:
                    case TokenType.INT:
                        Tipo t = variable_type();
                        string id = direct_variable_declarator();

                        variables.Campos.Add(id);
                        variables.tiposCampos.Add(id, t);

                        match(";");
                        variable_declaration_listP(variables);

                        break;
                    /*case TokenType.STRUCT:
                        struct_declarator(); match(TokenType.ID); match(";"); variable_declaration_listP();
                        break;                */
                }
                //null
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString() + " En la linea:" + lex.line + " columna:" + lex.column);
            }
        }
        #endregion
        
        #region void declaration
        
        string void_declaration()
        {
            match("void"); 
            string functionName = direct_variable_declarator();
            return functionName;
        }
        
        #endregion

        #region expressions

        Expr expr()
        {
            Expr expresion = sequence_expr();
            //expresion.print();
            return expresion;
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
                Id id = variable_nameExpr();

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
                    variable_arrayExpr(indexList);
                    
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
                ReferenceAccess reference = direct_variable_declaratorExpr();
                memberslist.Add(reference);
                id_access(memberslist);
            }//null
        }

        ReferenceAccess direct_variable_declaratorExpr()
        {
            Id id = variable_nameExpr();

            if (peek("["))
            {
                List<Expr> indexList = new List<Expr>();
                variable_arrayExpr(indexList);

                IndiceArreglo indicearreglo = new IndiceArreglo(indexList, id.lexeme);
                return indicearreglo;
            }
            else
                return id;
        }

        Id variable_nameExpr()
        {
            Id Id = new Id(currentToken.Lexema);
            match(TokenType.ID);
            return Id;
        }

        void variable_arrayExpr(List<Expr> indexList)
        {
            if (peek("["))
            {
                match("[");
                Expr expresion = expr();
                indexList.Add(expresion);
                match("]");
                variable_arrayExpr(indexList);//arreglos multidimensionales
            }
            //null
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
