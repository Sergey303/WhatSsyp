(function () {
    function getPanel(id) {
        return document.getElementById(id);
    }

    function showLogin() {
        getPanel("loginCard")
            .classList.remove("d-none");

        getPanel("appPanel")
            .classList.add("d-none");
    }

    function showApp() {
        getPanel("loginCard")
            .classList.add("d-none");

        getPanel("appPanel")
            .classList.remove("d-none");
    }

    function runReady(ready) {
        if (typeof ready === "function") {
            ready();
        }
    }

    function start(ready) {
        fetch("/api/me")
            .then(function (response) {
                if (response.ok) {
                    // showApp();
                    runReady(ready);
                } else {
                    // showLogin();
                }
            })
            .catch(function (error) {
                console.error(error);
                // showLogin();
            });
    }

    function login(login, password, ready) {
        fetch(
            "/api/login",
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    login: login,
                    password: password,
                    name: ""
                })
            })
            .then(function (response) {
                if (!response.ok) {
                    alert("Неверное имя или пароль");
                    return;
                }
                // showApp();
                window.location.assign('http://localhost:8080/index.html');
                runReady(ready);
            })
            .catch(function (error) {
                console.error(error);
            });
    }

    function regin(name, login, password, ready) {
        fetch(
            "/api/register",
            {
                method: "POST",
                headers: {"Content-Type": "application/json"},
                body: JSON.stringify({
                    login: login,
                    password: password,
                    name: name
                })
            })
            .then(function (response) {
                if (!response.ok) {
                    alert("Name or login is already taken!");
                    return;
                }
                runReady(ready);
            })
            .catch(function (error) {
                console.error(error);
            });
    }

    function logout() {
        fetch(
            "/api/logout",
            {
                method: "POST"
            })
            .then(function () {
                location.reload();
            })
            .catch(function (error) {
                console.error(error);
            });
    }

    window.Auth = {
        start: start,
        login: login,
        logout: logout,
        regin: regin
    };
})();