namespace OgrenciTakipApp.Models;

public class Student
{
    public int Id { get; set; }
    public string StudentNumber { get; set; } = string.Empty; // Okul No
    public string FullName { get; set; } = string.Empty;
    public string AccessPin { get; set; } = string.Empty;     // 4 haneli veli kodu

    public int TeacherId { get; set; }
    public Teacher? Teacher { get; set; }

    public ICollection<DailyReading> DailyReadings { get; set; } = new List<DailyReading>();
}