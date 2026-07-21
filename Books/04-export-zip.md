# Книга-игра. Часть 4: экспорт готовой книги в ZIP

## Чем ZIP отличается от JSON

В редакторе автор скачивает данные:

```text
book.json
```

При экспорте ASP.NET Core собирает готовую книгу:

```text
book-game.zip
├── index.html
├── book.js
├── book.css
└── book-data.js
```

После распаковки достаточно открыть `index.html`.

Одиночный проигрыватель поддерживает поля с ответами, но игнорирует `endForAll`.

---

## 1. Файлы шаблона

Создайте папку:

```text
wwwroot/export/
```

В ней будут:

```text
template.html
book.js
book.css
```

---

## 2. `template.html`

Создайте `wwwroot/export/template.html`:

```html
<!DOCTYPE html>
<!-- Эта страница будет открываться из распакованного ZIP без ASP.NET Core. -->
<html lang="ru">
<head>
    <!-- UTF-8 сохраняет русский текст из book-data.js. -->
    <meta charset="UTF-8">

    <!-- viewport делает экспортированную книгу удобной на разных экранах. -->
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <!-- book.js заменит этот текст на book.title после запуска. -->
    <title>Книга-игра</title>

    <!-- book.css лежит рядом с index.html внутри архива. -->
    <link rel="stylesheet" href="book.css">
</head>
<body>
    <!-- main содержит области, которыми управляет одиночный проигрыватель. -->
    <main>
        <!-- book.js запишет сюда book.title из book-data.js. -->
        <h1 id="book-title">Загрузка...</h1>

        <!-- showScene будет заменять содержимое значением scene.text. -->
        <article id="scene-text"></article>

        <!-- Здесь создаются кнопки, поля ответа или надпись о конце. -->
        <section id="choices"></section>
    </main>

    <!-- book-data.js должен выполниться первым и объявить const book. -->
    <script src="book-data.js"></script>

    <!-- book.js использует уже созданный book и открывает startSceneId. -->
    <script src="book.js"></script>
</body>
</html>
```

---

## 3. `book.js`

Код почти совпадает с `read.js`, но книга уже находится в `book-data.js`.

Создайте `wwwroot/export/book.js`:

```javascript
// Сохраняем ссылки на элементы, содержимое которых меняется при переходах.
const titleElement =
    document.getElementById("book-title");
const textElement =
    document.getElementById("scene-text");
const choicesElement =
    document.getElementById("choices");

// Одинаково нормализуем ввод читателя и answer из экспортированной книги.
function normalizeAnswer(value) {
    // Разрешаем различия вроде «ОРИОН - 73» и «орион-73».
    return value
        .trim()
        .toLowerCase()
        .replaceAll(" ", "");
}

// Для choice без answer сразу создаём кнопку, открывающую targetSceneId.
function createTransitionButton(choice) {
    const button = document.createElement("button");

    // Надпись берётся из choice.text экспортированного JSON.
    button.textContent = choice.text;

    // Клик вызывает showScene без перезагрузки index.html.
    button.addEventListener("click", () => {
        // targetSceneId связывает кнопку с объектом в book.scenes.
        showScene(choice.targetSceneId);
    });

    // showScene добавит готовый элемент в choicesElement.
    return button;
}

// Для choice с answer скрываем переход до успешной проверки.
function createAnswerChoice(choice) {
    // Контейнер удерживает поле, проверку, результат и будущую кнопку.
    const container = document.createElement("div");

    // Читатель вводит сюда найденный в сюжете ответ.
    const input = document.createElement("input");

    // type="text" подходит для слов, чисел и составных кодов.
    input.type = "text";

    // placeholder объясняет назначение пустого поля.
    input.placeholder = "Введите ответ";

    // Кнопка сравнит input.value с choice.answer.
    const checkButton = document.createElement("button");

    // «Проверить» отделяет проверку от будущего перехода.
    checkButton.textContent = "Проверить";

    // В span будет показано, принят ли ответ.
    const resultElement = document.createElement("span");

    // Сравнение выполняется после явного нажатия читателя.
    checkButton.addEventListener("click", () => {
        // normalizeAnswer применяется к обеим строкам одинаково.
        const isCorrect =
            normalizeAnswer(input.value) ===
            normalizeAnswer(choice.answer);

        // Только true открывает кнопку продолжения.
        if (isCorrect) {
            // Подтверждаем ответ перед появлением нового действия.
            resultElement.textContent = "Верно. ";

            // После успеха ответ больше не нужно редактировать.
            input.disabled = true;

            // Это не позволяет добавить несколько одинаковых кнопок.
            checkButton.disabled = true;

            // createTransitionButton использует тот же choice и targetSceneId.
            container.appendChild(
                createTransitionButton(choice)
            );
        } else {
            // При ошибке оставляем возможность попробовать снова.
            resultElement.textContent =
                "Ответ пока не подходит.";
        }
    });

    // Добавляем поле, проверку и результат в порядке показа читателю.
    container.appendChild(input);
    container.appendChild(checkButton);
    container.appendChild(resultElement);

    // showScene вставит весь контейнер как один переход.
    return container;
}

// Находим объект сцены и полностью перестраиваем видимую часть книги.
function showScene(sceneId) {
    // find возвращает объект с id, равным переданному sceneId.
    const scene = book.scenes.find(
        // targetSceneId и scene.id образуют связь между сценами.
        item => item.id === sceneId
    );

    // Отсутствующий объект означает незавершённый переход в черновике.
    if (!scene) {
        // Выводим id, который автору нужно добавить или исправить.
        textElement.textContent =
            `Сцена «${sceneId}» пока не создана.`;

        // Кнопки предыдущей сцены не должны оставаться под сообщением.
        choicesElement.replaceChildren();

        return;
    }

    // h1 получает общее название экспортированной книги.
    titleElement.textContent = book.title;

    // Вкладка браузера получает то же book.title.
    document.title = book.title;

    // article получает scene.text найденного объекта.
    textElement.textContent = scene.text;

    // Новый набор всегда строится с чистого choicesElement.
    choicesElement.replaceChildren();

    // В одиночном ZIP пустой choices означает обычный финал.
    if (scene.choices.length === 0) {
        // Вместо кнопок создаём абзац о завершении.
        const endingElement =
            document.createElement("p");

        // Сообщаем читателю, что эта ветка закончилась.
        endingElement.textContent = "Конец истории.";

        // Надпись появляется в обычной области переходов.
        choicesElement.appendChild(endingElement);

        return;
    }

    // Порядок элементов совпадает с порядком массива choices.
    for (const choice of scene.choices) {
        // answer выбирает между полем проверки и обычной кнопкой.
        const choiceElement = choice.answer
            ? createAnswerChoice(choice)
            : createTransitionButton(choice);

        // Готовый элемент появляется под текстом текущей сцены.
        choicesElement.appendChild(choiceElement);
    }
}

// После загрузки book-data.js начинаем с book.startSceneId.
showScene(book.startSceneId);
```

---

## 4. Кнопка в `edit.html`

Добавьте рядом с сохранением JSON:

```html
<!-- Кнопка экспортирует текущий объект book, включая незавершённый черновик. -->
<button id="download-zip-button" type="button">
    Скачать готовую книгу
</button>
```

---

## 5. Дополнение для `edit.js`

Рядом с остальными элементами добавьте:

```javascript
// Сохраняем ссылку на кнопку, которая отправит book в /api/book/export.
const downloadZipButton =
    document.getElementById("download-zip-button");
```

Перед `loadBook();` добавьте:

```javascript
// По клику вызываем серверную сборку ZIP.
downloadZipButton.addEventListener("click", downloadBookZip);

// Применяем форму, выполняем POST и скачиваем ответ как архив.
async function downloadBookZip() {
    // Иначе последние правки textarea не попадут в отправляемый book.
    applyCurrentScene();

    // Пока запрос выполняется, не создаём несколько одинаковых архивов.
    downloadZipButton.disabled = true;

    // Текст кнопки объясняет, почему она временно недоступна.
    downloadZipButton.textContent =
        "Создаём готовую книгу...";

    // try/catch обработает ошибку сети или серверной упаковки.
    try {
        // fetch передаёт весь объект book в Minimal API.
        const response = await fetch("api/book/export", {
            // POST нужен, потому что книга передаётся в теле запроса.
            method: "POST",

            // Заголовок позволяет ASP.NET Core разобрать тело как JSON.
            headers: {
                // application/json соответствует JSON.stringify(book).
                "Content-Type": "application/json"
            },

            // На сервер уходит текущая версия книги из памяти вкладки.
            body: JSON.stringify(book)
        });

        // response.ok показывает, удалось ли серверу собрать и вернуть архив.
        if (!response.ok) {
            // Этот текст будет показан автору в editor-message.
            throw new Error("Не удалось создать архив.");
        }

        // response.blob() сохраняет бинарный ответ без чтения как текста.
        const archiveBlob = await response.blob();

        // Blob URL позволяет скачать полученный архив обычной ссылкой.
        const archiveUrl =
            URL.createObjectURL(archiveBlob);

        // Элемент a нужен только для стандартного скачивания браузера.
        const downloadLink =
            document.createElement("a");

        // href указывает на blob, полученный от ASP.NET Core.
        downloadLink.href = archiveUrl;

        // download предлагает понятное имя book-game.zip.
        downloadLink.download = "book-game.zip";

        // Программный click передаёт архив пользователю.
        downloadLink.click();

        // После начала скачивания Blob URL больше не нужен.
        URL.revokeObjectURL(archiveUrl);

        // Сообщаем, что ZIP уже получен и передан браузеру.
        messageElement.textContent =
            "Готовая книга скачана.";
    } catch (error) {
        // Короткий текст появляется в интерфейсе редактора.
        messageElement.textContent = error.message;

        // Консоль сохраняет исходную ошибку для отладки.
        console.error(error);
    } finally {
        // finally выполняется и после успеха, и после ошибки.
        downloadZipButton.disabled = false;

        // Кнопка снова показывает обычное действие экспорта.
        downloadZipButton.textContent =
            "Скачать готовую книгу";
    }
}
```

---

## 6. ZIP в `Program.cs`

В начало файла добавьте:

```csharp
// ZipFile.CreateFromDirectory находится в System.IO.Compression.
using System.IO.Compression;

// JsonElement сохраняет гибкую структуру без Book, Scene и Choice.
using System.Text.Json;
```

Перед `app.Run();` добавьте:

```csharp
// POST /api/book/export получает JSON редактора и отвечает архивом.
app.MapPost("/api/book/export", async (JsonElement book) =>
{
    // Здесь лежат template.html, book.js и book.css для каждой книги.
    var sourceFolder = Path.Combine(
        // WebRootPath даёт абсолютный путь к статическим файлам.
        app.Environment.WebRootPath,
        // Добавляем подпапку с шаблоном одиночной книги.
        "export");

    // Каждый запрос получает отдельную папку и не мешает другому экспорту.
    var tempFolder = Path.Combine(
        // Path.GetTempPath работает в Windows и других системах.
        Path.GetTempPath(),
        // Guid почти исключает совпадение одновременных запросов.
        Guid.NewGuid().ToString("N"));

    // Архив создаётся рядом с папкой и удаляется после ответа.
    var zipPath = tempFolder + ".zip";

    // До копирования файлов каталог должен существовать.
    Directory.CreateDirectory(tempFolder);

    // finally выполнится даже при ошибке копирования или упаковки.
    try
    {
        // Пользователь откроет именно index.html после распаковки.
        File.Copy(
            // Берём template.html из wwwroot/export.
            Path.Combine(sourceFolder, "template.html"),
            // Внутри архива шаблон должен называться index.html.
            Path.Combine(tempFolder, "index.html"));

        // book.js будет выполнять переходы без ASP.NET Core.
        File.Copy(
            // Берём проигрыватель из папки шаблона.
            Path.Combine(sourceFolder, "book.js"),
            // Кладём book.js рядом с будущим index.html.
            Path.Combine(tempFolder, "book.js"));

        // Копируем book.css для оформления самостоятельной страницы.
        File.Copy(
            // Берём оформление из папки шаблона.
            Path.Combine(sourceFolder, "book.css"),
            // Кладём book.css рядом с будущим index.html.
            Path.Combine(tempFolder, "book.css"));

        // Создаём book-data.js с const book, чтобы локальный файл не использовал fetch.
        await File.WriteAllTextAsync(
            // book-data.js подключается раньше book.js.
            Path.Combine(tempFolder, "book-data.js"),
            // GetRawText сохраняет все поля, включая answer и endForAll.
            $"const book = {book.GetRawText()};");

        // В ZIP попадут все четыре файла из временной папки.
        ZipFile.CreateFromDirectory(
            // Источник — папка уже собранной самостоятельной книги.
            tempFolder,
            // Результат сохраняется во временный zipPath.
            zipPath);

        // После чтения файл можно удалить, а байты вернуть клиенту.
        var zipBytes =
            await File.ReadAllBytesAsync(zipPath);

        // Results.File формирует ответ для response.blob() на фронтенде.
        return Results.File(
            // В ответ уходят байты созданного архива.
            zipBytes,
            // application/zip сообщает браузеру формат ответа.
            "application/zip",
            // Это имя используется, если фронтенд не задаст download.
            "book-game.zip");
    }
    finally
    {
        // После чтения архива исходные файлы больше не нужны.
        if (Directory.Exists(tempFolder))
        {
            // recursive: true удаляет и файлы внутри папки.
            Directory.Delete(
                tempFolder,
                recursive: true);
        }

        // zipPath больше не нужен после загрузки байтов в память.
        if (File.Exists(zipPath))
        {
            // Освобождаем место во временной папке системы.
            File.Delete(zipPath);
        }
    }
});
```

`endForAll` попадёт в `book-data.js`, но одиночный `book.js` его не читает. Поэтому любой финал остаётся обычным одиночным финалом.
