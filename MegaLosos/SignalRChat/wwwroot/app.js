const name = "";
function sayHello() {
    alert("sup");
}

function accountReg(name, login, password) {
    if (login.length < 3 || login.length > 20) {
        alert("Login must be at least 3 characters and not exceed 20 characters!");
        return;
    }
    else if (name.length > 16) {
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
    const name = "";
    const filePath = "";
    const date = "";
    const jsonString = JSON.stringify({Name: name, text: text, filePath: filePath, date: date});
    if (text === "") {
        alert("FILL  IN  THE  TEXT!!!!");
        return;
    }
    Chat.send("chat", jsonString);
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

Chat.receive("chat", function (text) {
    const msgObj = JSON.parse(text);
    showMessage(msgObj.text, "chatBox");
    if (msgObj.filePath) {
        showFile("chatBox", msgObj.filePath)
    }
});

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

const signInName = document.getElementById("nameInp");

const signInLogin = document.getElementById("loginInp");

const signInPassword = document.getElementById("passwordInp");

Auth.start(startApp);
showFile("https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTAeefR4LoJQ_nFipMLNB3nVe5sWMnzvdIogIW05YVxaWDDrySuV8kduys&s=10","chatBox");

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