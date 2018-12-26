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
        // TODO In newer EF version inherit from ParameterExtractingExpressionVisitor

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

                if (genericMethodDefinition == EntityFrameworkQueryableExtensions.CacheableMethodInfo)
                {
                    _timeToLive = node.Arguments
                        .OfType<ConstantExpression>()
                        .Where(a => a.Value is TimeSpan)
                        .Select(a => (TimeSpan)a.Value)
                        .Single();

                    _isCacheable = true;
                }

                // how to remove expression from tree??
                //node.Update()
            }


            return base.VisitMethodCall(node);
        }

        public virtual Expression GetExtractCachableParameter(Expression expression, out Boolean isCacheable, out TimeSpan? timeToLive)
        {
            var visitedExpression = Visit(expression);

            isCacheable = _isCacheable;
            timeToLive = _timeToLive;

            return visitedExpression;
        }
    }
}
