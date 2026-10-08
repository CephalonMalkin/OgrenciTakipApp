namespace OgrenciTakipApp.Models;

public class DailyReading
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }

    public DateOnly ReadingDate { get; set; }
    public bool IsRead { get; set; }
    public DateTime MarkedAt { get; set; } = DateTime.Now;
}