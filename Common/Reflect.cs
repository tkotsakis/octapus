using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace Relational.Octapus.Common
{
    public static class Reflect<T>
    {
        public static string PropertyName<TProperty>(Expression<Func<T, TProperty>> propertySelector)
        {
            return Property(propertySelector).Name;
        }

        public static PropertyInfo Property<TProperty>(Expression<Func<T, TProperty>> propertySelector)
        {
            var lambdaExpression = (LambdaExpression)propertySelector;

            MemberExpression memberExpression;
            if (lambdaExpression.Body as UnaryExpression != null)
                memberExpression = (MemberExpression)((UnaryExpression)lambdaExpression.Body).Operand;
            else
                memberExpression = (MemberExpression)lambdaExpression.Body;

            return (PropertyInfo)memberExpression.Member;
        }

   
    }
}
