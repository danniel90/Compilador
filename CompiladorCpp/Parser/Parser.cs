using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lexical;
using SyntaxTree;
using Semantic;

namespace Syntax
{
    public class Parser
    {
        #region variables
        public Token currentToken;
        public Lexer lex;

        public static Env entorno;
        #endregion

        #region constructores
        public Parser(string path)
        {
            lex = new Lexer(path);
            entorno = new Env(null);
        }
        #endregion

        #region funciones

        public void compile()
        {
            currentToken = lex.nextToken();
            Sentence sentencias = program();

            Console.WriteLine(sentencias.genCode() + "\n\n");
            sentencias.validarSemantica();
        }

        Sentence program()
        { 
            Sentence sentencias = global_sentence(); 
            match("\0");
            return sentencias;
        }

        Sentence global_sentence()
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
                    Sentence sentencia = sentence();
                    return global_sentenceP(sentencia);

                default:
                    throw new Exception("Global sentence exception! wtf!!! linea: " + Lexer.line + " columna: " + Lexer.column + " currentToken -> " + currentToken.Lexema);
            }
        }

        Sentence global_sentenceP(Sentence sentencia1)
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
                    Sentence sentencia2 = sentence();
                    SentenceSenquence sentSequence = new SentenceSenquence(sentencia1, sentencia2);
                    return global_sentenceP(sentSequence);
                
                default:
                    return sentencia1;//null
            }
        }

        Sentence sentence()
        {
            try
            {
                switch (currentToken.Tipo)
                {
                    case TokenType.CONST:
                        return constant_declaration();                        

                    case TokenType.ENUM:
                        Sentence sentence = enum_declaration();
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
                        return variable_declaration();                        

                    case TokenType.VOID:
                        string id = void_declaration();
                        return function_declaration(null, id);

                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString() + "|SENTENCE| Error en la declaracion de sentenceia linea:" + Lexer.line + " columna:" + Lexer.column);
            }
        }

        #region variable declaration

        Sentence variable_declaration()
        {
            Tipo tipoVariables = variable_type();//tipo basico para todas las variables, si hay mas

            Tipo tipoVariable = tipoVariables;
            string idVariable = direct_variable_declarator(tipoVariable);

            if (peek("("))
            {
                return function_declaration(tipoVariable, idVariable);
            }
            else
            {
                VariableSubDeclarator primerVariable = new VariableSubDeclarator(tipoVariable, idVariable);

                VariableDeclarator primerDeclaracionVariable = new VariableDeclarator(primerVariable, null);//todavia no hemos visto inicializadores

                VariableDeclarations variableDeclarations = variable_declarator(primerDeclaracionVariable, tipoVariables);//para las variables que siguen
                match(";");

                return variableDeclarations;
            }
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
                    throw new Exception("Error en la declaracion de tipo linea: " + Lexer.line + " columna: " + Lexer.column + " currenttoken = " + currentToken.Lexema);
            }
        }

        string direct_variable_declarator(Tipo tipoVariable)
        {
            string id = variable_name();
            
            if (peek("["))
                tipoVariable = variable_array(tipoVariable);

            return id;
        }

        string variable_name()
        {
            string id = currentToken.Lexema;
            match(TokenType.ID);
            return id;
        }

        Tipo variable_array(Tipo tipoArreglo)
        {
            if (peek("["))
            {
                match("["); 
                //Expr sizeExpr = expr();
                int sizeExpr = Int32.Parse(currentToken.Lexema);
                match(TokenType.INTEGER_LITERAL);
                match("]");

                Tipo tipoArreglo2 = variable_array(tipoArreglo);
                
                return new Arreglo(tipoArreglo2, sizeExpr);
            }
            return tipoArreglo;//null
        }        

        #region variable_declarator

        VariableDeclarations variable_declarator(VariableDeclarator primerVariable, Tipo tipoVariables)
        {
            primerVariable.initialization = variable_initializer();//primer variable viene sin inicializador

            entorno.put(primerVariable.declaration.id, primerVariable.declaration.tipo);//tablasimbolos

            VariableDeclaration listaDeclaracionVariables = variable_declarators(primerVariable, tipoVariables);

            return new VariableDeclarations(listaDeclaracionVariables);
        }

        VariableDeclaration variable_declarators(VariableDeclaration beforeDeclaration, Tipo tipoVariables)
        {
            if (peek(","))
            {
                match(",");
                Tipo tipoVariable = tipoVariables;
                string idVariable = direct_variable_declarator(tipoVariable);
                Initializers init = variable_initializer();

                entorno.put(idVariable, tipoVariable);//tablasimbolos

                VariableSubDeclarator variableActual = new VariableSubDeclarator(tipoVariable, idVariable);                
                VariableDeclarator actualDeclaration = new VariableDeclarator(variableActual, init);
                VariableDeclarators variableDeclarators = new VariableDeclarators(beforeDeclaration, actualDeclaration);

                return variable_declarators(variableDeclarators, tipoVariables);
            }
            else
                return beforeDeclaration;//null
        }

        #endregion

        #region variable_initializer

        Initializers variable_initializer()
        {
            if (peek("="))
            {
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

        #endregion

        #region functions ()

        Sentence function_declaration(Tipo retorno, string id)
        {
            match("(");
            Tipo paramsTypeList = parameter_type_list(); 
            match(")");

            if (paramsTypeList != null)
            {
                Funcion funcion = new Funcion(retorno, paramsTypeList);
                entorno.put(id, funcion);//global
            }

            Sentence compoundStmnt = function_definition();

            FuntionDefinition funcDefinition = new FuntionDefinition(id, retorno, compoundStmnt);
            //FuncionDefinition funcDefinition = new FuncionDefinition(paramsTypeList, compoundStmnt, id, retorno);
            return funcDefinition;
        }

        #region parameters_type_lis

        Tipo parameter_type_list()
        {
            bool isConstant = declaration_specifer();
            Tipo tipoParam = parameter_type();

            if (tipoParam != null)
            {

                bool isReference = reference();
                string id = parameter_name(tipoParam);

                return parameter_type_listP(tipoParam);
            }
            else
                return null;
        }

        bool declaration_specifer()
        {
            if (peek("const"))
            {
                match("const");
                return true;
            }
            else
                return false;//null  
        }

        Tipo parameter_type()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:
                    return variable_type();

                case TokenType.STRUCT:
                case TokenType.ENUM:
                    throw new NotImplementedException("Falta implementar strcuts y enums como parametros");

                default:
                    return null;//null
            }
        }

        bool reference()
        {
            if (peek("&"))
            {
                match("&");
                return true;
            }
            else
                return false;//null            
        }

        string parameter_name(Tipo tipoParametro)
        {
            string parameterid = direct_variable_declarator(tipoParametro);
            return parameterid;
        }

        Tipo parameter_type_listP(Tipo beforeParameter)
        {
            if (peek(","))
            {
                match(",");

                bool isConstant = declaration_specifer();
                Tipo actualParameter = parameter_type();
                bool isReference = reference();
                string id = parameter_name(actualParameter);

                Product producto = new Product(beforeParameter, actualParameter);
                return parameter_type_listP(producto);
            }
            else
                return beforeParameter;//null   
        }

        #endregion

        #region function_definition
        
        Sentence function_definition()
        {
            return compound_statement();
        }

        Sentence compound_statement()
        {
            match("{");
            Sentence stList = statement_list();
            match("}");

            CompoundStatement cpStmt = new CompoundStatement(stList);
            return cpStmt;
        }

        Sentence statement_list()
        {
            Sentence stmnt = statement();

            return statement_listP(stmnt);
        }

        Sentence statement_listP(Sentence statement1)
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
                    Sentence statement2 = statement();

                    StatementSequence statementList = new StatementSequence(statement1, statement2);
                    return statement_listP(statementList);

                default:
                    return statement1;//null
            }            
        }

        Sentence statement()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:                
                    Sentence decStmt = declaration_statement(); 
                    match(";");
                    return decStmt;

                case TokenType.STRUCT:
                    string strId = struct_declarator();
                    Tipo varRecord = entorno.get(strId);
                    string strVarName = currentToken.Lexema;
                    match(TokenType.ID);
                    return new StructVariableDeclaration(strId, strVarName, varRecord);

                case TokenType.DECREMENT:
                case TokenType.INCREMENT:
                case TokenType.ID:
                    Sentence expStmt = expression_statement();
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
                    Sentence brkStmnt = break_statement(); 
                    match(";");
                    return brkStmnt;

                case TokenType.CONTINUE:
                    Sentence contStmnt = continue_statement(); 
                    match(";");
                    return contStmnt;

                case TokenType.RETURN:
                    Sentence ret = return_statement(); 
                    match(";");
                    return ret;
                
                default:
                    return null;
            }
            //null
        }

        Sentence declaration_statement()
        {
            Tipo tipoVariables = variable_type();//tipo basico para todas las variables, si hay mas

            Tipo tipoVariable = tipoVariables;
            string idVariable = direct_variable_declarator(tipoVariable);
            
            VariableSubDeclarator primerVariable = new VariableSubDeclarator(tipoVariable, idVariable);

            VariableDeclarator primerDeclaracionVariable = new VariableDeclarator(primerVariable, null);//todavia no hemos visto inicializadores

            VariableDeclarations variableDeclarations = variable_declarator(primerDeclaracionVariable, tipoVariables);//para las variables que siguen            

            return variableDeclarations;
        }        
        
        #region expression_statement

        Sentence expression_statement()
        {
            Expr expresion = expr();            
            ExpressionStatement exprStatement = new ExpressionStatement(expresion);
            return exprStatement;
        }
        
        #endregion

        #region if_statement

        Sentence if_statement()
        {
            match("if"); 
            match("(");
            Expr expresion = expr();           
            match(")");
            Sentence trueBlock = if_compound_statement();

            return elseif_(expresion, trueBlock);
        }

        Sentence if_compound_statement()
        {
            if (peek("{"))
                return compound_statement();            
            else
                return statement();
        }

        Sentence elseif_(Expr condicion, Sentence trueBlock)
        {
            if (peek("else"))
            {
                match("else");
                Sentence falseBLock = elseif_P(condicion, trueBlock);
                return new IfElseStatement(condicion, trueBlock, falseBLock);
            }
            else
                return new IfStatement(condicion, trueBlock);
        }

        Sentence elseif_P(Expr condicion, Sentence trueBlock)
        {
            if (peek("if"))
            {
                match("if");
                match("(");
                Expr expresion = expr();
                match(")");
                Sentence elseIfTrueBlock = if_compound_statement();

                return elseif_(expresion, elseIfTrueBlock);
            }
            else
            {
                Sentence falseBlock = if_compound_statement();
                return new IfElseStatement(condicion, trueBlock, falseBlock);
            }
        }

        #endregion

        #region return, continue, break statements

        Sentence return_statement()
        {
            match("return");
            Sentence expresion = return_statementP();

            ReturnStatement returnStmnt = new ReturnStatement(expresion);
            return returnStmnt;
        }

        Sentence return_statementP()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.INCREMENT:
                case TokenType.DECREMENT:
                case TokenType.ID:
                case TokenType.REAL_LITERAL:
                case TokenType.INTEGER_LITERAL:
                case TokenType.LEFT_PARENTHESIS:
                    Sentence expresion = expression_statement();
                    return expresion;

                default:
                    return null;
            }
            //null
        }

        Sentence break_statement()
        {
            match("break");
            return new BreakStatement();
        }

        Sentence continue_statement()
        {
            match("continue"); 
            return new ContinueStatement();
        }

        #endregion

        #region while_statement

        Sentence while_statement()
        {
            match("while");
            match("(");
            Expr expresion = expr();
            //Statement expresion = expression_statement();            
            match(")");
            Sentence ifCpStmnt = if_compound_statement();

            WhileStatement whileStmnt = new WhileStatement(expresion, ifCpStmnt);
            return whileStmnt;
        }

        #endregion

        #region do while

        Sentence do_while()
        {
            match("do");
            Sentence stmnt = compound_statement();

            match("while");
            match("(");
            Expr exprStmnt = expr();
            match(")");
            match(";");

            DoWhileStatement doWhileStmnt = new DoWhileStatement(exprStmnt, stmnt);
            return doWhileStmnt;
        }

        #endregion

        #region for_statement

        Sentence for_statement()
        {
            match("for");
            match("(");
            Sentence forInitialization = for_initialization(); 
            match(";");
            Sentence forControl = for_control(); 
            match(";");
            Sentence forIteration = for_iteration();
            match(")");

            Sentence ifCompoundStatement = if_compound_statement();

            ForStatement forStmnt = new ForStatement(forInitialization, forControl, forIteration, ifCompoundStatement);
            return forStmnt;
        }

        Sentence for_initialization()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:
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

        Sentence for_control()
        {
            if (peek(TokenType.INTEGER_LITERAL) || peek(TokenType.ID))
            {
                return expression_statement();
            } else 
                return null;
            //null
        }

        Sentence for_iteration()
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
                Env savedEnv = entorno;
                entorno = new Env(null);
                List<VariableDeclarator> varsDec = enum_initializer_list();
                match("}");

                EnumerationDeclaration enumDeclaration = new EnumerationDeclaration(enumName, varsDec, entorno);
                entorno = savedEnv;
                entorno.put(enumName, new Enumeracion());

                return enumDeclaration;
            }
            else
            {
                Tipo varEnum = entorno.get(enumName);
                string enumVarName = currentToken.Lexema;
                match(TokenType.ID);

                EnumerationVariableDeclaration enumVarDeclaration = new EnumerationVariableDeclaration(enumName, enumVarName, varEnum);

                return enumVarDeclaration;
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

            VariableSubDeclarator varSubDec = new VariableSubDeclarator(new Enumeracion(), varName);

            VariableDeclarator varDeclarator = new VariableDeclarator(varSubDec, varInit);
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
                Env savedEnv = entorno;
                entorno = new Env(null);
                Sentence strDec = variable_declaration_list();
                match("}");

                Registro record = new Registro(entorno);                
                entorno = savedEnv;
                entorno.put(structName, record);
                return strDec;
            }
            else
            {
                Tipo varRecord = entorno.get(structName);
                string strVarName = currentToken.Lexema;
                match(TokenType.ID);
                return new StructVariableDeclaration(structName, strVarName, varRecord);
            }
        }

        Sentence variable_declaration_list()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:
                    Tipo t = variable_type();
                    string id = direct_variable_declarator(t);
                    entorno.put(id, t);
                    match(";");

                    StructDeclaration strDec = new StructDeclaration();
                    
                    return variable_declaration_listP(strDec);
                    
                case TokenType.STRUCT:
                    string structName = struct_declarator();
                    string structVarName = currentToken.Lexema;
                    Tipo varRecord = entorno.get(structName);
                    match(TokenType.ID); 
                    match(";");

                    StructVariableDeclaration strVarDec = new StructVariableDeclaration(structName, structVarName, varRecord);
                    return variable_declaration_listP(strVarDec);
                
                default:
                    throw new Exception("Error en la declaracion de variables de struct linea: " + Lexer.line + " columna: " + Lexer.column + " currenttoken = " + currentToken.Lexema);
            }
        }

        Sentence variable_declaration_listP(Sentence strDec)
        {            
            switch (currentToken.Tipo)
            {
                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:
                    Tipo t = variable_type();
                    string id = direct_variable_declarator(t);

                    entorno.put(id, t);
                    
                    match(";");

                    StructDeclaration structDec = new StructDeclaration();
                    SentenceSenquence stSeq = new SentenceSenquence(strDec, structDec);
                    return variable_declaration_listP(stSeq);
                
                case TokenType.STRUCT:
                    string structName = struct_declarator();
                    string structVarName = currentToken.Lexema;
                    Tipo varRecord = entorno.get(structName);
                    match(TokenType.ID);
                    match(";");

                    StructVariableDeclaration strVarDec = new StructVariableDeclaration(structName, structVarName, varRecord);
                    SentenceSenquence stSeq2 = new SentenceSenquence(strDec, strVarDec);
                    return variable_declaration_listP(stSeq2);

                default:
                    return strDec;//null
            }            
        }

        #endregion
        
        #region void declaration
        
        string void_declaration()
        {
            match("void");
            string functionName = variable_name();
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
            Expr expr1 = assign_expr();
            return sequence_exprP(expr1);            
        }

        Expr sequence_exprP(Expr expr1)
        {
            if (peek(","))
            {
                match(",");
                Expr expr2 = assign_expr();
                SequenceExpr seqExpr = new SequenceExpr(expr1, expr2);
                return sequence_exprP(seqExpr);
            }
            else
                return expr1;//null
        }

        Expr assign_expr()
        {
            Expr leftExpr = OR_expr();
            return assign_exprP(leftExpr);
        }

        Expr assign_exprP(Expr id)
        {
            switch (currentToken.Tipo)
            {
                case TokenType.ASSIGNMENT:
                    match(TokenType.ASSIGNMENT);

                    Expr assignvalue = OR_expr();
                    AssignExpr assignexpr = new AssignExpr(id, assignvalue);
                    return assign_exprP(assignexpr);

                case TokenType.ADDITION_ASSIGNMENT:
                    match(TokenType.ADDITION_ASSIGNMENT);

                    Expr addassignvalue = OR_expr();
                    AdditionAssignExpr additionassignexpr = new AdditionAssignExpr(id, addassignvalue);
                    return assign_exprP(additionassignexpr);

                case TokenType.SUBSTRACTION_ASSIGNMENT:
                    match(TokenType.SUBSTRACTION_ASSIGNMENT);

                    Expr subassignvalue = OR_expr();
                    SubstractionAssignExpr substractionassignexpr = new SubstractionAssignExpr(id, subassignvalue);
                    return assign_exprP(substractionassignexpr);

                case TokenType.MULTIPLICATION_ASSIGNMENT:
                    match(TokenType.MULTIPLICATION_ASSIGNMENT);

                    Expr mulassignvalue = OR_expr();
                    MultiplicationAssignExpr multiplicationassignexpr = new MultiplicationAssignExpr(id, mulassignvalue);
                    return assign_exprP(multiplicationassignexpr);                     

                case TokenType.DIVISION_ASSIGNMENT:
                    match(TokenType.DIVISION_ASSIGNMENT);

                    Expr divassignvalue = OR_expr();
                    DivisionAssignExpr divisionassignexpr = new DivisionAssignExpr(id, divassignvalue);
                    return assign_exprP(divisionassignexpr);                    

                default:
                    return id;//null
            }            
        }
        
        Expr OR_expr()
        {
            Expr leftExpr = AND_expr();
            return OR_exprP(leftExpr);
        }
        
        Expr OR_exprP(Expr leftExpr)
        {
            if (peek(TokenType.OR))
            {
                match(TokenType.OR);

                Expr rightor = AND_expr();
                OrExpr orexpr = new OrExpr(leftExpr, rightor);
                return OR_exprP(orexpr);
            }
            else
                return leftExpr;//null
        }
        
        Expr AND_expr()
        {
            Expr leftExpr = equal_expr(); 
            return AND_exprP(leftExpr);
        }

        Expr AND_exprP(Expr leftExpr)
        {
            if (peek(TokenType.AND))
            {
                match(TokenType.AND);

                Expr rightand = equal_expr();
                AndExpr andexpr = new AndExpr(leftExpr, rightand);
                
                return AND_exprP(andexpr);
            }
            else
                return leftExpr;//null
        }

        Expr equal_expr()
        {
            Expr leftExpr = relation_expr();
            return equal_exprP(leftExpr);
        }

        Expr equal_exprP(Expr leftExpr)
        {
            switch (currentToken.Tipo)
            {
                case TokenType.EQUAL:
                    match(TokenType.EQUAL);

                    Expr rightequal = relation_expr();
                    EqualExpr equalexpr = new EqualExpr(leftExpr, rightequal);
                    return equal_exprP(equalexpr);

                case TokenType.NOTEQUAL:
                    match(TokenType.NOTEQUAL);

                    Expr rightnotequal = relation_expr();
                    NotEqualExpr notequalexpr = new NotEqualExpr(leftExpr, rightnotequal);
                    return equal_exprP(notequalexpr);                    

                default:
                    return leftExpr;//null
            }
        }

        Expr relation_expr()
        {
            Expr leftExpr = additive_expr();
            return relation_exprP(leftExpr);
        }

        Expr relation_exprP(Expr leftExpr)
        {
            switch (currentToken.Tipo)
            {
                case TokenType.GREATER:
                    match(TokenType.GREATER);

                    Expr rightgreater = additive_expr();
                    GreaterExpr greaterexpr = new GreaterExpr(leftExpr, rightgreater);
                    return relation_exprP(greaterexpr);                    

                case TokenType.GREATER_EQUAL:
                    match(TokenType.GREATER_EQUAL);

                    Expr rightgreaterequal = additive_expr();
                    GreaterEqualExpr greaterequalexpr = new GreaterEqualExpr(leftExpr, rightgreaterequal);
                    return relation_exprP(greaterequalexpr);                     

                case TokenType.LESS:
                    match(TokenType.LESS);

                    Expr rightless = additive_expr();
                    LessExpr lessexpr = new LessExpr(leftExpr, rightless);
                    return relation_exprP(lessexpr);

                case TokenType.LESS_EQUAL:
                    match(TokenType.LESS_EQUAL);

                    Expr rightlessequal = additive_expr();
                    LessEqualExpr lessequalexpr = new LessEqualExpr(leftExpr, rightlessequal);
                    return relation_exprP(lessequalexpr);

                default:
                    return leftExpr;//null
            }
        }
        
        Expr additive_expr()
        {
            Expr leftExpr = multiplicative_expr();
            return additive_exprP(leftExpr);            
        }

        Expr additive_exprP(Expr leftExpr)
        {
            switch(currentToken.Tipo){
                case TokenType.ADDITION:
                    match(TokenType.ADDITION);
                    
                    Expr rightadd = multiplicative_expr();
                    AdditionExpr additionexpr = new AdditionExpr(leftExpr, rightadd);
                    return additive_exprP(additionexpr);                    

                case TokenType.SUBSTRACTION:
                    match(TokenType.SUBSTRACTION);

                    Expr rightsub = multiplicative_expr();
                    SubstractionExpr substractionexpr = new SubstractionExpr(leftExpr, rightsub);
                    return additive_exprP(substractionexpr);                    

                default:
                    return leftExpr;//null
            }            
        }

        Expr multiplicative_expr()//E.node = multiplicative_expr(unary_expr())
        {
            Expr leftExpr = unary_expr();//T
            return multiplicative_exprP(leftExpr);//E'            
        }

        Expr multiplicative_exprP(Expr leftExpr)//E'
        {            
            switch (currentToken.Tipo)
            {
                case TokenType.MULTIPLICATION:
                    match(TokenType.MULTIPLICATION);
                    
                    Expr rightmul = unary_expr();
                    MultiplicationExpr multiplicationexpr = new MultiplicationExpr(leftExpr, rightmul);
                    return multiplicative_exprP(multiplicationexpr);                    

                case TokenType.DIVISION:
                    match(TokenType.DIVISION);

                    Expr rightdiv = unary_expr();
                    DivisionExpr divisionexpr = new DivisionExpr(leftExpr, rightdiv);
                    return multiplicative_exprP(divisionexpr);                    

                case TokenType.REMAINDER:
                    match(TokenType.REMAINDER);

                    Expr rightmod = unary_expr();
                    RemainderExpr remainderexpr = new RemainderExpr(leftExpr, rightmod);
                    return multiplicative_exprP(remainderexpr);                    

                default:
                    return leftExpr;//null
            }            
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
                        return postfix_exprP(postid);
                    
                    case TokenType.INCREMENT:
                    case TokenType.DECREMENT:
                        return postfix_exprP(id);

                    default:
                        return id;
                }
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
                    throw new Exception("Error en la expresion postId linea: " + Lexer.line + " columna: " + Lexer.column + " currenttoken -> " + currentToken.Lexema);
            }            
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
                    return expresion;//null
            }            
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
                    throw new Exception("Error en la expresion linea: " + Lexer.line + " columna: " + Lexer.column + " currenttoken -> " + currentToken.Lexema);
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
                    throw new Exception("En la linea: " + Lexer.line + " columna: " + Lexer.column + " se esperaba \'" + token + "\' con id \'" + type + "\' token actual -> " + currentToken.Lexema);
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
                throw new Exception("En la linea: " + Lexer.line + " columna: " + Lexer.column + " se esperaba \'" + tokentype + "\' token actual -> " + currentToken.Tipo);
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
                throw new Exception("En la linea: " + Lexer.line + " columna: " + Lexer.column + " | Peek Token -> \'" + token + "\' " + ex.ToString());
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
