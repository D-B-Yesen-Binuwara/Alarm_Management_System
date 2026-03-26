import { useState } from "react";

// pages
import Login from "./pages/Login";
import Register from "./pages/Register";

// role forms
import AdminRegister from "./pages/RegisterForms/AdminRegister";
import RegionRegister from "./pages/RegisterForms/RegionRegister";
import ProvinceRegister from "./pages/RegisterForms/ProvinceRegister";
import LeaRegister from "./pages/RegisterForms/LeaRegister";

// ✅ IMPORT LOGO
import logo from "./assets/slt-logo.png";

function App() {
  const [page, setPage] = useState("login");

  // 🔥 COMMON HEADER (SLT STYLE)
  const Header = () => (
    <div
      style={{
        background: "#008060",
        color: "white",
        padding: "10px",
        display: "flex",
        alignItems: "center",
      }}
    >
      <img src={logo} alt="SLT" style={{ width: "120px" }} />
      <h2 style={{ marginLeft: "10px" }}>INMS System</h2>
    </div>
  );

  // LOGIN
  if (page === "login") {
    return (
      <div>
        <Header />
        <Login setPage={setPage} />
        <p style={{ textAlign: "center" }}>
          Don’t have an account?{" "}
          <button onClick={() => setPage("register")}>Register</button>
        </p>
      </div>
    );
  }

  // REGISTER ROLE SELECT
  if (page === "register") {
    return (
      <div>
        <Header />
        <Register setPage={setPage} />
      </div>
    );
  }

  // ADMIN REGISTER
  if (page === "admin") {
    return (
      <div>
        <Header />
        <AdminRegister setPage={setPage} />
      </div>
    );
  }

  // REGION REGISTER
  if (page === "region") {
    return (
      <div>
        <Header />
        <RegionRegister setPage={setPage} />
      </div>
    );
  }

  // PROVINCE REGISTER
  if (page === "province") {
    return (
      <div>
        <Header />
        <ProvinceRegister setPage={setPage} />
      </div>
    );
  }

  // LEA REGISTER
  if (page === "lea") {
    return (
      <div>
        <Header />
        <LeaRegister setPage={setPage} />
      </div>
    );
  }

  // DASHBOARD
  if (page === "dashboard") {
    return (
      <div>
        <Header />
        <div style={{ textAlign: "center", marginTop: "100px" }}>
          <h2>Dashboard</h2>
          <button onClick={() => setPage("login")}>Logout</button>
        </div>
      </div>
    );
  }

  return <h2>Loading...</h2>;
}

export default App;
import { BrowserRouter as Router } from 'react-router-dom';
import Navbar from './components/Navbar';
import AppRoutes from './routes/AppRoutes';
import './App.css';

function App() {
  return (
    <Router>
      <div className="min-h-screen bg-gray-50 text-gray-900">
        <Navbar />
        <AppRoutes />
      </div>
    </Router>
  );
}

export default App;
