(function () {
    function start(ready) {
        fetch("/api/me")
            .then(function (response) {
                if (response.ok) {
                    if (typeof ready === "function") {
                        ready();
                    }
                }
            })
            .catch(function (error) {
                console.error(error);
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
                if (typeof ready === "function") {
                    ready();
                }
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
                if (typeof ready === "function") {
                    ready();
                }
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
                window.location.assign('/');
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