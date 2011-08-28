using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyntaxTree
{
    #region expresiones
    public abstract class Expr
    {
        public virtual string genCode() 
        {
            return "Expr";
        }
    }

    public class SequenceExpr : Expr
    {
        public override string genCode()
        {
            return "sequenceExpr";
        }
    }

    public class AssignExpr : Expr
    {
        public override string genCode()
        {
            return "assignExpr";
        }
    }

    public class BinaryExpr : Expr
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

    public class OrExpr : BinaryExpr
    {
        public OrExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "orExpr";
        }
    }

    public class AndExpr : BinaryExpr
    {
        public AndExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "andExpr";
        }
    }

    public class EqualExpr : BinaryExpr
    {
        public EqualExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "equalExpr";
        }
    }

    public class RelationExpr : BinaryExpr
    {
        public RelationExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "relationExpr";
        }
    }

    public class AdditiveExpr : BinaryExpr
    {
        public AdditiveExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "additiveExpr";
        }
    }

    public class MultiplicativeExpr : BinaryExpr
    {
        public MultiplicativeExpr(Expr left, Expr right) : base(left, right) { }

        public override string genCode()
        {
            return "multiplicativeExpr";
        }
    }

    public class UnaryExpr : Expr
    {
        public override string genCode()
        {
            return "unaryExpr";
        }
    }

    public class PostfixExpr : Expr
    {
        public override string genCode()
        {
            return "postfixExpr";
        }
    }

    public class PrimaryExpr : Expr
    {
        public override string genCode()
        {
            return "primaryExpr";
        }
    }
    #endregion

    #region 
    public abstract class Sentence
    {
        public virtual string genCode() 
        {
            return "Sentence";
        }
    }
    #endregion
}
