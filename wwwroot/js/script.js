async function getUserAuth() {
    const isAuth = await fetch("/authinfo", {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (isAuth.ok === true) {
        const resp = await isAuth.json();
        if (resp === "auth"){
            document.getElementById("log-inButton").style.display = "none";
            document.getElementById("sign-inButton").style.display = "none";
            document.getElementById("log-outButton").style.display = "unset";
            document.getElementById("infoButton").style.display = "unset";
        }
    }
    const greeting = await fetch("/userinfo", {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (greeting.ok === true) {
        const name = await greeting.json();
        document.getElementById("greeting").innerHTML = `Добро пожаловать: ${name.toString()}`;
        document.getElementById("greeting").style.display = "unset";
    }
}
async function getUserInfo() {
    const response = await fetch("/userinfo", {
        method: "GET",
        headers: { "Accept": "application/json" }
    });
    if (response.ok === true) {
        const info = await response.json();
        document.getElementById("user_login").innerHTML = `Ваш логин: ${info.toString()}`;
    }
}
getUserInfo();
getUserAuth();