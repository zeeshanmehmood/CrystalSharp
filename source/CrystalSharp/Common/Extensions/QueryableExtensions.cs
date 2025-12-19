using System.Linq;
using System.Linq.Expressions;

namespace CrystalSharp.Common.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> queryable, string propertyName)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T));
            MemberExpression memberExpression = Expression.PropertyOrField(parameterExpression, propertyName);
            LambdaExpression lambdaExpression = Expression.Lambda(memberExpression, parameterExpression);
            MethodCallExpression methodCallExpression = Expression.Call(
                typeof(Queryable),
                "OrderBy",
                [typeof(T), memberExpression.Type],
                queryable.Expression,
                Expression.Quote(lambdaExpression));

            return queryable.Provider.CreateQuery<T>(methodCallExpression);
        }

        public static IQueryable<T> OrderByDescending<T>(this IQueryable<T> queryable, string propertyName)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T));
            MemberExpression memberExpression = Expression.PropertyOrField(parameterExpression, propertyName);
            LambdaExpression lambdaExpression = Expression.Lambda(memberExpression, parameterExpression);
            MethodCallExpression methodCallExpression = Expression.Call(
                typeof(Queryable),
                "OrderByDescending",
                [typeof(T), memberExpression.Type],
                queryable.Expression,
                Expression.Quote(lambdaExpression));

            return queryable.Provider.CreateQuery<T>(methodCallExpression);
        }
    }
}
