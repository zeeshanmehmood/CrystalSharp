using CrystalSharp.Domain;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalSharp.MongoDb.Database
{
    public interface IMongoDbContext
    {
        Task SaveChanges<TDocument>(TDocument document, CancellationToken cancellationToken = default) where TDocument : IAggregateRoot<string>;
        Task<TDocument> Find<TDocument>(Guid globalUId, CancellationToken cancellationToken = default) where TDocument : IAggregateRoot<string>;
        IQueryable<TDocument> Query<TDocument>(Expression<Func<TDocument, bool>> predicate) where TDocument : IAggregateRoot<string>;
    }
}
