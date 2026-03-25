import React, { useState } from "react";
import "./Auth.css";

function Register() {
    const [serviceNumber, setServiceNumber] = useState("");
    const [password, setPassword] = useState("");
    const [role, setRole] = useState("user");

    const handleRegister = () => {
        console.log(serviceNumber, password, role);
    };

    return (
        <div className="container">
            <div className="header">Alarm Management System</div>

            <div className="card">
                <h2>User Registration</h2>

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

                <select
                    className="input"
                    onChange={(e) => setRole(e.target.value)}
                >
                    <option value="user">User</option>
                    <option value="admin">Admin</option>
                </select>

                <button className="button" onClick={handleRegister}>
                    Register
                </button>
            </div>
        </div>
    );
}

export default Register;