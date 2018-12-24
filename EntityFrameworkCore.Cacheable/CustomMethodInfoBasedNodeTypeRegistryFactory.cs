using Microsoft.EntityFrameworkCore.Query.Internal;
using Remotion.Linq.Parsing.Structure;

namespace EntityFrameworkCore.Cacheable
{
    internal class CustomMethodInfoBasedNodeTypeRegistryFactory : DefaultMethodInfoBasedNodeTypeRegistryFactory
    {
        public override INodeTypeProvider Create()
        {
            RegisterMethods(CacheableExpressionNode.SupportedMethods, typeof(CacheableExpressionNode));
            return base.Create();
        }
    }
}
