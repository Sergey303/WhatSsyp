/*
  api-helper.js

  Api.get("/api/rooms", showRooms);
  Api.post("/api/rooms", data, roomCreated);
*/
(function () {
    function readBody(response) {
        return response
            .text()
            .then(function (text) {
                if (text === "") {
                    return null;
                }

                const contentType =
                    response.headers.get("content-type") || "";

                if (contentType.includes("application/json")) {
                    return JSON.parse(text);
                }

                return text;
            });
    }

    function finish(done, data) {
        if (typeof done === "function") {
            done(data);
        }
    }

    function fail(error) {
        console.error(error);
    }

    function get(url, done) {
        fetch(url)
            .then(function (response) {
                if (!response.ok) {
                    throw new Error(
                        "GET " + url + ": " + response.status);
                }

                return readBody(response);
            })
            .then(function (data) {
                finish(done, data);
            })
            .catch(fail);
    }

    function post(url, data, done) {
        fetch(
            url,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(data)
            })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error(
                        "POST " + url + ": " + response.status);
                }

                return readBody(response);
            })
            .then(function (result) {
                finish(done, result);
            })
            .catch(fail);
    }

    window.Api = {
        get: get,
        post: post
    };
})();
