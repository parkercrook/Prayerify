using SQLite;

namespace Prayerify.Models
{
	public class Category
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }

		[Unique, NotNull]
		public string Name { get; set; } = string.Empty;

		public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
	}
}


