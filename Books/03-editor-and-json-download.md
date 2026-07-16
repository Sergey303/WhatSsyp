# Книга-игра. Часть 3: редактор и скачивание JSON

## Что есть на странице редактора

Редактор показывает:

- название книги;
- начальную сцену;
- список всех сцен;
- текст выбранной сцены;
- переходы выбранной сцены;
- галочку «Конец для всех»;
- кнопки добавления и удаления сцен.

Выбранная сцена записывается в адрес:

```text
/edit.html?scene=engine
```

Поэтому кнопки браузера **«Назад»** и **«Вперёд»** помогают переходить между недавно открытыми сценами.

---

## Формат переходов

Каждый переход занимает одну строку:

```text
Текст кнопки | id следующей сцены
```

Переход с ответом содержит третью часть:

```text
Открыть маршрутный замок | route-choice | 2042
```

Удалить переход можно удалением строки.

Изменить порядок — перестановкой строк.

Редактор не проверяет существование целевой сцены. Автор может сначала написать переход, скачать черновик, а сцену создать позже.

---

## 1. Полный `edit.html`

Создайте `wwwroot/edit.html`:

```html
<!DOCTYPE html>
<!-- lang="ru" сообщает браузеру и экранным дикторам язык редактора. -->
<html lang="ru">
<head>
    <!-- UTF-8 сохраняет русские тексты сцен и переходов без искажений. -->
    <meta charset="UTF-8">

    <!-- viewport позволяет пользоваться редактором в узком окне терминала. -->
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <!-- Название помогает отличить редактор от страницы чтения во второй вкладке. -->
    <title>Редактор книги-игры</title>
</head>
<body>
    <!-- main объединяет поля, которые edit.js переносит в объект book. -->
    <main>
        <!-- Заголовок сразу отделяет редактор от режима чтения. -->
        <h1>Редактор книги-игры</h1>

        <!-- label связывается с input и объясняет, какое свойство книги меняется. -->
        <label for="book-title-input">Название книги</label>

        <!-- При сохранении содержимое этого поля попадёт в book.title. -->
        <input id="book-title-input" type="text">

        <!-- Этот список управляет свойством book.startSceneId. -->
        <label for="start-scene-select">Начальная сцена</label>

        <!-- Варианты создаются из book.scenes. -->
        <select id="start-scene-select"></select>

        <!-- Создание новой сцены отделено от изменения уже выбранной. -->
        <h2>Новая сцена</h2>

        <!-- id станет адресом сцены для targetSceneId и параметра ?scene=. -->
        <label for="new-scene-id">Код новой сцены</label>

        <!-- Например, engine или secret-room; второй объект с тем же id не создаётся. -->
        <input id="new-scene-id" type="text" placeholder="secret-room">

        <!-- После нажатия сцена добавится в book.scenes и сразу откроется. -->
        <button id="add-scene-button" type="button">
            Добавить сцену
        </button>

        <!-- Ниже показываются данные только одной выбранной сцены. -->
        <h2>Сцена</h2>

        <!-- Этот список переключает открытую сцену, но не меняет startSceneId. -->
        <label for="scene-select">Выберите сцену</label>

        <!-- JavaScript добавит сюда все scene.id и будет хранить выбранный currentSceneId. -->
        <select id="scene-select"></select>

        <!-- Удаляется объект сцены; переходы к её id намеренно остаются в черновике. -->
        <button id="delete-scene-button" type="button">
            Удалить сцену
        </button>

        <!-- id выводится отдельно, чтобы его нельзя было случайно изменить вместе с текстом. -->
        <p>Код сцены: <strong id="current-scene-id"></strong></p>

        <!-- Содержимое textarea сохраняется в scene.text. -->
        <label for="scene-text-input">Текст сцены</label>

        <!-- Многострочное поле подходит для полноценного фрагмента истории. -->
        <textarea id="scene-text-input" rows="10"></textarea>

        <!-- Подпись напоминает формат каждой строки рядом с textarea. -->
        <label for="choices-text-input">
            Переходы: текст | id | необязательный ответ
        </label>

        <!-- Порядок строк станет порядком scene.choices и кнопок при чтении. -->
        <textarea id="choices-text-input" rows="8"></textarea>

        <!-- Галочка относится к текущей сцене и используется только together.js. -->
        <label>
            <!-- При снятой галочке endForAll удаляется из JSON, а не хранится как false. -->
            <input id="end-for-all-input" type="checkbox">
            Конец для всех в совместной игре
        </label>

        <!-- Кнопка переносит форму в book и выгружает весь черновик на компьютер. -->
        <button id="save-json-button" type="button">
            Сохранить и скачать JSON
        </button>

        <!-- Здесь виден результат добавления, удаления или скачивания. -->
        <p id="editor-message"></p>
    </main>

    <!-- edit.js подключён после формы, поэтому все элементы с id уже созданы. -->
    <script src="edit.js"></script>
</body>
</html>
```

---

## 2. Полный `edit.js`

Создайте `wwwroot/edit.js`:

```javascript
// После загрузки здесь находится весь изменяемый объект книги, а не только одна сцена.
let book = null;

// По этому id редактор находит объект, показанный сейчас в двух textarea.
let currentSceneId = null;

// Сохраняем ссылку на input, значение которого попадёт в book.title.
const bookTitleInput =
    document.getElementById("book-title-input");

// Этот select управляет book.startSceneId.
const startSceneSelect =
    document.getElementById("start-scene-select");

// Из этого input берётся id перед добавлением объекта в book.scenes.
const newSceneIdInput =
    document.getElementById("new-scene-id");

// На эту кнопку назначается обработчик создания или открытия сцены.
const addSceneButton =
    document.getElementById("add-scene-button");

// Этот select переключает текущую сцену и меняет ?scene= в адресе.
const sceneSelect =
    document.getElementById("scene-select");

// Обработчик кнопки удаляет текущий объект из book.scenes.
const deleteSceneButton =
    document.getElementById("delete-scene-button");

// strong показывает id отдельно от редактируемого текста.
const currentSceneIdElement =
    document.getElementById("current-scene-id");

// Значение textarea переносится в scene.text при переключении или скачивании.
const sceneTextInput =
    document.getElementById("scene-text-input");

// Каждая строка textarea будет разобрана в один объект choice.
const choicesTextInput =
    document.getElementById("choices-text-input");

// checked управляет необязательным свойством scene.endForAll.
const endForAllInput =
    document.getElementById("end-for-all-input");

// По нажатию текущий объект book скачивается как JSON-файл.
const saveJsonButton =
    document.getElementById("save-json-button");

// Здесь показываются результаты действий без всплывающих окон.
const messageElement =
    document.getElementById("editor-message");

// Получаем JSON, выбираем сцену из ?scene= и заполняем редактор.
async function loadBook() {
    // try/catch позволяет объяснить ошибку fetch прямо в интерфейсе.
    try {
        // Используем тот же /api/book, что и страница чтения.
        const response = await fetch("/api/book");

        // response.ok показывает, вернул ли сервер успешный HTTP-код.
        if (!response.ok) {
            // Этот текст будет показан в editor-message.
            throw new Error("Не удалось загрузить книгу.");
        }

        // Полученный объект можно менять в памяти до скачивания.
        book = await response.json();

        // ?scene= позволяет открыть конкретную сцену и пользоваться кнопкой «Назад».
        const sceneIdFromUrl = getSceneIdFromUrl();

        // Не открываем id из URL, если такого объекта нет в book.scenes.
        const sceneExists = book.scenes.some(
            // some возвращает true при первом совпадении scene.id.
            scene => scene.id === sceneIdFromUrl
        );

        // При пустом или неправильном ?scene= открываем book.startSceneId.
        currentSceneId = sceneExists
            ? sceneIdFromUrl
            : book.startSceneId;

        // Заполняем input текущим book.title.
        bookTitleInput.value = book.title;

        // Один select переключает редактор, второй выбирает начало книги.
        renderSceneLists();

        // Заполняем textarea, id и галочку значениями выбранного объекта.
        renderCurrentScene();

        // Записываем фактически открытую сцену в URL без лишнего шага для «Назад».
        replaceSceneInUrl(currentSceneId);
    } catch (error) {
        // Сообщение загрузки появляется в editor-message.
        messageElement.textContent = error.message;

        // Исходный объект error остаётся в инструментах разработчика.
        console.error(error);
    }
}

// Возвращаем значение параметра scene из текущего адреса редактора.
function getSceneIdFromUrl() {
    // URLSearchParams отделяет параметры после знака ?.
    const parameters =
        new URLSearchParams(window.location.search);

    // При отсутствии параметра метод возвращает null.
    return parameters.get("scene");
}

// Исправляем ?scene=, не добавляя страницу в историю браузера.
function replaceSceneInUrl(sceneId) {
    // URL позволяет изменить один параметр и сохранить остальные части адреса.
    const url = new URL(window.location.href);

    // set добавляет scene или заменяет прежнее значение.
    url.searchParams.set("scene", sceneId);

    // replaceState не вызывает новый запрос и не создаёт шаг для «Назад».
    history.replaceState({}, "", url);
}

// Запоминаем переход на другую сцену для кнопок «Назад» и «Вперёд».
function pushSceneToUrl(sceneId) {
    // URL позволяет изменить один параметр и сохранить остальные части адреса.
    const url = new URL(window.location.href);

    // set добавляет scene или заменяет прежнее значение.
    url.searchParams.set("scene", sceneId);

    // pushState создаёт новый шаг истории без перезагрузки edit.html.
    history.pushState({}, "", url);
}

// Находим в book.scenes объект с id из currentSceneId.
function getCurrentScene() {
    // find возвращает сам объект, поэтому его свойства можно менять напрямую.
    return book.scenes.find(
        // Уникальный scene.id связывает список, URL и JSON.
        scene => scene.id === currentSceneId
    );
}

// Формируем текст второй textarea из массива scene.choices.
function choicesToText(choices) {
    // map сохраняет порядок переходов из JSON.
    return choices
        .map(choice => {
            // Первые две части нужны обычному переходу.
            let line =
                `${choice.text} | ${choice.targetSceneId}`;

            // Необязательное answer добавляет третью часть только для загадки.
            if (choice.answer) {
                // Формат становится: текст | targetSceneId | answer.
                line += ` | ${choice.answer}`;
            }

            // Одна строка объекта займёт одну строку textarea.
            return line;
        })
        // join("\n") делает каждый переход отдельной строкой.
        .join("\n");
}

// Разбираем вторую textarea и строим новый массив scene.choices.
function textToChoices(text) {
    // Один перенос строки отделяет один переход от другого.
    return text
        .split("\n")
        // Пустые строки разрешены и не создают лишних choice.
        .filter(line => line.trim() !== "")
        // map создаёт choice в том же порядке, что и строки.
        .map(sourceLine => {
            // Первая часть — текст, вторая — id, остальные снова собираются в answer.
            const parts = sourceLine.split("|");

            // Даже пустой текст сохраняется: редактор допускает незавершённый черновик.
            const choiceText =
                (parts[0] ?? "").trim();

            // Существование targetSceneId намеренно не проверяется.
            const targetSceneId =
                (parts[1] ?? "").trim();

            // join позволяет самому ответу содержать символ |.
            const answer = parts
                .slice(2)
                .join("|")
                .trim();

            // Сначала записываем обязательные поля text и targetSceneId.
            const choice = {
                // trim уже удалил только пробелы по краям.
                text: choiceText,

                // Автор может создать targetSceneId позже.
                targetSceneId: targetSceneId
            };

            // Пустая третья часть не должна создавать answer: "".
            if (answer !== "") {
                // Страница чтения увидит answer и сначала покажет поле ввода.
                choice.answer = answer;
            }

            // Объект попадёт в новый массив scene.choices.
            return choice;
        });
}

// Перед переключением или скачиванием переносим текущую форму в объект книги.
function applyCurrentScene() {
    // До завершения loadBook записывать данные некуда.
    if (!book) {
        return;
    }

    // Используем currentSceneId, а не select, который уже мог изменить значение.
    const scene = getCurrentScene();

    // После удаления или неправильного id объект может отсутствовать.
    if (!scene) {
        return;
    }

    // Обновляем общее свойство book.title из верхнего input.
    book.title = bookTitleInput.value;

    // Выбранный option становится book.startSceneId.
    book.startSceneId = startSceneSelect.value;

    // Полностью заменяем scene.text содержимым первой textarea.
    scene.text = sceneTextInput.value;

    // Новый choices строится из второй textarea сверху вниз.
    scene.choices = textToChoices(
        choicesTextInput.value
    );

    // Только отмеченная сцена должна завершать совместную игру у всех.
    if (endForAllInput.checked) {
        // true попадёт в JSON и будет прочитано together.js.
        scene.endForAll = true;
    } else {
        // Обычные сцены остаются короче и понятнее.
        delete scene.endForAll;
    }
}

// Перестраиваем два select после добавления или удаления сцены.
function renderSceneLists() {
    // Сохраняем startSceneId до очистки option.
    const selectedStartSceneId = book.startSceneId;

    // Иначе после повторного вызова сцены появились бы несколько раз.
    sceneSelect.replaceChildren();

    // Второй select тоже строится заново из актуального book.scenes.
    startSceneSelect.replaceChildren();

    // Для каждого объекта создаём option в каждом списке.
    for (const scene of book.scenes) {
        // Этот option открывает сцену для изменения.
        const sceneOption =
            document.createElement("option");

        // sceneSelect.value можно сразу передать в openScene.
        sceneOption.value = scene.id;

        // В простой модели отдельного названия сцены нет.
        sceneOption.textContent = scene.id;

        sceneSelect.appendChild(sceneOption);

        // Один DOM-option нельзя поместить одновременно в два select.
        const startOption =
            document.createElement("option");

        // startSceneSelect тоже хранит scene.id.
        startOption.value = scene.id;

        // Автор видит одинаковые обозначения в обоих списках.
        startOption.textContent = scene.id;

        startSceneSelect.appendChild(startOption);
    }

    // После перестройки возвращаем currentSceneId в список редактора.
    sceneSelect.value = currentSceneId;

    // В списке начала восстанавливаем book.startSceneId.
    startSceneSelect.value = selectedStartSceneId;
}

// Заполняем интерфейс значениями объекта из currentSceneId.
function renderCurrentScene() {
    // Используем currentSceneId, а не select, который уже мог изменить значение.
    const scene = getCurrentScene();

    // После удаления или неправильного id объект может отсутствовать.
    if (!scene) {
        return;
    }

    // id выводится отдельно и не редактируется в textarea.
    currentSceneIdElement.textContent = scene.id;

    // Первая textarea получает scene.text.
    sceneTextInput.value = scene.text;

    // choicesToText переводит массив JSON в удобный авторский формат.
    choicesTextInput.value =
        choicesToText(scene.choices);

    // Галочка отмечена только при точном scene.endForAll === true.
    endForAllInput.checked =
        scene.endForAll === true;

    // select синхронизируется с currentSceneId после истории или создания.
    sceneSelect.value = currentSceneId;

    // Сообщение прежнего действия не должно относиться к новой сцене.
    messageElement.textContent = "";
}

// Сначала сохраняем текущую форму, затем меняем currentSceneId и интерфейс.
function openScene(sceneId, addToHistory) {
    // Это обновляет book в текущей вкладке, но ещё не скачивает файл.
    applyCurrentScene();

    // Запоминаем id сцены, которую теперь должен показывать редактор.
    currentSceneId = sceneId;

    // Заполняем форму данными новой сцены без запроса к серверу.
    renderCurrentScene();

    // true передаётся при выборе сцены, но не при обработке popstate.
    if (addToHistory) {
        // pushState создаёт новый шаг истории без перезагрузки edit.html.
        pushSceneToUrl(sceneId);
    }
}

// Превращаем название книги в имя, разрешённое в Windows.
function createFileName(title) {
    // Такие пробелы не должны стать дефисами в начале и конце имени.
    const trimmedTitle = title.trim();

    // Windows не разрешает эти знаки; пробелы заменяем дефисами.
    const safeTitle = trimmedTitle
        .replace(/[\\/:*?"<>|]/g, "-")
        .replaceAll(" ", "-");

    // При пустом названии файл всё равно получит имя book.json.
    return safeTitle || "book";
}

// Обновляем book из формы и создаём из него файл на компьютере автора.
function saveAndDownloadJson() {
    // В файл должны попасть последние изменения открытой сцены.
    applyCurrentScene();

    // null, 2 добавляет отступы для удобного чтения вручную.
    const json = JSON.stringify(book, null, 2);

    // Blob хранит JSON до скачивания и не отправляет его на сервер.
    const file = new Blob(
        // Единственным содержимым Blob становится строка json.
        [json],

        // MIME-тип и UTF-8 помогают системе распознать файл и русский текст.
        { type: "application/json;charset=utf-8" }
    );

    // Blob URL позволяет браузеру скачать созданный в памяти файл.
    const fileUrl = URL.createObjectURL(file);

    // Элемент a нужен только для программного запуска скачивания.
    const downloadLink =
        document.createElement("a");

    // href указывает на созданный Blob.
    downloadLink.href = fileUrl;

    // download предлагает понятное имя вместо случайного URL.
    downloadLink.download =
        createFileName(book.title) + ".json";

    // Программный click работает как нажатие обычной ссылки.
    downloadLink.click();

    // После начала скачивания Blob URL больше не нужен.
    URL.revokeObjectURL(fileUrl);

    // Сообщаем, что текущая версия book уже выгружена в JSON.
    messageElement.textContent =
        "Изменённый JSON скачан.";
}

// Выбор option открывает сцену и добавляет её URL в историю.
sceneSelect.addEventListener("change", () => {
    // true включает pushState для кнопки браузера «Назад».
    openScene(sceneSelect.value, true);
});

// Выбор начала сразу обновляет startSceneId без отдельной кнопки.
startSceneSelect.addEventListener("change", () => {
    // Значение select уже является id существующей сцены.
    book.startSceneId = startSceneSelect.value;
});

// Добавляем объект с id, начальным текстом и пустым choices.
addSceneButton.addEventListener("click", () => {
    // Перед переключением переносим изменения текущей сцены в book.
    applyCurrentScene();

    // trim не позволяет пробелам стать частью scene.id.
    const newSceneId =
        newSceneIdInput.value.trim();

    // Сцена без id не сможет быть целью перехода.
    if (newSceneId === "") {
        // Просим заполнить поле вместо создания неправильного объекта.
        messageElement.textContent =
            "Введите код новой сцены.";

        return;
    }

    // Один id должен обозначать одну сцену во всей книге.
    const alreadyExists = book.scenes.some(
        // Ищем объект, у которого scene.id совпадает с введённым newSceneId.
        scene => scene.id === newSceneId
    );

    // Повторный id не создаёт дубликат, а открывает найденную сцену.
    if (alreadyExists) {
        // openScene покажет объект и обновит URL.
        openScene(newSceneId, true);

        // После открытия id больше не нужен в форме создания.
        newSceneIdInput.value = "";

        return;
    }

    // push изменяет массив book.scenes в памяти вкладки.
    book.scenes.push({
        // Это значение смогут использовать targetSceneId и ?scene=.
        id: newSceneId,

        // Автор сразу видит, какое поле нужно заменить сюжетом.
        text: "Напишите здесь, что произошло.",

        // Пустой choices временно делает сцену финальной.
        choices: []
    });

    newSceneIdInput.value = "";

    // renderCurrentScene должен показать созданный объект.
    currentSceneId = newSceneId;

    // Новая сцена появляется в редакторе и среди вариантов начала.
    renderSceneLists();

    // textarea заполняются начальными значениями.
    renderCurrentScene();

    // ?scene=newSceneId сохраняет прежнюю сцену для «Назад».
    pushSceneToUrl(newSceneId);
});

// Удаляем объект из book.scenes, но не исправляем переходы других сцен.
deleteSceneButton.addEventListener("click", () => {
    // Книге нужна хотя бы одна сцена для startSceneId.
    if (book.scenes.length === 1) {
        // Объясняем, почему последнюю сцену удалить нельзя.
        messageElement.textContent =
            "В книге должна остаться хотя бы одна сцена.";

        return;
    }

    // Индекс нужен splice и выбору соседней сцены после удаления.
    const sceneIndex = book.scenes.findIndex(
        // Ищем позицию объекта, который сейчас открыт в редакторе.
        scene => scene.id === currentSceneId
    );

    // splice изменяет book.scenes прямо в памяти.
    book.scenes.splice(sceneIndex, 1);

    // Берём следующую или предыдущую, если удалена последняя.
    const nextScene = book.scenes[
        Math.min(sceneIndex, book.scenes.length - 1)
    ];

    // startSceneId не должен ссылаться на отсутствующий объект.
    if (book.startSceneId === currentSceneId) {
        // Ближайшая оставшаяся сцена становится новым началом.
        book.startSceneId = nextScene.id;
    }

    // После удаления редактор должен открыть существующий объект.
    currentSceneId = nextScene.id;

    // Удалённая сцена исчезает из обоих списков.
    renderSceneLists();

    // Показываем текст и переходы выбранной оставшейся сцены.
    renderCurrentScene();

    // replaceState убирает из URL id удалённой сцены.
    replaceSceneInUrl(currentSceneId);

    // Сообщение напоминает, что незавершённые ссылки оставлены намеренно.
    messageElement.textContent =
        "Сцена удалена. Переходы к ней оставлены в черновике.";
});

// Нажатие переносит форму в book и выгружает файл.
saveJsonButton.addEventListener("click", () => {
    // saveAndDownloadJson выполняет оба действия одной операцией.
    saveAndDownloadJson();
});

// popstate срабатывает при переходе по сохранённым ?scene=.
window.addEventListener("popstate", () => {
    // Перед уходом переносим textarea в прежний объект.
    applyCurrentScene();

    // После «Назад» URL уже изменён браузером.
    const sceneIdFromUrl = getSceneIdFromUrl();

    // В истории мог остаться id позднее удалённой сцены.
    const sceneExists = book.scenes.some(
        // Проверяем, остался ли в книге id, полученный из истории браузера.
        scene => scene.id === sceneIdFromUrl
    );

    // Редактор остаётся на текущем существующем объекте.
    if (!sceneExists) {
        // replaceState синхронизирует URL с открытой сценой.
        replaceSceneInUrl(currentSceneId);

        return;
    }

    // currentSceneId получает id, выбранный кнопкой браузера.
    currentSceneId = sceneIdFromUrl;

    // Интерфейс обновляется без нового запроса к серверу.
    renderCurrentScene();
});

// Запускаем loadBook после объявления функций и обработчиков.
loadBook();
```

---

## Как работает удаление

Удаляется только объект сцены.

Переходы к её id остаются в книге:

```text
Открыть старую дверь | deleted-room
```

Это позволяет удалить сцену временно и создать её заново позже.

Кнопка **«Сохранить и скачать JSON»** всегда выгружает текущий черновик, даже если часть переходов пока не согласована.
