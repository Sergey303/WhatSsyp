/*
  workshop-chat-helper.js
  Учебный helper SignalR.

  Снаружи только:
    Chat.connect();
    Chat.send("chat", "строка");
    Chat.receive("chat", function (text) { ... });
*/
(function () {
    let connection = null;
    let state = "new";
    let demoMode = false;

    const receivers = {};
    const registered = {};
    const sendQueue = [];

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
        const list = receivers[name] || [];

        for (const fn of list) {
            fn(text);
        }
    }

    function registerSignalR(name) {
        if (!connection || registered[name]) {
            return;
        }

        registered[name] = true;

        connection.on(
            name,
            function (text) {
                localReceive(
                    name,
                    cleanText(text));
            });
    }

    function invoke(name, text) {
        return connection
            .invoke(
                "Send",
                name,
                text)
            .catch(function (error) {
                console.error(error);

                localReceive(
                    "system",
                    "Не получилось отправить: " + name);
            });
    }

    function flushQueue() {
        while (sendQueue.length > 0) {
            const item = sendQueue.shift();
            invoke(item.name, item.text);
        }
    }

    function flushDemoQueue() {
        while (sendQueue.length > 0) {
            const item = sendQueue.shift();
            localReceive(item.name, item.text);
        }
    }

    function connect(options) {
        options = options || {};

        if (state === "connecting"
            || state === "connected") {
            return;
        }

        const url =
            options.url || "/chatHub";

        if (!window.signalR) {
            demoMode = true;
            state = "demo";

            localReceive(
                "system",
                "SignalR не найден. Сообщения видны только в этой вкладке.");

            flushDemoQueue();

            return;
        }

        state = "connecting";
        demoMode = false;

        for (const name in registered) {
            delete registered[name];
        }

        connection =
            new signalR.HubConnectionBuilder()
                .withUrl(url)
                .build();

        for (const name in receivers) {
            registerSignalR(name);
        }

        connection.onclose(function () {
            state = "closed";

            localReceive(
                "system",
                "Связь с сервером закрыта.");
        });

        connection
            .start()
            .then(function () {
                state = "connected";
                demoMode = false;

                localReceive(
                    "system",
                    "Подключились к серверу.");

                flushQueue();
            })
            .catch(function (error) {
                console.error(error);

                state = "demo";
                demoMode = true;

                localReceive(
                    "system",
                    "Сервер не найден. Сообщения видны только в этой вкладке.");

                flushDemoQueue();
            });
    }

    function send(name, text) {
        name = cleanName(name);
        text = cleanText(text);

        if (demoMode || state === "demo") {
            localReceive(name, text);
            return;
        }

        if (state !== "connected") {
            sendQueue.push({
                name: name,
                text: text
            });

            return;
        }

        invoke(name, text);
    }

    function receive(name, fn) {
        name = cleanName(name);

        if (typeof fn !== "function") {
            console.warn(
                "Chat.receive ждёт функцию вторым параметром.");

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