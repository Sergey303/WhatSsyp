


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
    title.textContent =
        "Игра началась";

    // hidedtext.classList.add('hide');
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
    const openedtext = 
        document.getElementById(
            "nnn");  //Кавычки ставятся если это константа, без ковычек - если параметр
 
    console.log(openedtext);

    openedtext.classList.add('hide');
}

function openMmmhideNnn() {
    openMmm();
    hideNnn();
}


function hideXStartGame() {
    hideX("hide-when-press-start", false)
    startGame();
    hideX("open-when-press-start", true)
}


function hideX(idhtmlElement, visible) {
    const openedtext = 
        document.getElementById(
            idhtmlElement); 

    console.log(openedtext);

if (visible == true) {
    openedtext.classList.remove('hide')
}
else {
    openedtext.classList.add('hide')
}
}

function keydown(event)
{
    console.log(event.key);
    if (event.key == 'Enter')
    console.log(
        "Nnnn");
}