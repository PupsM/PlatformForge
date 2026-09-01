
---

```markdown
# PlatformImage

**Легковесная кроссплатформенная библиотека для загрузки изображений на .NET 10**  
С поддержкой BMP, TGA, PNG и безопасным кодом.

---

## 🤖 Об этом проекте

**Важное примечание:**  
Этот проект написан **при помощи искусственного интеллекта DeepSeek** (архитектура, реализация, тестирование).  
Код является **оригинальной реализацией**, вдохновлённой опытом работы с `stb_image`, но написанной **с нуля на C#**.

**Почему я указываю это?**  
- Это честно перед сообществом.  
- ИИ — мой инструмент, как компилятор или IDE.  
- Архитектура (интерфейсы, фабрика, безопасный код) — результат совместной работы со мной и DeepSeek.

**Важно:** Алгоритмы декодирования реализованы **строго по официальным спецификациям форматов**, а не путём копирования кода из других библиотек.

---

## 📦 Что внутри?

| Компонент | Описание |
| :--- | :--- |
| **Core/** | ImageData, PixelFormat, ImageLoader, Diagnostics |
| **Decoders/** | BmpDecoder, TgaDecoder, PngDecoder, IImageDecoder, ImageDecoderFactory |
| **Utils/** | BinaryReaderExtensions |
| **IO/** | ImageLoader (основной API для загрузки) |

---

## 🚀 Быстрый старт

### 1. Установка (через ссылку на проект)

```xml
<ItemGroup>
  <ProjectReference Include="..\PlatformImage\PlatformImage.csproj" />
</ItemGroup>
```

### 2. Базовый пример

```csharp
using PlatformImage.IO;
using PlatformImage.Core;

// Загрузка изображения из файла
var image = ImageLoader.Load("image.png");

// Доступ к данным
Console.WriteLine($"Размер: {image.Width}x{image.Height}");
Console.WriteLine($"Формат: {image.Format}");
Console.WriteLine($"Альфа-канал: {image.HasAlpha}");
Console.WriteLine($"Всего байт: {image.TotalBytes}");

// Доступ к пиксельным данным
ReadOnlySpan<byte> pixels = image.Data;

// Работа с пикселями (например, получить цвет первого пикселя в RGBA)
if (image.Format == PixelFormat.RGBA)
{
    byte r = pixels[0];
    byte g = pixels[1];
    byte b = pixels[2];
    byte a = pixels[3];
}

// Освобождение ресурсов
image.Dispose();
```

### 3. Загрузка из потока

```csharp
using var stream = File.OpenRead("image.bmp");
var image = ImageLoader.Load(stream);
```

### 4. Получение информации без загрузки

```csharp
// Быстрое получение информации о файле без полной загрузки
var info = ImageLoader.GetInfo("image.tga");
Console.WriteLine($"Размер: {info.Width}x{info.Height}, Формат: {info.Format}");
```

---

## 🎯 Возможности

### Поддерживаемые форматы

| Формат | Поддержка | Особенности |
| :--- | :--- | :--- |
| **BMP** | ✅ 24-bit RGB, 32-bit RGBA | Автоматическая конвертация BGR → RGB |
| **TGA** | ✅ 24-bit RGB, 32-bit RGBA | Поддержка RLE-сжатия, BGR → RGB |
| **PNG** | ✅ 24-bit RGB, 32-bit RGBA | Поддержка фильтров (0-4), zlib-распаковка |

### ImageData

```csharp
var image = ImageLoader.Load("image.png");

// Свойства
int width = image.Width;
int height = image.Height;
PixelFormat format = image.Format;
int bytesPerPixel = image.BytesPerPixel;  // 3 для RGB, 4 для RGBA
int stride = image.Stride;                 // Ширина * BytesPerPixel
int totalBytes = image.TotalBytes;
bool hasAlpha = image.HasAlpha;

// Доступ к данным (без копирования)
ReadOnlySpan<byte> data = image.Data;

// Копирование данных (для безопасного хранения)
var copy = image.Copy();

// Освобождение
image.Dispose();
```

### Глобальные настройки

```csharp
// Переворачивать изображения по вертикали при загрузке
ImageLoader.FlipVerticallyOnLoad = true;

// Теперь все изображения загружаются перевёрнутыми
var image = ImageLoader.Load("image.png");

// Сброс
ImageLoader.FlipVerticallyOnLoad = false;
```

### Обработка ошибок

```csharp
try
{
    var image = ImageLoader.Load("unknown.xyz");
}
catch (UnsupportedFormatException ex)
{
    Console.WriteLine($"Неподдерживаемый формат: {ex.Message}");
}
catch (ImageLoaderException ex)
{
    Console.WriteLine($"Ошибка загрузки: {ex.Message}");
}
catch (EndOfStreamException ex)
{
    Console.WriteLine($"Файл повреждён или обрезан: {ex.Message}");
}
```

### Диагностика

```csharp
// Подписка на логи
Diagnostics.OnLog += (message) =>
{
    Console.WriteLine($"[ImageLoader] {message}");
};

// Уровень логирования
Diagnostics.CurrentLevel = Diagnostics.LogLevel.Debug;
```

---

## 🏗️ Архитектура

### Диаграмма классов

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         ImageLoader (статический)                          │
│  + FlipVerticallyOnLoad : bool                                            │
│  + Load(string) : ImageData                                               │
│  + Load(Stream) : ImageData                                               │
│  + GetInfo(string) : ImageInfo                                            │
│  + GetInfo(Stream) : ImageInfo                                            │
└──────────────────────────┬─────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                     ImageDecoderFactory (статический)                      │
│  + GetDecoder(Stream) : IImageDecoder?                                    │
│  - Decoders : List<IImageDecoder>                                         │
└──────────────────────────┬─────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                         IImageDecoder                                      │
│  + CanDecode(Stream) : bool                                               │
│  + Decode(Stream) : ImageData                                             │
│  + GetInfo(Stream) : ImageInfo                                            │
└──────────┬──────────────────┬──────────────────┬──────────────────────────┘
           │                  │                  │
           ▼                  ▼                  ▼
┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
│   BmpDecoder     │ │   TgaDecoder     │ │   PngDecoder     │
│  - BMP 24/32-bit │ │  - TGA 24/32-bit │ │  - PNG 24/32-bit │
│  - BGR → RGB     │ │  - RLE support   │ │  - zlib support  │
│  - Flip vertical │ │  - BGR → RGB     │ │  - Filters 0-4   │
└──────────────────┘ └──────────────────┘ └──────────────────┘
```

### Особенности реализации

1. **Безопасный код**  
   Проект **не использует `unsafe`**. Вместо этого применяются безопасные операции с массивами и `ReadOnlySpan`.

2. **Отказоустойчивость**  
   Проверки на `EndOfStream`, пустые данные, неподдерживаемые форматы.

3. **Модульная архитектура**  
   Каждый декодер реализует интерфейс `IImageDecoder`, что позволяет легко добавлять новые форматы.

4. **Автоматический выбор декодера**  
   `ImageDecoderFactory` определяет формат по сигнатуре файла.

5. **Диагностика**  
   Встроенный класс `Diagnostics` с поддержкой уровней логирования.

6. **Эффективность**  
   `ImageData` использует `ReadOnlySpan` для доступа к данным без копирования.

---

## 🔌 Расширяемость

Хотите добавить поддержку нового формата (например, JPEG, GIF, WebP)?

```csharp
public class JpegDecoder : IImageDecoder
{
    public bool CanDecode(Stream stream)
    {
        // Проверка сигнатуры JPEG (FF D8 FF)
        // ...
    }

    public ImageData Decode(Stream stream)
    {
        // Декодирование JPEG
        // ...
    }

    public ImageInfo GetInfo(Stream stream)
    {
        // Получение информации без полной загрузки
        // ...
    }
}

// Регистрация в фабрике
// В ImageDecoderFactory добавьте:
// Decoders.Add(new JpegDecoder());
```

---

## 📚 Источники вдохновения

При разработке декодеров использовались официальные спецификации форматов:

- **PNG**: [RFC 2083](https://www.w3.org/TR/PNG/) — спецификация Portable Network Graphics
- **BMP**: [Microsoft Windows Bitmap Specification](https://learn.microsoft.com/en-us/windows/win32/gdi/bitmap-storage)
- **TGA**: [TrueVision TGA Specification](https://www.fileformat.info/format/tga/egff.htm)

Алгоритмы реализованы **с нуля** на C# с использованием безопасного кода.  
Проект вдохновлён опытом работы с `stb_image`, но **не является портом или копией** — это самостоятельная реализация.

---

## 🛠️ Требования к системе

- **.NET 10** или выше
- **Нет внешних зависимостей** (только стандартные библиотеки .NET)

---

## 🧪 Проверка на плагиат

Этот код НЕ скопирован из `stb_image`, `stb_image_write` или других open-source библиотек.  
Все архитектурные решения (интерфейсы, фабрика, безопасный код) — **оригинальны** и созданы в диалоге с DeepSeek.

При проверке через MOSS/Turnitin вы найдете:
- Совпадения с математическими алгоритмами (Paeth Predictor, PNG фильтры) — **это часть спецификаций, а не чужого кода**.
- Совпадения с названиями чанков PNG (`IHDR`, `IDAT`, `IEND`) — **это часть спецификации**.
- **Ни одной строки кода**, скопированной из `stb_image` или других проектов.

---

## 📄 Лицензия

MIT — делайте что хотите, но с указанием авторства.

---

## 🌟 Благодарности

- **DeepSeek** — за генерацию кода, рефакторинг и объяснение алгоритмов.
- Сообществу .NET — за документацию по `Stream`, `DeflateStream` и `ReadOnlySpan`.
- Авторам спецификаций PNG, BMP и TGA — за чёткую документацию форматов.

---

## 🤝 Контакты

Если у вас есть вопросы по архитектуре или вы нашли баг — открывайте Issue.

---

_Пишу код вместе с ИИ, а не вместо него._
```

---