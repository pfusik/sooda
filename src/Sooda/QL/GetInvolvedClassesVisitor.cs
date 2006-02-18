using System;
using Sooda.Schema;

using Sooda.QL.TypedWrappers;

namespace Sooda.QL
{
    /// <summary>
    /// Summary description for GetInvolvedClassesVisitor.
    /// </summary> 
    public class GetInvolvedClassesVisitor : ISoqlVisitor
    {
        private ClassInfo _rootClass;
        private ClassInfoCollection _result = new ClassInfoCollection();

        public GetInvolvedClassesVisitor(ClassInfo rootClass)
        {
            _rootClass = rootClass;
        }

        public void GetInvolvedClasses(SoqlExpression expr)
        {
            expr.Accept(this);
        }

        public ClassInfoCollection Results
        {
            get { return _result; }
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlTypedWrapperExpression v)
        {
            v.InnerExpression.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlBooleanWrapperExpression v)
        {
            v.InnerExpression.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlBinaryExpression v)
        {
            v.par1.Accept(this);
            v.par2.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlBooleanAndExpression v)
        {
            v.Left.Accept(this);
            v.Right.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlBooleanInExpression v)
        {
            v.Left.Accept(this);
            foreach (SoqlExpression expr in v.Right)
            {
                expr.Accept(this);
            }
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlBooleanIsNullExpression v)
        {
            v.Expr.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlBooleanLiteralExpression v)
        {
            // nothing here
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlBooleanNegationExpression v)
        {
            v.par.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlBooleanOrExpression v)
        {
            v.par1.Accept(this);
            v.par2.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlBooleanRelationalExpression v)
        {
            v.par1.Accept(this);
            v.par2.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlExistsExpression v)
        {
            v.Query.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlFunctionCallExpression v)
        {
            if (v.Parameters != null)
            {
                foreach (SoqlExpression e in v.Parameters)
                {
                    e.Accept(this);
                }
            }
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlContainsExpression v)
        {
            if (v.Path != null)
            {
                v.Path.Accept(this);
            }
            else
            {
                if (!_result.Contains(_rootClass))
                    _result.Add(_rootClass);
            }
            v.Expr.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlSoodaClassExpression v)
        {
            if (v.Path != null)
            {
                v.Path.Accept(this);
            }
            else
            {
                if (!_result.Contains(_rootClass))
                    _result.Add(_rootClass);
            }
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlCountExpression v)
        {
            if (v.Path != null)
            {
                v.Path.Accept(this);
            }
            else
            {
                if (!_result.Contains(_rootClass))
                    _result.Add(_rootClass);
            }
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlAsteriskExpression v)
        {
            if (v.Left != null)
            {
                v.Left.Accept(this);
            }
            else
            {
                if (!_result.Contains(_rootClass))
                    _result.Add(_rootClass);
            }
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlLiteralExpression v)
        {
            // nothing here
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlNullLiteral v)
        {
            // nothing here
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlParameterLiteralExpression v)
        {
            // nothing here
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlPathExpression v)
        {
            v.GetAndAddClassInfo(_rootClass, _result);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlQueryExpression v)
        {
            throw new NotImplementedException();
            // TODO:  Add GetInvolvedClassesVisitor.Sooda.QL.ISoqlVisitor.Visit implementation
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlUnaryNegationExpression v)
        {
            v.par.Accept(this);
        }

        void Sooda.QL.ISoqlVisitor.Visit(SoqlRawExpression v)
        {
            throw new NotSupportedException("RAW queries not supported.");
        }
    }
}
