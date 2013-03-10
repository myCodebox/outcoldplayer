// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using SQLite;

    public class RepositoryBase
    {
        private readonly DbContext dbContext;

        public RepositoryBase()
        {
            this.dbContext = new DbContext();
            this.Connection = this.dbContext.CreateConnection();
        }

        protected SQLiteAsyncConnection Connection { get; private set; }
    }
}
