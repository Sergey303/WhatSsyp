const signInName = document.getElementById("nameInp");
const signInLogin = document.getElementById("loginInp");
const signInPassword = document.getElementById("passwordInp");
const roomNameInput = document.getElementById("roomNameInput");
const roomsBlock = document.getElementById("rooms");

function startApp() {
    Chat.connect();
}

function sendPct() {
    const text = document.getElementById("messageInput").value.trim();
    const block = document.createElement("image");
    block.className = "image";
    block.src = text;
    block.width = 300;
    block.height = 200;
    Chat.send("pct", text);
}

Chat.receive("pct", function (json) {
    showPct(json);
});

Chat.receive("chat", function (text) {
    const msgObj = JSON.parse(text);
    showMessage(msgObj.text, "chatBox");
    if (msgObj.filePath) {
        showFile("chatBox", msgObj.filePath)
    }
});

function showMessage(text) {
    const messages = document.getElementById("messages");
    const block = document.createElement("div");
    block.className = "message";
    block.textContent = text;
    messages.appendChild(block);
}

function sendMessage() {
    const name = document.getElementById("nameInput").value.trim();
    const text = document.getElementById("messageInput").value.trim();
    const inpMessage = document.getElementById("messageInp");
    const text = inpMessage.value.trim();
    const name = "";
    const filePath = "";
    const date = "";
    const jsonString = JSON.stringify({Name: name, text: text, filePath: filePath, date: date});
    if (text === "" || name === "") {
        alert("Нужно имя и сообщение");
        return;
    }
    if (text === "/roll") {
        Chat.send(text);
        return;
    }
    //Chat.send("chat", name + ": " + text);
    Chat.send("chat", jsoNSrting);
    document.getElementById("messageInput").value = "";
    text.value = "";
}

function rollDice() {
    const name = document.getElementById("nameInput").value.trim();
    const num1 = Math.floor(Math.random() * 1000) + 1;
    const num2 = Math.floor(Math.random() * 1000) + 1;
    const num3 = Math.floor(Math.random() * 3) + 1;
    if (num3==1) {
        Chat.send("chat", name + " начал(а) игру, сколько будет " + num1 + " + " + num2 + "?");
    }
    if (num3==2) {
        num4 = Math.floor(Math.random() * 20) + 1;
        num5 = Math.floor(Math.random() * 20) + 1;
        Chat.send("chat", name + " начал(а) игру, сколько будет " + num4 + " * " + num5 + "?");
    }
    if (num3==3) {
        if (num1>=num2) {
            Chat.send("chat", name + " начал(а) игру, сколько будет " + num1 + " - " + num2 + "?");
        } else {
            Chat.send("chat", name + " начал(а) игру, сколько будет " + num2 + " - " + num1 + "?");
        }
    }
}

function showRooms(rooms) {
    roomsBlock.textContent = "";
    for (const room of rooms) {
        const item = document.createElement("li");
        item.classname = "list-group-item";
        item.textContent = room.name;
        roomsBlock.appendChild(item);
    }
}

function loadRooms() {
    Api.get("/api/rooms", showRooms);
}

function roomCreated() {
    roomNameInput.value = "";
    loadRooms();
}

function createRoom() {
    const name = roomNameInput.value;
    Api.get("api/rooms",()=>{});
    if (name === "") {
        return;
    }
    Api.post("/api/rooms", {name: name}, roomCreated);
}

Chat.receive("roomMembers", function(text){const members = JSON.parse(text); console.log(members);});

function joinR() {
    const room = document.getElementById("roomNameInput").value.trim();
    const data = {RoomName:room, UserName:nameI};
    Chat.send("joinRoom", JSON.stringify(data));
}

function sayHello() {
    alert("sup");
}

function accountReg(name, login, password) {
    if (login.length < 3 || login.length > 20) {
        alert("Login must be at least 3 characters and not exceed 20 characters!");
        return;
    }
    else if (length(name) > 16) {
        alert("Name must not exceed 16 characters!");
        return;
    }
    else if (password.length > 20) {
        alert("password must not exceed 20 characters!");
        return;
    }
}

function showFile(filePath, messageBlockId) {
    console.log("1");
    const fileUrl = "http://172.16.47.22:8080/" + filePath;
    console.log("2");
    const messages = document.getElementById(messageBlockId);
    console.log("3");
    const fileMTemp = document.getElementById("file-template");
    console.log("4");
    const newFileMsg = fileMTemp.content.cloneNode(true);
    console.log("5");
    activateMedia(fileUrl, newFileMsg);
    console.log("6");
    messages.appendChild(newFileMsg);
    console.log("7");
}

function startApp() {
    Chat.connect();
}
function regin() {
    const name_ = signInName.value;
    const login_ = signInLogin.value;
    const password_ = signInPassword.value;
    Auth.regin(name_, login_, password_, login)
}

function login() {
    const login = signInLogin.value;
    const password = signInPassword.value;

    Auth.login(login, password, startApp);
}

function signOut() {
    Auth.logout();
}

Auth.start(startApp);
Chat.send("chat", "Пользователь подключился к серверу!");

// document.getElementById("fileInp").addEventListener("change", function (event) {
//     const file = document.getElementById("fileInp").file;
    
//     if (file) {
//         const reader = new FileReader();
        
//         reader.onload = function(e) {
//             try {
//                 const jsonData = JSON.parse(e.target.result);
//                 console.log('JSON data:', jsonData);
//             } catch (error) {
//                 console.error('Invalid JSON file:', error);
//             }
//         };
        
//         reader.readAsText(file);
//     }

//     console.log(fileJSON);
// });

//showFile("https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTAeefR4LoJQ_nFipMLNB3nVe5sWMnzvdIogIW05YVxaWDDrySuV8kduys&s=10","chatBox");