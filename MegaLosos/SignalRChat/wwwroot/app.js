
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

function sendMessage() {
    const inpMessage = document.getElementById("messageInp");
    const text = inpMessage.value.trim();
    if (text === "") {
        alert("FILL  IN  THE  TEXT!!!!");
        return;
    }
    Chat.send("chat", text);
    document.getElementById("messageInp").value = "";
    // alert(name + ": " + text);
}

function showMessage(text, messageBlockId) {
    const messages = document.getElementById(messageBlockId);
    const block = document.createElement("div");

    block.className = "message";
    block.textContent = text;
    messages.appendChild(block);
}

Chat.receive("chat", function (text) {
    showMessage(text, "chatBox");
});

const signInName = document.getElementById("nameInp");

const signInLogin = document.getElementById("loginInp");

const signInPassword = document.getElementById("passwordInp");

Auth.start(startApp);

function startApp() {
    Chat.connect();
}
function regin() {
    const name = signInName.value;
    const login = signInLogin.value;
    const password = signInPassword.value;
    Auth.regin(name, login, password, 
        Auth.login(login, password, startApp))
}

function login() {
    const login = signInLogin.value;
    const password = signInPassword.value;

    Auth.login(login, password, startApp);
}

function signOut() {
    Auth.logout();
}