// После response.json() здесь будет объект книги со всеми сценами и переходами.
let book = null;

// Сохраняем ссылку на h1, чтобы показывать book.title.
const titleElement =
    document.getElementById("book-title");

// Сохраняем ссылку на article, где будет меняться scene.text.
const textElement =
    document.getElementById("scene-text");

// Сохраняем ссылку на section для кнопок и полей ответа.
const choicesElement =
    document.getElementById("choices");

// Сохраняем ссылку на абзац для ошибки загрузки /api/book.
const errorElement =
    document.getElementById("error-message");

// Получаем JSON, создаём объект book и открываем начальную сцену.
async function loadBook() {
    // try/catch позволяет показать ошибку на странице, а не остановить скрипт молча.
    try {
        // fetch отправляет GET-запрос на адрес, созданный через MapGet.
        const response = await fetch("/api/book");

        // response.ok равен false при HTTP-ошибках, например 404 или 500.
        if (!response.ok) {
            // Этот текст попадёт в catch и будет показан читателю.
            throw new Error("Не удалось загрузить книгу.");
        }

        // response.json() разбирает ответ и создаёт объект с title, scenes и choices.
        book = await response.json();

        // startSceneId хранит id сцены, с которой начинается книга.
        showScene(book.startSceneId);
    } catch (error) {
        // Короткое сообщение записываем прямо на страницу.
        errorElement.textContent = error.message;

        // Исходная ошибка остаётся в консоли для поиска причины.
        console.error(error);
    }
}

// Одинаково нормализуем введённый и правильный ответы перед сравнением.
function normalizeAnswer(value) {
    // trim удаляет случайные пробелы до и после ответа.
    return value
        .trim()
        // toLowerCase делает «ОРИОН» и «орион» одинаковыми.
        .toLowerCase()
        // Разрешаем ввести «ОРИОН - 73» вместо «ОРИОН-73».
        .replaceAll(" ", "");
}

// Для choice без answer сразу создаём кнопку, открывающую targetSceneId.
function createTransitionButton(choice) {
    const button = document.createElement("button");

    // Надпись на кнопке берём из choice.text.
    button.textContent = choice.text;

    // Клик меняет сцену без перезагрузки HTML-страницы.
    button.addEventListener("click", () => {
        // targetSceneId сообщает showScene, какой объект найти в book.scenes.
        showScene(choice.targetSceneId);
    });

    // Вызывающая функция добавит кнопку в choicesElement.
    return button;
}

// Для choice с answer сначала показываем ввод и проверку, а не переход.
function createAnswerChoice(choice) {
    // Контейнер удерживает поле, проверку, результат и будущую кнопку перехода.
    const container = document.createElement("div");

    // Читатель вводит сюда найденный в истории код или слово.
    const input = document.createElement("input");

    // type="text" подходит для букв, цифр и составных кодов.
    input.type = "text";

    // placeholder объясняет назначение пустого поля.
    input.placeholder = "Введите ответ";

    // Эта кнопка сравнит введённое значение с choice.answer.
    const checkButton = document.createElement("button");

    // «Проверить» отделяет проверку ответа от перехода к сцене.
    checkButton.textContent = "Проверить";

    // В span появится «Верно» или сообщение о неподходящем ответе.
    const resultElement = document.createElement("span");

    // До клика читатель может спокойно исправлять ввод.
    checkButton.addEventListener("click", () => {
        // Нормализуем текущее значение input.
        const enteredAnswer =
            normalizeAnswer(input.value);

        // Тем же способом нормализуем answer из JSON.
        const correctAnswer =
            normalizeAnswer(choice.answer);

        // Сравниваем две уже нормализованные строки.
        if (enteredAnswer === correctAnswer) {
            // Подтверждаем ответ до появления кнопки продолжения.
            resultElement.textContent = "Верно. ";

            // После успеха поле блокируется, чтобы состояние не выглядело противоречиво.
            input.disabled = true;

            // Иначе повторный клик добавил бы несколько одинаковых кнопок.
            checkButton.disabled = true;

            // Верный ответ открывает обычную кнопку с тем же targetSceneId.
            const transitionButton =
                createTransitionButton(choice);

            // Добавляем кнопку продолжения в контейнер проверки.
            container.appendChild(transitionButton);
        } else {
            // Сообщаем только о неудаче, чтобы загадку можно было решать дальше.
            resultElement.textContent =
                "Ответ пока не подходит.";
        }
    });

    // Первым помещаем в контейнер поле ввода.
    container.appendChild(input);

    // Рядом помещаем действие «Проверить».
    container.appendChild(checkButton);

    // Последним добавляем span с результатом проверки.
    container.appendChild(resultElement);

    // showScene добавит весь контейнер под текстом сцены.
    return container;
}

// Находим сцену, заменяем текст страницы и заново создаём её переходы.
function showScene(sceneId) {
    // find возвращает первый объект с подходящим scene.id.
    const scene = book.scenes.find(
        // Совпадение scene.id и sceneId связывает переход с целевой сценой.
        item => item.id === sceneId
    );

    // undefined означает, что автор ещё не создал сцену с таким id.
    if (!scene) {
        // Выводим отсутствующий id, чтобы черновик было проще исправить.
        textElement.textContent =
            `Сцена «${sceneId}» пока не создана.`;

        // Кнопки предыдущей сцены не должны оставаться под сообщением.
        choicesElement.replaceChildren();

        return;
    }

    // Заголовок берётся из общего свойства book.title.
    titleElement.textContent = book.title;

    // article получает scene.text найденного объекта.
    textElement.textContent = scene.text;

    // Перед новым набором полностью очищаем choicesElement.
    choicesElement.replaceChildren();

    // Пустой массив choices означает, что у сцены нет продолжения.
    if (scene.choices.length === 0) {
        // Для финальной сцены создаём абзац вместо кнопок.
        const endingElement =
            document.createElement("p");

        // Явно сообщаем читателю, что ветка закончилась.
        endingElement.textContent = "Конец истории.";

        // Помещаем надпись о конце в обычную область переходов.
        choicesElement.appendChild(endingElement);

        return;
    }

    // Порядок массива choices станет порядком элементов на странице.
    for (const choice of scene.choices) {
        // Поле answer определяет, нужен ли ввод перед переходом.
        const choiceElement = choice.answer
            // Для загадки строим контейнер с input и кнопкой проверки.
            ? createAnswerChoice(choice)
            // Без answer переход доступен сразу.
            : createTransitionButton(choice);

        // Готовый button или div появляется под текстом сцены.
        choicesElement.appendChild(choiceElement);
    }
}

// После объявления функций начинаем первый запрос к /api/book.
loadBook();