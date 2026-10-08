# OgrenciTakipApp

OgrenciTakipApp, okullarda veya kurumlarda ogrencilerin gunluk kitap okuma sureclerini takip etmek amaciyla gelistirilmis ASP.NET Core MVC tabanli bir web uygulamasidir. Sistem; ogretmenlerin sinif bazli gunluk takip yapmasini, Excel uzerinden toplu ogrenci ice aktarabilmesini ve ogrencilerin ya da velilerin kisisel okuma gecmislerini goruntuleyebilmesini saglar.

## Mimari ve Teknolojik Altyapi

Uygulama Mimarisi: ASP.NET Core MVC (.NET 8)
Veritabani ve ORM: Entity Framework Core (Code-First)
Veritabani Saglayicisi: Gelistirme ortaminda SQLite, kurumsal gecis icin MSSQL veya PostgreSQL uyumlu
Excel Motoru: ClosedXML (Dosyalari diske kaydetmeden bellek uzerinde MemoryStream ile isler)
Arayuz: Bootstrap 5 tabanli, Dark ve Light tema destekli responsive tasarim

## Gelistirme Ortaminda Kurulum

Projeyi klonladiktan sonra terminal uzerinden proje kok dizininde su komutlar sirasiyla calistirilmalidir:

1. Bagimliliklari yukleyin:
dotnet restore

2. Uygulamayi baslatin:
dotnet run

Uygulama varsayilan olarak https://localhost:5001 veya http://localhost:5000 portunda ayaga kalkar.

## Erisim ve Kimlik Dogrulama Yapisi

Ogretmen Paneli: /Teacher/Login
Gelistirme ve test erisimi icin gerekli hesap bilgileri yetkili gelistirici tarafindan saglanir.

Ogrenci ve Veli Paneli:
Giris islemi; ogrenci okul numarasi ve toplu ice aktarma sirasinda sistem tarafindan kisiye ozel uretilen 4 haneli erisim PIN kodu (AccessPin) ile yapilir.

## Temel Moduller ve Calisma Mantigi

1. Ogretmen Takip Modulu
Dashboard uzerinden tarih secici kullanilarak geriye donuk gunluk okuma verileri filtrelenebilir.
Ekranda toplam ogrenci sayisi ve secilen tarihte kitap okuyan ogrenci sayisini gosteren ozet metrikler yer alir.
Her ogrencinin gecmis tum okuma kayitlari detayli karne sayfasinda listelenir.

2. Toplu Ogrenci Yukleme (ClosedXML Entegrasyonu)
TeacherController icerisindeki DownloadTemplate metodu ile sistem standart bir .xlsx sablonu uretip indirir.
UploadExcel metodu ile yuklenen dosyadaki ogrenciler veritabanina aktarilir.
Mukerrer kayit engelleme mekanizmasi sayesinde, sistemde zaten kayitli olan okul numaralari tekrar eklenmez, yalnizca yeni ogrenciler ice aktarilir.
Ice aktarilan her yeni ogrenciye sistem tarafindan rastgele ve benzersiz 4 haneli bir giris PIN kodu uretilir.

3. Arayuz ve Tema Yonetimi
Arayuz mobil ve masaustu ekranlara uyumlu sekilde duzenlenmistir.
Kullanici tercihlerine gore Dark ve Light tema gecisi dinamik olarak calisir.

## Kurumsal Altyapiya Gecis Notlari

Veritabani Degisimi: Proje bagimsiz calisabilmesi adina yerel SQLite ile kurgulanmistir. Kurumsal ortama tasinirken Program.cs icerisindeki DbContext tanimi ilgili provider (UseSqlServer veya UseNpgsql) ile guncellenip appsettings.json altindaki connection string verilerek kolayca baglanabilir.

Kimlik Dogrulama: Gelistirme surumunde dogrulama veritabani eslesmesi uzerinden yonetilmektedir. Ihtiyaca gore ASP.NET Core Identity, JWT veya LDAP/Active Directory servislerine entegre edilebilir.

Dagitim (Deployment): Uygulama .NET Core yapisi geregi Docker konteynerlerinde, Linux sunucularda (Nginx reverse proxy arkasinda Kestrel) veya Windows Server IIS uzerinde calismaya hazirdir.
