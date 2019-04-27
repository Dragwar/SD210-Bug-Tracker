using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;

namespace BugTracker.MyHelpers
{
    /// <summary>
    /// Credit to <see cref="https://www.automatetheplanet.com/get-property-names-using-lambda-expressions/"/>
    /// </summary>
    public static class ObjectHelper
    {
        private static readonly string expressionCannotBeNullMessage = "The expression cannot be null";
        private static readonly string invalidExpressionMessage = "Invalid expression";

        public static string GetMemberName<T>(this T instance, Expression<Func<T, object>> expression)
        {
            return GetMemberName(expression.Body);
        }

        public static List<string> GetMemberNames<T>(this T instance, params Expression<Func<T, object>>[] expressions)
        {
            if ((expressions?.Length ?? 0) <= 0)
            {
                return instance.GetType().GetProperties().Select(p => p.Name).ToList();
            }

            List<string> memberNames = new List<string>();
            foreach (Expression<Func<T, object>> cExpression in expressions)
            {
                memberNames.Add(GetMemberName(cExpression.Body));
            }

            return memberNames;
        }

        public static string GetMemberName<T>(this T instance, Expression<Action<T>> expression)
        {
            return GetMemberName(expression.Body);
        }

        private static string GetMemberName(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentException(expressionCannotBeNullMessage);
            }

            if (expression is MemberExpression memberExpression)
            {
                // Reference type property or field
                return memberExpression.Member.Name;
            }

            if (expression is MethodCallExpression methodCallExpression)
            {
                // Reference type method
                return methodCallExpression.Method.Name;
            }

            if (expression is UnaryExpression unaryExpression)
            {
                // Property, field of method returning value type
                if (unaryExpression.Operand is MethodCallExpression methodExpression)
                {
                    return methodExpression.Method.Name;
                }
                return ((MemberExpression)unaryExpression.Operand).Member.Name;
            }

            throw new ArgumentException(invalidExpressionMessage);
        }

        /// <summary>Ignores private members. This will clone a object via JSON</summary>
        public static T Clone<T>(this T source)
        {
            string json = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}