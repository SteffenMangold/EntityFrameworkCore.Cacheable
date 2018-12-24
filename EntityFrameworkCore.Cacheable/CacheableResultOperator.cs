using Microsoft.EntityFrameworkCore.Query.ResultOperators;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using System;
using System.Linq.Expressions;

namespace EntityFrameworkCore.Cacheable
{
    public class CacheableResultOperator : SequenceTypePreservingResultOperatorBase, IQueryAnnotation
    {
        public CacheableResultOperator(TimeSpan timeToLive)
        {
            TimeToLive = timeToLive;
        }

        public virtual TimeSpan TimeToLive { get; }

        public virtual IQuerySource QuerySource { get; set; }

        public virtual QueryModel QueryModel { get; set; }

        public override string ToString() => $"Cacheable(\"{TimeToLive}\")";

        public override ResultOperatorBase Clone(CloneContext cloneContext)
            => new CacheableResultOperator(TimeToLive);

        public override void TransformExpressions(Func<Expression, Expression> transformation)
        {
        }

        public override StreamedSequence ExecuteInMemory<T>(StreamedSequence input) => input;
    }
}
