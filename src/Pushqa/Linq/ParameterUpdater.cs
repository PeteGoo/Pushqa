using System;
using System.Linq.Expressions;

namespace Pushqa.Linq {
    internal class ParameterUpdater {
        public static Expression<Func<T, bool>> UpdateParameter<T>(
   Expression<Func<T, bool>> expr,
   ParameterExpression newParameter) {
            var visitor = new ParameterUpdateVisitor(expr.Parameters[0], newParameter);
            var body = visitor.Visit(expr.Body);

            return Expression.Lambda<Func<T, bool>>(body, newParameter);
        }
    }

    class ParameterUpdateVisitor : ExpressionVisitor {
        private ParameterExpression _oldParameter;
        private ParameterExpression _newParameter;

        public ParameterUpdateVisitor(ParameterExpression oldParameter, ParameterExpression newParameter) {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node) {
            if (object.ReferenceEquals(node, _oldParameter))
                return _newParameter;

            return base.VisitParameter(node);
        }
    }


}