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

    public class Node
    {
        public int lexline = -1;

        public Node()
        {
            lexline = Lexer.line;
        }
    }

    #region program    

    #region Sentence

    public abstract class Sentence : Node
    {
        public void print()
        {
            Console.WriteLine(genCode());
        }

        public abstract string genCode();

        public abstract void validarSemantica();

        public abstract void interpretar();

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
                if (!(((FunctionDefinition)main).Tiporetorno is Entero))
                    throw ErrorMessage("Retorno en la funcion main deberia ser de tipo int");
            
            sentences.validarSemantica();
            main.validarSemantica();
        }

        public override void interpretar()
        {
            if (!(sentences is FunctionDefinition))//osea q hay otras sentences aparte del main
                sentences.interpretar();
            main.interpretar();
        }
    }

    public class EmptySentence : Sentence
    {
        public EmptySentence() { }
        
        public override string genCode()
        {
            return "EmptySentence";
        }

        public override void validarSemantica() { }

        public override void interpretar() { }
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

        public override void interpretar()
        {
            if (!(sent1 is FunctionDefinition))
                sent1.interpretar();
            
            if (!(sent2 is FunctionDefinition))
            sent2.interpretar();
        }
    }

    #region VariableDeclaration

    public abstract class VariableDeclaration : Sentence
    {
        public EnvValues entornoValores;

        public VariableDeclaration()
        {
            entornoValores = Parser.entornoValores;
        }
    }

    public class VariableDeclarations : VariableDeclaration
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

        public override void interpretar()
        {
            variableDeclarations.interpretar();
        }
    }    

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

        public override void interpretar()
        {
            varDeclarator1.interpretar();
            varDeclarator2.interpretar();
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

        public override void interpretar()
        {
            if (initialization != null)
            {
                Valor v = initialization.interpretar();
                this.entornoValores.put(declaration.id, v);
            }
            else
            {
                Valor v_default = getDefaultValue(declaration.tipo);
                this.entornoValores.put(declaration.id, v_default);
            }
        }

        public static Valor getDefaultValue(Tipo tipo)
        {
            /*if (tipo is Entero)
            {
                return new ValorEntero(0);
            }
            else if (tipo is Flotante)
            {
                return new ValorFlotante(0);
            }
            else if (tipo is Booleano)
            {
                return new ValorBooleano(false);
            }
            else if (tipo is Caracter)
            {
                return new ValorCaracter('.');
            }
            else if (tipo is Cadena)
            {
                return new ValorCadena("");
            }
            else*/// if (tipo is Arreglo)
            if (tipo is Arreglo)
            {
                return getValorArreglo((Arreglo)tipo);
            }
            else
                return new ValorDefault();
        }

        private static ValorArreglo getValorArreglo(Arreglo array)
        {
            ValorArreglo v_array = new ValorArreglo();
            v_array.valor = new List<Valor>();

            if (array.tipoArreglo is Arreglo)
            {
                for (int x = 0; x < array.size; x++)
                {
                    ValorArreglo v_subarray = getValorArreglo((Arreglo)array.tipoArreglo);
                    v_array.valor.Add(v_subarray);
                }
            }
            else
            {
                for (int x = 0; x < array.size; x++)
                {
                    Valor v = getDefaultValue(array.tipoArreglo);
                    v_array.valor.Add(v);
                }                
            }
            return v_array;
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
        /*public abstract Tipo validarSemantico();
        public abstract string genCode();
        public abstract void interpretar();

        protected Exception ErrorMessage(string message)
        {
            throw new Exception(message + " Linea:" + lexline);
        }*/
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

        public override Valor interpretar()
        {
            return Expresion.interpretar();
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
                VariableInitializer vInit = (VariableInitializer)tinit;

                tipoArreglo = vInit.validarSemantico();

                return tipoArreglo;
            }
            else if (tinit is VariableInitializerList)
            {
                VariableInitializerList vList = (VariableInitializerList)tinit;

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

        public override Valor interpretar()
        {
            ValorArreglo vArray = new ValorArreglo();
            vArray.valor = new List<Valor>();

            foreach (Initializers vinit in initializerList)
            {                                

                if (vinit is VariableInitializer)
                {
                    VariableInitializer vInitializer = (VariableInitializer)vinit;
                    Valor valorArreglo = vInitializer.interpretar();

                    vArray.valor.Add(valorArreglo);
                }
                else
                {
                    VariableInitializerList vList = (VariableInitializerList)vinit;
                    ValorArreglo vSubArray = new ValorArreglo();
                    vSubArray.valor = new List<Valor>();

                    foreach (Initializers vinit2 in vList.initializerList)
                    {
                        Valor valorArreglo = vinit2.interpretar();
                        vSubArray.valor.Add(valorArreglo);
                    }
                    vArray.valor.Add(vSubArray);
                }               
            }
            return vArray;
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
        public EnvTypes entornoTiposLocal;
        public string idFuncion;
        public Tipo Tiporetorno;
        public Valor ValorRetorno;
        public bool returned;

        public Sentence compoundStatement;

        public FunctionDefinition(string nombreFuncion, Tipo ret, Sentence cpStmnt)
        {
            idFuncion = nombreFuncion;
            Tiporetorno = ret;
            compoundStatement = cpStmnt;
            entornoTiposLocal = Parser.entornoTipos;
            ValorRetorno = null;
            returned = false;
        }

        public FunctionDefinition() { }

        public void init(string nombreFuncion, Tipo ret, Sentence cpStmnt)
        {
            idFuncion = nombreFuncion;
            Tiporetorno = ret;
            compoundStatement = cpStmnt;
            entornoTiposLocal = Parser.entornoTipos;
        }

        public override string genCode()
        {
            return "FunctionDefinition :" + idFuncion + "\n"+ compoundStatement.genCode();
        }

        public override void validarSemantica()
        {
            compoundStatement.validarSemantica();
        }

        public override void interpretar()
        {
            compoundStatement.interpretar();
        }
    }

    #endregion

    #region cin, cout

    public abstract class myConsole : Sentence
    {
        public List<Expr> expresiones;

        public myConsole(List<Expr> exprs)
        {
            expresiones = exprs;
        }
    }

    public class ConsoleIn : myConsole
    {
        public ConsoleIn(List<Expr> exprs) : base(exprs) { }

        public override string genCode()
        {
            return "ConsoleIn";
        }

        public override void validarSemantica()
        {
            foreach (Expr exp in expresiones)
            {
                if (!(exp is ReferenceAccess))
                {
                    throw ErrorMessage("Error no se puede guardar entrada en este tipo de variable.");
                }
                exp.validarSemantico();
            }
        }

        public override void interpretar()
        {
            foreach (Expr exp in expresiones)
            {
                string line = Console.ReadLine();

                Tipo t_exp = exp.validarSemantico();

                string id = ((ReferenceAccess)exp).lexeme;
                EnvValues env = ((ReferenceAccess)exp).getEntornoValores();

                if (t_exp is Entero)
                {
                    int val = Convert.ToInt32(line);
                    env.set(id, new ValorEntero(val));
                }
                else if (t_exp is Flotante)
                {
                    float val = float.Parse(line, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    env.set(id, new ValorFlotante(val));
                }
                else if (t_exp is Booleano)
                {
                    bool val = Convert.ToBoolean(Convert.ToInt32(line));
                    env.set(id, new ValorBooleano(val));
                }
                else if (t_exp is Caracter)
                {
                    char val = Convert.ToChar(line);
                    env.set(id, new ValorCaracter(val));
                }
                else //if (t_exp is Cadena)
                {
                    env.set(id, new ValorCadena(line));
                }
            }
        }
    }

    public class ConsoleOut : myConsole
    {
        public bool endl;

        public ConsoleOut(List<Expr> exprs) : base(exprs)
        {
            endl = false;
        }

        public override string genCode()
        {
            return "ConsoleOut";
        }

        public override void validarSemantica()
        {
            foreach (Expr exp in expresiones)
                exp.validarSemantico();
        }

        public override void interpretar()
        {
            foreach (Expr ex in expresiones)
            {
                Valor v = ex.interpretar();
                Console.Write(v.ToString());
            }
            
            if (endl)
                Console.WriteLine();
        }
    }

    #endregion

    #endregion

    #region Statement

    public abstract class Statement : Sentence
    {
        public Sentence enclosingCycle, enclosingFunction;

        public Statement()
        {
            enclosingCycle = Parser.cicloActual;
            enclosingFunction = Parser.funcionActual;
        }

        protected bool returnedBreakContinue()
        {
            if (enclosingCycle != null)
                if (((IterationStatement)enclosingCycle).dobreak || ((IterationStatement)enclosingCycle).docontinue)
                    return true;

            if (enclosingFunction != null)
                if (((FunctionDefinition)enclosingFunction).returned)
                    return true;
            
            return false;
        }
    }

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

        public override void interpretar()
        {
            if (returnedBreakContinue()) 
                return;
            stmt1.interpretar();


            if (returnedBreakContinue())
                return;
            stmt2.interpretar();
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

        public override void interpretar()
        {
            if (returnedBreakContinue())
                return;

            Sentencias.interpretar();
        }
    }

    public class ExpressionStatement : Statement
    {
        public Expr expresion;

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

        public override void interpretar()
        {
            if (returnedBreakContinue())
                return;

            expresion.interpretar();
        }
    }

    public abstract class FunctionStatement : Statement
    {
        public EnvValues entornoValoresLocal;

        public FunctionStatement()
        {
            entornoValoresLocal = Parser.entornoValores;
        }

        protected void clearEnvValues()
        {
            entornoValoresLocal.tablaValores.Clear();
        }
    }

    public class IfStatement : FunctionStatement
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
            Tipo t = condicion.validarSemantico();
            
            if (!(t is Entero || t is Booleano || t is Flotante))
                throw ErrorMessage("La condicion del if deberia ser de tipo booleano/numerico. Tipo:" + t.ToString());

            BloqueVerdadero.validarSemantica();
        }

        public override void interpretar()
        {
            if (returnedBreakContinue())
                return;

            Valor vFrCtrl = condicion.interpretar();

            if (vFrCtrl is ValorEntero)
            {
                ValorEntero vFrCtrl2 = (ValorEntero)vFrCtrl;

                if (vFrCtrl2.valor > 0)
                    BloqueVerdadero.interpretar();
            }
            else if (vFrCtrl is ValorFlotante)
            {
                ValorFlotante vFrCtrl2 = (ValorFlotante)vFrCtrl;

                if (vFrCtrl2.valor > 0)
                    BloqueVerdadero.interpretar();
            }
            else
            {
                ValorBooleano vFrCtrl2 = (ValorBooleano)vFrCtrl;

                if (vFrCtrl2.valor == true)
                    BloqueVerdadero.interpretar();
            }

            clearEnvValues();
        }
    }

    public class IfElseStatement : FunctionStatement
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
            Tipo t = condicion.validarSemantico();

            if (!(t is Entero || t is Booleano || t is Flotante))
                throw ErrorMessage("La condicion del if deberia ser de tipo booleano/numerico. Tipo:" + t.ToString());

            BloqueVerdadero.validarSemantica();
            BloqueFalso.validarSemantica();
        }

        public override void interpretar()
        {
            if (returnedBreakContinue())
                return;

            Valor vFrCtrl = condicion.interpretar();

            if (vFrCtrl is ValorEntero)
            {
                ValorEntero vFrCtrl2 = (ValorEntero)vFrCtrl;

                if (vFrCtrl2.valor > 0)
                    BloqueVerdadero.interpretar();
                else
                    BloqueFalso.interpretar();
            }
            else if (vFrCtrl is ValorFlotante)
            {
                ValorFlotante vFrCtrl2 = (ValorFlotante)vFrCtrl;

                if (vFrCtrl2.valor > 0)
                    BloqueVerdadero.interpretar();
                else
                    BloqueFalso.interpretar();
            }
            else
            {
                ValorBooleano vFrCtrl2 = (ValorBooleano)vFrCtrl;

                if (vFrCtrl2.valor == true)
                    BloqueVerdadero.interpretar();
                else
                    BloqueFalso.interpretar();
            }
            clearEnvValues();
        }
    }

    #region iterationStatements

    public abstract class IterationStatement : FunctionStatement
    {
        public bool dobreak;
        public bool docontinue;

        public IterationStatement()
        {
            dobreak = docontinue = false;
        }
    }

    public class DoWhileStatement : IterationStatement
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

        public override void interpretar()
        {
            if (returnedBreakContinue())
                return;

            while (true)
            {
                compoundStatement.interpretar();
                
                Valor vFrCtrl = expresion.interpretar();

                if (vFrCtrl is ValorEntero)
                {
                    ValorEntero vFrCtrl2 = (ValorEntero)vFrCtrl;

                    if (vFrCtrl2.valor <= 0)
                        break;
                }
                else if (vFrCtrl is ValorFlotante)
                {
                    ValorFlotante vFrCtrl2 = (ValorFlotante)vFrCtrl;

                    if (vFrCtrl2.valor <= 0)
                        break;
                }
                else
                {
                    ValorBooleano vFrCtrl2 = (ValorBooleano)vFrCtrl;

                    if (vFrCtrl2.valor == false)
                        break;
                }
                
                if (dobreak)
                    break;

                if (((FunctionDefinition)enclosingFunction).returned)
                    return;
            }
            clearEnvValues();
        }
    }

    public class WhileStatement : IterationStatement
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

        public override void interpretar()
        {
            if (returnedBreakContinue())
                return;

            while (true)
            {
                Valor vFrCtrl = expresion.interpretar();

                if (vFrCtrl is ValorEntero)
                {
                    ValorEntero vFrCtrl2 = (ValorEntero)vFrCtrl;

                    if (vFrCtrl2.valor <= 0)
                        break;
                }
                else if (vFrCtrl is ValorFlotante)
                {
                    ValorFlotante vFrCtrl2 = (ValorFlotante)vFrCtrl;

                    if (vFrCtrl2.valor <= 0)
                        break;
                }
                else
                {
                    ValorBooleano vFrCtrl2 = (ValorBooleano)vFrCtrl;

                    if (vFrCtrl2.valor == false)
                        break;
                }
                compoundstatement.interpretar();
                
                if (dobreak)
                    break;
                
                if (((FunctionDefinition)enclosingFunction).returned)
                    return;
            }
            clearEnvValues();
        }
    }

    public class ForStatement : IterationStatement
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
            if (forInitialization != null)
            {
                if ((forInitialization is VariableDeclaration) || (forInitialization is ExpressionStatement))
                    forInitialization.validarSemantica();
                else
                    throw ErrorMessage("Inicializacion de for deberia ser declaracion o una expresion.");
            }

            if (forControl != null)
            {
                if (forControl is ExpressionStatement)
                    forControl.validarSemantica();
                else
                    throw ErrorMessage("Control del for deberia ser una expresion.");
            }

            if (forIteration != null)
            {
                if (forIteration is ExpressionStatement)
                    forIteration.validarSemantica();
                else
                    throw ErrorMessage("Iteracion del for deberia ser una expresion.");                
            }

            CompoundStatement.validarSemantica();
        }

        public override void interpretar()
        {
            if (returnedBreakContinue())
                return;

            if (forInitialization != null)
                forInitialization.interpretar();
            
            while (true)
            {
                if (forControl != null)
                {
                    Valor vFrCtrl = ((ExpressionStatement)forControl).expresion.interpretar();

                    if (vFrCtrl is ValorEntero)
                    {
                        ValorEntero vFrCtrl2 = (ValorEntero)vFrCtrl;

                        if (vFrCtrl2.valor <= 0)
                            break;
                    }
                    else if (vFrCtrl is ValorFlotante)
                    {
                        ValorFlotante vFrCtrl2 = (ValorFlotante)vFrCtrl;

                        if (vFrCtrl2.valor <= 0)
                            break;
                    }
                    else
                    {
                        ValorBooleano vFrCtrl2 = (ValorBooleano)vFrCtrl;

                        if (vFrCtrl2.valor == false)
                            break;
                    }
                }
                
                CompoundStatement.interpretar();

                if (dobreak)
                    break;

                if (((FunctionDefinition)enclosingFunction).returned)
                    return;
                
                if (forIteration != null)
                    ((ExpressionStatement)forIteration).interpretar();
            }
            clearEnvValues();
        }
    }

    #endregion

    public class ContinueStatement : Statement
    {

        public ContinueStatement() { }

        public override string genCode()
        {
            return "ContinueStatement";
        }

        public override void validarSemantica()
        {
            if (enclosingCycle == null)
                throw ErrorMessage("ContinueStatement sin ciclo."); 
        }

        public override void interpretar()
        {
            if (returnedBreakContinue())
                return;
            
            IterationStatement itStmnt = (IterationStatement)enclosingCycle;
            itStmnt.docontinue = true;            
        }
    }

    public class BreakStatement : Statement
    {
        public BreakStatement() { }

        public override string genCode()
        {
            return "BreakStatement";
        }

        public override void validarSemantica()
        {
            if (enclosingCycle == null)
                throw ErrorMessage("BreakStatement sin ciclo."); 
        }

        public override void interpretar()
        {
            if (returnedBreakContinue())
                return;

            IterationStatement itStmnt = (IterationStatement)enclosingCycle;
            itStmnt.dobreak = true;
        }
    }

    public class ReturnStatement : Statement
    {
        public Expr expresion;

        public ReturnStatement(Expr expr)
        {            
            expresion = expr;
        }

        public override string genCode()
        {
            return "ReturnStatement";
        }

        public override void validarSemantica()
        {
            if (expresion != null)
                expresion.validarSemantico();

            if (enclosingFunction == null)
                throw ErrorMessage("Return inalcanzable o sin funcion."); 
        }

        public override void interpretar()
        {
            if (returnedBreakContinue())
                return;

            if (expresion != null){                
                Valor v = expresion.interpretar();
                ((FunctionDefinition)enclosingFunction).ValorRetorno = v;
            }

            ((FunctionDefinition)enclosingFunction).returned = true;
        }
    }

    #endregion

    #region StructDeclaration

    public class StructVariableDeclaration : Sentence
    {
        public string strId, strVarId;
        public Tipo tipo;
        public Valor valorRegistro;
        public EnvValues entornoValoresActual;

        public StructVariableDeclaration(string strid,string strvarname, Tipo tipostr, Valor valRegistro)
        {
            strId = strid;
            strVarId = strvarname;
            tipo = tipostr;
            valorRegistro = valRegistro;
            entornoValoresActual = Parser.entornoValores;
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

        public override void interpretar()
        {            
            entornoValoresActual.put(strVarId, valorRegistro);
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

        public override void interpretar()
        {

        }
    }    

    #endregion

    #region enums

    public class EnumerationDeclaration : Sentence
    {
        public string enumId;
        public List<VariableDeclarator> variables;        

        public EnvValues entornoValoresEnum;
        public EnvValues entornoLocal;

        public EnumerationDeclaration(string id, List<VariableDeclarator> vars, EnvValues entorno)
        {
            enumId = id;
            variables = vars;
            entornoValoresEnum = Parser.entornoValores;
            entornoLocal = entorno;
        }

        public override string genCode()
        {
            return "EnumerationDeclaration";
        }

        public override void validarSemantica()
        {
            foreach (VariableDeclarator vDec in variables)
                vDec.validarSemantica();
        }

        public override void interpretar()
        {
            VariableDeclarator vDec = variables[0];
            if (vDec.initialization != null)
            {
                Valor v = vDec.initialization.interpretar();
                entornoValoresEnum.put(vDec.declaration.id, v);
                entornoLocal.put(vDec.declaration.id, v);
            }
            else
            {
                entornoValoresEnum.put(vDec.declaration.id, new ValorEntero(0));
                entornoLocal.put(vDec.declaration.id, new ValorEntero(0));
            }

            for (int x = 1; x < variables.Count; x++)
            {
                if (variables[x].initialization != null)
                {
                    Valor v = variables[x].initialization.interpretar();
                    entornoValoresEnum.put(variables[x].declaration.id, v);
                    entornoLocal.put(variables[x].declaration.id, v);
                }
                else 
                {
                    ValorEntero v = (ValorEntero)entornoValoresEnum.get(variables[x-1].declaration.id);
                    entornoValoresEnum.put(variables[x].declaration.id, new ValorEntero( v.valor + 1));
                    entornoLocal.put(variables[x].declaration.id, v);
                }
            }
        }
    }

    public class EnumerationVariableDeclaration : Sentence
    {
        public string enumerationName, enumerationVarName;
        public Tipo tipo;
        public Valor valorEnum;
        public EnvValues entornoValoresActual;

        public EnumerationVariableDeclaration(string enumName, string enumVarName, Tipo tipoenum, Valor valEnum)
        {
            enumerationName = enumName;
            enumerationVarName = enumVarName;
            tipo = tipoenum;

            valorEnum = valEnum;
            entornoValoresActual = Parser.entornoValores;
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

        public override void interpretar()
        {
            entornoValoresActual.put(enumerationVarName, valorEnum);
        }
    }

    #endregion

    #endregion

    #region expresiones

    public class Expr : Node
    {

        public EnvTypes entornoTiposActual;
        public EnvValues entornoValoresActual;

        public Expr()
        {
            entornoTiposActual = Parser.entornoTipos;
            entornoValoresActual = Parser.entornoValores;
        }

        public void print()
        {
            Console.WriteLine(genCode());
        }

        public virtual string genCode()
        {
            throw ErrorMessage("Expr / genCode().");
        }

        public virtual Tipo validarSemantico()
        {
            throw ErrorMessage("Expr / validarSemantico().");            
        }

        public virtual Valor interpretar()
        {
            throw ErrorMessage("Expr / interpretar().");
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

        public override Valor interpretar()
        {
            expr1.interpretar();
            return expr2.interpretar();
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

            if (!(Id is ReferenceAccess))
                throw ErrorMessage("El id de asignacion deberia ser de referencia!!");

            if (t.esEquivalente(value.validarSemantico()))
                return t;
            
            throw ErrorMessage("Los tipos de variable y de valor/expresion no son equivalentes.");
        }

        public override Valor interpretar()
        {
            Valor v = value.interpretar();

            ((ReferenceAccess)Id).setElem(v);

            return v;
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
            
            if (!(Id is ReferenceAccess))
                throw ErrorMessage("El id de asignacion deberia ser de referencia!!");

            if ((t is Entero) || (t is Flotante) || (t is Cadena))
            {

                if (t.esEquivalente(value.validarSemantico()))
                    return t;
                else
                    throw ErrorMessage("Los tipos de variable y de valor/expresion no son equivalentes.");
            } else
                throw ErrorMessage("No se puede sumar esos tipos.");
        }

        public override Valor interpretar()
        {
            Valor v = value.interpretar();

            ReferenceAccess t_id = (ReferenceAccess)Id;

            Valor oldval = t_id.interpretar();//this.entornoValoresActual.get(((ReferenceAccess)Id).lexeme);

            Valor newval;
            if (oldval is ValorEntero)
                newval = new ValorEntero(((ValorEntero)oldval).valor
                                        + 
                                        ((ValorEntero)v).valor);
            else if (oldval is ValorFlotante)
                newval = new ValorFlotante(((ValorFlotante)oldval).valor
                                          +
                                          ((ValorFlotante)v).valor
                    );
            else
                newval = new ValorCadena(
                        ((ValorCadena)oldval).valor 
                        +
                        ((ValorCadena)v).valor
                        );

            //this.entornoValoresActual.set(t_id.lexeme, newval);
            t_id.setElem(newval);

            return newval;
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

            if (!(Id is ReferenceAccess))
                throw ErrorMessage("El id de asignacion deberia ser de referencia!!");

            if ((t is Entero) || (t is Flotante))
            {

                if (t.esEquivalente(value.validarSemantico()))
                    return t;
                else
                    throw ErrorMessage("Los tipos de variable y de valor/expresion no son equivalentes.");
            }
            else
                throw ErrorMessage("No se puede restar esos tipos.");
        }

        public override Valor interpretar()
        {
            Valor v = value.interpretar();

            if (Id is Id)
            {
                Id t_id = (Id)Id;

                Valor v_id = this.entornoValoresActual.get(t_id.lexeme);

                Valor newval;
                if (v_id is ValorEntero)
                    newval = new ValorEntero(
                        ((ValorEntero)v_id).valor
                        -
                        ((ValorEntero)v).valor
                        );                 
                else
                    newval = new ValorFlotante(
                        ((ValorFlotante)v_id).valor
                        -
                        ((ValorFlotante)v).valor
                        );
                
                this.entornoValoresActual.set(t_id.lexeme, newval);

                return newval;
            }
            else
                throw ErrorMessage("Solo Id se pueden asignar.");
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

            if (!(Id is ReferenceAccess))
                throw ErrorMessage("El id de asignacion deberia ser de referencia!!");

            if ((t is Entero) || (t is Flotante))
            {

                if (t.esEquivalente(value.validarSemantico()))
                    return t;

                throw ErrorMessage("Los tipos de variable y de valor/expresion no son equivalentes.");
            } else
                throw ErrorMessage("No se puede multiplicar esos tipos.");
        }

        public override Valor interpretar()
        {
            Valor v = value.interpretar();

            if (Id is Id)
            {
                Id t_id = (Id)Id;

                Valor v_id = this.entornoValoresActual.get(t_id.lexeme);

                Valor newval;
                
                if (v_id is ValorEntero)
                    newval = new ValorEntero(
                        ((ValorEntero)v_id).valor
                        *
                        ((ValorEntero)v).valor
                        );
                else
                    newval = new ValorFlotante(
                        ((ValorFlotante)v_id).valor
                        *
                        ((ValorFlotante)v).valor
                        );                
                
                this.entornoValoresActual.set(t_id.lexeme, newval);                

                return newval;
            }
            else
                throw ErrorMessage("Solo Id se pueden asignar.");
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

            if (!(Id is ReferenceAccess))
                throw ErrorMessage("El id de asignacion deberia ser de referencia!!");

            if ((t is Entero) || (t is Flotante))
            {
                if (t.esEquivalente(value.validarSemantico()))
                    return t;

                throw ErrorMessage("Los tipos de variable y de valor/expresion no son equivalentes.");
            }
            else
                throw ErrorMessage("No se puede dividir esos tipos");
        }

        public override Valor interpretar()
        {
            Valor v = value.interpretar();

            if (Id is Id)
            {
                Id t_id = (Id)Id;

                Valor v_id = this.entornoValoresActual.get(t_id.lexeme);

                Valor newval;

                if (v_id is ValorEntero)
                    newval = new ValorEntero(
                        ((ValorEntero)v_id).valor
                        /
                        ((ValorEntero)v).valor
                        );
                else
                    newval = new ValorFlotante(
                        ((ValorFlotante)v_id).valor
                        /
                        ((ValorFlotante)v).valor
                        );                

                this.entornoValoresActual.set(t_id.lexeme, newval);
                return newval;
            }
            else
                throw ErrorMessage("Solo Id se pueden asignar.");
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

        public override Valor interpretar()
        {
            Valor v_izq = leftExpr.interpretar();
            Valor v_der = rightExpr.interpretar();

            Valor result;
            if (v_izq is ValorEntero)
                result = new ValorBooleano(
                    Convert.ToBoolean(((ValorEntero)v_izq).valor)
                    ||
                    Convert.ToBoolean(((ValorEntero)v_der).valor)
                    );
            else if (v_izq is ValorFlotante)
                result = new ValorBooleano(
                    Convert.ToBoolean(((ValorFlotante)v_izq).valor)
                    ||
                    Convert.ToBoolean(((ValorFlotante)v_der).valor)
                    );
            else
                result = new ValorBooleano(
                        ((ValorBooleano)v_izq).valor
                        ||
                        ((ValorBooleano)v_der).valor
                        );

            return result;
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
                return new Booleano();

            if (t_izq is Entero && t_der is Entero)
                return new Booleano();

            if (t_izq is Flotante && t_der is Flotante)
                return new Booleano();

            throw ErrorMessage("Deberian ser tipos booleano/entero/flotante las dos expresiones del AND.");
        }

        public override Valor interpretar()
        {
            Valor v_izq = leftExpr.interpretar();
            Valor v_der = rightExpr.interpretar();

            Valor result;
            if (v_izq is ValorEntero)
                result = new ValorBooleano(
                    Convert.ToBoolean(((ValorEntero)v_izq).valor)
                    &&
                    Convert.ToBoolean(((ValorEntero)v_der).valor)
                    );
            else if (v_izq is ValorFlotante)
                result = new ValorBooleano(
                    Convert.ToBoolean(((ValorFlotante)v_izq).valor)
                    &&
                    Convert.ToBoolean(((ValorFlotante)v_der).valor)
                    );            
            else
                result = new ValorBooleano(
                        ((ValorBooleano)v_izq).valor
                        &&
                        ((ValorBooleano)v_der).valor
                        );

            return result;
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
                return new Booleano();

            if (t_izq is Entero && t_der is Entero)
                return new Booleano();

            if (t_izq is Flotante && t_der is Flotante)
                return new Booleano();

            if (t_izq is Cadena && t_der is Cadena)
                return new Booleano();

            if (t_der is Caracter && t_der is Caracter)
                return new Booleano();

            throw ErrorMessage("Deberian ser tipos booleano/entero/flotante/cadena/caracter las dos expresiones del EQUAL.");
        }

        public override Valor interpretar()
        {
            Valor v_izq = leftExpr.interpretar();
            Valor v_der = rightExpr.interpretar();

            Valor result;
            if (v_izq is ValorEntero)
                result = new ValorBooleano(
                    ((ValorEntero)v_izq).valor
                    ==
                    ((ValorEntero)v_der).valor
                    );
            else if (v_izq is ValorFlotante)
                result = new ValorBooleano(
                    ((ValorFlotante)v_izq).valor
                    ==
                    ((ValorFlotante)v_der).valor
                    );
            else if (v_izq is ValorCadena)
                result = new ValorBooleano(
                    ((ValorCadena)v_izq).valor
                    ==
                    ((ValorCadena)v_der).valor
                    );
            else if (v_izq is ValorCaracter)
                result = new ValorBooleano(
                        ((ValorCaracter)v_izq).valor
                        ==
                        ((ValorCaracter)v_der).valor
                        );
            else
                result = new ValorBooleano(
                        ((ValorBooleano)v_izq).valor
                        ==
                        ((ValorBooleano)v_der).valor
                        );

            return result;
        }
    }

    public class NotEqualExpr : BinaryExpr
    {
        public NotEqualExpr(Expr left, Expr right): base (left, right) { }

        public override string genCode()
        {
            return "notEqual";
        }

        public override Tipo validarSemantico()
        {
            Tipo t_izq = leftExpr.validarSemantico();
            Tipo t_der = rightExpr.validarSemantico();

            if (t_izq is Booleano && t_der is Booleano)
                return new Booleano();

            if (t_izq is Entero && t_der is Entero)
                return new Booleano();

            if (t_izq is Flotante && t_der is Flotante)
                return new Booleano();

            if (t_izq is Cadena && t_der is Cadena)
                return new Booleano();

            if (t_der is Caracter && t_der is Caracter)
                return new Booleano();

            throw ErrorMessage("Deberian ser tipos booleano/entero/flotante/cadena/caracter las dos expresiones del NOT EQUAL.");
        }

        public override Valor interpretar()
        {
            Valor v_izq = leftExpr.interpretar();
            Valor v_der = rightExpr.interpretar();

            Valor result;
            if (v_izq is ValorEntero)
                result = new ValorBooleano(
                    ((ValorEntero)v_izq).valor
                    !=
                    ((ValorEntero)v_der).valor
                    );
            else if (v_izq is ValorFlotante)
                result = new ValorBooleano(
                    ((ValorFlotante)v_izq).valor
                    !=
                    ((ValorFlotante)v_der).valor
                    );
            else if (v_izq is ValorCadena)
                result = new ValorBooleano(
                    ((ValorCadena)v_izq).valor
                    !=
                    ((ValorCadena)v_der).valor
                    );
            else if (v_izq is ValorCaracter)
                result = new ValorBooleano(
                        ((ValorCaracter)v_izq).valor
                        !=
                        ((ValorCaracter)v_der).valor
                        );
            else
                result = new ValorBooleano(
                        ((ValorBooleano)v_izq).valor
                        !=
                        ((ValorBooleano)v_der).valor
                        );

            return result;
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
                return new Booleano();

            if (t_izq is Entero && t_der is Entero)
                return new Booleano();

            if (t_izq is Flotante && t_der is Flotante)
                return new Booleano();

            if (t_izq is Cadena && t_der is Cadena)
                return new Booleano();

            if (t_der is Caracter && t_der is Caracter)
                return new Booleano();

            throw ErrorMessage("Deberian ser tipos booleano/entero/flotante/cadena/caracter las dos expresiones del GREATER THAN.");
        }

        public override Valor interpretar()
        {
            Valor v_izq = leftExpr.interpretar();
            Valor v_der = rightExpr.interpretar();

            Valor result;
            if (v_izq is ValorEntero)
                result = new ValorBooleano(
                    ((ValorEntero)v_izq).valor
                    >
                    ((ValorEntero)v_der).valor
                    );
            else if (v_izq is ValorFlotante)
                result = new ValorBooleano(
                    ((ValorFlotante)v_izq).valor
                    >
                    ((ValorFlotante)v_der).valor
                    );
            else if (v_izq is ValorCadena)
                result = new ValorBooleano(
                        Convert.ToInt32(((ValorCadena)v_izq).valor)
                        >
                        Convert.ToInt32(((ValorCadena)v_der).valor)
                        );
            else if (v_izq is ValorCaracter)
                result = new ValorBooleano(
                        Convert.ToInt32(((ValorCaracter)v_izq).valor)
                        >
                        Convert.ToInt32(((ValorCaracter)v_der).valor)
                        );
            else
                result = new ValorBooleano(
                        Convert.ToInt32(((ValorBooleano)v_izq).valor)
                        >
                        Convert.ToInt32(((ValorBooleano)v_der).valor)
                        );

            return result;
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
                return new Booleano();

            if (t_izq is Entero && t_der is Entero)
                return new Booleano();

            if (t_izq is Flotante && t_der is Flotante)
                return new Booleano();

            if (t_izq is Cadena && t_der is Cadena)
                return new Booleano();

            if (t_der is Caracter && t_der is Caracter)
                return new Booleano();

            throw ErrorMessage("Deberian ser tipos booleano/entero/flotante/cadena/caracter las dos expresiones del GREATER EQUAL THAN.");
        }

        public override Valor interpretar()
        {
            Valor v_izq = leftExpr.interpretar();
            Valor v_der = rightExpr.interpretar();

            Valor result;
            if (v_izq is ValorEntero)
                result = new ValorBooleano(
                    ((ValorEntero)v_izq).valor
                    >=
                    ((ValorEntero)v_der).valor
                    );
            else if (v_izq is ValorFlotante)
                result = new ValorBooleano(
                    ((ValorFlotante)v_izq).valor
                    >=
                    ((ValorFlotante)v_der).valor
                    );
            else if (v_izq is ValorCadena)
                result = new ValorBooleano(
                        Convert.ToInt32(((ValorCadena)v_izq).valor)
                        >=
                        Convert.ToInt32(((ValorCadena)v_der).valor)
                        );
            else if (v_izq is ValorCaracter)
                result = new ValorBooleano(
                        Convert.ToInt32(((ValorCaracter)v_izq).valor)
                        >=
                        Convert.ToInt32(((ValorCaracter)v_der).valor)
                        );
            else
                result = new ValorBooleano(
                        Convert.ToInt32(((ValorBooleano)v_izq).valor)
                        >=
                        Convert.ToInt32(((ValorBooleano)v_der).valor)
                        );

            return result;
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
                return new Booleano();

            if (t_izq is Entero && t_der is Entero)
                return new Booleano();

            if (t_izq is Flotante && t_der is Flotante)
                return new Booleano();

            if (t_izq is Cadena && t_der is Cadena)
                return new Booleano();

            if (t_der is Caracter && t_der is Caracter)
                return new Booleano();

            throw ErrorMessage("Deberian ser tipos booleano/entero/flotante/cadena/caracter las dos expresiones del LESS THAN.");
        }

        public override Valor interpretar()
        {
            Valor v_izq = leftExpr.interpretar();
            Valor v_der = rightExpr.interpretar();

            Valor result;
            if (v_izq is ValorEntero)
                result = new ValorBooleano(
                    ((ValorEntero)v_izq).valor
                    <
                    ((ValorEntero)v_der).valor
                    );
            else if (v_izq is ValorFlotante)
                result = new ValorBooleano(
                    ((ValorFlotante)v_izq).valor
                    <
                    ((ValorFlotante)v_der).valor
                    );
            else if (v_izq is ValorCadena)
                result = new ValorBooleano(
                        Convert.ToInt32(((ValorCadena)v_izq).valor)
                        <
                        Convert.ToInt32(((ValorCadena)v_der).valor)
                        );
            else if (v_izq is ValorCaracter)
                result = new ValorBooleano(
                        Convert.ToInt32(((ValorCaracter)v_izq).valor)
                        <
                        Convert.ToInt32(((ValorCaracter)v_der).valor)
                        );
            else
                result = new ValorBooleano(
                        Convert.ToInt32(((ValorBooleano)v_izq).valor)
                        <
                        Convert.ToInt32(((ValorBooleano)v_der).valor)
                        );

            return result;
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
                return new Booleano();

            if (t_izq is Entero && t_der is Entero)
                return new Booleano();

            if (t_izq is Flotante && t_der is Flotante)
                return new Booleano();

            if (t_izq is Cadena && t_der is Cadena)
                return new Booleano();

            if (t_der is Caracter && t_der is Caracter)
                return new Booleano();

            throw ErrorMessage("Deberian ser tipos booleano/entero/flotante/cadena/caracter las dos expresiones del LESS EQUAL THAN.");
        }

        public override Valor interpretar()
        {
            Valor v_izq = leftExpr.interpretar();
            Valor v_der = rightExpr.interpretar();

            Valor result;
            if (v_izq is ValorEntero)
                result = new ValorBooleano(
                    ((ValorEntero)v_izq).valor
                    <=
                    ((ValorEntero)v_der).valor
                    );
            else if (v_izq is ValorFlotante)
                result = new ValorBooleano(
                    ((ValorFlotante)v_izq).valor
                    <=
                    ((ValorFlotante)v_der).valor
                    );
            else if (v_izq is ValorCadena)
                result = new ValorBooleano(
                        Convert.ToInt32(((ValorCadena)v_izq).valor)
                        <=
                        Convert.ToInt32(((ValorCadena)v_der).valor)
                        );
            else if (v_izq is ValorCaracter)
                result = new ValorBooleano(
                        Convert.ToInt32(((ValorCaracter)v_izq).valor)
                        <=
                        Convert.ToInt32(((ValorCaracter)v_der).valor)
                        );
            else
                result = new ValorBooleano(
                        Convert.ToInt32(((ValorBooleano)v_izq).valor)
                        <=
                        Convert.ToInt32(((ValorBooleano)v_der).valor)
                        );

            return result;
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

        public override Valor interpretar()
        {
            Valor v_izq = leftExpr.interpretar();
            Valor v_der = rightExpr.interpretar();

            Valor result;
            if (v_izq is ValorEntero)
                result = new ValorEntero(
                    ((ValorEntero)v_izq).valor
                    +
                    ((ValorEntero)v_der).valor
                    );
            else if (v_izq is ValorFlotante)
                result = new ValorFlotante(
                    ((ValorFlotante)v_izq).valor
                    +
                    ((ValorFlotante)v_der).valor
                    );
            else
                result = new ValorCadena(
                    ((ValorCadena)v_izq).valor
                    +
                    ((ValorCadena)v_der).valor
                    );

            return result;
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

        public override Valor interpretar()
        {
            Valor v_izq = leftExpr.interpretar();
            Valor v_der = rightExpr.interpretar();

            Valor result;
            if (v_izq is ValorEntero)
                result = new ValorEntero(
                    ((ValorEntero)v_izq).valor
                    -
                    ((ValorEntero)v_der).valor
                    );
            else
                result = new ValorFlotante(
                    ((ValorFlotante)v_izq).valor
                    -
                    ((ValorFlotante)v_der).valor
                    );

            return result;
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

        public override Valor interpretar()
        {
            Valor v_izq = leftExpr.interpretar();
            Valor v_der = rightExpr.interpretar();

            Valor result;
            if (v_izq is ValorEntero)
                result = new ValorEntero(
                    ((ValorEntero)v_izq).valor
                    *
                    ((ValorEntero)v_der).valor
                    );
            else
                result = new ValorFlotante(
                    ((ValorFlotante)v_izq).valor
                    *
                    ((ValorFlotante)v_der).valor
                    );
            
            return result;
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

        public override Valor interpretar()
        {
            Valor v_izq = leftExpr.interpretar();
            Valor v_der = rightExpr.interpretar();

            Valor result;
            if (v_izq is ValorEntero)
                result = new ValorEntero(
                    ((ValorEntero)v_izq).valor
                    /
                    ((ValorEntero)v_der).valor
                    );
            else
                result = new ValorFlotante(
                    ((ValorFlotante)v_izq).valor
                    /
                    ((ValorFlotante)v_der).valor
                    );

            return result;
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

        public override Valor interpretar()
        {
            Valor v_izq = leftExpr.interpretar();
            Valor v_der = rightExpr.interpretar();

            Valor result;
            if (v_izq is ValorEntero)
                result = new ValorEntero(
                    ((ValorEntero)v_izq).valor
                    %
                    ((ValorEntero)v_der).valor
                    );
            else
                result = new ValorFlotante(
                    ((ValorFlotante)v_izq).valor
                    %
                    ((ValorFlotante)v_der).valor
                    );

            return result;
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

        public override Valor interpretar()
        {            
 	        Valor val = this.Id.interpretar();

            Valor newVal;
            if (val is ValorEntero)
                newVal = new ValorEntero(((ValorEntero)val).valor++);
            else
                newVal = new ValorFlotante(((ValorFlotante)val).valor++);


            this.entornoValoresActual.set(((ReferenceAccess)Id).lexeme, newVal);
            return newVal;
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

        public override Valor interpretar()
        {            
 	        Valor val = this.Id.interpretar();

            Valor newVal;
            if (val is ValorEntero)
                newVal = new ValorEntero(((ValorEntero)val).valor--);
            else
                newVal = new ValorFlotante(((ValorFlotante)val).valor--);

            this.entornoValoresActual.set(((ReferenceAccess)Id).lexeme, newVal);
            return newVal;
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

                throw ErrorMessage("Solo se permiten tipos booleano/entero/flotante para Not.");
            }
            else
            {
                throw ErrorMessage("La expresion deberia ser de referencia para Not.");
            }
        }

        public override Valor interpretar()
        {            
 	        Valor val = this.Id.interpretar();

            Valor newVal;
            if (val is ValorEntero)
                newVal = new ValorEntero(
                    Convert.ToInt32(
                        !(Convert.ToBoolean(((ValorEntero)val).valor))));
            else if (val is ValorFlotante)
                newVal = new ValorFlotante(
                    Convert.ToInt32(
                        !(Convert.ToBoolean(((ValorFlotante)val).valor))));
            else
                newVal = new ValorBooleano(!((ValorBooleano)val).valor);

            this.entornoValoresActual.set(((ReferenceAccess)Id).lexeme, newVal);
            return newVal;
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
        
        public override Valor interpretar()
        {            
 	        Valor val = this.Id.interpretar();
            EnvValues env = ((ReferenceAccess)Id).getEntornoValores();

            Valor newVal;
            if (val is ValorEntero)
                newVal = new ValorEntero(((ValorEntero)val).valor + 1);
            else
                newVal = new ValorFlotante(((ValorFlotante)val).valor + 1);             
            
            env.set(((ReferenceAccess)Id).lexeme, newVal);
            return val;
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

        public override Valor interpretar()
        {                     
 	        Valor val = this.Id.interpretar();

            Valor newVal;
            if (val is ValorEntero)
                newVal = new ValorEntero(((ValorEntero)val).valor--);
            else
                newVal = new ValorFlotante(((ValorFlotante)val).valor--);


            this.entornoValoresActual.set(((ReferenceAccess)Id).lexeme, newVal);
            return val;
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

        public override Valor interpretar()
        {
            return new ValorEntero(value);
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

        public override Valor interpretar()
        {
            return new ValorFlotante(value);
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

        public override Valor interpretar()
        {
            return new ValorBooleano(value);
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

        public override Valor interpretar()
        {
            return new ValorCaracter(value);
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

        public override Valor interpretar()
        {
            return new ValorCadena(value);
        }
    }

    #endregion


    public abstract class ReferenceAccess : Expr
    {
        public string lexeme;

        public ReferenceAccess(string lex)
        {
            lexeme = lex;
        }

        public abstract void setElem(Valor valor);

        public abstract EnvValues getEntornoValores();
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
            Tipo t = entornoTiposActual.get(lexeme);
            return t;
        }

        public override Valor interpretar()
        {
            return this.entornoValoresActual.get(this.lexeme);
        }

        public override EnvValues getEntornoValores()
        {
            return this.entornoValoresActual;
        }

        public override void setElem(Valor valor)
        {
            this.entornoValoresActual.set(this.lexeme, valor);
        }
    }    

    public class MiembroRegistro : ReferenceAccess
    {
        public ReferenceAccess member;

        public MiembroRegistro(string lex, ReferenceAccess mem) : base(lex)
        {
            member = mem;
        }

        public override string genCode()
        {
            return "MiembroRegistro";
        }

        public override Tipo validarSemantico()
        {
            Tipo t = this.entornoTiposActual.get(lexeme);

            if (t is Registro)
            {
                Registro record = (Registro)t;//1
                
                Tipo t2 = record.entornoTiposStruct.get(member.lexeme);//2

                if (t2 is Registro)
                {
                    member.entornoTiposActual = record.entornoTiposStruct;
                    return member.validarSemantico();
                }
                else
                    return t2;
            }
            else if (t is Enumeracion)
            {
                Enumeracion enume = (Enumeracion)t;

                Tipo t2 = enume.entornoTiposEnum.get(member.lexeme);//2

                return t2;                
            }
            else
                throw ErrorMessage("El tipo " + lexeme + " no es de tipo struct o enumeracion.");
        }

        public override Valor interpretar()
        {
            Valor v = this.entornoValoresActual.get(this.lexeme);//1

            if (v is ValorRegistro)
            {
                Valor v2 = ((ValorRegistro)v).valor.get(member.lexeme);//2

                if (v2 is ValorRegistro)
                {
                    member.entornoValoresActual = ((ValorRegistro)v).valor;
                    return member.interpretar();
                }
                else
                    return v2;
            }
            else
            {
                Valor v2 = ((ValorEnumeracion)v).valor.get(member.lexeme);
                return v2;
            }
        }

        public override EnvValues getEntornoValores()
        {
            ValorRegistro v = (ValorRegistro)this.entornoValoresActual.get(this.lexeme);
            
            Valor v2 = v.valor.get(member.lexeme);

            if (v2 is ValorRegistro)
            {
                ValorRegistro record = (ValorRegistro)v2;
                member.entornoValoresActual = record.valor;
                return member.getEntornoValores();
            }
            else
                return this.entornoValoresActual;
        }

        public override void setElem(Valor valor)
        {
            Valor v = this.entornoValoresActual.get(this.lexeme);

            if (v is ValorRegistro)
            {
                Valor v2 = ((ValorRegistro)v).valor.get(member.lexeme);

                if (v2 is ValorRegistro)
                {
                    member.entornoValoresActual = ((ValorRegistro)v).valor;
                    member.setElem(valor);
                }
                else
                    ((ValorRegistro)v).valor.set(member.lexeme, valor);
            }
            else
            {
                /*Valor v2 = ((ValorEnumeracion)v).valor.get(member.lexeme);

                ((ValorEnumeracion)v).valor.set(member.lexeme, valor);*/
            }
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
            Tipo t = this.entornoTiposActual.get(this.lexeme);

            if (t is Arreglo)
            {
                Arreglo array = (Arreglo)t;//1

                for (int x = 0; x < IndexList.Count; x++)
                {
                    Tipo t_ind = IndexList[x].validarSemantico();

                    if (!(t_ind.esEquivalente(new Entero())))
                    {
                        throw ErrorMessage("Solo se permite indexacion con expresiones numericas.");
                    }
                }

                return validarSemanticohelper(array.tipoArreglo);
            }
            else
                throw ErrorMessage(this.lexeme + " no es de tipo arreglo.");
        }

        public Tipo validarSemanticohelper(Tipo t)
        {
            if (t is Arreglo)
                return validarSemanticohelper(((Arreglo)t).tipoArreglo);
            else
                return t;
        }

        public override Valor interpretar()
        {
            ValorArreglo v = (ValorArreglo)this.entornoValoresActual.get(this.lexeme);//1

            Valor ret = v;
            for (int x = 0; x < IndexList.Count; x++)
            {
                ValorEntero index = (ValorEntero)IndexList[x].interpretar();
                
                if (index.valor < (((ValorArreglo)ret).valor).Count)
                    ret = ((ValorArreglo)ret).valor[index.valor];
                else 
                    throw ErrorMessage("Indice fuera de limites de arreglo.");
            }

            if (ret is ValorDefault)
                throw ErrorMessage("Elemento de arreglo no inicializado.");

            return ret;
        }

        public override void setElem(Valor valor)
        {
            ValorArreglo v = (ValorArreglo)this.entornoValoresActual.get(this.lexeme);//1
            
            Valor ret = v;
            
            for (int x = 0; x < IndexList.Count - 1; x++)
            {
                ValorEntero index = (ValorEntero)IndexList[x].interpretar();

                if (index.valor < (((ValorArreglo)ret).valor).Count)
                    ret = ((ValorArreglo)ret).valor[index.valor];
                else
                    throw ErrorMessage("Indice fuera de limites de arreglo.");
            }

            ValorEntero idx = (ValorEntero)IndexList.Last().interpretar();

            if (idx.valor < (((ValorArreglo)ret).valor).Count)
                ((ValorArreglo)ret).valor[idx.valor] = valor;
            else
                throw ErrorMessage("Indice fuera de limites de arreglo.");
        }

        public override EnvValues getEntornoValores()
        {
            return this.entornoValoresActual;
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

        public override Tipo validarSemantico()
        {
            Tipo t = entornoTiposActual.get(this.lexeme);

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

        public override Valor interpretar()
        {
            ValorFuncion vFunc = (ValorFuncion)this.entornoValoresActual.get(this.lexeme);
            vFunc.funcion.interpretar();
            return ((FunctionDefinition)vFunc.funcion).ValorRetorno;
        }

        public override EnvValues getEntornoValores()
        {
            throw ErrorMessage("No se puede devolver entorno de valores. LlamadaFuncion / getEntornoValores()");
        }

        public override void setElem(Valor valor)
        {
            throw ErrorMessage("No se puede setear valores. LlamadaFuncion / getEntornoValores()");
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
        public EnvTypes entornoTiposEnum;

        public Enumeracion(EnvTypes entornoTipos)
        {
            entornoTiposEnum = entornoTipos;
        }

        public override bool esEquivalente(Tipo t)
        {
            return t is Enumeracion;
        }
    }

    public class Registro : Tipo
    {
        public EnvTypes entornoTiposStruct;

        public Registro(EnvTypes entornoTipos)
        {
            entornoTiposStruct = entornoTipos;
        }

        public override bool esEquivalente(Tipo t)
        {
            if (t is Registro)
            {
                Registro r = (Registro)t;

                if (entornoTiposStruct.tablaSimbolos.Count == r.entornoTiposStruct.tablaSimbolos.Count)
                {
                    for (int i = 0; i < entornoTiposStruct.tablaSimbolos.Count; i++)
                    {
                        Tipo miCampo = entornoTiposStruct.tablaSimbolos.ElementAt(i).Value;
                        Tipo otroCampo = r.entornoTiposStruct.tablaSimbolos.ElementAt(i).Value;

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

                //for (int x = 0; x < )
                
                return true;
                //return Parametros.esEquivalente(func.Parametros);
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

    public class ValorDefault : Valor
    {
        public override Valor clone()
        {
            throw new Exception("Valor nulo.");
        }

        public override string ToString()
        {
            return "ValorDefault";
        }
    }

    public class ValorEntero : Valor
    {
        public int valor;

        public ValorEntero(int val)
        {
            valor = val;
        }

        public override Valor clone()
        {
            return new ValorEntero(valor);
        }

        public override string ToString()
        {
            return valor.ToString();
        }
    }

    public class ValorFlotante : Valor
    {
        public float valor;

        public ValorFlotante(float val)
        {
            valor = val;
        }

        public override Valor clone()
        {
            return new ValorFlotante(valor);
        }

        public override string ToString()
        {
            return valor.ToString();
        }
    }

    public class ValorCaracter : Valor
    {
        public char valor;

        public ValorCaracter(char val)
        {
            valor = val;
        }

        public override Valor clone()
        {
            return new ValorCaracter(valor);
        }

        public override string ToString()
        {
            return valor.ToString();
        }
    }

    public class ValorCadena : Valor
    {
        public string valor;

        public ValorCadena(string val)
        {
            valor = val;
        }

        public override Valor clone()
        {
            return new ValorCadena(valor);
        }

        public override string ToString()
        {
            return valor;
        }
    }

    public class ValorBooleano : Valor
    {
        public bool valor;

        public ValorBooleano(bool val)
        {
            valor = val;
        }

        public override Valor clone()
        {
            return new ValorBooleano(valor);
        }

        public override string ToString()
        {
            return valor.ToString();
        }
    }

    public class ValorArreglo : Valor
    {
        public List<Valor> valor;

        public ValorArreglo() { }

        public override Valor clone()
        {
            ValorArreglo v_array = new ValorArreglo();
            v_array.valor = valor;
            return v_array;
        }
    }

    public class ValorRegistro : Valor
    {
        public EnvValues valor;

        public ValorRegistro() { }

        public override Valor clone()
        {
            ValorRegistro v_record = new ValorRegistro();
            v_record.valor = valor;
            return v_record;
        }
    }

    public class ValorEnumeracion : Valor
    {
        public EnvValues valor;

        public ValorEnumeracion() { }

        public override Valor clone()
        {
            ValorEnumeracion v_enum = new ValorEnumeracion();
            v_enum.valor = valor;
            return v_enum;
        }
    }

    public class ValorFuncion : Valor
    {
        public Sentence funcion;

        public ValorFuncion(Sentence func)
        {
            funcion = func;
        }

        public override Valor clone()
        {
            return new ValorFuncion(funcion);
        }

        public override string ToString()
        {
            return ((FunctionDefinition)funcion).idFuncion;
        }
    }

    #endregion
}

