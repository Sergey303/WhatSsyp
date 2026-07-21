
Chat.connect();

function openMmm() {
    const hidedtext = 
        document.getElementById(
            "mmm");

    console.log(hidedtext);


    hidedtext.classList.remove('hide');
    // hidedtext.classList.add('hide');
}

const title =
    document.getElementById(
        "title");

function startGame() {
    Chat.send("Привет", "привет привет")  // Первый паоаметр - называние события, второй - данные события (сэнд отправляет эти данныые)
    title.innerHTML =
        "Игра началась";
}

function helloReaction(text) {
console.log("Событие 'Привет' нам пристало", text);
}

Chat.receive("Привет", helloReaction)  // Мы "подписываемся" на событие = указываем функцию которая выполнится на каждое событие (В ее аргумент (text) записываются отправленные данные (в нашем случае привет привет))


function openMmm() {
    const hidedtext = 
        document.getElementById(
            "mmm");

    console.log(hidedtext);


    hidedtext.classList.remove('hide');
    // hidedtext.classList.add('hide');
}

function hideNnn() {
    const openedtext = document.getElementById("nnn");  //Кавычки ставятся если это константа, без ковычек - если параметр
 
    console.log(openedtext);

    openedtext.classList.add('hide');
}

function openMmmhideNnn() {
    openMmm();
    hideNnn();
}

function randomNumber(a, b){
    return Math.floor(Math.random() * (b - a + 1)) + a;
}

let r;
let f; // так как мы делаем их черел лет их можно будет потом переписать

function hideXStartGame() {
    hideOrOpenX("hide-when-press-start", false)
    startGame();
    hideOrOpenX("open-when-press-start", true)
    const x = randomNumber(0,100);
    const y = randomNumber(0,100);

    const text = document.getElementById(
        "text"); 

        text.innerHTML = x + "+" + y;
        r = x;
        f = y;
     
    const toSend = {Chislo1:  x,  Chislo2:  y}; // toSend в данном случае это объект, в фигурных скобках указываются его поля ляля {навзаниве: значение}

    const stringToSend = JSON.stringify (toSend);  // JSON.stringify - функция которая делает из объекта toSend строчку stringToSend (строка имеет формат JSON)
    console.log(stringToSend, toSend);
    Chat.send("начало игры", stringToSend)

}

Chat.receive("начало игры", startServerGame)  // Каждый рвз когда будет вызван чат сенд, бцдет вызван startServerGameЮ который заполнит переменную PoluchenyStringToSend

function startServerGame(PoluchenyStringToSend) {
    // PoluchenyStringToSend будет иметь значение stringToSend, так как сервер переслал их 
    console.log("PoluchenyStringToSend:", PoluchenyStringToSend);

    const PoluchenyToSend = JSON.parse (PoluchenyStringToSend) // JSON.parse - функция, обратная JSON.stringify. Делает из строчки JSONформата PoluchenyStringToSend объект PoluchenyToSend

    const x = PoluchenyToSend.Chislo1; // Получаем значение поля "Chislo1" что принадлежит объекту PoluchenyToSend
    const y = PoluchenyToSend.Chislo2;


    const text = document.getElementById(
        "text"); 
    text.innerHTML = x + "+" + y;
    r = x;
    f = y;
}  



function hideOrOpenX(idhtmlElement, visible) { 
    const openedtext = 
        document.getElementById(
            idhtmlElement); 

    console.log(openedtext);

if (visible == true) {
    openedtext.classList.remove('hide');
}
else {
    openedtext.classList.add('hide');
}
}

function keydown(event)
{
    console.log(event.key);
    if (event.key == 'Enter')
    {
    console.log(
        "Nnnn");
    const s = document.getElementById("somethig");
    if (s.value == "") {
        console.log("Это не число!");
    }
    const k = Number(s.value);
   
    const text = document.getElementById(
        "anser"); 

        if (Number.isNaN(k)) {

                text.innerHTML = ("эт не число");
                changeBackground(false);
        }
        else{
        if(r+f == k){
            text.innerHTML = ("Верно");
            changeBackground(true);
            Chat.send("Победа", text.innerHTML)
            
        }
        else {
            text.innerHTML = ("не");
            changeBackground(false);
        }
        }
    }
}

Chat.receive("Победа", pereslatResult) 

function pereslatResult() {
    const text = document.getElementById(
        "anser"); 
    text.innerHTML = ("Не успел");
}


function changeBackground(ansver) {
    const color = 
        document.getElementById(
            "aaa");

    console.log(color);


    if (ansver == true) {
        color.classList.remove('aaab');
        color.classList.add('aaabc');
    }
    else {
        color.classList.remove('aaabc');
        color.classList.add('aaab');
    }

}




function clickAnswer() {
    // Вызываем keydown
    keydown({ key: 'Enter' });
}

function startAgain() {
    hideXStartGame();
        // Очищаем поле ввода от старого ответа пользователя
        document.getElementById("somethig").value = "";

        // Очищаем старую надпись 
        document.getElementById("anser").innerHTML = "";
}
