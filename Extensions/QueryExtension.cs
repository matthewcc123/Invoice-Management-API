using InvoiceManagement.Api.Enum;
using InvoiceManagement.Api.Models;
using System.Linq.Expressions;
using System.Reflection;

namespace InvoiceManagement.Api.Extensions
{
    public static class QueryExtension
    {

        public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> src,string? propertyName , object? value)
        {
            //Nothing to Filter
            if (value == null || string.IsNullOrEmpty(propertyName)) return src;

            //Find Property on target Model
            var propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (propertyInfo == null) return src;

            Expression body;
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyInfo);

            //String
            if (propertyInfo.PropertyType == typeof(string))
            {
                var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
                var containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;

                var notNull = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
                var propertyToLower = Expression.Call(property, toLowerMethod);
                var searchValue = Expression.Constant(value.ToString()!.ToLower());

                body = Expression.AndAlso(notNull, Expression.Call(propertyToLower, containsMethod, searchValue));
            }
            //Date Range
            else if (value is DateRange range && (
                propertyInfo.PropertyType == typeof(DateTime) || propertyInfo.PropertyType == typeof(DateTime?) ||
                propertyInfo.PropertyType == typeof(DateOnly) || propertyInfo.PropertyType == typeof(DateOnly?) ||
                propertyInfo.PropertyType == typeof(DateTimeOffset) || propertyInfo.PropertyType == typeof(DateTimeOffset?)))
            {
                Expression propertyValue = property;
                Expression? hasValue = null;

                //Handle nullable types
                if (Nullable.GetUnderlyingType(property.Type) != null)
                {
                    hasValue = Expression.Property(property, "HasValue");
                    propertyValue = Expression.Property(property, "Value");
                }

                Expression? filter = null;

                if (propertyValue.Type == typeof(DateOnly))
                {
                    if (range.From.HasValue)
                    {
                        var from = Expression.Constant(DateOnly.FromDateTime(range.From.Value.ToUniversalTime()), typeof(DateOnly));
                        filter = Expression.GreaterThanOrEqual(propertyValue, from);
                    }
                    if (range.To.HasValue)
                    {
                        var to = Expression.Constant(DateOnly.FromDateTime(range.To.Value.ToUniversalTime()), typeof(DateOnly));
                        var upper = Expression.LessThanOrEqual(propertyValue, to);
                        filter = filter == null ? upper : Expression.AndAlso(filter, upper);
                    }
                }
                else if (propertyValue.Type == typeof(DateTime))
                {
                    if (range.From.HasValue)
                    {
                        var from = Expression.Constant(range.From.Value.ToUniversalTime().Date, typeof(DateTime));
                        filter = Expression.GreaterThanOrEqual(propertyValue, from);
                    }
                    if (range.To.HasValue)
                    {
                        var to = Expression.Constant(range.To.Value.ToUniversalTime().Date.AddDays(1), typeof(DateTime));
                        var upper = Expression.LessThan(propertyValue, to);
                        filter = filter == null ? upper : Expression.AndAlso(filter, upper);
                    }
                }
                else if (propertyValue.Type == typeof(DateTimeOffset))
                {
                    if (range.From.HasValue)
                    {
                        var from = Expression.Constant(new DateTimeOffset(range.From.Value.ToUniversalTime().Date), typeof(DateTimeOffset));
                        filter = Expression.GreaterThanOrEqual(propertyValue, from);
                    }
                    if (range.To.HasValue)
                    {
                        var to = Expression.Constant(new DateTimeOffset(range.To.Value.ToUniversalTime().Date.AddDays(1)), typeof(DateTimeOffset));
                        var upper = Expression.LessThan(propertyValue, to);
                        filter = filter == null ? upper : Expression.AndAlso(filter, upper);
                    }
                }

                if (hasValue != null && filter != null) filter = Expression.AndAlso(hasValue, filter);
                body = filter!;
            }
            //Equality
            else
            {
                var targetType = Nullable.GetUnderlyingType(propertyInfo.PropertyType)
                                 ?? propertyInfo.PropertyType;

                object convertedValue;

                if (targetType.IsEnum)
                {
                    convertedValue = value is string s
                        ? System.Enum.Parse(targetType, s, true)
                        : System.Enum.ToObject(targetType, value);
                }
                else
                {
                    convertedValue = Convert.ChangeType(value, targetType)!;
                }

                var constant = Expression.Constant(convertedValue, targetType);

                Expression right = property.Type == targetType
                    ? constant
                    : Expression.Convert(constant, property.Type);

                body = Expression.Equal(property, right);
            }

            //Create Lambda x => x.PropertyName == value
            return src.Where(Expression.Lambda<Func<T, bool>>(body, parameter));

        }

        public static IQueryable<T> ApplySearch<T>(this IQueryable<T> source, string? search, params Expression<Func<T, string?>>[] properties)
        {
            if (string.IsNullOrWhiteSpace(search) || properties.Length == 0)
                return source;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? body = null;

            var containsMethod = typeof(string).GetMethod(
                nameof(string.Contains),
                new[] { typeof(string) })!;

            var toLowerMethod = typeof(string).GetMethod(
                nameof(string.ToLower),
                Type.EmptyTypes)!;

            var searchConstant = Expression.Constant(search.ToLower());

            foreach (var property in properties)
            {
                var propertyBody = new ReplaceParameterVisitor(property.Parameters[0], parameter).Visit(property.Body)!;

                var notNull = Expression.NotEqual(
                    propertyBody,
                    Expression.Constant(null, typeof(string)));

                var toLower = Expression.Call(propertyBody, toLowerMethod);
                var contains = Expression.Call(toLower, containsMethod, searchConstant);

                var condition = Expression.AndAlso(notNull, contains);

                body = body == null
                    ? condition
                    : Expression.OrElse(body, condition);
            }

            var lambda = Expression.Lambda<Func<T, bool>>(body!, parameter);

            return source.Where(lambda);
        }

        public static IQueryable<T> ApplySort<T>(this IQueryable<T> src, string? propertyName, SortOrder? order)
        {
            //Nothing to Filter
            if (order == null || string.IsNullOrEmpty(propertyName))
            {
                return src;
            }

            //Find Property on target Model
            var propertyInfo = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            //Noting to Filter if not Found
            if (propertyInfo == null)
                return src;

            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyInfo); 

            var keySelector = Expression.Lambda(property, parameter);  // x => x.propertyName

            var methodName = order == SortOrder.Asc
                ? nameof(Queryable.OrderBy)
                : nameof(Queryable.OrderByDescending);

            var method = typeof(Queryable).GetMethods().Single(m => m.Name == methodName && m.GetParameters().Length == 2);

            var genericMethod = method.MakeGenericMethod(typeof(T), propertyInfo.PropertyType);

            return (IQueryable<T>)genericMethod.Invoke(null, new object[] { src, keySelector })!;

        }

        public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> src, int pageNumber, int pageSize)
        {
            return src.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }


        private sealed class ReplaceParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParameter;
            private readonly ParameterExpression _newParameter;

            public ReplaceParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _oldParameter ? _newParameter : base.VisitParameter(node);
            }
        }

    }
}
