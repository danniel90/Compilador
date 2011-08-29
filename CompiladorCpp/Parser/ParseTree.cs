using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyntaxTree
{
    #region tipo

    public abstract class Tipo
    {
        public virtual bool esEquivalente(Tipo t){ return false; }
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

    public class Enumeracion : Tipo
    {
        public override bool esEquivalente(Tipo t)
        {
            return t is Enumeracion;
        }
    }     

    public class ListaCampos
    {
        public List<string> Campos;

        public Dictionary<String, Tipo> tiposCampos;

        public ListaCampos()
        {
            Campos = new List<string>();
            tiposCampos = new Dictionary<string,Tipo>();
        }
    }

    public class Registro : Tipo
    {
        public ListaCampos CamposRegistro;

        public Registro(ListaCampos campos)
        {
            CamposRegistro = campos;
        }

        public override bool esEquivalente(Tipo t)
        {
            Registro r = (Registro)t;

            if (CamposRegistro.Campos.Count == r.CamposRegistro.Campos.Count)
            {
                for (int i = 0; i < CamposRegistro.Campos.Count; i++)
                {
                    string miCampo = CamposRegistro.Campos[i];
                    string otroCampo = r.CamposRegistro.Campos[i];

                    if (!CamposRegistro.tiposCampos[miCampo].esEquivalente(r.CamposRegistro.tiposCampos[otroCampo]))
                        return false;
                }
                return true;
            }
            else
                return false;
        }
    }

    public class Arreglo : Tipo
    {
        public List<int> Dimensiones;
        public Tipo tipoArreglo;

        public Arreglo(List<int> dimensiones, Tipo type)
        {
            Dimensiones = dimensiones;
            tipoArreglo = type;
        }

        public override bool esEquivalente(Tipo t)
        {
            if (t is Arreglo)
            {
                Arreglo otroArreglo = (Arreglo)t;

                if (otroArreglo.Dimensiones.Count == Dimensiones.Count)
                {
                    for (int i = 0; i < Dimensiones.Count; i++)
                    {
                        if (Dimensiones[i] != otroArreglo.Dimensiones[i])
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

    #endregion

    #region program
    
    #region Sentence
    
    public abstract class Sentence
    {
        public void print()
        {
            Console.WriteLine(genCode());
        }
        public virtual string genCode()
        {
            return "Sentencia";
        }
    }

    #region VariableDeclaration

    public class VariableDeclaration : Sentence
    {
        public Tipo tipo;
        public List<VariableDeclarator> ids;

        public VariableDeclaration(Tipo tipoVariable, List<VariableDeclarator> listaIds)
        {
            tipo = tipoVariable;
            ids = listaIds;
        }
    }

    public class VariableSubDeclaration : Sentence
    {
        public Tipo tipo;
        public string Id;

        public VariableSubDeclaration(Tipo tipovar, string id)
        {
            tipo = tipovar;
            Id = id;
        }
    }

    public class VariableDeclarator : Sentence
    {
        public string Id;
        Initializers Inicializacion;

        public VariableDeclarator(string id, Initializers inicializacion)
        {
            Id = id;
            Inicializacion = inicializacion;
        }
    }

    #region initializers

    public abstract class Initializers 
    {
    }

    public class VariableInitializer : Initializers
    {
        Expr Expresion;

        public VariableInitializer(Expr expresion)
        {
            Expresion = expresion;
        }
    }

    public class VariableInitializerList : Initializers
    {
        List<Initializers> initializerList;

        public VariableInitializerList(List<Initializers> initializersList)
        {
            initializerList = initializersList;
        }
    }

    #endregion

    #endregion

    #region functionDefinition
    
    public class FuncionDefinition : Sentence
    {
        List<Product> Parametros;
        Statement CompoundStatement;
        string funcionId;
        Tipo tipoRetorno;
        
            public FuncionDefinition(List<Product> parametros, Statement CompoundStmnt, string id, Tipo retorno)
        {
            Parametros = parametros;
            CompoundStatement = CompoundStmnt;
            funcionId = id;
            tipoRetorno = retorno;
        }
    }

    public class Product
    {
        Tipo tipoParametro;
        string IdParametro;

        public Product(Tipo tipo,string idParametro)
        {
            tipoParametro = tipo;
            IdParametro = idParametro;
        }
    }    

    #endregion
    
    #endregion

    #region Statement

    public abstract class Statement : Sentence
    {
    }

    public class CompoundStatement : Statement
    {
        public List<Statement> Sentencias;

        public CompoundStatement(List<Statement> listaSentencias)
        {
            Sentencias = listaSentencias;
        }
    }

    public class DeclarationStatement : Statement
    {
        public Tipo tipo;
        public List<VariableDeclarator> ids;

        public DeclarationStatement(Tipo tipoVariable, List<VariableDeclarator> listaIds)
        {
            tipo = tipoVariable;
            ids = listaIds;
        }
    }

    public class ExpressionStatement : Statement
    {
        Expr expresion;

        public ExpressionStatement(Expr expr)
        {
            expresion = expr;
        }
    }

    public class IfStatement : Statement
    {
        Statement Expresion;
        Statement BloqueVerdadero, BloqueFalso;

        public IfStatement(Statement expresion, Statement bloqueTrue, Statement bloqueFalse)
        {
            Expresion = expresion;
            BloqueVerdadero = bloqueTrue;
            BloqueFalso = bloqueFalse;
        }
    }

    public class DoWhileStatement : Statement
    {
        public Statement expresion, compoundStatement;

        public DoWhileStatement(Statement expr, Statement cpStmnt)
        {
            expresion = expr;
            compoundStatement = cpStmnt;
        }
    }

    public class WhileStatement : Statement
    {
        public Statement expresion, compoundstatement;

        public WhileStatement(Statement expr, Statement cpStmnt)
        {
            expresion = expr;
            compoundstatement = cpStmnt;
        }
    }

    public class ForStatement : Statement
    {
        public Statement forInitialization, forControl, forIteration, CompoundStatement;

        public ForStatement(Statement forInit, Statement forCtrl, Statement forIter, Statement cpStmnt)
        {
            forInitialization = forInit;
            forControl = forCtrl;
            forIteration = forIter;
            CompoundStatement = cpStmnt;
        }
    }

    public class ContinueStatement : Statement
    {
        public ContinueStatement()
        {
        }
    }

    public class BreakStatement : Statement
    {
        public BreakStatement()
        {
        }
    }

    public class ReturnStatement : Statement
    {
        public Statement expresion;

        public ReturnStatement(Statement expr)
        {
            expresion = expr;
        }
    }

    #endregion

    #region StructDeclaration

    public class StructVariableDeclaration : Sentence
    {
        string strId, strVarId;

        public StructVariableDeclaration(string strid,string strvarname)
        {
            strId = strid;
            strVarId = strvarname;
        }
    }

    public class StructDeclaration : Sentence
    {
        string structName;
        ListaCampos variables;

        public StructDeclaration(string strName, ListaCampos vars)
        {
            structName = strName;
            variables = vars;
        }
    }

    public class StructVariableDeclarationStatement : Statement
    {
        string strId, strVarId;

        public StructVariableDeclarationStatement(string strid,string strvarname)
        {
            strId = strid;
            strVarId = strvarname;
        }
    }

    #endregion

    #region enums

    public class EnumerationDeclaration : Sentence
    {
        string enumId;
        List<VariableDeclarator> variables;

        public EnumerationDeclaration(string id, List<VariableDeclarator> vars)
        {
            enumId = id;
            variables = vars;
        }
    }

    public class EnumerationVariableDeclaration : Sentence
    {
        string enumerationName, enumerationVarName;

        public EnumerationVariableDeclaration(string enumName, string enumVarName)
        {
            enumerationName = enumName;
            enumerationVarName = enumVarName;
        }
    }

    #endregion

    #endregion


    #region expresiones

    public abstract class Expr
    {
        public void print()
        {
            Console.WriteLine(genCode());
        }
        public virtual string genCode() 
        {
            return "Expr";
        }
    }

    #region SequenceExpr

    public class SequenceExpr : Expr
    {
        public List<Expr> listaExpresiones;

        public SequenceExpr(List<Expr> expresiones)
        {
            listaExpresiones = expresiones;
        }

        public override string genCode()
        {
            return "SequenceExpr";
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
    }

    public class AdditionAssignExpr : AssignExpr
    {
        public AdditionAssignExpr(Expr id, Expr valor) : base(id, valor) { }

        public override string genCode()
        {
            return "AdditionAssignExpr";
        }
    }

    public class SubstractionAssignExpr : AssignExpr
    {
        public SubstractionAssignExpr(Expr id, Expr valor) : base(id, valor) { }

        public override string genCode()
        {
            return "SubstractionAssignExpr";
        }
    }

    public class MultiplicationAssignExpr : AssignExpr
    {
        public MultiplicationAssignExpr(Expr id, Expr valor) : base(id, valor) { }

        public override string genCode()
        {
            return "MultiplicationAssignExpr";
        }
    }

    public class DivisionAssignExpr : AssignExpr
    {
        public DivisionAssignExpr(Expr id, Expr valor) : base (id, valor) { }

        public override string genCode()
        {
            return "DivisionAssignExpr";
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
    }

    public class NotEqualExpr : BinaryExpr
    {
        public NotEqualExpr(Expr left, Expr right): base (left, right) { }

        public override string genCode()
        {
            return "Equal";
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
    }

    public class GreaterEqualExpr : BinaryExpr
    {
        public GreaterEqualExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "GreaterEqualExpr";
        }
    }

    public class LessExpr : BinaryExpr
    {
        public LessExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "LessExpr";
        }
    }

    public class LessEqualExpr : BinaryExpr
    {
        public LessEqualExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "LessEqualExpr";
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
    }

    public class SubstractionExpr : BinaryExpr
    {
        public SubstractionExpr(Expr left, Expr right) : base(left, right){ }

        public override string genCode()
        {
            return "SubstractionExpr";
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
    }

    public class DivisionExpr : BinaryExpr
    {
        public DivisionExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "DivisionExpr";
        }
    }

    public class RemainderExpr : BinaryExpr
    {
        public RemainderExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "RemainderExpr";
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
    }

    public class PreDecrementExpr : UnaryExpr
    {
        public PreDecrementExpr(Expr Id) : base(Id) { }

        public override string genCode()
        {
            return "PreDecrementExpr";
        }
    }

    public class NotExpr : UnaryExpr
    {
        public NotExpr(Expr Id) : base(Id) { }

        public override string genCode()
        {
            return "NotExpr";
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
    }

    public class PostDecrementExpr : PostfixExpr
    {
        public PostDecrementExpr(Expr id) : base(id) { }

        public override string genCode()
        {
            return "PostDecrementExpr";
        }
    }

    #endregion    

    #region terminales : id, enteroLiteral, realLiteral, booleanoLiteral, caracterLiteral, cadenaLiteral, miembroRegistro, indiceArreglo, functionCall    

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
    }    

    public abstract class ReferenceAccess : Expr
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
    }

    public class MiembroRegistro : ReferenceAccess
    {
        public List<ReferenceAccess> Members;

        public MiembroRegistro(string id, List<ReferenceAccess> members): base(id)
        {
            Members = members;
        }

        public override string genCode()
        {
            return "MiembroRegistro";
        }
    }

    public class IndiceArreglo : ReferenceAccess
    {
        public List<Expr> IndexList;       

        public IndiceArreglo(List<Expr> indexList, string id) : base(id)
        {
            IndexList = indexList;
        }

        public override string genCode()
        {
            return "IndiceArreglo";
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
    }

    #endregion

    #endregion

}
