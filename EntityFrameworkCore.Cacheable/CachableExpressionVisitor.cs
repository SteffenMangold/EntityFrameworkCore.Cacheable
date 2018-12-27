using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.Cacheable
{
    public class CachableExpressionVisitor : ExpressionVisitor
    {
        private Boolean _isCacheable = false;
        private TimeSpan? _timeToLive = null;

        public CachableExpressionVisitor()
        {
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.IsGenericMethod)
            {
                var genericMethodDefinition = node.Method.GetGenericMethodDefinition();

                // find cachable query extention calls
                if (genericMethodDefinition == EntityFrameworkQueryableExtensions.CacheableMethodInfo)
                {
                    // get parameter with "last one win"
                    _timeToLive = node.Arguments
                        .OfType<ConstantExpression>()
                        .Where(a => a.Value is TimeSpan)
                        .Select(a => (TimeSpan)a.Value)
                        .Last();

                    _isCacheable = true;

                    // cut out extension expression
                    return Visit(node.Arguments[0]);
                }
            }

            return base.VisitMethodCall(node);
        }

        /// <summary>
        /// Visit the query expression tree and find extract cachable parameter
        /// </summary>
        /// <param name="expression">Query expression</param>
        /// <param name="isCacheable">Is expression marked as cacheable</param>
        /// <param name="timeToLive">Timespan befor expiration of cached query result</param>
        /// <returns></returns>
        public virtual Expression GetExtractCachableParameter(Expression expression, out Boolean isCacheable, out TimeSpan? timeToLive)
        {
            var visitedExpression = Visit(expression);

            isCacheable = _isCacheable;
            timeToLive = _timeToLive;

            return visitedExpression;
        }
    }
}
