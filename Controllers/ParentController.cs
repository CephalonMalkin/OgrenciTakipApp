using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OgrenciTakipApp.Data;
using OgrenciTakipApp.Models;

namespace OgrenciTakipApp.Controllers;

public class ParentController : Controller
{
    private readonly AppDbContext _context;

    public ParentController(AppDbContext context)
    {
        _context = context;
    }

    // 1. Veli Giriş Ekranı (GET)
    public IActionResult Login()
    {
        return View();
    }

    // 2. Veli Giriş Kontrolü (POST)
    [HttpPost]
    public async Task<IActionResult> Login(string studentNumber, string accessPin)
    {
        if (string.IsNullOrWhiteSpace(studentNumber) || string.IsNullOrWhiteSpace(accessPin))
        {
            ViewBag.Error = "Lütfen okul numarası ve PIN kodunu giriniz.";
            return View();
        }

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.StudentNumber == studentNumber && s.AccessPin == accessPin);

        if (student == null)
        {
            ViewBag.Error = "Okul numarası veya PIN kodu hatalı!";
            return View();
        }

        // Giriş başarılı, okuma takip sayfasına yönlendir
        return RedirectToAction("Dashboard", new { id = student.Id });
    }

    // 3. Öğrencinin Okuma Durum Paneli (GET)
    public async Task<IActionResult> Dashboard(int id)
    {
        var student = await _context.Students
            .Include(s => s.DailyReadings)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null) return NotFound();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var hasReadToday = student.DailyReadings.Any(r => r.ReadingDate == today && r.IsRead);

        ViewBag.HasReadToday = hasReadToday;
        ViewBag.Today = today.ToString("dd.MM.yyyy");

        return View(student);
    }

    // 4. "Bugün Okundu" Butonuna Basıldığında Çalışan İşlem (POST)
    [HttpPost]
    public async Task<IActionResult> MarkAsRead(int studentId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Zaten işaretlenmiş mi kontrol et (Mükerrer kaydı engeller)
        var existingRecord = await _context.DailyReadings
            .FirstOrDefaultAsync(r => r.StudentId == studentId && r.ReadingDate == today);

        if (existingRecord == null)
        {
            var reading = new DailyReading
            {
                StudentId = studentId,
                ReadingDate = today,
                IsRead = true,
                MarkedAt = DateTime.Now
            };

            _context.DailyReadings.Add(reading);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Dashboard", new { id = studentId });
    }
}