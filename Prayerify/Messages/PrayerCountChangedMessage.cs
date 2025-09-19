namespace Prayerify.Messages
{
    public class PrayerCountChangedMessage
    {
        public int NewCount { get; }

        public PrayerCountChangedMessage(int newCount)
        {
            NewCount = newCount;
        }
    }
}