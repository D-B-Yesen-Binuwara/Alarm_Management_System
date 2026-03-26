import { useState } from "react";
import "../Auth.css";
import logo from "../../assets/slt-logo.png";

function ProvinceRegister({ setPage }) {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const handleRegister = (e) => {
    e.preventDefault();

    const user = {
      name,
      email,
      password,
      role: "Province Officer",
    };

    localStorage.setItem("user", JSON.stringify(user));

    alert("Registered Successfully ✅");
    setPage("login");
  };

  return (
    <div className="auth-container">
      <img src={logo} className="logo" />

      <h2>Province Officer Register</h2>

      <form onSubmit={handleRegister}>
        <input
          type="text"
          placeholder="Name"
          onChange={(e) => setName(e.target.value)}
        />

        <input
          type="email"
          placeholder="Email"
          onChange={(e) => setEmail(e.target.value)}
        />

        <input
          type="password"
          placeholder="Password"
          onChange={(e) => setPassword(e.target.value)}
        />

        <button className="btn-primary">Register</button>
      </form>
    </div>
  );
}

export default ProvinceRegister;