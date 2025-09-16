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
		public string Subject { get; set; } = string.Empty;

		[NotNull]
		public string Body { get; set; } = string.Empty;

		public bool IsAnswered { get; set; }

		public bool IsDeleted { get; set; }

		public string CreatedUtc { get; set; } = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(DateTime.UtcNow.Humanize());
	}
}


