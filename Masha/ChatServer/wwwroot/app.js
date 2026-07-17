console.log("JavaScript работает");
// function sayHello() {
//     alert("Привет!");
// }
// function showRules(){
//     alert("Здесь вы можете прочитать правила игры");
// }
// const f = document.getElementById("f");
// console.log(f);
// const nameInput = document.getElementById("nameInput");

// function showName() {
//     const name=nameInput.value; //var
//     console.log(name);
// }
// const title = document.getElementById("title");

// function startGame() {
//     title.textContent = "Игра началась";
// }

// const messages = document.getElementById("messages");
// const message = document.createElement("div");
// message.textContent = "Маша: Привет";
// message.className = "message";
// message.className = "alert alert-info";
// messages.appendChild(message);
// const colors = ["Красный", "Синий", "Зеленый", "yellow", "black"];
// console.log(colors[0]);
// console.log(colors[1]);
// console.log(colors.length);
// console.log(colors[4]);
// for (const color of colors) {
//     console.log(color);
// }
// const index = Math.floor(Math.random() * colors.length);
// const color = colors[index];
// console.log(color);

const messages = document.getElementById("messages");
const nameInput = document.getElementById("nameInput");
const messageInput = document.getElementById("messageInput");

Chat.receive("chat", function (text) {
    showMessage(text);
});
Chat.receive("system", function (text) {
    showMessage(text);
});
Chat.connect();
// Chat.send("chat", "Привет");

// const player = {
//     name: "Маша",
//     score: 5
// };

function sendMessage(){
    const name = nameInput.value;
    const text = messageInput.value;
    if (name === "") {
        return;
    }
    if (text === "") {
        return;
    }
    Chat.send("chat", name + ": " + text);
    messageInput.value = "";

}
function showMessage(text) {
    const block = document.createElement("div");

    block.className = "alert alert-info";

    block.textContent = text;

    block.className = "alert alert-info";

    messages.appendChild(block);
}
    

function roomCreated(){
    Api.get("/api/rooms", showRooms);
}

Api.post("/api/rooms",{
    name: "Коты"
}, roomCreated);

const dice = document.getElementById("dice");
function showDice(number){
    dice.textContent = number;
}
function rollDice(){
    Api.get("/api/dice", showDice);
}

const roomNameInput = document
.getElementById("roomNameInput");
const roomBlock = document.getElementById("rooms");

function showRooms(rooms){
    roomsBlock.textContent = "";

    for (const room of rooms) {
        const item = document.createElement("li");
        item.className="list-group-item";
        item.textContent=room.name;
        roomsBlock.appendChild(item);
    }
}

function loadRooms(){
    Api.get("/api/rooms", showRooms);
}

function roomCreated(){
    roomNameInput.value="";
    loadRooms();
}

function createRoom(){
    const name = roomNameInput.value;
    if (name===""){
        return;
    }

    Api.post("/api/rooms",{
        name: name
    }, 
    roomCreated);
}


loadRooms();

// const text = JSON.stringfly(player);
messageInput.addEventListener("keydown", function (event){
    if (event.key === "Enter"){
        sendMessage();
    }



});