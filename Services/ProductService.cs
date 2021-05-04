using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace GrpcProduct
{
    public class ProductService : ProductServiceProto.ProductServiceProtoBase
    {
        private readonly ILogger<ProductService> _logger;
        public ProductService(ILogger<ProductService> logger)
        {
            _logger = logger;
        }

        public override Task<ProductReply> SendProduct(ProductRequest request, ServerCallContext context)
        {
            return Task.FromResult(new ProductReply
            {
                Message = "Product - id: " + request.Id + " description: " + request.Description + " amount: " + request.Amount + " price: " + request.Price + " status: " + request.Status
            });
        }
    }
}
