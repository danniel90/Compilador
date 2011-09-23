using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Lexical;
//using Semantic;
using Syntax;
using Environment;

namespace SyntaxTree
{ 

    #region program

    public class Node
    {
        public int lexline = -1;

        public Node()
        {
            lexline = Lexer.line;            
        }
    }

    #region Sentence    

    public abstract class Sentence : Node
    {
        public void print()
        {
            Console.WriteLine(genCode());
        }

        public abstract string genCode();

        public abstract void validarSemantica();

        protected Exception ErrorMessage(string message)
        {
            throw new Exception(message + " Linea:" + lexline);
        }
    }

    public class Program : Sentence
    {
        public Sentence main;

        public Sentence sentences;

        public Program() { }

        public void ProgramInit(Sentence sents)
        {
            sentences = sents;
        }

        public override string genCode()
        {
            return sentences.genCode();
        }

        public override void validarSemantica()
        {
            if (main == null)
                throw ErrorMessage("No hay funcion \"main\".");
            else
                if (!(((FunctionDefinition)main).retorno is Entero))
                    throw ErrorMessage("Retorno ew la funcion main deberia ser de tipo int");
            
            sentences.validarSemantica();
            main.validarSemantica();
        }
    }

    public class SentenceSenquence : Sentence
    {
        public Sentence sent1, sent2;

        public SentenceSenquence(Sentence s1, Sentence s2)
        {
            sent1 = s1;
            sent2 = s2;
        }

        public override string genCode()
        {
            return sent1.genCode() + ", " + sent2.genCode() + "\n";
        }

        public override void validarSemantica()
        {
            sent1.validarSemantica();
            sent2.validarSemantica();
        }
    }

    #region VariableDeclaration

    public class VariableDeclarations : Sentence
    {
        public VariableDeclaration variableDeclarations;

        public VariableDeclarations(VariableDeclaration VarDeclarations)
        {
            variableDeclarations = VarDeclarations;
        }

        public override string genCode()
        {
            return variableDeclarations.genCode();
        }

        public override void validarSemantica()
        {
            variableDeclarations.validarSemantica();
        }
    }

    public abstract class VariableDeclaration : Sentence { }

    public class VariableDeclarators : VariableDeclaration
    {
        public VariableDeclaration varDeclarator1, varDeclarator2;

        public VariableDeclarators(VariableDeclaration vDec1, VariableDeclaration vDec2)
        {
            varDeclarator1 = vDec1;
            varDeclarator2 = vDec2;
        }

        public override string genCode()
        {
            return varDeclarator1.genCode() + ", " + varDeclarator2.genCode() + "\n";
        }

        public override void validarSemantica()
        {
            varDeclarator1.validarSemantica();
            varDeclarator2.validarSemantica();
        }
    }

    public class VariableDeclarator : VariableDeclaration
    {
        public VariableSubDeclarator declaration;
        public Initializers initialization;

        public VariableDeclarator(VariableSubDeclarator dec, Initializers init)
        {
            declaration = dec;
            initialization = init;
        }

        public override string genCode()
        {
            string initString = "";
            
            if (initialization != null)
                initString = " = " + initialization.genCode();

            return "VariableSubDeclarator" + initString;
        }

        public override void validarSemantica()
        {
            if (initialization != null)
            {
                if (!declaration.tipo.esEquivalente(initialization.validarSemantico()))
                    throw ErrorMessage("La inicializacion de la variable es incorrecta.");
            }
        }
    }
    
    public class VariableSubDeclarator
    {
        public Tipo tipo;
        public string id;

        public VariableSubDeclarator(Tipo typevar, string idvar)
        {
            tipo = typevar;
            id = idvar;
        }
    }

    #region initializers

    public abstract class Initializers : Expr
    {        
    }

    public class VariableInitializer : Initializers
    {
        Expr Expresion;

        public VariableInitializer(Expr expresion)
        {
            Expresion = expresion;
        }        

        public override Tipo validarSemantico()
        {
            return Expresion.validarSemantico();
        }

        public override string genCode()
        {
            return "initializer";
        }
    }

    public class VariableInitializerList : Initializers
    {
        List<Initializers> initializerList;

        public VariableInitializerList(List<Initializers> initializersList)
        {
            initializerList = initializersList;
        }

        public override Tipo validarSemantico()
        {
            Tipo t0 = validarSemanticoHelper();

            for (int x = 1; x < initializerList.Count; x++)
            {
                Tipo t = initializerList[x].validarSemantico();

                if (!t0.esEquivalente(t))
                    throw ErrorMessage("Inicializacion incorrecta de arreglo. t0:" + t0.ToString() + " t" + x +":" + t.ToString());
            }
            
            return new Arreglo(t0, initializerList.Count);
        }

        public Tipo validarSemanticoHelper()
        {
            Tipo tipoArreglo;
            Initializers tinit = initializerList[0];
            if (tinit is VariableInitializer)
            {
                VariableInitializer vInit = (VariableInitializer)initializerList[0];

                tipoArreglo = vInit.validarSemantico();

                return tipoArreglo;
            }
            else if (tinit is VariableInitializerList)
            {
                VariableInitializerList vList = (VariableInitializerList)initializerList[0];

                tipoArreglo = vList.validarSemanticoHelper();

                return new Arreglo(tipoArreglo, vList.initializerList.Count);
            }
            else
                throw ErrorMessage("Error en la inicializacion, tipo distinto de initializer??.");
        }

        public override string genCode()
        {
            return "{ initializer }";
        }
    }

    /*public class VariableInitializerList : Initializers
    {
        List<Initializers> initializerList;

        public VariableInitializerList(List<Initializers> initializersList)
        {
            initializerList = initializersList;
        }

        public override Tipo validarSemantico()
        {
            Tipo t0 = validarSemanticoHelper();

            for (int x = 1; x < initializerList.Count; x++)
            {
                Tipo t = initializerList[x].validarSemantico();

                if (!t0.esEquivalente(t))
                    throw new Exception("Inicializacion incorrecta de arreglo.");
            }

            return t0;
        }

        public Tipo validarSemanticoHelper()
        {
            Tipo tipoArreglo;

            if (initializerList[0] is VariableInitializer)
            {
                VariableInitializer vInit = (VariableInitializer)initializerList[0];

                tipoArreglo = vInit.validarSemantico();

                return tipoArreglo;
            }
            else if (initializerList[0] is VariableInitializerList)
            {
                VariableInitializerList vList = (VariableInitializerList)initializerList[0];

                tipoArreglo = vList.validarSemantico();

                return new Arreglo(tipoArreglo, vList.initializerList.Count);
            }
            else
                throw new Exception("Error en la inicializacion, tipo distinto de initializer??.");
        }

        public override string genCode()
        {
            return "{ initializer }";
        }
    }*/

    #endregion

    #endregion

    #region FunctionDefinition

    public class FunctionDefinition : Sentence
    {
        public Env entornoLocal;
        public string idFuncion;
        public Tipo retorno;
        public Sentence compoundStatement;        

        public FunctionDefinition(string nombreFuncion, Tipo ret, Sentence cpStmnt)
        {
            idFuncion = nombreFuncion;
            retorno = ret;
            compoundStatement = cpStmnt;
            entornoLocal = Parser.entorno;
        }

        public FunctionDefinition() { }

        public void init(string nombreFuncion, Tipo ret, Sentence cpStmnt)
        {
            idFuncion = nombreFuncion;
            retorno = ret;
            compoundStatement = cpStmnt;
            entornoLocal = Parser.entorno;
        }

        public override string genCode()
        {
            return "FunctionDefinition :" + idFuncion + "\n"+ compoundStatement.genCode();
        }

        public override void validarSemantica()
        {
            compoundStatement.validarSemantica();
        }
    }

    #endregion

    #endregion

    #region Statement

    public abstract class Statement : Sentence { }

    public class StatementSequence : Statement
    {
        public Sentence stmt1, stmt2;

        public StatementSequence(Sentence s1, Sentence s2)
        {
            stmt1 = s1;
            stmt2 = s2;
        }

        public override string genCode()
        {
            return stmt1.genCode() + ", " + stmt2.genCode() + "\n";
        }

        public override void validarSemantica()
        {
            stmt1.validarSemantica();
            stmt2.validarSemantica();
        }
    }

    public class CompoundStatement : Statement
    {
        public Sentence Sentencias;

        public CompoundStatement(Sentence listaSentencias)
        {
            Sentencias = listaSentencias;
        }

        public override string genCode()
        {
            return Sentencias.genCode();
        }

        public override void validarSemantica()
        {
            Sentencias.validarSemantica();
        }
    }

    public class ExpressionStatement : Statement
    {
        Expr expresion;

        public ExpressionStatement(Expr expr)
        {
            expresion = expr;
        }

        public override string genCode()
        {
            return "ExpressionStatement";
        }

        public override void validarSemantica()
        {
            expresion.validarSemantico();
        } 
    }

    public class IfStatement : Statement
    {
        public Expr condicion;
        public Sentence BloqueVerdadero;

        public IfStatement(Expr cond, Sentence bloqueTrue)
        {
            condicion = cond;
            BloqueVerdadero = bloqueTrue;
        }

        public override string genCode()
        {
            return "IfStatement";
        }

        public override void  validarSemantica()
        {
            /*Tipo t = condicion.validarSemantico();
            if (t is Cadena || t is Caracter || t is Registro || t is Enumeracion)
                throw ErrorMessage("La condicion del if deberia ser de tipo booleano/numerico. Tipo:" + t.ToString());*/

            BloqueVerdadero.validarSemantica();
        }
    }

    public class IfElseStatement : Statement
    {
        public Expr condicion;
        public Sentence BloqueVerdadero;
        public Sentence BloqueFalso;

        public IfElseStatement(Expr cond, Sentence bloqueTrue, Sentence bloqueFalse)
        {
            condicion = cond;
            BloqueVerdadero = bloqueTrue;
            BloqueFalso = bloqueFalse;
        }

        public override string genCode()
        {
            return "IfElseStatement";
        }

        public override void validarSemantica()
        {
            /*Tipo t = condicion.validarSemantico();
            if (t is Cadena || t is Caracter || t is Registro || t is Enumeracion)
                throw ErrorMessage("La condicion del if deberia ser de tipo booleano/numerico. Tipo:" + t.ToString());*/

            BloqueVerdadero.validarSemantica();
            BloqueFalso.validarSemantica();
        }
    }

    public class DoWhileStatement : Statement
    {
        public Expr expresion;
        public Sentence compoundStatement;

        public DoWhileStatement() { }

        public DoWhileStatement(Expr expr, Sentence cpStmnt)
        {
            expresion = expr;
            compoundStatement = cpStmnt;
        }

        public void DoWhileInit(Expr expr, Sentence cpStmnt)
        {
            expresion = expr;
            compoundStatement = cpStmnt;
        }

        public override string genCode()
        {
            return "DoWhileStatement";
        }

        public override void validarSemantica()
        {
            Tipo t = expresion.validarSemantico();

            if (!(t is Entero || t is Flotante || t is Booleano))
                throw ErrorMessage("La condicion del if deberia ser de tipo booleano/numerico.");
            compoundStatement.validarSemantica();
        }
    }

    public class WhileStatement : Statement
    {
        public Expr expresion;
        public Sentence compoundstatement;

        public WhileStatement() { }

        public WhileStatement(Expr expr, Sentence cpStmnt)
        {
            expresion = expr;
            compoundstatement = cpStmnt;
        }

        public void WhileInit(Expr expr, Sentence cpStmnt)
        {
            expresion = expr;
            compoundstatement = cpStmnt;
        }

        public override string genCode()
        {
            return "WhileStatement";
        }

        public override void validarSemantica()
        {
            Tipo t = expresion.validarSemantico();

            if (!(t is Entero || t is Flotante || t is Booleano))
                throw ErrorMessage("La condicion del if deberia ser de tipo booleano/numerico.");
            compoundstatement.validarSemantica();
        }
    }

    public class ForStatement : Statement
    {
        public Sentence forInitialization, forControl, forIteration, CompoundStatement;

        public ForStatement() { }

        public ForStatement(Sentence forInit, Sentence forCtrl, Sentence forIter, Sentence cpStmnt)
        {
            forInitialization = forInit;
            forControl = forCtrl;
            forIteration = forIter;
            CompoundStatement = cpStmnt;
        }

        public void ForInit(Sentence forInit, Sentence forCtrl, Sentence forIter, Sentence cpStmnt)
        {
            forInitialization = forInit;
            forControl = forCtrl;
            forIteration = forIter;
            CompoundStatement = cpStmnt;
        }

        public override string genCode()
        {
            return "ForStatement";
        }

        public override void validarSemantica()
        {
            forInitialization.validarSemantica();
            forControl.validarSemantica();
            forIteration.validarSemantica();

            CompoundStatement.validarSemantica();
        }
    }   

    public class ContinueStatement : Statement
    {
        public Statement stmt;
        public Sentence enclosing;

        public ContinueStatement() 
        {
            enclosing = Parser.cicloActual;
        }

        public override string genCode()
        {
            return "ContinueStatement";
        }

        public override void validarSemantica()
        {
            if (enclosing == null)
                throw ErrorMessage("ContinueStatement sin ciclo."); 
        }
    }

    public class BreakStatement : Statement
    {
        public Statement stmt;
        public Sentence enclosing;

        public BreakStatement()
        {
            enclosing = Parser.cicloActual;
        }

        public override string genCode()
        {
            return "BreakStatement";
        }

        public override void validarSemantica()
        {
            if (enclosing == null)
                throw ErrorMessage("BreakStatement sin ciclo."); 
        }
    }

    public class ReturnStatement : Statement
    {
        public Sentence expresion;
        public Sentence enclosing;

        public ReturnStatement(Sentence expr)
        {            
            expresion = expr;
            enclosing = Parser.funcionActual;
        }

        public override string genCode()
        {
            return "ReturnStatement";
        }

        public override void validarSemantica()
        {
            if (expresion != null)
                expresion.validarSemantica();

            if (enclosing == null)
                throw ErrorMessage("Return inalcanzable o sin funcion."); 
        }
    }

    #endregion

    #region StructDeclaration

    public class StructVariableDeclaration : Sentence
    {
        public string strId, strVarId;
        public Tipo tipo;

        public StructVariableDeclaration(string strid,string strvarname, Tipo tipostr)
        {
            strId = strid;
            strVarId = strvarname;
            tipo = tipostr;
        }

        public override string genCode()
        {
            return "StructVariableDeclaration";
        }

        public override void validarSemantica()
        {
            if (!(tipo is Registro))
                throw ErrorMessage("Tipo deberia ser de struct! Tipo:" + tipo.ToString() + " id:" + strId + ".");
        }
    }

    public class StructDeclaration : Sentence
    {
        public StructDeclaration() { }

        public override string genCode()
        {
            return "StructDeclaration";
        }

        public override void validarSemantica()
        {
            
        }
    }    

    #endregion

    #region enums

    public class EnumerationDeclaration : Sentence
    {
        public string enumId;
        public List<VariableDeclarator> variables;
        public Env entornoEnum;

        public EnumerationDeclaration(string id, List<VariableDeclarator> vars, Env entorno)
        {
            enumId = id;
            variables = vars;
            entornoEnum = entorno;
        }

        public override string genCode()
        {
            return "EnumerationDeclaration";
        }

        public override void validarSemantica()
        {
            
        }
    }

    public class EnumerationVariableDeclaration : Sentence
    {
        public string enumerationName, enumerationVarName;
        public Tipo tipo;

        public EnumerationVariableDeclaration(string enumName, string enumVarName, Tipo tipoenum)
        {
            enumerationName = enumName;
            enumerationVarName = enumVarName;
            tipo = tipoenum;
        }

        public override string genCode()
        {
            return "EnumerationVariableDeclaration";
        }

        public override void validarSemantica()
        {
            if (!(tipo is Enumeracion))
                throw ErrorMessage("Tipo deberia ser de enum! Tipo:" + tipo.ToString() + " id:" + enumerationName + ".");
        }
    }

    #endregion

    #endregion

    #region expresiones

    public class Expr : Node
    {

        public Env entornoActual;

        public Expr()
        {
            entornoActual = Parser.entorno;
        }

        public void print()
        {
            Console.WriteLine(genCode());
        }

        public virtual string genCode()
        {
            throw ErrorMessage("Expr genCode()");
        }

        public virtual Tipo validarSemantico()
        {
            throw ErrorMessage("Expr validarSemantico()");            
        }

        protected Exception ErrorMessage(string message)
        {
            throw new Exception(message + " Linea:" + lexline);
        }
    }

    #region SequenceExpr

    public class SequenceExpr : Expr
    {
        public Expr  expr1, expr2;

        public SequenceExpr(Expr exp1, Expr exp2)
        {
            expr1 = exp1;
            expr2 = exp2;
        }

        public override string genCode()
        {
            return "SequenceExpr";
        }

        public override Tipo validarSemantico()
        {
            expr1.validarSemantico();
            return expr2.validarSemantico();            
        }
    }

    #endregion

    #region AssignExpr

    public class AssignExpr : Expr
    {
        public Expr Id, value;

        public AssignExpr(Expr id, Expr valor)
        {
            Id = id;
            value = valor;
        }

        public override string genCode()
        {
            return "AssignExpr";
        }

        public override Tipo validarSemantico()
        {
            Tipo t = Id.validarSemantico();
            if (t.esEquivalente(value.validarSemantico()))
                return t;
            
            throw ErrorMessage("Los tipos de variable y de valor/expresion no son equivalentes.");
        }
    }

    public class AdditionAssignExpr : AssignExpr
    {
        public AdditionAssignExpr(Expr id, Expr valor) : base(id, valor) { }

        public override string genCode()
        {
            return "AdditionAssignExpr";
        }

        public override Tipo validarSemantico()
        {
            Tipo t = Id.validarSemantico();
            if (t.esEquivalente(value.validarSemantico()))
                return t;
            
            throw ErrorMessage("Los tipos de variable y de valor/expresion no son equivalentes.");
        }
    }

    public class SubstractionAssignExpr : AssignExpr
    {
        public SubstractionAssignExpr(Expr id, Expr valor) : base(id, valor) { }

        public override string genCode()
        {
            return "SubstractionAssignExpr";
        }

        public override Tipo validarSemantico()
        {
            Tipo t = Id.validarSemantico();
            if (t.esEquivalente(value.validarSemantico()))
                return t;
            
            throw ErrorMessage("Los tipos de variable y de valor/expresion no son equivalentes.");
        }
    }

    public class MultiplicationAssignExpr : AssignExpr
    {
        public MultiplicationAssignExpr(Expr id, Expr valor) : base(id, valor) { }

        public override string genCode()
        {
            return "MultiplicationAssignExpr";
        }

        public override Tipo validarSemantico()
        {
            Tipo t = Id.validarSemantico();
            if (t.esEquivalente(value.validarSemantico()))
                return t;            
            
            throw ErrorMessage("Los tipos de variable y de valor/expresion no son equivalentes.");
        }
    }

    public class DivisionAssignExpr : AssignExpr
    {
        public DivisionAssignExpr(Expr id, Expr valor) : base (id, valor) { }

        public override string genCode()
        {
            return "DivisionAssignExpr";
        }

        public override Tipo validarSemantico()
        {
            Tipo t = Id.validarSemantico();
            if (t.esEquivalente(value.validarSemantico()))
                return t;
            
            throw ErrorMessage("Los tipos de variable y de valor/expresion no son equivalentes.");
        }
    }

    #endregion

    #region Binary

    public abstract class BinaryExpr : Expr
    {
        public Expr leftExpr, rightExpr;

        public BinaryExpr(Expr left, Expr right)
        {
            leftExpr = left;
            rightExpr = right;
        }

        public override string genCode()
        {
            return "BinaryExpr";
        }
    }

    #region OrExpr

    public class OrExpr : BinaryExpr
    {
        public OrExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "orExpr";
        }

        public override Tipo validarSemantico()
        {
            Tipo t_izq = leftExpr.validarSemantico();
            Tipo t_der = rightExpr.validarSemantico();

            if (t_izq is Booleano && t_der is Booleano)            
                return t_der;

            if (t_izq is Entero && t_der is Entero)
                return t_der;

            if (t_izq is Flotante && t_der is Flotante)
                return t_der;
            
            throw ErrorMessage("Deberian ser tipos booleano/entero/flotante las dos expresiones del OR.");
        }
    }

    #endregion

    #region AndExpr

    public class AndExpr : BinaryExpr
    {
        public AndExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "andExpr";
        }

        public override Tipo validarSemantico()
        {
            Tipo t_izq = leftExpr.validarSemantico();
            Tipo t_der = rightExpr.validarSemantico();

            if (t_izq is Booleano && t_der is Booleano)
                return t_der;

            if (t_izq is Entero && t_der is Entero)
                return t_der;

            if (t_izq is Flotante && t_der is Flotante)
                return t_der;

            throw ErrorMessage("Deberian ser tipos booleano/entero/flotante las dos expresiones del AND.");
        }
    }

    #endregion

    #region EqualExpr

    public class EqualExpr : BinaryExpr
    {
        public EqualExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "equalExpr";
        }

        public override Tipo validarSemantico()
        {
            Tipo t_izq = leftExpr.validarSemantico();
            Tipo t_der = rightExpr.validarSemantico();

            if (t_izq is Booleano && t_der is Booleano)
                return t_der;

            if (t_izq is Entero && t_der is Entero)
                return t_der;

            if (t_izq is Flotante && t_der is Flotante)
                return t_der;

            if (t_izq is Cadena && t_der is Cadena)
                return t_der;

            if (t_der is Caracter && t_der is Caracter)
                return t_der;

            throw ErrorMessage("Deberian ser tipos booleano/entero/flotante/cadena/caracter las dos expresiones del EQUAL.");
        }
    }

    public class NotEqualExpr : BinaryExpr
    {
        public NotEqualExpr(Expr left, Expr right): base (left, right) { }

        public override string genCode()
        {
            return "Equal";
        }

        public override Tipo validarSemantico()
        {
            Tipo t_izq = leftExpr.validarSemantico();
            Tipo t_der = rightExpr.validarSemantico();

            if (t_izq is Booleano && t_der is Booleano)
                return t_der;

            if (t_izq is Entero && t_der is Entero)
                return t_der;

            if (t_izq is Flotante && t_der is Flotante)
                return t_der;

            if (t_izq is Cadena && t_der is Cadena)
                return t_der;

            if (t_der is Caracter && t_der is Caracter)
                return t_der;

            throw ErrorMessage("Deberian ser tipos booleano/entero/flotante/cadena/caracter las dos expresiones del NOT EQUAL.");
        }
    }
    
    #endregion

    #region RelationExpr

    public class GreaterExpr : BinaryExpr
    {
        public GreaterExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "GreaterExpr";
        }

        public override Tipo validarSemantico()
        {
            Tipo t_izq = leftExpr.validarSemantico();
            Tipo t_der = rightExpr.validarSemantico();

            if (t_izq is Booleano && t_der is Booleano)
                return t_der;

            if (t_izq is Entero && t_der is Entero)
                return t_der;

            if (t_izq is Flotante && t_der is Flotante)
                return t_der;

            if (t_izq is Cadena && t_der is Cadena)
                return t_der;

            if (t_der is Caracter && t_der is Caracter)
                return t_der;

            throw ErrorMessage("Deberian ser tipos booleano/entero/flotante/cadena/caracter las dos expresiones del GREATER THAN.");
        }
    }

    public class GreaterEqualExpr : BinaryExpr
    {
        public GreaterEqualExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "GreaterEqualExpr";
        }

        public override Tipo validarSemantico()
        {
            Tipo t_izq = leftExpr.validarSemantico();
            Tipo t_der = rightExpr.validarSemantico();

            if (t_izq is Booleano && t_der is Booleano)
                return t_der;

            if (t_izq is Entero && t_der is Entero)
                return t_der;

            if (t_izq is Flotante && t_der is Flotante)
                return t_der;

            if (t_izq is Cadena && t_der is Cadena)
                return t_der;

            if (t_der is Caracter && t_der is Caracter)
                return t_der;

            throw ErrorMessage("Deberian ser tipos booleano/entero/flotante/cadena/caracter las dos expresiones del GREATER EQUAL THAN.");
        }
    }

    public class LessExpr : BinaryExpr
    {
        public LessExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "LessExpr";
        }

        public override Tipo validarSemantico()
        {
            Tipo t_izq = leftExpr.validarSemantico();
            Tipo t_der = rightExpr.validarSemantico();

            if (t_izq is Booleano && t_der is Booleano)
                return t_der;

            if (t_izq is Entero && t_der is Entero)
                return t_der;

            if (t_izq is Flotante && t_der is Flotante)
                return t_der;

            if (t_izq is Cadena && t_der is Cadena)
                return t_der;

            if (t_der is Caracter && t_der is Caracter)
                return t_der;

            throw ErrorMessage("Deberian ser tipos booleano/entero/flotante/cadena/caracter las dos expresiones del LESS THAN.");
        }
    }

    public class LessEqualExpr : BinaryExpr
    {
        public LessEqualExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "LessEqualExpr";
        }

        public override Tipo validarSemantico()
        {
            Tipo t_izq = leftExpr.validarSemantico();
            Tipo t_der = rightExpr.validarSemantico();

            if (t_izq is Booleano && t_der is Booleano)
                return t_der;

            if (t_izq is Entero && t_der is Entero)
                return t_der;

            if (t_izq is Flotante && t_der is Flotante)
                return t_der;

            if (t_izq is Cadena && t_der is Cadena)
                return t_der;

            if (t_der is Caracter && t_der is Caracter)
                return t_der;

            throw ErrorMessage("Deberian ser tipos booleano/entero/flotante/cadena/caracter las dos expresiones del LESS EQUAL THAN.");
        }
    }

    #endregion

    #region AdditiveExpr

    public class AdditionExpr : BinaryExpr
    {
        public AdditionExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "additionExpr";
        }

        public override Tipo validarSemantico()
        {
            Tipo t_izq = leftExpr.validarSemantico();
            Tipo t_der = rightExpr.validarSemantico();

            if (t_izq is Entero && t_der is Entero)
                return t_der;

            if (t_izq is Flotante && t_der is Flotante)
                return t_der;

            if (t_izq is Cadena && t_der is Cadena)
                return t_der;

            throw ErrorMessage("Tipos invalidos en las expresiones del ADDITION.");
        }
    }

    public class SubstractionExpr : BinaryExpr
    {
        public SubstractionExpr(Expr left, Expr right) : base(left, right){ }

        public override string genCode()
        {
            return "SubstractionExpr";
        }

        public override Tipo validarSemantico()
        {
            Tipo t_izq = leftExpr.validarSemantico();
            Tipo t_der = rightExpr.validarSemantico();

            if (t_izq is Entero && t_der is Entero)
                return t_der;

            if (t_izq is Flotante && t_der is Flotante)
                return t_der;

            throw ErrorMessage("Tipos invalidos en las expresiones del SUBSTRACTION.");
        }
    }

    #endregion

    #region Multiplicative
    
    public class MultiplicationExpr : BinaryExpr
    {
        public MultiplicationExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "multiplicationExpr";
        }

        public override Tipo validarSemantico()
        {
            Tipo t_izq = leftExpr.validarSemantico();
            Tipo t_der = rightExpr.validarSemantico();

            if (t_izq is Entero && t_der is Entero)
                return t_der;

            if (t_izq is Flotante && t_der is Flotante)
                return t_der;

            throw ErrorMessage("Tipos invalidos en las expresiones del MULTIPLICATION.");
        }
    }

    public class DivisionExpr : BinaryExpr
    {
        public DivisionExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "DivisionExpr";
        }

        public override Tipo validarSemantico()
        {
            Tipo t_izq = leftExpr.validarSemantico();
            Tipo t_der = rightExpr.validarSemantico();

            if (t_izq is Entero && t_der is Entero)
                return t_der;

            if (t_izq is Flotante && t_der is Flotante)
                return t_der;

            throw ErrorMessage("Tipos invalidos en las expresiones del DIVITION.");
        }
    }

    public class RemainderExpr : BinaryExpr
    {
        public RemainderExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "RemainderExpr";
        }

        public override Tipo validarSemantico()
        {
            Tipo t_izq = leftExpr.validarSemantico();
            Tipo t_der = rightExpr.validarSemantico();

            if (t_izq is Entero && t_der is Entero)
                return t_der;

            if (t_izq is Flotante && t_der is Flotante)
                return t_der;

            throw ErrorMessage("Tipos invalidos en las expresiones del REMAINDER.");
        }
    }

    #endregion

    #endregion

    #region Unary

    public abstract class UnaryExpr : Expr
    {
        public Expr Id;

        public UnaryExpr(Expr id)
        {
            Id = id;
        }

        public override string genCode()
        {
            return "unaryExpr";
        }
    }

    public class PreIncrementExpr : UnaryExpr
    {
        public PreIncrementExpr(Expr Id) : base(Id) { }

        public override string genCode()
        {
            return "PreIncrementExpr";
        }

        public override Tipo validarSemantico()
        {
            if (Id is ReferenceAccess)
            {
                ReferenceAccess refAcc = (ReferenceAccess)Id;

                Tipo t_ref = refAcc.validarSemantico();

                if (t_ref is Entero)
                    return t_ref;
                if (t_ref is Flotante)
                    return t_ref;

                throw ErrorMessage("Solo se permiten tipos numericos para PreIncrement.");
            }
            else
            {
                throw ErrorMessage("La expresion deberia ser de referencia para PreIncrement.");
            }
        }
    }

    public class PreDecrementExpr : UnaryExpr
    {
        public PreDecrementExpr(Expr Id) : base(Id) { }

        public override string genCode()
        {
            return "PreDecrementExpr";
        }

        public override Tipo validarSemantico()
        {
            if (Id is ReferenceAccess)
            {
                ReferenceAccess refAcc = (ReferenceAccess)Id;

                Tipo t_ref = refAcc.validarSemantico();

                if (t_ref is Entero)
                    return t_ref;

                if (t_ref is Flotante)
                    return t_ref;

                throw ErrorMessage("Solo se permiten tipos numericos para PreDecrement.");
            }
            else
            {
                throw ErrorMessage("La expresion deberia ser de referencia para PreDecrement.");
            }
        }
    }

    public class NotExpr : UnaryExpr
    {
        public NotExpr(Expr Id) : base(Id) { }

        public override string genCode()
        {
            return "NotExpr";
        }

        public override Tipo validarSemantico()
        {
            if (Id is ReferenceAccess)
            {
                ReferenceAccess refAcc = (ReferenceAccess)Id;

                Tipo t_ref = refAcc.validarSemantico();

                if (t_ref is Booleano)
                    return t_ref;

                if (t_ref is Entero)
                    return t_ref;

                if (t_ref is Flotante)
                    return t_ref;

                if (t_ref is Cadena)
                    return t_ref;

                if (t_ref is Caracter)
                    return t_ref;

                throw ErrorMessage("Solo se permiten tipos booleano/entero/flotante/cadena/caracter para Not.");
            }
            else
            {
                throw ErrorMessage("La expresion deberia ser de referencia para Not.");
            }
        }
    }

    #endregion

    #region PostFix

    public abstract class PostfixExpr : Expr
    {
        public Expr Id;

        public PostfixExpr(Expr id)
        {
            Id = id;
        }

        public override string genCode()
        {
            return "PostfixExpr";
        }        
    }

    public class PostIncrementExpr : PostfixExpr
    {
        public PostIncrementExpr(Expr id) : base(id) { }

        public override string genCode()
        {
            return "PostIncrementExpr";
        }

        public override Tipo validarSemantico()
        {
            if (Id is ReferenceAccess)
            {
                ReferenceAccess refAcc = (ReferenceAccess)Id;

                Tipo t_ref = refAcc.validarSemantico();

                if (t_ref is Flotante)
                    return t_ref;

                if (t_ref is Entero)
                    return t_ref;

                throw ErrorMessage("Solo se permiten tipos numericos para PostIncrement.");
            }
            else
            {
                throw ErrorMessage("La expresion deberia ser de referencia para PostIncrement.");
            }
        }
    }

    public class PostDecrementExpr : PostfixExpr
    {
        public PostDecrementExpr(Expr id) : base(id) { }

        public override string genCode()
        {
            return "PostDecrementExpr";
        }

        public override Tipo validarSemantico()
        {
            if (Id is ReferenceAccess)
            {
                ReferenceAccess refAcc = (ReferenceAccess)Id;

                Tipo t_ref = refAcc.validarSemantico();

                if (t_ref is Entero)
                    return t_ref;

                if (t_ref is Flotante)
                    return t_ref;

                throw ErrorMessage("Solo se permiten tipos numericos para PostDecrement.");
            }
            else
            {
                throw ErrorMessage("La expresion deberia ser de referencia para PostDecrement.");
            }
        }
    }

    #endregion    

    #region terminales : literales basicas, referencias id, miembroRegistro, indiceArreglo, functionCall

    
    #region literales tipos basicos: int float, char, bool, string
    
    public class EnteroLiteral : Expr
    {
        public int value;
        
        public EnteroLiteral(int val)
        {
            value = val;
        }

        public override string genCode()
        {
            return "EnteroLiteral";
        }

        public override Tipo validarSemantico()
        {
            return new Entero();
        }
    }

    public class RealLiteral : Expr
    {
        public float value;

        public RealLiteral(float val)
        {
            value = val;
        }

        public override string genCode()
        {
            return "RealLiteral";
        }

        public override Tipo validarSemantico()
        {
            return new Flotante();
        }
    }

    public class BooleanoLiteral : Expr
    {
        public bool value;

        public BooleanoLiteral(bool val)
        {
            value = val;
        }

        public override string genCode()
        {
            return "BooleanoLiteral";
        }

        public override Tipo validarSemantico()
        {
            return new Booleano();
        }
    }

    public class CaracterLiteral : Expr
    {
        public char value;

        public CaracterLiteral(char val)
        {
            value = val;
        }

        public override string genCode()
        {
            return "CaracterLiteral";
        }

        public override Tipo validarSemantico()
        {
            return new Caracter();
        }
    }

    public class CadenaLiteral : Expr
    {
        public string value;

        public CadenaLiteral(string val)
        {
            value = val;
        }

        public override string genCode()
        {
            return "CadenaLiteral";
        }

        public override Tipo validarSemantico()
        {
            return new Cadena();
        }
    }

    #endregion


    public class ReferenceAccess : Expr
    {
        public string lexeme;

        public ReferenceAccess(string lex)
        {
            lexeme = lex;
        }
    }

    public class Id : ReferenceAccess
    {

        public Id(string lex) : base(lex) { }

        public override string genCode()
        {
            return "Id";
        }

        public override Tipo validarSemantico()
        {
            Tipo t = entornoActual.get(lexeme);
            return t;
        }
    }

    public class MiembroRegistro : ReferenceAccess
    {
        public ReferenceAccess member;

        public MiembroRegistro(string lex, ReferenceAccess mem)
            : base(lex)
        {
            member = mem;
        }

        public override string genCode()
        {
            return "MiembroRegistro";
        }

        public override Tipo validarSemantico()
        {
            Tipo t = this.entornoActual.get(lexeme);

            if (t is Registro)
            {
                Registro record = (Registro)t;
                //return record.entornoStruct.get(member.lexeme);
                
                Tipo t2 = record.entornoStruct.get(member.lexeme);

                if (t2 is Registro)
                {
                    //return member.validarSemantico();
                    //Registro record2 = (Registro)t2;

                    member.entornoActual = record.entornoStruct;
                    return member.validarSemantico();
                }
                else
                    return t2;
            }
            else
                throw ErrorMessage("El tipo " + lexeme + " no es de tipo struct.");                
        }

    }

    public class IndiceArreglo : ReferenceAccess
    {
        public List<Expr> IndexList;

        public IndiceArreglo(List<Expr> indexList, string id) : base(id)
        {
            IndexList = indexList;
            lexeme = id;
        }

        public override string genCode()
        {
            return "IndiceArreglo";
        }

        public override Tipo validarSemantico()
        {
            /*Tipo t = entornoActual.get(this.lexeme);

            if (t is Arreglo)
            {
                //Arreglo array = (Arreglo)t;

            }
            else
                throw ErrorMessage(this.lexeme + " no es de tipo arreglo.");*/
            throw ErrorMessage("No implementado. validarSemantico()/IndiceArreglo");

            /*if (!Parser.tablaSimbolos.ContainsKey(this.lexeme))
                throw new Exception("La variable" + this.lexeme + " no existe.");cc
            
            Tipo t_arreglo = Parser.tablaSimbolos[this.lexeme];

            if (t_arreglo is Arreglo)
            {
                Arreglo arr = (Arreglo)t_arreglo;

                for (int i = 0; i < IndexList.Count; i++)
                {
                    if (!(IndexList[i].validarSemantico().esEquivalente(new Entero())) && !(IndexList[i].validarSemantico().esEquivalente(new Flotante())))
                    {
                        throw new Exception("Solo se permite indexacion con expresiones numericas");
                    }
                }

                return arr.tipoArreglo;
            }
            else
                throw new Exception(this.lexeme + " no es de tipo arreglo");*/
        }
    }

    public class LlamadaFuncion : ReferenceAccess
    {        
        public List<Expr> listaParametros;

        public LlamadaFuncion(string id, List<Expr> parametros) : base(id)
        {
            listaParametros = parametros;
        }

        public override string genCode()
        {
            return "LlamadaFuncion";
        }

        public Tipo validarSemanticaHelper(Tipo product)
        {
            Product p = (Product)product;

            return p.Type1;
        }

        public override Tipo validarSemantico()
        {
            Tipo t = entornoActual.get(this.lexeme);

            if (t is Funcion)
            {
                Funcion func = (Funcion)t;

                if (listaParametros.Count == func.Parametros.Count)
                {
                    for (int x = 0; x < listaParametros.Count; x++)
                    {
                        Tipo t1 = listaParametros[x].validarSemantico();
                        Tipo t2 = func.Parametros[x];
                        if (!t1.esEquivalente(t2))
                            throw ErrorMessage("Tipo de parametro incorrecto.");
                    }
                }
                else
                    throw ErrorMessage("Cantidad erronea de parametros en la llamada de funcion " + this.lexeme + ".");

                    return func.tipoRetorno;
            }
            else
                throw ErrorMessage(this.lexeme + " no es una Funcion.");            
        }
    }

    #endregion

    #endregion

    #region tipo

    public abstract class Tipo
    {
        public abstract bool esEquivalente(Tipo t);
    }

    public class Entero : Tipo
    {
        public override bool esEquivalente(Tipo t)
        {
            return t is Entero;
        }
    }

    public class Flotante : Tipo
    {
        public override bool esEquivalente(Tipo t)
        {
            return t is Flotante;
        }
    }

    public class Caracter : Tipo
    {
        public override bool esEquivalente(Tipo t)
        {
            return t is Caracter;
        }
    }

    public class Cadena : Tipo
    {
        public override bool esEquivalente(Tipo t)
        {
            return t is Cadena;
        }
    }

    public class Booleano : Tipo
    {
        public override bool esEquivalente(Tipo t)
        {
            return t is Booleano;
        }
    }

    public class Void : Tipo
    {
        public override bool esEquivalente(Tipo t)
        {
            return t is Void;
        }
    }

    public class Enumeracion : Tipo
    {
        public override bool esEquivalente(Tipo t)
        {
            return t is Enumeracion;
        }
    }

    public class Registro : Tipo
    {
        public Env entornoStruct;

        public Registro(Env entorno)
        {
            entornoStruct = entorno;
        }

        public override bool esEquivalente(Tipo t)
        {
            if (t is Registro)
            {
                Registro r = (Registro)t;

                if (entornoStruct.tablaSimbolos.Count == r.entornoStruct.tablaSimbolos.Count)
                {
                    for (int i = 0; i < entornoStruct.tablaSimbolos.Count; i++)
                    {
                        Tipo miCampo = entornoStruct.tablaSimbolos.ElementAt(i).Value;
                        Tipo otroCampo = r.entornoStruct.tablaSimbolos.ElementAt(i).Value;

                        if (!(miCampo.esEquivalente(otroCampo)))
                            return false;
                    }
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }
    }

    public class Arreglo : Tipo
    {
        public Tipo tipoArreglo;
        //public Expr size;
        public int size;

        public Arreglo(Tipo tipo, int tam)
        {
            tipoArreglo = tipo;
            size = tam;
        }

        public override bool esEquivalente(Tipo t)
        {
            if (t is Arreglo)
            {
                Arreglo array = (Arreglo)t;

                if (!tipoArreglo.esEquivalente(array.tipoArreglo))
                    return false;

                return size == array.size;
            }
            else
                return false;
        }
    }

    public class Funcion : Tipo
    {
        public Tipo tipoRetorno;
        public List<Tipo> Parametros;

        public Funcion(Tipo retorno, List<Tipo> parametros)
        {
            tipoRetorno = retorno;
            Parametros = parametros;
        }

        public override bool esEquivalente(Tipo t)
        {
            if (t is Funcion)
            {
                Funcion func = (Funcion)t;

                if (!tipoRetorno.esEquivalente(func.tipoRetorno))
                    return false;
                
                return true;
                //return Parametros.esEquivalente(func.Parametros);
            }
            else
                return false;
        }
    }

    public class Product : Tipo
    {
        public Tipo Type1, Type2;

        public Product(Tipo type1, Tipo type2)
        {
            Type1 = type1;
            Type2 = type2;
        }

        public override bool esEquivalente(Tipo t)
        {
            if (t is Product)
            {
                Product p = (Product)t;

                if (!Type1.esEquivalente(p.Type1))
                    return false;

                if (!Type2.esEquivalente(p.Type2))
                    return false;

                return true;
            }
            else
                return false;
        }
    }

    #endregion
    
    #region valor

    public abstract class Valor
    {
        public abstract Valor clone();
    }

     
    
    #endregion
}
