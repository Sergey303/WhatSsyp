# Книга-игра. Часть 5: совместное прохождение через SignalR

## Идея совместной игры

Все участники открывают одну книгу, но каждый выбирает путь самостоятельно.

```text
Игрок 1 → архив
Игрок 2 → пассажиры
Игрок 3 → технический тоннель
```

В разных ветках находятся разные части ответа. Участники сидят в одном классе и могут обмениваться находками вслух.

Серверу не нужно хранить маршруты игроков.

SignalR используется только для двух общих событий:

```text
сцена с endForAll: true
        ↓
общий финал у всех

перезапуск
        ↓
все возвращаются в начало
```

Обычный тупик завершает путь только одного игрока. Он может начать заново и продолжить помогать команде.

---

## 1. Поле общей сцены

Обычный конец:

```json
{
  "id": "empty-city",
  "text": "Ты остался в пустом городе.",
  "choices": []
}
```

Общий конец:

```json
{
  "id": "black-line",
  "text": "Все маршруты исчезли.",
  "choices": [],
  "endForAll": true
}
```

Поле может обозначать как общую победу, так и общее поражение. Смысл задаёт текст сцены.

---

## 2. Полный `together.html`

Создайте `wwwroot/together.html`:

```html
<!DOCTYPE html>
<!-- lang="ru" сообщает браузеру язык совместной страницы. -->
<html lang="ru">
<head>
    <!-- UTF-8 сохраняет русские тексты книги и сообщения SignalR. -->
    <meta charset="UTF-8">

    <!-- viewport помогает открыть страницу в окнах разной ширины. -->
    <meta name="viewport" content="width=device-width, initial-scale=1.0">

    <!-- Название отличает совместный режим от редактора и одиночного чтения. -->
    <title>Совместная книга-игра</title>
</head>
<body>
    <!-- main содержит локальную сцену игрока и состояние соединения с хабом. -->
    <main>
        <!-- together.js заменит текст значением book.title после загрузки JSON. -->
        <h1 id="book-title">Подключение...</h1>

        <!-- Каждый браузер показывает здесь собственную текущую сцену. -->
        <article id="scene-text"></article>

        <!-- Локальные кнопки, поля ответа и кнопки перезапуска создаются здесь. -->
        <section id="choices"></section>

        <!-- Сообщения о подключении не смешиваются с текстом сюжета. -->
        <p id="connection-message"></p>
    </main>

    <!-- Библиотека создаёт HubConnectionBuilder и связь с /gameHub. -->
    <script src="/lib/microsoft/signalr/dist/browser/signalr.js"></script>

    <!-- together.js использует хаб только для общего финала и перезапуска. -->
    <script src="together.js"></script>
</body>
</html>
```

---

## 3. Полный `together.js`

Создайте `wwwroot/together.js`:

```javascript
// После fetch здесь находится общий JSON, но каждый браузер выбирает свой маршрут.
let book = null;

// true блокирует личные переходы после сообщения GameEndedForAll от хаба.
let gameEndedForAll = false;

// Сохраняем ссылки на название, текст, переходы и строку состояния SignalR.
const titleElement =
    document.getElementById("book-title");
const textElement =
    document.getElementById("scene-text");
const choicesElement =
    document.getElementById("choices");
const connectionMessageElement =
    document.getElementById("connection-message");

// Настраиваем клиент, который вызывает методы GameHub и получает его сообщения.
const connection = new signalR.HubConnectionBuilder()
    // URL должен совпадать с app.MapHub<GameHub>("/gameHub").
    .withUrl("/gameHub")
    // После краткого разрыва клиент сам попробует подключиться снова.
    .withAutomaticReconnect()
    // build возвращает connection для start, invoke и connection.on.
    .build();

// Одинаково нормализуем ввод игрока и answer из JSON.
function normalizeAnswer(value) {
    // Команда может произнести код вслух, а игрок ввести его с другим регистром или пробелами.
    return value
        .trim()
        .toLowerCase()
        .replaceAll(" ", "");
}

// Обычный choice меняет сцену только в текущем браузере и не вызывает хаб.
function createTransitionButton(choice) {
    const button = document.createElement("button");

    // Надпись берётся из choice.text.
    button.textContent = choice.text;

    // Клик не меняет страницы остальных участников.
    button.addEventListener("click", () => {
        // false означает: сцену открыл сам игрок, а не сообщение GameEndedForAll.
        showScene(choice.targetSceneId, false);
    });

    // showScene добавит кнопку в choicesElement текущей вкладки.
    return button;
}

// Для choice.answer сначала показываем поле общего кода и проверку.
function createAnswerChoice(choice) {
    // Контейнер удерживает input, проверку, результат и будущую кнопку.
    const container = document.createElement("div");

    // Один игрок вводит сюда ответ, который команда могла собрать из разных веток.
    const input = document.createElement("input");

    // Поле принимает слова, числа и составные коды.
    input.type = "text";

    // placeholder сообщает, что ожидается общий ответ команды.
    input.placeholder = "Введите общий ответ";

    // Нажатие сравнит input.value с choice.answer локально.
    const checkButton = document.createElement("button");

    // «Проверить» отделяет проверку общего кода от перехода по сюжету.
    checkButton.textContent = "Проверить";

    // В span будет показано, принят ли введённый код.
    const resultElement = document.createElement("span");

    // До клика игрок может исправлять ввод после обсуждения.
    checkButton.addEventListener("click", () => {
        // normalizeAnswer применяется к обеим строкам одинаково.
        const isCorrect =
            normalizeAnswer(input.value) ===
            normalizeAnswer(choice.answer);

        // Только совпадение открывает локальную кнопку продолжения.
        if (isCorrect) {
            // Подтверждаем команде, что код подошёл.
            resultElement.textContent = "Верно. ";

            // После успеха ответ больше не нужно редактировать.
            input.disabled = true;

            // Иначе каждый клик добавлял бы новую одинаковую кнопку.
            checkButton.disabled = true;

            // Кнопка появится только в этом браузере и откроет targetSceneId.
            container.appendChild(
                createTransitionButton(choice)
            );
        } else {
            // При ошибке команда может исследовать другие ветки и попробовать снова.
            resultElement.textContent =
                "Ответ пока не подходит.";
        }
    });

    // Помещаем поле, проверку и результат в один контейнер.
    container.appendChild(input);
    container.appendChild(checkButton);
    container.appendChild(resultElement);

    // showScene вставит контейнер под текстом сцены.
    return container;
}

// Пустой choices без endForAll завершает маршрут только текущего браузера.
function showLocalEnding() {
    // Абзац объясняет, что остальные участники продолжают игру.
    const endingElement =
        document.createElement("p");

    // Локальный тупик не отправляет сообщений в GameHub.
    endingElement.textContent =
        "Твой путь завершён. Команда продолжает исследование.";

    // Кнопка возвращает только эту вкладку к book.startSceneId.
    const restartButton =
        document.createElement("button");

    // Надпись подчёркивает, что перезапустится только этот игрок.
    restartButton.textContent = "Начать свой путь заново";

    // Обработчик меняет только локальную сцену.
    restartButton.addEventListener("click", () => {
        // false не разрешает обходить уже наступивший общий финал.
        showScene(book.startSceneId, false);
    });

    // Сначала показываем объяснение локального конца.
    choicesElement.appendChild(endingElement);

    // Затем даём игроку сразу вернуться в исследование.
    choicesElement.appendChild(restartButton);
}

// Обрабатываем sceneId, который хаб прислал сообщением GameEndedForAll.
function showGlobalEnding(sceneId) {
    // После true личные кнопки перестают менять сцену.
    gameEndedForAll = true;

    // true сообщает showScene, что EndGameForAll уже обработан хабом.
    showScene(sceneId, true);

    // Любой участник может попросить хаб вернуть всю команду к началу.
    const restartButton =
        document.createElement("button");

    // Надпись предупреждает, что действие затронет все браузеры.
    restartButton.textContent = "Начать заново всей командой";

    // По клику вызываем метод RestartGame у GameHub.
    restartButton.addEventListener("click", async () => {
        // invoke вызывает хаб; переход произойдёт после сообщения GameRestarted.
        await connection.invoke("RestartGame");
    });

    // Кнопка появляется только на общей синхронизированной сцене.
    choicesElement.appendChild(restartButton);
}

// Строим локальную сцену или общую сцену, полученную от SignalR.
// openedFromHub равен true только для GameEndedForAll и GameRestarted.
function showScene(sceneId, openedFromHub) {
    // После GameEndedForAll сцену может менять только обработчик хаба.
    if (gameEndedForAll && !openedFromHub) {
        // Игнорируем поздний клик по кнопке прежней локальной сцены.
        return;
    }

    // Находим объект по id, который выбрал игрок или прислал хаб.
    const scene = book.scenes.find(
        // targetSceneId и сообщения SignalR используют те же scene.id.
        item => item.id === sceneId
    );

    // Неизвестный id означает незавершённый переход в JSON.
    if (!scene) {
        // Выводим отсутствующий id в текущем браузере.
        textElement.textContent =
            `Сцена «${sceneId}» пока не создана.`;

        // Кнопки прежней сцены не должны оставаться под ошибкой.
        choicesElement.replaceChildren();

        // Без найденной сцены строить текст и переходы невозможно.
        return;
    }

    // Все участники видят одно book.title.
    titleElement.textContent = book.title;

    // Обычно sceneId различается; при общем финале он одинаков у всех.
    textElement.textContent = scene.text;

    // Перед новой сценой очищаем choicesElement.
    choicesElement.replaceChildren();

    // endForAll требует синхронизировать эту сцену через GameHub.
    if (scene.endForAll === true) {
        // Если сцену уже открыл GameEndedForAll, второй EndGameForAll не нужен.
        if (!openedFromHub) {
            // invoke вызывает GameHub.EndGameForAll и передаёт scene.id.
            connection.invoke("EndGameForAll", scene.id)
                // Если invoke не дошёл до хаба, сообщаем об этом в строке связи.
                .catch(error => {
                    connectionMessageElement.textContent =
                        error.message;
                });
        }

        // Локальные кнопки не создаём: хаб пришлёт эту сцену всем через GameEndedForAll.
        return;
    }

    // Пустой choices без endForAll не влияет на других игроков.
    if (scene.choices.length === 0) {
        // Добавляем сообщение и кнопку личного перезапуска.
        showLocalEnding();

        // После локального тупика переходы уже заменены сообщением и кнопкой личного старта.
        return;
    }

    // Каждый браузер самостоятельно строит элементы своего маршрута.
    for (const choice of scene.choices) {
        // answer добавляет поле ввода; без answer создаётся кнопка.
        const choiceElement = choice.answer
            ? createAnswerChoice(choice)
            : createTransitionButton(choice);

        // Готовый элемент появляется только в текущей вкладке.
        choicesElement.appendChild(choiceElement);
    }
}

// Сначала получаем общий JSON, затем открываем соединение с /gameHub.
async function startGame() {
    // В одном catch обрабатываем загрузку книги и первое подключение.
    try {
        // Все участники загружают одну и ту же книгу с /api/book.
        const response = await fetch("api/book");

        // При 404 или 500 подключаться к игре без книги бессмысленно.
        if (!response.ok) {
            // Текст попадёт в connection-message.
            throw new Error("Не удалось загрузить книгу.");
        }

        // После этого можно строить локальные переходы из book.scenes.
        book = await response.json();

        // connection.start регистрирует браузер для общих сообщений.
        await connection.start();

        // Участник видит, что общий финал теперь сможет прийти с сервера.
        connectionMessageElement.textContent =
            "Совместная игра подключена.";

        // false означает обычное локальное открытие.
        showScene(book.startSceneId, false);
    } catch (error) {
        // Ошибка сети появляется в строке состояния.
        connectionMessageElement.textContent = error.message;

        // Консоль сохраняет исходный error для отладки.
        console.error(error);
    }
}

// Регистрируем обработчик сообщения GameEndedForAll до connection.start().
connection.on("GameEndedForAll", sceneId => {
    // Каждый браузер открывает sceneId и блокирует личные переходы.
    showGlobalEnding(sceneId);
});

// GameRestarted приходит всем после вызова RestartGame на хабе.
connection.on("GameRestarted", () => {
    // После false локальные переходы снова разрешены.
    gameEndedForAll = false;

    // true разрешает обработчику хаба изменить сцену после финала.
    showScene(book.startSceneId, true);
});

// onreconnecting срабатывает, когда автоматическое переподключение началось.
connection.onreconnecting(() => {
    // Пока связь не вернулась, общие сообщения могут задержаться.
    connectionMessageElement.textContent =
        "Связь потеряна. Переподключаемся...";
});

// onreconnected срабатывает после успешной повторной связи.
connection.onreconnected(() => {
    // Участник снова может получать GameEndedForAll и GameRestarted.
    connectionMessageElement.textContent =
        "Связь восстановлена.";
});

// Обработчики SignalR уже зарегистрированы, теперь вызываем startGame.
startGame();
```

---

## 4. Код хаба

Добавьте методы в существующий хаб или создайте `GameHub.cs`:

```csharp
// Hub даёт доступ к Clients и Context для связи с браузерами.
using Microsoft.AspNetCore.SignalR;

// Простой вариант хранит одно общее состояние для всех подключений приложения.
public class GameHub : Hub
{
    // lock(Sync) не позволяет двум игрокам одновременно записать разные первые финалы.
    private static readonly object Sync = new();

    // null означает, что игра продолжается; строка нужна новым подключениям.
    private static string? endingSceneId;

    // Новый или обновивший страницу клиент должен узнать об уже наступившем финале.
    public override async Task OnConnectedAsync()
    {
        // Копия используется после выхода из lock во время await.
        string? currentEndingSceneId;

        // Пока копируем endingSceneId, другой запрос не может его изменить.
        lock (Sync)
        {
            // Берём id, который был записан первым вызовом EndGameForAll.
            currentEndingSceneId = endingSceneId;
        }

        // При null новый участник просто продолжит подключение.
        if (currentEndingSceneId is not null)
        {
            // Clients.Caller отправляет GameEndedForAll только этому браузеру.
            await Clients.Caller.SendAsync(
                "GameEndedForAll",
                currentEndingSceneId);
        }

        // Базовый метод завершает стандартную обработку подключения Hub.
        await base.OnConnectedAsync();
    }

    // Метод вызывает браузер, который первым дошёл до scene.endForAll.
    public async Task EndGameForAll(string sceneId)
    {
        // isFirstGlobalEnding станет true только у первого принятого финала.
        var isFirstGlobalEnding = false;

        // lock объединяет проверку null и запись sceneId в одну операцию.
        lock (Sync)
        {
            // Поздний игрок не должен заменить сцену, уже показанную команде.
            if (endingSceneId is null)
            {
                // endingSceneId понадобится текущим и новым подключениям.
                endingSceneId = sceneId;

                // Только первый запрос должен разослать GameEndedForAll.
                isFirstGlobalEnding = true;
            }
        }

        // Повторные вызовы заканчиваются без второй рассылки.
        if (isFirstGlobalEnding)
        {
            // Clients.All рассылает GameEndedForAll с одним sceneId.
            await Clients.All.SendAsync(
                "GameEndedForAll",
                sceneId);
        }
    }

    // Метод может вызвать любой участник с общей финальной сцены.
    public async Task RestartGame()
    {
        // lock не позволяет одновременно перезапустить и записать новый финал.
        lock (Sync)
        {
            // null снова означает, что команда играет.
            endingSceneId = null;
        }

        // GameRestarted заставит все клиенты снять блокировку и открыть startSceneId.
        await Clients.All.SendAsync("GameRestarted");
    }
}
```

---

## 5. Подключение хаба в `Program.cs`

Если эта строка ещё не добавлена, перед `app.Run();` укажите:

```csharp
// together.js подключается к этому URL через withUrl("/gameHub").
app.MapHub<GameHub>("/gameHub");
```

---

## Что происходит во время игры

```text
каждый выбирает свой маршрут локально
              ↓
игроки обсуждают найденные ответы голосом
              ↓
обычный тупик → перезапуск одного игрока
              ↓
endForAll → сообщение серверу
              ↓
SignalR показывает финал всей команде
```

Сервер не знает, кто был в архиве, кто нашёл код и кто дошёл до тупика. Совместной остаётся только действительно общая часть приключения.
