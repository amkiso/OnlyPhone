setInterval(() => {
    fetch('/Account/KeepAlive', { method: 'POST' })
        .catch(err => console.log("KeepAlive lỗi:", err));
}, 60000); // 60 giây