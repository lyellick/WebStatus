namespace WebStatus.Common.Models
{
    public class Status
    {
        public bool IsUp { get; set; }
        public DateTimeOffset Checked { get; set; }
    }
}
