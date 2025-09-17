using SQLite;
using Humanizer;
using System.Globalization;

namespace Prayerify.Models
{
	public class Prayer
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }

		[Indexed]
		public int? CategoryId { get; set; }

		[NotNull]
		public string PrayerTitle { get; set; } = string.Empty;

		[NotNull]
		public string PrayerDescription { get; set; } = string.Empty;

		public bool IsAnswered { get; set; }

		public bool IsDeleted { get; set; }

		public string CreatedUtc { get; set; } = DateTime.Now.ToString("MMMM dd, yyyy");

		// Non-database property for display purposes
		[Ignore]
		public string CategoryName { get; set; } = string.Empty;
	}
}


