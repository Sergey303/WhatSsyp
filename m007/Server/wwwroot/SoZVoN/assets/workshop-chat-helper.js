/*
  workshop-chat-helper.js
  Очень простой учебный helper для SignalR.

  Снаружи есть только 3 метода:

    Chat.connect();
    Chat.send("название", "строка");
    Chat.receive("название", function (строка) { ... });

  Важно: helper отправляет и принимает только строки.
  Если нужно отправить имя + текст + очки, преврати объект в JSON-строку:

    const json = JSON.stringify({ name: "Маша", text: "Привет" });
    Chat.send("chat", json);

  Этот файл на первых занятиях лучше не менять руками.
*/
(function () {
  let connection = null;
  let connected = false;
  let demoMode = false;

  // Здесь хранятся функции-получатели.
  // Например: receivers["chat"] = [function (text) { ... }]
  const receivers = {};

  // Чтобы не подписываться на одно и то же событие SignalR много раз.
  const registered = {};

  function cleanName(name) {
    name = String(name || "").trim();
    return name === "" ? "message" : name;
  }

  function cleanText(text) {
    if (text === undefined || text === null) {
      return "";
    }
    return String(text);
  }

  function localReceive(name, text) {
    name = cleanName(name);
    text = cleanText(text);

    const list = receivers[name] || [];
    for (const fn of list) {
      fn(text);
    }
  }

  function registerSignalR(name) {
    name = cleanName(name);

    if (!connection || registered[name]) {
      return;
    }

    registered[name] = true;

    // Сервер отправляет событие с таким же названием.
    // Например: Clients.All.SendAsync("chat", "Привет!")
    connection.on(name, function (text) {
      localReceive(name, text);
    });
  }

  async function connect(options) {
    options = options || {};
    const url = options.url || "chatHub";

    if (!window.signalR) {
      demoMode = true;
      localReceive("system", "SignalR не найден. Демо-режим: сообщения видны только в этой вкладке.");
      return;
    }

    connection = new signalR.HubConnectionBuilder()
      .withUrl(url)
      .withAutomaticReconnect()
      .build();

    for (const name in receivers) {
      registerSignalR(name);
    }

    connection.onreconnecting(function () {
      connected = false;
      localReceive("system", "Связь потеряна. Пробуем подключиться снова...");
    });

    connection.onreconnected(function () {
      connected = true;
      localReceive("system", "Связь восстановлена.");
    });

    try {
      await connection.start();
      connected = true;
      demoMode = false;
      localReceive("system", "Подключились к серверу.");
    } catch (error) {
      console.error(error);
      connected = false;
      demoMode = true;
      localReceive("system", "Сервер не найден. Демо-режим: сообщения видны только в этой вкладке.");
    }
  }

  async function send(name, text) {
    name = cleanName(name);
    text = cleanText(text);

    if (demoMode || !connection || !connected) {
      localReceive(name, text);
      return;
    }

    try {
      // На сервере нужен метод: Send(string name, string text)
      await connection.invoke("Send", name, text);
    } catch (error) {
      console.error(error);
      localReceive("system", "Не получилось отправить: " + name);
    }
  }

  function receive(name, fn) {
    name = cleanName(name);

    if (typeof fn !== "function") {
      console.warn("Chat.receive ждёт функцию вторым параметром.");
      return;
    }

    if (!receivers[name]) {
      receivers[name] = [];
    }

    receivers[name].push(fn);
    registerSignalR(name);
  }

  window.Chat = {
    connect: connect,
    send: send,
    receive: receive
  };
})();
