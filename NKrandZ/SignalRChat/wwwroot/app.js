function loadRooms(){
    loadListR("/api/rooms");
}
function loadUsers(){
    loadList("/api/users");
}
function loadChatUsers(){
    loadListC("/api/chat-users");
}
function loadRoomUsers(roomName){
    loadListC("/api/rooms/" + roomName + "/users");
}
function loadListR(url){
    fetch(url)
        .then(function (response) {
            return response.json();
        })
        .then(function (response) {
            showListR(response);
        });
}
function loadListC(url){
    fetch(url)
        .then(function (response) {
            return response.json();
        })
        .then(function (response) {
            showListC(response);
        });
}
function loadList(url){
    fetch(url)
        .then(function (response) {
            return response.json();
        })
        .then(function (response) {
            showList(response);
        });
}
function showListR(items){
    const result = document.getElementById("roomResult");
    result.innerHTML = "";

    for (const item of items) {
        const block = document.createElement("option");
        block.value = item;
        block.textContent = item;
        result.appendChild(block);
    }
}
function showListC(items){
    const result = document.getElementById("chatResult");
    result.innerHTML = "";

    for (const item of items) {
        const block = document.createElement("option");
        block.value = item;
        block.textContent = item;
        result.appendChild(block);
    }
}
function showList(items){
    const result = document.getElementById("result");
    result.innerHTML = "";

    for (const item of items) {
        const block = document.createElement("div");
        block.textContent = item;
        result.appendChild(block);
    }
}

Chat.receive("loginResult", function (text) {
    if (text == "Login Complete") {
        document.getElementById("login").hidden = true;
        document.getElementById("bd").hidden = false;
    }
});

Chat.receive("chat", function (json) {
    try {
        const message = JSON.parse(json);
        showMessage(message.room + ": " + message.name + ": " + message.text);
    } catch (error) {
        showMessage("Failed to load Json");
    }
});

Chat.connect();

function register() {
    const username = document.getElementById("nameInput").value.trim();
    const login = document.getElementById("loginInput").value.trim();
    const password = document.getElementById("passwordInput").value.trim();
    const loginMessage = {
        username: username,
        login: login,
        password: password
    }
    const json = JSON.stringify(loginMessage);
    Chat.send("register", json);
}

function login() {
    const login = document.getElementById("loginInput").value.trim();
    const password = document.getElementById("passwordInput").value.trim();
    const loginMessage = {
        username: "",
        login: login,
        password: password
    }
    const json = JSON.stringify(loginMessage);
    Chat.send("login", json);
}

function joinRoom(rm) {
    const message = {
        text: "",
        name: document.getElementById("nameInput").value.trim(),
        time: new Date().toLocaleTimeString(),
        room: rm
    };
    
    const json = JSON.stringify(message);
    Chat.send("joinRoom", json);
}

function newRoom() {
    const element = document.getElementById('newRoomProp');
    element.hidden = !element.hidden;
    const button = document.getElementById('newRoom');
    if (button.innerText === '✖️') {
        button.innerText = '➕';
    } else {
        button.innerText = '✖️';
    }
}

function appendRoom() {
    const name = document.getElementById("roomNameInput").value.trim();
    document.getElementById("roomNameInput").value = "";
    const message = {
        text: "",
        name: document.getElementById("nameInput").value.trim(),
        time: new Date().toLocaleTimeString(),
        room: name
    };
    
    const json = JSON.stringify(message);
    Chat.send("newRoom", json);
}

function sendMessage() {
    const message = {
        text: document.getElementById("messageInput").value,
        name: document.getElementById("nameInput").value.trim(),
        time: new Date().toLocaleTimeString(),
        room: document.getElementById('roomResult').value
    };
    if (message.text == ""){
        break;
    }
    const json = JSON.stringify(message);
    Chat.send("chat", json)
    document.getElementById("messageInput").value = "";
}

function showMessage(text) {
    const messages = document.getElementById("messages");
    const block = document.createElement("div");
    block.className = "message";
    block.textContent = text;
    messages.appendChild(block);
}