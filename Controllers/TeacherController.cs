using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OgrenciTakipApp.Data;
using OgrenciTakipApp.Models;
using ClosedXML.Excel;
using System.IO;

namespace OgrenciTakipApp.Controllers;

public class TeacherController : Controller
{
    private readonly AppDbContext _context;

    public TeacherController(AppDbContext context)
    {
        _context = context;
    }

    // 1. Öğretmen Giriş Sayfası (GET)
    public IActionResult Login()
    {
        return View();
    }

    // 2. Giriş Doğrulama (POST)
    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Kullanıcı adı ve şifre zorunludur.";
            return View();
        }

        var teacher = await _context.Teachers
            .FirstOrDefaultAsync(t => t.Username == username && t.Password == password);

        if (teacher == null)
        {
            ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
            return View();
        }

        return RedirectToAction("Dashboard", new { teacherId = teacher.Id });
    }

    // 3. Sınıf Genel Durumu & Tarih Filtreli Takip (GET)
    public async Task<IActionResult> Dashboard(int teacherId, string? selectedDate)
    {
        var teacher = await _context.Teachers.FindAsync(teacherId);
        if (teacher == null) return NotFound();

        DateOnly targetDate = string.IsNullOrEmpty(selectedDate)
            ? DateOnly.FromDateTime(DateTime.Today)
            : DateOnly.Parse(selectedDate);

        var students = await _context.Students
            .Where(s => s.TeacherId == teacherId)
            .Include(s => s.DailyReadings.Where(r => r.ReadingDate == targetDate))
            .OrderBy(s => s.StudentNumber)
            .ToListAsync();

        ViewBag.Teacher = teacher;
        ViewBag.SelectedDate = targetDate.ToString("yyyy-MM-dd");
        ViewBag.DisplayDate = targetDate.ToString("dd.MM.yyyy");

        return View(students);
    }

    // 4. Öğrenci Bazlı Geçmiş Karnesi (GET)
    public async Task<IActionResult> StudentHistory(int id)
    {
        var student = await _context.Students
            .Include(s => s.Teacher)
            .Include(s => s.DailyReadings)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null) return NotFound();

        student.DailyReadings = student.DailyReadings
            .OrderByDescending(r => r.ReadingDate)
            .ToList();

        return View(student);
    }

    // 5. Yeni Öğrenci Ekleme Sayfası (GET)
    public IActionResult CreateStudent(int teacherId)
    {
        ViewBag.TeacherId = teacherId;
        return View();
    }

    // 6. Yeni Öğrenci Kaydetme (POST) - Otomatik PIN Üretimi
    [HttpPost]
    public async Task<IActionResult> CreateStudent(Student student)
    {
        if (string.IsNullOrWhiteSpace(student.FullName) || string.IsNullOrWhiteSpace(student.StudentNumber))
        {
            ViewBag.Error = "Ad Soyad ve Öğrenci No alanları zorunludur.";
            ViewBag.TeacherId = student.TeacherId;
            return View(student);
        }

        bool exists = await _context.Students.AnyAsync(s => s.StudentNumber == student.StudentNumber);
        if (exists)
        {
            ViewBag.Error = "Bu öğrenci numarası zaten kullanımda!";
            ViewBag.TeacherId = student.TeacherId;
            return View(student);
        }

        student.AccessPin = Random.Shared.Next(1000, 10000).ToString();

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        return RedirectToAction("Dashboard", new { teacherId = student.TeacherId });
    }

    // 7. Öğrenci Silme İşlemi (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStudent(int id, int teacherId)
    {
        var student = await _context.Students.FindAsync(id);
        if (student != null)
        {
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Dashboard", new { teacherId = teacherId });
    }

    // 8. Excel Şablonu İndirme (GET)
    [HttpGet]
    public IActionResult DownloadTemplate()
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Ogrenciler");

            worksheet.Cell(1, 1).Value = "OgrenciNo";
            worksheet.Cell(1, 2).Value = "AdSoyad";

            worksheet.Cell(2, 1).Value = "101";
            worksheet.Cell(2, 2).Value = "Ahmet Yılmaz";

            worksheet.Cell(3, 1).Value = "102";
            worksheet.Cell(3, 2).Value = "Ayşe Demir";

            var header = worksheet.Range("A1:B1");
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Columns().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Ogrenci_Sablonu.xlsx");
            }
        }
    }

    // 9. Excel Dosyası ile Toplu Öğrenci Yükleme (POST)
    [HttpPost]
    public async Task<IActionResult> UploadExcel(IFormFile excelFile, int teacherId)
    {
        if (excelFile == null || excelFile.Length == 0)
        {
            TempData["Error"] = "Lütfen bir Excel dosyası seçin.";
            return RedirectToAction("Dashboard", new { teacherId = teacherId });
        }

        try
        {
            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                    int eklenenSayisi = 0;

                    foreach (var row in rows)
                    {
                        var studentNumber = row.Cell(1).GetValue<string>()?.Trim();
                        var fullName = row.Cell(2).GetValue<string>()?.Trim();

                        if (string.IsNullOrEmpty(studentNumber) || string.IsNullOrEmpty(fullName))
                            continue;

                        bool varMi = await _context.Students.AnyAsync(s => s.StudentNumber == studentNumber);
                        if (!varMi)
                        {
                            var yeniOgrenci = new Student
                            {
                                StudentNumber = studentNumber,
                                FullName = fullName,
                                TeacherId = teacherId,
                                AccessPin = Random.Shared.Next(1000, 10000).ToString()
                            };

                            _context.Students.Add(yeniOgrenci);
                            eklenenSayisi++;
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"{eklenenSayisi} adet yeni öğrenci sınıfa eklendi.";
                }
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Excel okunurken hata oluştu: " + ex.Message;
        }

        return RedirectToAction("Dashboard", new { teacherId = teacherId });
    }
}