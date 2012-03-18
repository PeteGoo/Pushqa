using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Pushqa.Linq {

    public class GrandParent {
        public Parent Parent { get; set; }
    }

    public class Parent {
        public Child Child { get; set; }

        public string Method(string s) {
            return s + "abc";
        }
    }

    public class Child {
        public string Name { get; set; }
    }

    public static class ExpressionUtils {
        public static Expression<Func<T1, T3>> Combine<T1, T2, T3>(
            this Expression<Func<T1, T2>> outer, Expression<Func<T2, T3>> inner, bool inline) {
            var invoke = Expression.Invoke(inner, outer.Body);
            Expression body = inline ? new ExpressionRewriter().AutoInline(invoke) : invoke;
            return Expression.Lambda<Func<T1, T3>>(body, outer.Parameters);
        }

        public static Expression<Func<T1, T2>> Combine<T1, T2>(
    this Expression<Func<T1, T2>> outer, Expression<Func<T1, T2>> inner, bool inline) {
            var invoke = Expression.Invoke(inner, outer.Body);
            Expression body = inline ? new ExpressionRewriter().AutoInline(invoke) : invoke;
            return Expression.Lambda<Func<T1, T2>>(body, outer.Parameters);
        }
    }

    public class ExpressionRewriter {
        internal Expression AutoInline(InvocationExpression expression) {
            isLocked = true;
            if (expression == null) throw new ArgumentNullException("expression");
            LambdaExpression lambda = (LambdaExpression) expression.Expression;
            ExpressionRewriter childScope = new ExpressionRewriter(this);
            var lambdaParams = lambda.Parameters;
            var invokeArgs = expression.Arguments;
            if (lambdaParams.Count != invokeArgs.Count) throw new InvalidOperationException("Lambda/invoke mismatch");
            for (int i = 0; i < lambdaParams.Count; i++) {
                childScope.Subst(lambdaParams[i], invokeArgs[i]);
            }
            return childScope.Apply(lambda.Body);
        }

        public ExpressionRewriter() {
            subst = new Dictionary<Expression, Expression>();
        }

        private ExpressionRewriter(ExpressionRewriter parent) {
            if (parent == null) throw new ArgumentNullException("parent");
            subst = new Dictionary<Expression, Expression>(parent.subst);
            inline = parent.inline;
        }

        private bool isLocked, inline;
        private readonly Dictionary<Expression, Expression> subst;

        private void CheckLocked() {
            if (isLocked)
                throw new InvalidOperationException(
                    "You cannot alter the rewriter after Apply has been called");

        }

        public ExpressionRewriter Subst(Expression from,
                                        Expression to) {
            CheckLocked();
            subst.Add(from, to);
            return this;
        }

        public ExpressionRewriter Inline() {
            CheckLocked();
            inline = true;
            return this;
        }

        public Expression Apply(Expression expression) {
            isLocked = true;
            return Walk(expression) ?? expression;
        }

        private static IEnumerable<Expression> CoalesceTerms(
            IEnumerable<Expression> sourceWithNulls, IEnumerable<Expression> replacements) {
            if (sourceWithNulls != null && replacements != null) {
                using (var left = sourceWithNulls.GetEnumerator())
                using (var right = replacements.GetEnumerator()) {
                    while (left.MoveNext() && right.MoveNext()) {
                        yield return left.Current ?? right.Current;
                    }
                }
            }
        }

        private Expression[] Walk(IEnumerable<Expression> expressions) {
            if (expressions == null) return null;
            return expressions.Select(expr => Walk(expr)).ToArray();
        }

        private static bool HasValue(Expression[] expressions) {
            return expressions != null && expressions.Any(expr => expr != null);
        }

        // returns null if no need to rewrite that branch, otherwise
        // returns a re-written branch
        private Expression Walk(Expression expression) {
            if (expression == null) return null;
            Expression tmp;
            if (subst.TryGetValue(expression, out tmp)) return tmp;
            switch (expression.NodeType) {
                case ExpressionType.Constant:
                case ExpressionType.Parameter: {
                    return expression; // never a need to rewrite if not already matched
                }
                case ExpressionType.MemberAccess: {
                    MemberExpression me = (MemberExpression) expression;
                    Expression target = Walk(me.Expression);
                    return target == null ? null : Expression.MakeMemberAccess(target, me.Member);
                }
                case ExpressionType.Add:
                case ExpressionType.Divide:
                case ExpressionType.Multiply:
                case ExpressionType.Subtract:
                case ExpressionType.AddChecked:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.SubtractChecked:
                case ExpressionType.And:
                case ExpressionType.Or:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                case ExpressionType.Power:
                case ExpressionType.Modulo:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.LeftShift:
                case ExpressionType.RightShift:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex: {
                    BinaryExpression binExp = (BinaryExpression) expression;
                    Expression left = Walk(binExp.Left), right = Walk(binExp.Right);
                    return (left == null && right == null)
                               ? null
                               : Expression.MakeBinary(
                                   binExp.NodeType, left ?? binExp.Left, right ?? binExp.Right, binExp.IsLiftedToNull,
                                   binExp.Method, binExp.Conversion);
                }
                case ExpressionType.Not:
                case ExpressionType.UnaryPlus:
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.TypeAs:
                case ExpressionType.ArrayLength: {
                    UnaryExpression unExp = (UnaryExpression) expression;
                    Expression operand = Walk(unExp.Operand);
                    return operand == null
                               ? null
                               : Expression.MakeUnary(unExp.NodeType, operand,
                                                      unExp.Type, unExp.Method);
                }
                case ExpressionType.Conditional: {
                    ConditionalExpression ce = (ConditionalExpression) expression;
                    Expression test = Walk(ce.Test), ifTrue = Walk(ce.IfTrue), ifFalse = Walk(ce.IfFalse);
                    if (test == null && ifTrue == null && ifFalse == null) return null;
                    return Expression.Condition(test ?? ce.Test, ifTrue ?? ce.IfTrue, ifFalse ?? ce.IfFalse);
                }
                case ExpressionType.Call: {
                    MethodCallExpression mce = (MethodCallExpression) expression;
                    Expression instance = Walk(mce.Object);
                    Expression[] args = Walk(mce.Arguments);
                    if (instance == null && !HasValue(args)) return null;
                    return Expression.Call(instance, mce.Method, CoalesceTerms(args, mce.Arguments));
                }
                case ExpressionType.TypeIs: {
                    TypeBinaryExpression tbe = (TypeBinaryExpression) expression;
                    tmp = Walk(tbe.Expression);
                    return tmp == null ? null : Expression.TypeIs(tmp, tbe.TypeOperand);
                }
                case ExpressionType.New: {
                    NewExpression ne = (NewExpression) expression;
                    Expression[] args = Walk(ne.Arguments);
                    if (HasValue(args)) return null;
                    return ne.Members == null
                               ? Expression.New(ne.Constructor, CoalesceTerms(args, ne.Arguments))
                               : Expression.New(ne.Constructor, CoalesceTerms(args, ne.Arguments), ne.Members);
                }
                case ExpressionType.ListInit: {
                    ListInitExpression lie = (ListInitExpression) expression;
                    NewExpression ctor = (NewExpression) Walk(lie.NewExpression);
                    var inits = lie.Initializers.Select(init => new {
                        Original = init,
                        NewArgs = Walk(init.Arguments)
                    }).ToArray();
                    if (ctor == null && !inits.Any(init => HasValue(init.NewArgs))) return null;
                    ElementInit[] initArr = inits.Select(init => Expression.ElementInit(
                        init.Original.AddMethod, CoalesceTerms(init.NewArgs, init.Original.Arguments))).ToArray();
                    return Expression.ListInit(ctor ?? lie.NewExpression, initArr);

                }
                case ExpressionType.NewArrayBounds:
                case ExpressionType.NewArrayInit:
                    /* not quite right... leave as not-implemented for now
                {
                    NewArrayExpression nae = (NewArrayExpression)expression;
                    Expression[] expr = Walk(nae.Expressions);
                    if (!HasValue(expr)) return null;
                    return expression.NodeType == ExpressionType.NewArrayBounds
                        ? Expression.NewArrayBounds(nae.Type, CoalesceTerms(expr, nae.Expressions))
                        : Expression.NewArrayInit(nae.Type, CoalesceTerms(expr, nae.Expressions));
                }*/
                case ExpressionType.Invoke:
                case ExpressionType.Lambda:
                case ExpressionType.MemberInit:
                case ExpressionType.Quote:
                    throw new NotImplementedException("Not implemented: " + expression.NodeType);
                default:
                    throw new NotSupportedException("Not supported: " + expression.NodeType);
            }

        }
    }

    internal static class Program {
        private static void Main() {
            Expression<Func<GrandParent, Parent>> myFirst = gp => gp.Parent;
            Expression<Func<Parent, string>> mySecond = p => p.Child.Name;

            Expression<Func<GrandParent, string>> outputWithInline = myFirst.Combine(mySecond, false);
            Expression<Func<GrandParent, string>> outputWithoutInline = myFirst.Combine(mySecond, true);

            Expression<Func<GrandParent, string>> call =
                ExpressionUtils.Combine<GrandParent, Parent, string>(
                    gp => gp.Parent, p => p.Method(p.Child.Name), true);

            unchecked {
                Expression<Func<double, double>> mathUnchecked =
                    ExpressionUtils.Combine<double, double, double>(x => (x*x) + x, x => x - (x/x), true);
            }
            checked {
                Expression<Func<double, double>> mathChecked =
                    ExpressionUtils.Combine<double, double, double>(x => x - (x*x), x => (x/x) + x, true);
            }
            Expression<Func<int, int>> bitwise =
                ExpressionUtils.Combine<int, int, int>(x => (x & 0x01) | 0x03, x => x ^ 0xFF, true);
            Expression<Func<int, bool>> logical =
                ExpressionUtils.Combine<int, bool, bool>(x => x == 123, x => x != false, true);
            Expression<Func<int[][], int>> arrayAccess =
                ExpressionUtils.Combine<int[][], int[], int>(x => x[0], x => x[0], true);
            Expression<Func<string, bool>> isTest =
                ExpressionUtils.Combine<string, object, bool>(s => s, s => s is Regex, true);

            Expression<Func<List<int>>> f = () => new List<int>(new int[] {1, 1, 1}.Length);
            Expression<Func<string, Regex>> asTest =
                ExpressionUtils.Combine<string, object, Regex>(s => s, s => s as Regex, true);
            var initTest = ExpressionUtils.Combine<int, int[], List<int>>(i => new[] {i, i, i},
                                                                          arr => new List<int>(arr.Length), true);
            var anonAndListTest = ExpressionUtils.Combine<int, int, List<int>>(
                i => new {age = i}.age, i => new List<int> {i, i}, true);
            /*
            var arrBoundsInit = ExpressionUtils.Combine<int, int[], int[]>(
                i => new int[i], arr => new int[arr[0]] , true);
            var arrInit = ExpressionUtils.Combine<int, int, int[]>(
                i => i, i => new int[1] { i }, true);*/
        }
    }
}