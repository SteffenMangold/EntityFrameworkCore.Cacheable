using Microsoft.EntityFrameworkCore.Query.Internal;
using Remotion.Linq.Parsing.Structure;

namespace EntityFrameworkCore.Cacheable
{
    /// <summary>
    /// Extended <see cref="DefaultMethodInfoBasedNodeTypeRegistryFactory"/> implementation to support <see cref="CacheableExpressionNode"/>.
    /// </summary>
    internal class CustomMethodInfoBasedNodeTypeRegistryFactory : DefaultMethodInfoBasedNodeTypeRegistryFactory
    {
        public override INodeTypeProvider Create()
        {
            // add expression to supported list
            RegisterMethods(CacheableExpressionNode.SupportedMethods, typeof(CacheableExpressionNode));

            return base.Create();
        }
    }
}
