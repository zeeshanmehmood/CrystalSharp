using System.Linq;
using System.Linq.Expressions;

namespace CrystalSharp.Common.Extensions
{
    public static class QueryableExtensions
    {
        extension<T>(IQueryable<T> queryable)
        {
            public IQueryable<T> OrderBy(string propertyName)
            {
                return DoOrdering<T>(queryable, propertyName, "OrderBy");
            }

            public IQueryable<T> OrderByDescending(string propertyName)
            {
                return DoOrdering<T>(queryable, propertyName, "OrderByDescending");
            }

            private static IQueryable<T> DoOrdering(IQueryable<T> queryableSource, string propertyName, string orderingMethod)
            {
                ParameterExpression parameterExpression = Expression.Parameter(typeof(T));
                MemberExpression memberExpression = Expression.PropertyOrField(parameterExpression, propertyName);
                LambdaExpression lambdaExpression = Expression.Lambda(memberExpression, parameterExpression);
                MethodCallExpression methodCallExpression = Expression.Call(
                    typeof(Queryable),
                    orderingMethod,
                    [typeof(T), memberExpression.Type],
                    queryableSource.Expression,
                    Expression.Quote(lambdaExpression));

                return queryableSource.Provider.CreateQuery<T>(methodCallExpression);
            }
        }
    }
}
