using Microsoft.EntityFrameworkCore;

namespace Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TranslationModel> Translations { get; set; }
    }

    public class TranslationModel
    {
        public int Id { get; set; }
        public int RowId { get; set; }
        public string LangCode { get; set; } = "";
        public string WordKey { get; set; } = "";
        public string EnglishValue { get; set; } = "";
        public string ArabicValue { get; set; } = "";
    }
}