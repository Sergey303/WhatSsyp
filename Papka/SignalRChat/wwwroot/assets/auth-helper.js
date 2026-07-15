/*
  auth-helper.js

  Auth.start(startApp);
  Auth.login(name, password, startApp);
  Auth.logout();
*/
(function () {
    function getPanel(id) {
        return document.getElementById(id);
    }

    function showLogin() {
        getPanel("loginPanel")
            .classList.remove("d-none");

        getPanel("appPanel")
            .classList.add("d-none");
        getPanel("roomPanel")
            .classList.add("d-none");
    }

    function showApp() {
        getPanel("loginPanel")
            .classList.add("d-none");

        getPanel("appPanel")
            .classList.remove("d-none");
        getPanel("roomPanel")
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
                    showApp();
                    runReady(ready);
                } else {
                    showLogin();
                }
            })
            .catch(function (error) {
                console.error(error);
                showLogin();
            });
    }

    function login(name, password, ready) {
        fetch(
            "/api/login",
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    name: name,
                    password: password
                })
            })
            .then(function (response) {
                if (!response.ok) {
                    alert("Неверное имя или пароль");
                    return;
                }

                showApp();
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
        logout: logout
    };
})();