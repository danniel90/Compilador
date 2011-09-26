using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lexical;
using SyntaxTree;
//using Semantic;
using Environment;

namespace Syntax
{
    public class Parser
    {
        #region variables
        
        public Token currentToken;
        public Lexer lex;

        public static EnvTypes entornoTipos;
        public static EnvValues entornoValores; 

        public static Sentence funcionActual = null;
        public static Sentence cicloActual = null;
        public static Sentence main = null;

        #endregion

        #region constructores

        public Parser(string path)
        {
            lex = new Lexer(path);
            entornoTipos = new EnvTypes(null);
            entornoValores = new EnvValues(null);
        }
        #endregion

        #region funciones

        public void compile()
        {
            currentToken = lex.nextToken();
            Sentence programa = program();

            Console.WriteLine(programa.genCode() + "\n\n");
            programa.validarSemantica();
            programa.interpretar();
            int x = 0;
        }

        Sentence program()
        {
            Program programa = new Program();
            Sentence sentencias = global_sentence();
            programa.ProgramInit(sentencias);
            programa.main = main;

            match("\0");
            return programa;
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
                    
                case TokenType.CIN:
                case TokenType.COUT:

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
                
                case TokenType.CIN:
                case TokenType.COUT:
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
                        return function_declaration(new SyntaxTree.Void(), id);

                    case TokenType.CIN:
                        Sentence sCin = cin();
                        match(";");
                        return sCin;

                    case TokenType.COUT:
                        Sentence sCout = cout();
                        match(";");
                        return sCout;

                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString() + "|SENTENCE| Error en la declaracion de sentencia linea:" + Lexer.line + " columna:" + Lexer.column);
            }
        }

        #region variable declaration

        Sentence variable_declaration()
        {
            Tipo tipoVariables = variable_type();//tipo basico para todas las variables, si hay mas
            
            string idVariable = direct_variable_declarator();
            Tipo tipoVariable = variable_array(tipoVariables);

            if (peek("("))
                return function_declaration(tipoVariable, idVariable);
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

        string direct_variable_declarator()
        {
            return variable_name();
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
                
                Arreglo array_t = new Arreglo(tipoArreglo2, sizeExpr);
                //array_t.isReference = true;
                return array_t;
            } else
                return tipoArreglo;//null
        }        

        #region variable_declarator

        VariableDeclarations variable_declarator(VariableDeclarator primerVariable, Tipo tipoVariables)
        {
            primerVariable.initialization = variable_initializer();//primer variable viene sin inicializador

            entornoTipos.put(primerVariable.declaration.id, primerVariable.declaration.tipo);//tablasimbolos

            VariableDeclaration listaDeclaracionVariables = variable_declarators(primerVariable, tipoVariables);

            return new VariableDeclarations(listaDeclaracionVariables);
        }

        VariableDeclaration variable_declarators(VariableDeclaration beforeDeclaration, Tipo tipoVariables)
        {
            if (peek(","))
            {
                match(",");
          
                string idVariable = direct_variable_declarator();
                Tipo tipoVariable = variable_array(tipoVariables);
                Initializers init = variable_initializer();

                entornoTipos.put(idVariable, tipoVariable);//tablasimbolos

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

        Initializers const_variable_initializer()
        {            
            match("=");
            return initializer();            
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
                Expr expresion = OR_expr();
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
            EnvTypes savedEnvTypes = entornoTipos;
            entornoTipos = new EnvTypes(entornoTipos);

            EnvValues savedEnvValues = entornoValores;
            entornoValores = new EnvValues(entornoValores);

            match("(");
            Dictionary<string,Tipo> paramsTypeList = parameter_type_list();
            match(")");            

            FunctionDefinition funcDefinition = new FunctionDefinition();
            funcionActual = funcDefinition;
            
            Sentence compoundStmnt = function_definition();
            
            funcDefinition.init(id, retorno, compoundStmnt);
            entornoTipos = savedEnvTypes;
            entornoValores = savedEnvValues;

            Funcion funcion = new Funcion(retorno, paramsTypeList);
            entornoTipos.put(id, funcion);

            ValorFuncion funcionVal = new ValorFuncion(funcDefinition);
            entornoValores.put(id, funcionVal);

            funcionActual = null;

            if (id == "main")
                main = funcDefinition;

            return funcDefinition;
        }

        #region parameters_type_list

        Dictionary<string, Tipo> parameter_type_list()
        {
            bool isConstant = declaration_specifer();
            Tipo tipo = parameter_type();            

            if (tipo != null)
            {
                Dictionary<string, Tipo> paramsList = new Dictionary<string, Tipo>();

                bool isReference = reference();
                string id = parameter_name();
                
                tipo.isConstant = isConstant;
                tipo.isReference = isReference;

                Tipo tipoParam = tipo;
                if (!(tipo is Registro) && !(tipo is Enumeracion))
                {
                    tipoParam = variable_array(tipo);
                    if (tipoParam is Arreglo)
                        tipoParam.isReference = true;
                }
                
                entornoTipos.put(id, tipoParam);
                paramsList.Add(id,tipoParam);

                //entornoValores.put(id, null);
                
                parameter_type_listP(paramsList);
                return paramsList;
            }
            else
                return new Dictionary<string, Tipo>();
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
                    string strId = struct_declarator();
                    Tipo varRecord = entornoTipos.get(strId);
                    return varRecord;

                case TokenType.ENUM:
                    string enumName = enum_declarator();
                    Tipo varEnum = entornoTipos.get(enumName);
                    return varEnum;
                    //throw new NotImplementedException("Falta implementar strcuts y enums como parametros");

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

        string parameter_name()
        {
            string parameterid = direct_variable_declarator();
            return parameterid;
        }

        void parameter_type_listP(Dictionary<string, Tipo> Parameters)
        {
            if (peek(","))
            {
                match(",");

                bool isConstant = declaration_specifer();
                Tipo actualType = parameter_type();
                bool isReference = reference();
                
                string id = parameter_name();

                actualType.isConstant = isConstant;
                actualType.isReference = isReference;
                
                Tipo actualParameter = actualType;
                if (!(actualType is Registro) && !(actualType is Enumeracion))
                {
                    actualParameter = variable_array(actualType);
                    if (actualParameter is Arreglo)
                        actualParameter.isReference = true;
                }

                entornoTipos.put(id, actualParameter);
                Parameters.Add(id,actualParameter);

                //entornoValores.put(id, null);
                
                parameter_type_listP(Parameters);
            }            
            //null   
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
                case TokenType.SEMICOLON:
                case TokenType.CIN:
                case TokenType.COUT:

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
                    
                    Tipo varRecord = entornoTipos.get(strId);
                    Valor valRecord = entornoValores.get(strId);

                    string strVarName = currentToken.Lexema;
                    match(TokenType.ID);
                    entornoTipos.put(strVarName, varRecord);

                    StructVariableDeclaration strVarDec = new StructVariableDeclaration(strId, strVarName, varRecord, valRecord);
                    match(";");
                    return strVarDec;

                case TokenType.ENUM:
                    Sentence sentence = enum_declaration();
                    match(";");
                    return sentence;
                    /*string enumName = enum_declarator();
                    Tipo varEnum = entornoTipos.get(enumName);
                    Valor valEnum = entornoValores.get(enumName);

                    string enumVarName = currentToken.Lexema;
                    match(TokenType.ID);

                    EnumerationVariableDeclaration enumVarDeclaration = new EnumerationVariableDeclaration(enumName, enumVarName, varEnum, valEnum);
                    entornoTipos.put(enumVarName, varEnum);
                    return enumVarDeclaration;*/

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

                case TokenType.CIN:
                    Sentence sCin = cin();
                    match(";");
                    return sCin;

                case TokenType.COUT:
                    Sentence sCout = cout();
                    match(";");
                    return sCout;

                /*case TokenType.SEMICOLON:
                    Sentence empty = new EmptySentence();
                    match(";");
                    return empty;*/
                
                default:
                    //return null;
                    Sentence empty = new EmptySentence();
                    match(";");
                    return empty;
            }
            //null
        }

        #region declarationStatement

        Sentence declaration_statement()
        {
            Tipo tipoVariables = variable_type();//tipo basico para todas las variables, si hay mas
            
            string idVariable = direct_variable_declarator();
            Tipo tipoVariable = variable_array(tipoVariables);
            
            VariableSubDeclarator primerVariable = new VariableSubDeclarator(tipoVariable, idVariable);

            VariableDeclarator primerDeclaracionVariable = new VariableDeclarator(primerVariable, null);//todavia no hemos visto inicializadores

            VariableDeclarations variableDeclarations = variable_declarator(primerDeclaracionVariable, tipoVariables);//para las variables que siguen            

            return variableDeclarations;
        }

        #endregion

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
            EnvTypes savedEnvTypes = entornoTipos;
            entornoTipos = new EnvTypes(entornoTipos);
            
            EnvValues savedEnvValues = entornoValores;
            entornoValores = new EnvValues(entornoValores);

            match("if");
            match("(");
            Expr expresion = expr();
            match(")");
            Sentence trueBlock = if_compound_statement();

            entornoTipos = savedEnvTypes;
            entornoValores = savedEnvValues;
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
                return if_statement();
            }
            else
            {
                EnvTypes savedEnvTypes = entornoTipos;
                entornoTipos = new EnvTypes(entornoTipos);

                EnvValues savedEnvValues = entornoValores;
                entornoValores = new EnvValues(entornoValores);

                Sentence falseBLock = if_compound_statement();
                
                entornoTipos = savedEnvTypes;
                entornoValores = savedEnvValues;
                return falseBLock;
            }
        }

        #endregion

        #region return, continue, break statements

        Sentence return_statement()
        {
            match("return");
            Expr expresion = return_statementP();

            ReturnStatement returnStmnt = new ReturnStatement(expresion);
            //funcionActual = null;
            return returnStmnt;
        }

        Expr return_statementP()
        {
            switch (currentToken.Tipo)
            {
                case TokenType.INCREMENT:
                case TokenType.DECREMENT:
                case TokenType.ID:
                case TokenType.REAL_LITERAL:
                case TokenType.INTEGER_LITERAL:
                case TokenType.LEFT_PARENTHESIS:
                    Expr expresion = expr();
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
            EnvTypes savedEnvTypes = entornoTipos;
            entornoTipos = new EnvTypes(entornoTipos);

            EnvValues savedEnvValues = entornoValores;
            entornoValores = new EnvValues(entornoValores);

            WhileStatement whileStmnt = new WhileStatement();
            Sentence cicloAnterior = cicloActual;
            cicloActual = whileStmnt;

            match("while");
            match("(");
            Expr expresion = expr();
            match(")");
            Sentence ifCpStmnt = if_compound_statement();
            
            whileStmnt.WhileInit(expresion, ifCpStmnt);
            cicloActual = cicloAnterior;

            entornoTipos = savedEnvTypes;
            entornoValores = savedEnvValues;
            return whileStmnt;
        }

        #endregion

        #region do while

        Sentence do_while()
        {
            EnvTypes savedEnvTypes = entornoTipos;
            entornoTipos = new EnvTypes(entornoTipos);

            EnvValues savedEnvValues = entornoValores;
            entornoValores = new EnvValues(entornoValores);

            DoWhileStatement doWhileStmnt = new DoWhileStatement();
            Sentence cicloAnterior = cicloActual;
            cicloActual = doWhileStmnt;

            match("do");
            Sentence stmnt = compound_statement();
            match("while");
            match("(");
            Expr expresion = expr();
            match(")");
            match(";");

            doWhileStmnt.DoWhileInit(expresion, stmnt);
            cicloActual = cicloAnterior;

            entornoTipos = savedEnvTypes;
            entornoValores = savedEnvValues;
            return doWhileStmnt;
        }

        #endregion

        #region for_statement

        Sentence for_statement()
        {
            EnvTypes savedEnvTypes = entornoTipos;
            entornoTipos = new EnvTypes(entornoTipos);

            EnvValues savedEnvValues = entornoValores;
            entornoValores = new EnvValues(entornoValores);

            ForStatement forStmnt = new ForStatement();
            Sentence cicloAnterior = cicloActual;
            cicloActual = forStmnt;

            match("for");
            match("(");
            Sentence forInitialization = for_initialization(); 
            match(";");
            Sentence forControl = for_control(); 
            match(";");
            Sentence forIteration = for_iteration();
            match(")");

            Sentence compoundStatement = if_compound_statement();

            forStmnt.ForInit(forInitialization, forControl, forIteration, compoundStatement);
            cicloActual = cicloAnterior;

            entornoTipos = savedEnvTypes;
            entornoValores = savedEnvValues;
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
            
            Tipo tipoVariables = variable_type();
            tipoVariables.isConstant = true;

            string idVariable = direct_variable_declarator();
            Tipo tipoVariable = variable_array(tipoVariables);
            
            Initializers const_init = const_variable_initializer();

            VariableSubDeclarator primerVariable = new VariableSubDeclarator(tipoVariable, idVariable);
            VariableDeclarator constDeclaracionVariable = new VariableDeclarator(primerVariable, const_init);
            match(";");
            entornoTipos.put(idVariable, tipoVariable);
            return constDeclaracionVariable;
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
                EnvTypes savedEnvTypes = entornoTipos;
                entornoTipos = new EnvTypes(entornoTipos);

                EnvValues savedEnvValues = entornoValores;
                entornoValores = new EnvValues(null);

                match("{");
                List<VariableDeclarator> varsDec = enum_initializer_list(savedEnvValues, savedEnvTypes);
                match("}");

                EnumerationDeclaration enumDeclaration = new EnumerationDeclaration(enumName, varsDec, savedEnvValues);
                ValorEnumeracion valEnum = new ValorEnumeracion();
                Enumeracion varEnum = new Enumeracion(entornoTipos);
                valEnum.valor = entornoValores;

                entornoTipos = savedEnvTypes;
                entornoTipos.put(enumName, varEnum);

                entornoValores = savedEnvValues;
                entornoValores.put(enumName, valEnum);
                return enumDeclaration;
            }
            else
            {
                Tipo varEnum = entornoTipos.get(enumName);
                Valor valEnum = entornoValores.get(enumName);

                string enumVarName = currentToken.Lexema;
                match(TokenType.ID);

                EnumerationVariableDeclaration enumVarDeclaration = new EnumerationVariableDeclaration(enumName, enumVarName, varEnum, valEnum);
                entornoTipos.put(enumVarName, varEnum);
                return enumVarDeclaration;
            }
        }

        List<VariableDeclarator> enum_initializer_list(EnvValues savedEnvValues, EnvTypes savedEnvTypes)
        {
            List<VariableDeclarator> varDecList = new List<VariableDeclarator>();
            VariableDeclarator varDec = enum_constant_expression(savedEnvValues, savedEnvTypes);
            varDecList.Add(varDec);
            enum_initializer_listP(varDecList, savedEnvValues, savedEnvTypes);
            return varDecList;
        }

        void enum_initializer_listP(List<VariableDeclarator> vars, EnvValues savedEnvValues, EnvTypes savedEnvTypes)
        {
            if (peek(","))
            {
                match(",");
                VariableDeclarator varDec = enum_constant_expression(savedEnvValues, savedEnvTypes);
                vars.Add(varDec);
                enum_initializer_listP(vars, savedEnvValues, savedEnvTypes);
            }
            //null
        }

        VariableDeclarator enum_constant_expression(EnvValues savedEnvValues, EnvTypes savedEnvTypes)
        {
            string varName = variable_name();

            EnvValues savedEnvValues2 = entornoValores;
            entornoValores = savedEnvValues;
            VariableInitializer varInit = enum_constant_expressionP();
            entornoValores = savedEnvValues2;

            VariableSubDeclarator varSubDec = new VariableSubDeclarator(new Entero(), varName);

            entornoTipos.put(varName, new Entero());
            savedEnvTypes.put(varName, new Entero());
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
                EnvTypes savedEnvTypes = entornoTipos;
                entornoTipos = new EnvTypes(null);

                EnvValues savedEnvValues = entornoValores;
                entornoValores = new EnvValues(null);

                Sentence strVarDecs = variable_declaration_list(savedEnvTypes, savedEnvValues);
                match("}");

                Registro record = new Registro(entornoTipos);
                ValorRegistro Valrec = new ValorRegistro();
                Valrec.valor = entornoValores;

                entornoTipos = savedEnvTypes;
                entornoTipos.put(structName, record);

                entornoValores = savedEnvValues;                
                entornoValores.put(structName, Valrec);

                return strVarDecs;
            }
            else
            {
                Tipo varRecord = entornoTipos.get(structName);
                Valor valRecord = entornoValores.get(structName);

                string strVarName = variable_name();

                entornoTipos.put(strVarName, varRecord);
                //entornoValores.put(strVarName, valRecord);
                return new StructVariableDeclaration(structName, strVarName, varRecord, valRecord);
            }
        }

        Sentence variable_declaration_list(EnvTypes savedEnvTypes, EnvValues savedEnvValues)
        {
            switch (currentToken.Tipo)
            {
                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:
                    Tipo tipoVariables = variable_type();
                    string idVariable = direct_variable_declarator();
                    Tipo tipoVariable = variable_array(tipoVariables);
                    match(";");

                    VariableSubDeclarator primerVariable = new VariableSubDeclarator(tipoVariable, idVariable);
                    VariableDeclarator primerDeclaracionVariable = new VariableDeclarator(primerVariable, null);
                    
                    entornoTipos.put(idVariable, tipoVariable);
                    return variable_declaration_listP(primerDeclaracionVariable, savedEnvTypes, savedEnvValues);
                    
                case TokenType.STRUCT:
                    string structName = struct_declarator();
                    
                    Tipo varRecord = savedEnvTypes.get(structName);
                    Valor valRecord = savedEnvValues.get(structName);

                    string structVarName = variable_name();
                    match(";");
                    
                    StructVariableDeclaration strVarDec = new StructVariableDeclaration(structName, structVarName, varRecord,valRecord);

                    entornoTipos.put(structVarName, varRecord);
                    return variable_declaration_listP(strVarDec, savedEnvTypes, savedEnvValues);
                
                case TokenType.ENUM:
                    string enumName = enum_declarator();
                    Tipo varEnum = savedEnvTypes.get(enumName);
                    Valor valEnum = savedEnvValues.get(enumName);

                    string enumVarName = currentToken.Lexema;
                    match(TokenType.ID);

                    entornoTipos.put(enumName, varEnum);
                    EnumerationVariableDeclaration enumVarDec = new EnumerationVariableDeclaration(enumName, enumVarName, varEnum, valEnum);

                    return variable_declaration_listP(enumVarDec, savedEnvTypes, savedEnvValues);

                default:
                    throw new Exception("Error en la declaracion de variables de struct linea: " + Lexer.line + " columna: " + Lexer.column + " currenttoken = " + currentToken.Lexema);
            }
        }

        Sentence variable_declaration_listP(Sentence primerDeclaracionVariable, EnvTypes savedEnvTypes, EnvValues savedEnvValues)
        {            
            switch (currentToken.Tipo)
            {
                case TokenType.STRING:
                case TokenType.BOOL:
                case TokenType.CHAR:
                case TokenType.FLOAT:
                case TokenType.INT:
                    Tipo tipoVariables = variable_type();
                    string idVariable = direct_variable_declarator();
                    Tipo tipoVariable = variable_array(tipoVariables);
                    match(";");

                    VariableSubDeclarator segundaVariable = new VariableSubDeclarator(tipoVariable, idVariable);
                    VariableDeclarator segundaDeclaracionVariable = new VariableDeclarator(segundaVariable, null);                                        
                    
                    SentenceSenquence stSeq = new SentenceSenquence(primerDeclaracionVariable, segundaDeclaracionVariable);
                    entornoTipos.put(idVariable, tipoVariable);
                    return variable_declaration_listP(stSeq, savedEnvTypes, savedEnvValues);
                
                case TokenType.STRUCT:
                    string structName = struct_declarator();
                    
                    Tipo varRecord = savedEnvTypes.get(structName);
                    Valor valRecord = savedEnvValues.get(structName);
                    
                    string structVarName = variable_name();
                    match(";");
                    
                    StructVariableDeclaration strVarDec = new StructVariableDeclaration(structName, structVarName, varRecord, valRecord);
                    
                    SentenceSenquence stSeq2 = new SentenceSenquence(primerDeclaracionVariable, strVarDec);
                    entornoTipos.put(structVarName, varRecord);
                    return variable_declaration_listP(stSeq2, savedEnvTypes, savedEnvValues);

                case TokenType.ENUM:
                    string enumName = enum_declarator();
                    Tipo varEnum = savedEnvTypes.get(enumName);
                    Valor valEnum = savedEnvValues.get(enumName);

                    string enumVarName = currentToken.Lexema;
                    match(TokenType.ID);

                    entornoTipos.put(enumName, varEnum);
                    EnumerationVariableDeclaration enumVarDec = new EnumerationVariableDeclaration(enumName, enumVarName, varEnum, valEnum);
                    SentenceSenquence stSeq3 = new SentenceSenquence(primerDeclaracionVariable, enumVarDec);
                    return variable_declaration_listP(stSeq3, savedEnvTypes, savedEnvValues);

                default:
                    return primerDeclaracionVariable;//null
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

        #region cin/cout

        Sentence cin()
        {
            List<Expr> exprs = new List<Expr>();

            match("cin"); 
            match(">>"); 
            Expr e = expr();
            exprs.Add(e);
            cinP(exprs);
            return new ConsoleIn(exprs);
        }

        void cinP(List<Expr> exprs)
        {
            if (peek(">>"))
            {
                match(">>"); 
                Expr e = expr();
                exprs.Add(e);
                cinP(exprs);
            }
            //null
        }

        Sentence cout()
        {
            List<Expr> exprs = new List<Expr>();

            match("cout");
            match("<<");
            Expr e = expr();
            exprs.Add(e);

            bool endl = coutP(exprs);
            ConsoleOut ConsOut = new ConsoleOut(exprs);
            ConsOut.endl = endl;
            return ConsOut;
        }

        bool coutP(List<Expr> exprs)
        {
            if (peek("<<"))
            {
                match("<<");

                if (peek("endl"))
                {
                    match("endl");
                    return true;
                }
                else
                {
                    Expr e = expr();
                    exprs.Add(e);
                    return coutP(exprs);                    
                }
            }
            return false;
            //null
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
                    //List<ReferenceAccess> membersList = new List<ReferenceAccess>();
                    MiembroRegistro miembroRegistro = (MiembroRegistro)id_access(id);
                    return miembroRegistro;

                    //MiembroRegistro miembroRegistro = new MiembroRegistro(id.lexeme, membersList);
                    //return miembroRegistro;

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

        //void id_access(List<ReferenceAccess> memberslist)
        ReferenceAccess id_access(ReferenceAccess id)
        {
            match(".");
            ReferenceAccess reference = direct_variable_declaratorExpr();
            return id_accessP(id,reference);
        }

        ReferenceAccess id_accessP(ReferenceAccess record, ReferenceAccess member)
        {
            if (peek("."))
            {
                match(".");
                ReferenceAccess reference = direct_variable_declaratorExpr();
                ReferenceAccess refAcc = id_accessP(member, reference);
                return new MiembroRegistro(record.lexeme, refAcc);
            }
            else
                return new MiembroRegistro(record.lexeme, member);
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
            if (!peek(")") )
            {
                parameters.Add(OR_expr()); parameter_listP(parameters);
            }
            //null
        }

        void parameter_listP(List<Expr> parameters)
        {
            if (peek(","))
            {
                match(","); parameters.Add(OR_expr()); parameter_listP(parameters);
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
