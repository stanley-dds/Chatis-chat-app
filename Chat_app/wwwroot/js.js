console.log("js.js loaded successfully!");

// Getting references to sections
const authSection = document.getElementById("authSection");
const chatSection = document.getElementById("chatSection");

// Handling registration
document.getElementById("registerForm").addEventListener("submit", async function (event) {
    event.preventDefault(); // Prevent page reload

    const username = document.getElementById("registerUsername").value;
    const password = document.getElementById("registerPassword").value;

    if (!username || !password) {
        alert("Please fill in all fields.");
        return;
    }

    try {
        const response = await fetch("https://localhost:7024/api/auth/register", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ username, password })
        });

        // Check response content type
        const contentType = response.headers.get("content-type");
        if (contentType && contentType.indexOf("application/json") !== -1) {
            const data = await response.json();
            if (response.ok) {
                alert(data.message || "Registration successful!");
            } else {
                alert(data.message || "Registration error");
            }
        } else {
            const text = await response.text();
            console.error("Unexpected server response:", text);
            alert("Registration error: unexpected server response.");
        }
    } catch (error) {
        console.error("Error during registration:", error);
        alert("Error connecting to server.");
    }
});

// Handling login
document.getElementById("loginForm").addEventListener("submit", async function (event) {
    event.preventDefault(); // Prevent page reload

    const username = document.getElementById("loginUsername").value;
    const password = document.getElementById("loginPassword").value;

    if (!username || !password) {
        alert("Please fill in all fields.");
        return;
    }

    try {
        const response = await fetch("https://localhost:7024/api/auth/login", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ username, password })
        });

        const contentType = response.headers.get("content-type");
        if (contentType && contentType.indexOf("application/json") !== -1) {
            const data = await response.json();
            if (response.ok && data.token) {
                localStorage.setItem("token", data.token);
                alert("Login successful!");
                initializeChat(); // Proceed to chat
            } else {
                alert(data.message || "Login error");
            }
        } else {
            const text = await response.text();
            console.error("Unexpected server response:", text);
            alert("Login error: unexpected server response.");
        }
    } catch (error) {
        console.error("Error during login:", error);
        alert("Error connecting to server.");
    }
});

// Function to initialize chat after login
function initializeChat() {
    // Hide auth section and show chat section
    authSection.classList.add("hidden");
    chatSection.classList.remove("hidden");

    // Initialize SignalR connection
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("https://localhost:7024/chatHub", {
            accessTokenFactory: () => localStorage.getItem("token") // Retrieve token from localStorage
        })
        .configureLogging(signalR.LogLevel.Information) // Added for debugging
        .build();

    // Handling incoming messages from server
    connection.on("ReceiveMessage", (user, message) => {
        console.log(`Message received from ${user}: ${message}`);
        const msg = document.createElement("div");
        msg.textContent = `${user}: ${message}`;
        document.getElementById("messagesList").appendChild(msg);
    });

    // Connect to the hub
    connection.start()
        .then(() => console.log("Connected to the chat hub"))
        .catch(err => {
            console.error("Connection failed: ", err);
            alert("Failed to connect to chat.");
        });

    // Handling message sending
    document.getElementById("messageForm").addEventListener("submit", async function (event) {
        event.preventDefault(); // Prevent page reload

        const token = localStorage.getItem("token"); // Retrieve token

        if (!token) {
            alert("Please log in before sending a message.");
            return;
        }

        const message = document.getElementById("messageInput").value;

        if (!message) {
            alert("Please enter a message.");
            return;
        }

        try {
            console.log(`Sending message: ${message}`);
            const response = await fetch("https://localhost:7024/api/chat", {
                method: "POST",
                headers: {
                    "Authorization": "Bearer " + token,
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(message)
            });

            const contentType = response.headers.get("content-type");
            if (contentType && contentType.indexOf("application/json") !== -1) {
                const data = await response.json();
                if (response.ok) {
                    // Add message to chat
                    console.log(`Message sent: ${message}`);
                    const msg = document.createElement("div");
                    msg.textContent = `You: ${message}`;
                    document.getElementById("messagesList").appendChild(msg);

                    document.getElementById("messageInput").value = ""; // Clear input
                } else {
                    console.error("Error sending message:", data.message);
                    alert(data.message || "Error sending message");
                }
            } else {
                const text = await response.text();
                console.error("Unexpected server response:", text);
                alert("Error sending message: unexpected server response.");
            }
        } catch (error) {
            console.error("Error sending message:", error);
            alert("Error sending message.");
        }
    });
}
