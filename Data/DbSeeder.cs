using OgrenciTakipApp.Models;

namespace OgrenciTakipApp.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        // Veritabanı tablolarının var olduğundan emin ol
        context.Database.EnsureCreated();

        // Eğer sistemde kayıtlı öğretmen yoksa örnek verileri ekle
        if (!context.Teachers.Any())
        {
            var teacher = new Teacher
            {
                FullName = "Ahmet Yılmaz",
                Username = "ogretmen",
                Password = "123" // Test için başlangıç şifresi
            };

            context.Teachers.Add(teacher);
            context.SaveChanges();

            var student1 = new Student
            {
                FullName = "Ali Kaya",
                StudentNumber = "101",
                AccessPin = "1234",
                TeacherId = teacher.Id
            };

            var student2 = new Student
            {
                FullName = "Zeynep Demir",
                StudentNumber = "102",
                AccessPin = "5678",
                TeacherId = teacher.Id
            };

            context.Students.AddRange(student1, student2);
            context.SaveChanges();
        }
    }
}