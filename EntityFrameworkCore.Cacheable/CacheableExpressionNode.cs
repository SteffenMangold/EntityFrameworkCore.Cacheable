using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace EntityFrameworkCore.Cacheable
{
    public class CacheableExpressionNode : ResultOperatorExpressionNodeBase
    {
        public static readonly IReadOnlyCollection<MethodInfo> SupportedMethods = new[]
            { EntityFrameworkQueryableExtensions.CacheablehMethodInfo };

        private readonly ConstantExpression _cacheableExpression;

        public CacheableExpressionNode(
            MethodCallExpressionParseInfo parseInfo, ConstantExpression cacheableExpression)
            : base(parseInfo, null, null)
            => _cacheableExpression = cacheableExpression;

        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext)
            => new CacheableResultOperator((TimeSpan)_cacheableExpression.Value);

        public override Expression Resolve(
            ParameterExpression inputParameter,
            Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
            => Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
    }
}
