import { useState } from "react";
import "../pages/Auth.css";
import logo from "../assets/slt-logo.png";

function Login({ setPage }) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const handleLogin = (e) => {
    e.preventDefault();

    const user = JSON.parse(localStorage.getItem("user"));

    if (user && user.email === email && user.password === password) {
      alert("Login success ✅");
      setPage("dashboard");
    } else {
      alert("Invalid login ❌");
    }
  };

  return (
    <div className="auth-container">
      <img src={logo} alt="SLT Logo" className="logo" />

      <h2>Login</h2>

      <form onSubmit={handleLogin}>
        <input
          type="email"
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />

        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />

        <button className="btn-primary">Login</button>
      </form>

      <p>
        Don’t have an account?{" "}
        <button onClick={() => setPage("register")}>Register</button>
      </p>
    </div>
  );
}

export default Login;