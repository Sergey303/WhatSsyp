const loginName = document.getElementById("loginName");
const loginPassword = document.getElementById("password");
const UserName = document.getElementById("username");
var nameI;
function startApp() {
    Chat.connect();
    console.log("1");
}
function login() {
    const name = loginName.value;
    const password = loginPassword.value;
    const username = UserName.value;
    Auth.login(name, password, username, startApp);
    nameI = UserName.value;
}
function registration() {
    const name = loginName.value;
    const password = loginPassword.value;
    const username = UserName.value;
    Api.post("/olele", {Name:name, Password:password, UserName: username}, function(response) {
        if (response.ok) {
            alert("Успешная регистрация");
        } else {
            alert("Регситрация не удалась");
        }
    });
}



Chat.receive("chat", function(text) {
    showMessage(text);
});

function sendMessage() {
    const text = document.getElementById("messageInput").value;
    if (text==="") {
        alert("Сообщение");
        return;
    }
    Chat.send("chat", text);
    document.getElementById("messageInput").value = "";   
}
function showMessage(text) {
    const messages = document.getElementById("messages");
    const block = document.createElement("div");
    block.className = "message";
    block.textContent = text;
    messages.appendChild(block);
}




//Api.get("/api/rooms". showRooms);
const roomNameInput = document.getElementById("roomNameInput");
const roomsBlock = document.getElementById("rooms");
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
    //const name = document.getElementById("nameInput").value.trim();
    const room = document.getElementById("roomNameInput").value.trim();
    const data = {RoomName:room, UserName:nameI};
    Chat.send("joinRoom", JSON.stringify(data));
}
