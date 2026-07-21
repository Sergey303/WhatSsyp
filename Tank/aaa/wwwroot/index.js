
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
    title.innerHTML =
        "Игра началась";


}

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
            
        }
        else{
        if(r+f == k){
            text.innerHTML = ("Верно");
        }
        else {
            text.innerHTML = ("не");
        }
        }

    }
}

function clickAnswer() {
    // Вызываем вашу большую функцию проверки 
    // и притворяемся, что был нажат Enter
    keydown({ key: 'Enter' });
}

function startAgain() {
    hideXStartGame();
        // 2. Очищаем поле ввода от старого ответа пользователя
        document.getElementById("somethig").value = "";

        // 3. Очищаем старую надпись "Верно / Неверно"
        document.getElementById("anser").innerHTML = "";
}
