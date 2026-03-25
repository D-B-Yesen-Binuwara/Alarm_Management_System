import { useState } from "react";   // ✅ MUST ADD THIS
import "./App.css";

// pages
import Login from "./pages/Login";
import Register from "./pages/Register";

function App() {
    const [page, setPage] = useState("login");

    // 👉 LOGIN PAGE
    if (page === "login") {
        return (
            <div>
                <Login />

                <p style={{ textAlign: "center", marginTop: "10px" }}>
                    Don’t have an account?{" "}
                    <button onClick={() => setPage("register")}>
                        Register
                    </button>
                </p>
            </div>
        );
    }

    // 👉 REGISTER PAGE
    if (page === "register") {
        return (
            <div>
                <Register setPage={setPage} />

                <p style={{ textAlign: "center", marginTop: "10px" }}>
                    Already have an account?{" "}
                    <button onClick={() => setPage("login")}>
                        Login
                    </button>
                </p>
            </div>
        );
    }

    return <h2>Loading...</h2>;
}

export default App;