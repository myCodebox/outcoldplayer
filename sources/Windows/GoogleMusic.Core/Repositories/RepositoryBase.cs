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
        }

        protected SQLiteAsyncConnection Connection
        {
            get
            {
                return this.dbContext.CreateConnection();
            }
        }
    }
}
