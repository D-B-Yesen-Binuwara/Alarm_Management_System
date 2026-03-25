import React, { useState } from "react";
import "./Auth.css";

function Login() {
    const [serviceNumber, setServiceNumber] = useState("");
    const [password, setPassword] = useState("");

    const handleLogin = () => {
        console.log(serviceNumber, password);
    };

    return (
        <div className="container">
            <div className="header">Alarm Management System</div>

            <div className="card">
                <h2>Login</h2>

                <input
                    className="input"
                    type="text"
                    placeholder="Service Number"
                    onChange={(e) => setServiceNumber(e.target.value)}
                />

                <input
                    className="input"
                    type="password"
                    placeholder="Password"
                    onChange={(e) => setPassword(e.target.value)}
                />

                <button className="button" onClick={handleLogin}>
                    Login
                </button>
            </div>
        </div>
    );
}

export default Login;